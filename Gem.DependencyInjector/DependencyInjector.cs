using System;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Gem.DependencyInjector
{
    public class DependencyInjector : IDisposable
    {
        protected IDictionary<string, object> map = new Dictionary<string, object>();  // Usually type.FullName as key-to-object instance map.
        protected IDictionary<Type, IDictionary<string, bool>> instanceTypeKeyMap = new Dictionary<Type, IDictionary<string, bool>>(); // Usually an implementation key-to-key map.
        protected IDictionary<Type, IDictionary<string, bool>> returnTypeKeyMap = new Dictionary<Type, IDictionary<string, bool>>(); // Usually an interface key-to-key map.
        protected IDictionary<Type, IConfigureDependencies> configFiles = new Dictionary<Type, IConfigureDependencies>(); // Storing all config files.
        // Gathering all instances to inject.
        protected IDictionary<string, DefinitionMetaData> metaMap = new Dictionary<string, DefinitionMetaData>();

        #region Configuration...
        public static DependencyInjector ScanAssemblies(int maxFilesForSequentialScan)
        {
            List<Type> scanForTypesWithAttribute = AssemblyScanner.ScanForTypeImplementingInterfaceWithAttribute<IConfigureDependencies, ConfigurationAttribute>(maxFilesForSequentialScan);
            var configurations = new List<IConfigureDependencies>();
            foreach (Type type in scanForTypesWithAttribute)
            {
                configurations.Add((IConfigureDependencies) Activator.CreateInstance(type));
            }

            return new DependencyInjector(configurations.ToArray());
        }

        public DependencyInjector(params IConfigureDependencies[] configuraitons)
        {
            ExpandConfiguration(configuraitons);
        }

        public DependencyInjector ExpandConfiguration(params IConfigureDependencies[] configuraitons)
        {
            foreach (IConfigureDependencies configuraiton in configuraitons)
            {
                var key = configuraiton.GetType();
                if ( !configFiles.ContainsKey(key))
                {
                    configFiles.Add(configuraiton.GetType(), configuraiton);
                    BuildMetaDataModel(configuraiton);    
                } else
                {
                  System.Diagnostics.Trace.WriteLine("Config already exists for key: " + key);
                }
                
            }
            Configure();
            return this;
        }

        public void Reconfigure()
        {
            ClearMaps();
            foreach (var configuraiton in configFiles.Values) BuildMetaDataModel(configuraiton);

            Configure();
        }

        protected virtual void BuildMetaDataModel(IConfigureDependencies configuraiton)
        {
            ConfigurationAttribute[] configAttributeArr = (ConfigurationAttribute[])configuraiton.GetType().GetCustomAttributes(typeof(ConfigurationAttribute), false);
            if (configAttributeArr != null && configAttributeArr.Length == 1)
            {
                // Research how to enable internal methods in the config files, this is necessary to maintain design visibility for some classes.
                MethodInfo[] methodInfos = configuraiton.GetType().GetMethods(/*BindingFlags.Public | BindingFlags.NonPublic*/);
                foreach (var methodInfo in methodInfos)
                {
                    DefinitionAttribute[] definitionAttributeArr = (DefinitionAttribute[])methodInfo.GetCustomAttributes(typeof(DefinitionAttribute), false);
                    if (definitionAttributeArr != null && definitionAttributeArr.Length == 1)
                    {
                        var definitionAttribute = definitionAttributeArr[0];
                        if (definitionAttribute != null)
                        {
                            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                            List<Type> paramTypes = new List<Type>();
                            foreach (var parameterInfo in parameterInfos)
                            {
                                Type type = parameterInfo.ParameterType;
                                paramTypes.Add(type);
                            }

                            Type produceType;
                            if (definitionAttribute.ProduceType != null) produceType = definitionAttribute.ProduceType;
                            else produceType = methodInfo.ReturnType;

                            string id;
                            if (!string.IsNullOrEmpty(definitionAttribute.Id)) id = definitionAttribute.Id;
                            else id = produceType.FullName;

                            DefinitionMetaData def = new DefinitionMetaData(configuraiton, paramTypes, methodInfo, produceType, id);
                            if (metaMap.ContainsKey(def.Id))
                                throw new ArgumentException(string.Format("MetMap already contains Id {0}. Failed adding DefinitionMetaData", def.Id, def.ToString()));
                            metaMap.Add(def.Id, def);
                        }
                    }
                }
            }
        }

        //Now figure out wich order objects must be resolved in, but only if all params needed is already in the map...
        //If circular dependencies exists or something is unresolvable, then throw exception.
        private void Configure()
        {
            DefinitionMetaData[] sortedDependencies = ParallelQuickSort.Sort(new List<DefinitionMetaData>(metaMap.Values).ToArray());
            List<int> unresolvedIndexes = new List<int>();

            for (int i = 0; i < sortedDependencies.Length; i++)
            {
                DefinitionMetaData def = sortedDependencies[i];

                InstantiateDefinition(i, unresolvedIndexes, def);
            }

            for (int i = 0; i < 20; i++)
            {
                // Make list behave like stack
                if (unresolvedIndexes.Count > 0)
                {
                    int unresolvedIndex = unresolvedIndexes[0];
                    unresolvedIndexes.RemoveAt(0);
                    DefinitionMetaData def = sortedDependencies[unresolvedIndex];
                    InstantiateDefinition(unresolvedIndex, unresolvedIndexes, def);
                }
                else break;
            }

            DependencyInjectorConfigurationException ex = HandleUnresolvedDefinitions(sortedDependencies, unresolvedIndexes);
            if (ex != null) throw ex;
            metaMap.Clear();
        }

        protected virtual void InstantiateDefinition(int i, List<int> unresolvedIndexes, DefinitionMetaData def)
        {
            bool canInstantiate = true;
            List<object> arguments = new List<object>(def.Parameters.Count);
            foreach (Type paramType in def.Parameters)
            {
                if (map.ContainsKey(paramType.FullName)) arguments.Add(map[paramType.FullName]);
                else if (instanceTypeKeyMap.ContainsKey(paramType) && instanceTypeKeyMap[paramType].Count == 1)
                    arguments.Add(GetUniqueInstance(instanceTypeKeyMap, paramType));
                else if (returnTypeKeyMap.ContainsKey(paramType) && returnTypeKeyMap[paramType].Count == 1)
                    arguments.Add(GetUniqueInstance(returnTypeKeyMap, paramType));
                else
                {
                    canInstantiate = false;
                    unresolvedIndexes.Add(i);
                    break;
                }
            }

            if (canInstantiate)
            {
                if (!map.ContainsKey(def.Id))
                {
                    object instance = def.ExacutableBody.Invoke(def.OwningInstance, arguments.ToArray());
                    if (instance is IDependencyInjectorAware) ((IDependencyInjectorAware) instance).Container = this;

                    map.Add(def.Id, instance);

                    // Usually one to one mapping, but if return type is a superclass this could be a one to many...
                    Type instanceType = instance.GetType();
                    if (!instanceTypeKeyMap.ContainsKey(instanceType))
                        instanceTypeKeyMap.Add(instanceType, new Dictionary<string, bool>());
                    instanceTypeKeyMap[instanceType].Add(def.Id, true);

                    // Usually interfaces are stored here, and this is a one to many mapping...
                    if (!returnTypeKeyMap.ContainsKey(def.OutPutType))
                        returnTypeKeyMap.Add(def.OutPutType, new Dictionary<string, bool>());
                    returnTypeKeyMap[def.OutPutType].Add(def.Id, true);
                }
                else
                    throw new DependencyInjectorConfigurationException(
                        "An object with the same identifier has already been created in the dependency container: " + def.Id);
            }
        }

        private DependencyInjectorConfigurationException HandleUnresolvedDefinitions(DefinitionMetaData[] sortedDependencies, List<int> unresolvedIndexes)
        {
            if (unresolvedIndexes.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Unable to resolve object definitions identified as: ");
                foreach (int i in unresolvedIndexes)
                {
                    builder.AppendLine(sortedDependencies[i].Id);
                }

                return new DependencyInjectorConfigurationException(builder.ToString());
            }
            return null;
        }
        #endregion

        #region Interact with the container...
        public T GetObject<T>()
        {
            return (T) GetObject(typeof(T));
        }

        public object GetObject(Type type)
        {
            object instance = GetRequestedObject(type);
            return CheckInstance(instance);
        }

        private static object CheckInstance(object instance)
        {
            if (instance is IObjectFactory) instance = ((IObjectFactory) instance).Create();
            return instance;
        }

        private object GetRequestedObject(Type type)
        {   // if provided type is interface and we only have one candidate, then return that candidate.
            if (returnTypeKeyMap.ContainsKey(type))
            {
                if (returnTypeKeyMap[type].Count == 1) return GetUniqueInstance(returnTypeKeyMap, type);
                throw CreateMultipleInstancesError(returnTypeKeyMap, type,
                                                   " objects implementing interface found. Use object definition id to select among the candidate objects. Interface signature: ");
            }

            // if provided type is base class or instance class and we only have one candidate, then return that candidate.
            if (instanceTypeKeyMap.ContainsKey(type))
            {
                if (instanceTypeKeyMap[type].Count == 1) return GetUniqueInstance(instanceTypeKeyMap, type);
                throw CreateMultipleInstancesError(instanceTypeKeyMap, type,
                                                   " objects of provided type found. Use object definition id to select among the candidate objects. Type signature: ");
            }

            // if none of the above, then type value must be converted to it's name as that is always possible to get for named objects.
            string key = type.FullName;
            try
            {
                return GetObject(key);
            }
            catch (DependencyInjectorGetObjectException getObjectException)
            {
                throw new DependencyInjectorGetObjectException(
                    "Wrapping inner exception to provide type information. No object of type found in the dependency injector container. Type signature: " +
                    type.FullName, getObjectException);
            }
        }

        private DependencyInjectorGetObjectException CreateMultipleInstancesError(IDictionary<Type, IDictionary<string, bool>> dict, Type type, string errorMsg)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(dict[type].Count).Append(errorMsg).Append(type.FullName).AppendLine();
            List<string> objectDefinitionIds = new List<string>();
            int counter = 1;
            foreach (string id in dict[type].Keys)
            {
                builder.Append("Candidate "+counter+"; object definition id: ").Append(id).AppendLine();
                objectDefinitionIds.Add(id);
                counter++;
            }
            return new DependencyInjectorGetObjectException(builder.ToString(), objectDefinitionIds.ToArray());
        }

        protected object GetUniqueInstance(IDictionary<Type, IDictionary<string, bool>> dict, Type type)
        {
            foreach (string id in dict[type].Keys) return map[id];
            throw new DependencyInjectorGetObjectException("No object of provided type could be found in the dependency injector container. Type signature: "+type.FullName);
        }

        public T GetObject<T>(string id)
        {
            if (map.ContainsKey(id)) return (T)CheckInstance(map[id]);
            throw new DependencyInjectorGetObjectException("Could not find provided object definition id in the dependency injector container. Object definition id: " + id);
        }

        public object GetObject(string id)
        {
            if (map.ContainsKey(id)) return CheckInstance(map[id]);
            throw new DependencyInjectorGetObjectException("Could not find provided object definition id in the dependency injector container. Object definition id: " + id);
        }

        public List<T> GetAllObjectsInheriting<T>()
        {
            return GetAllObjectsInheriting(typeof (T)).ConvertAll(e => (T)e);
        }

        public List<object> GetAllObjectsInheriting(Type type)
        {
            // Search through interfaces first because objects that are produced by factories will also be registered here...
            List<object> objects = FindObjects(type, returnTypeKeyMap);
            if(objects.Count == 0) objects = FindObjects(type, instanceTypeKeyMap);
            return objects;
        }

        private List<object> FindObjects(Type type, IDictionary<Type, IDictionary<string, bool>> dict)
        {
            if (dict.ContainsKey(type))
            {
                var objects = new List<object>();
                ICollection<string> ids = returnTypeKeyMap[type].Keys;
                foreach (string id in ids)
                {
                    objects.Add(CheckInstance(map[id]));
                }
                return objects;
            }
            return new List<object>();
        }

        public bool ContainsObject<T>()
        {
            return ContainsObject(typeof(T));
        }

        public bool ContainsObject(Type t)
        {
            return ContainsObject(t.FullName);
        }

        public bool ContainsObject(string key)
        {
            return map.ContainsKey(key);
        }
        #endregion


        public void Dispose()
        {
            ClearMaps();
            configFiles.Clear();
        }

        // TODO: Wind through all dictionaries and Dispose all objects implementing IDisposable before clearing
        private void ClearMaps()
        {
            foreach (IDisposable value in map.Values.OfType<IDisposable>())
            {
                (value).Dispose();
            }

            metaMap.Clear();
            map.Clear();
            instanceTypeKeyMap.Clear();
            returnTypeKeyMap.Clear();
        }
    }
}