using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Gem.DependencyInjector.Test
{
    [TestFixture]
    public class AssemblyScannerTest
    {
        [Test]
        public void TestScanForTypesWithAttribute()
        {
            List<Type> scanForTypesWithAttribute = AssemblyScanner.ScanForTypesWithAttribute<ConfigurationAttribute>(1);
            Assert.IsNotEmpty(scanForTypesWithAttribute);
        }

        [Test]
        public void TestScanForTypeImplementingInterface()
        {
            List<Type> scanForTypesImplementingInterface = AssemblyScanner.ScanForTypeImplementingInterface<IConfigureDependencies>(1);
            Assert.IsNotEmpty(scanForTypesImplementingInterface);
        }

        [Test]
        public void TestScanForTypeImplementingInterfaceWithAttribute()
        {
            List<Type> scanForTypeImplementingInterfaceWithAttribute = AssemblyScanner.ScanForTypeImplementingInterfaceWithAttribute<IConfigureDependencies, ConfigurationAttribute>(1);
            Assert.IsNotEmpty(scanForTypeImplementingInterfaceWithAttribute);
        }

        [Test]
        public void TestScanForTypeImplementingInterfaceWithAttributeSequential()
        {
            int maxFilesForSequentialScan = 100;
            List<Type> scanForTypeImplementingInterfaceWithAttribute = AssemblyScanner.ScanForTypeImplementingInterfaceWithAttribute<IConfigureDependencies, ConfigurationAttribute>(maxFilesForSequentialScan);
            Assert.IsNotEmpty(scanForTypeImplementingInterfaceWithAttribute);
        }

        [Test]
        public void TestScanForTypeInheritingFromSuperType()
        {
            List<Type> scanForTypeInheritingFromSuperType = AssemblyScanner.ScanForTypeInheritingFromSuperType<SomeDomainCmdServiceC>(1);
            Assert.IsNotEmpty(scanForTypeInheritingFromSuperType);
        }

        [Test]
        public void TestScanForTypeInheritingFromSuperTypeWithAttribute()
        {
            List<Type> scanForTypeInheritingFromSuperTypeWithAttribute = AssemblyScanner.ScanForTypeInheritingFromSuperTypeWithAttribute<SomeDomainCmdServiceC, MyCustomAttribute>(1);
            Assert.IsNotEmpty(scanForTypeInheritingFromSuperTypeWithAttribute);
        }
    }
}