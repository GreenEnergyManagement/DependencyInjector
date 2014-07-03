using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Gem.DependencyInjector
{
    public static class AssemblyScanner
    {
        public static List<Type> ScanForTypesWithAttribute<T>(int maxFilesForSequentialScan = 4000)
        {
            return Scan(null, false, typeof(T), maxFilesForSequentialScan);
        }

        public static List<Type> ScanForTypeImplementingInterface<T>(int maxFilesForSequentialScan = 4000)
        {
            return Scan(typeof(T), true, null, maxFilesForSequentialScan);
        }

        public static List<Type> ScanForTypeImplementingInterfaceWithAttribute<T, TAttribute>(int maxFilesForSequentialScan = 4000)
        {
            return Scan(typeof(T), true, typeof(TAttribute), maxFilesForSequentialScan);
        }

        public static List<Type> ScanForTypeInheritingFromSuperType<T>(int maxFilesForSequentialScan = 4000)
        {
            return Scan(typeof(T), false, null, maxFilesForSequentialScan);
        }

        public static List<Type> ScanForTypeInheritingFromSuperTypeWithAttribute<T, TAttribute>(int maxFilesForSequentialScan = 4000)
        {
            return Scan(typeof(T), false, typeof(TAttribute), maxFilesForSequentialScan);
        }

        private static List<Type> Scan(Type superType, bool scanForInterface, Type attributeType, int maxFilesForSequentialScan = 4000)
        {
            
            List<Type> candidates = new List<Type>();
            string assemblyScanPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            IEnumerable<string> files = Directory.EnumerateFiles(assemblyScanPath, "*.dll", SearchOption.AllDirectories);
            List<string> fileList = new List<string>(files);
            if (fileList.Count > maxFilesForSequentialScan) Parallel.ForEach(fileList, f => DoScan(f, superType, scanForInterface, attributeType, candidates));
            else foreach (string file in fileList) DoScan(file, superType, scanForInterface, attributeType, candidates);
            
            return candidates;
        }

        private static void DoScan(string dllFilePath, Type superType, bool scanForInterface, Type attributeType, List<Type> candidates)
        {
            FileInfo dllFile = new FileInfo(dllFilePath);
            if (dllFile.Name.Contains("System") || dllFile.Name.Contains("Microsoft")) return;
            if (dllFile.Name.Equals(typeof(AssemblyScanner).Assembly.ManifestModule.Name)) return;
                    
            bool loadAssembly = false;
            try
            {
                // usre reflection-read-only to load assemblies and inspect their public signed key before loading them fully
                //var assembly = Assembly.ReflectionOnlyLoad(dllFilePath);
                AssemblyName an = AssemblyName.GetAssemblyName(dllFilePath);
                Assembly assembly = Assembly.Load(an);
                var types = assembly.GetTypes();

                bool scanForAttribute = (attributeType != null);
                bool candidateFound = (superType == null); // If superType is null, then every type is a candidate.
                foreach (var type in types)
                {
                    if (superType != null)
                    {
                        if (scanForInterface)
                        {
                            if (TypeImplementsInterface(type, superType)) candidateFound = true;
                            else candidateFound = false;
                        }
                        else
                        {
                            if (type.IsSubclassOf(superType)) candidateFound = true;
                            else candidateFound = false;
                        }
                    }

                    if (scanForAttribute && candidateFound)
                    {
                        Attribute[] customAttributes = (Attribute[]) type.GetCustomAttributes(attributeType, false);
                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            foreach (Attribute attribute in customAttributes)
                            {
                                if (attribute.GetType() == attributeType)
                                {
                                    AddCandidate(type, candidates, out loadAssembly);
                                    break;
                                }
                            }
                        }
                    }
                    else if (candidateFound) AddCandidate(type, candidates, out loadAssembly);
                }
                    
            }
            catch (Exception)
            {
                //WHAT, tyring to load a win32 dll??? make sure we can skip that.
                // We can easily skip that if we only load assemblies signed without a key...
            }

            if(loadAssembly) LoadAssembly(dllFilePath);
        }

        private static void AddCandidate(Type type, List<Type> candidates, out bool loadAssembly)
        {
            candidates.Add(type);
            loadAssembly = true;
        }

        private static bool TypeImplementsInterface(Type type, Type typeToFind)
        {
            var ifaces = type.GetInterfaces();
            foreach (Type iface in ifaces)
            {
                if (iface == typeToFind) return true;
            }
            return false;
        }

        private static void LoadAssembly(string dll)
        {
            AssemblyName an = AssemblyName.GetAssemblyName(dll);
            Assembly.Load(an);
        }
    }
}