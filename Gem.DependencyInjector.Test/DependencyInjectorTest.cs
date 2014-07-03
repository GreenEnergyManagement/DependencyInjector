using System.Collections.Generic;
using NUnit.Framework;

namespace Gem.DependencyInjector.Test
{
    [TestFixture]
    public class DependencyInjectorTest
    {

        [Test]
        public void TestSimpleOrderingOfObjectsInMetaDataMap()
        {
            var container = new DependencyInjector(new ServiceConfiguration());            
            Assert.IsNotNull(container);
            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
        }

        [Test]
        public void TestOrderingOfObjectsInMetaDataMap()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            Assert.IsNotNull(container);

            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));
        }

        [Test]
        public void TestOrderingOfAggregatedObjectsInMetaDataMap()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig(), new ComplexConfig());
            Assert.IsNotNull(container);

            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));
            Assert.IsTrue(container.ContainsObject<IAggregate>());
        }

        [Test]
        public void TestConfigurationExpansion()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            Assert.IsNotNull(container);

            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));

            container.ExpandConfiguration(new ComplexConfig());
            Assert.IsTrue(container.ContainsObject<IAggregate>());
        }

        [Test]
        public void TestReConfiguration()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            Assert.IsNotNull(container);

            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));

            container.ExpandConfiguration(new ComplexConfig());
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            container.Reconfigure();
            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            IAggregate aggregate = container.GetObject<IAggregate>();
            Assert.IsNotNull(aggregate);

            var b = container.GetObject("SomeDomainCmdServiceB");
            Assert.IsNotNull(b);
        }

        [Test]
        public void TestDisposeAndReConfiguration()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            Assert.IsNotNull(container);

            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));

            container.ExpandConfiguration(new ComplexConfig());
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            container.Reconfigure();
            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            IAggregate aggregate = container.GetObject<IAggregate>();
            Assert.IsNotNull(aggregate);

            var b = container.GetObject("SomeDomainCmdServiceB");
            Assert.IsNotNull(b);

            container.Dispose();

            Assert.Throws<DependencyInjectorGetObjectException>(() => container.GetObject("SomeDomainCmdServiceB"));
            container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            Assert.IsNotNull(container);

            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));

            container.ExpandConfiguration(new ComplexConfig());
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            container.Reconfigure();
            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            aggregate = container.GetObject<IAggregate>();
            Assert.IsNotNull(aggregate);

            b = container.GetObject("SomeDomainCmdServiceB");
            Assert.IsNotNull(b);
        }

        [Test]
        public void TestGetObjectFromContainer()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC());
            container.Reconfigure();
            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            IAggregate aggregate = container.GetObject<IAggregate>();
            Assert.IsNotNull(aggregate);

            var b = container.GetObject("SomeDomainCmdServiceB");
            Assert.IsNotNull(b);

            // If some instance uniquely implements interface but the object definition id is specified, can it still be retrieved through the interface??
            ISomeNewDomainCmdService newCmdService = container.GetObject<ISomeNewDomainCmdService>();
            Assert.IsNotNull(newCmdService);
            SomeNewDomainCmdService newCmdService2 = container.GetObject<SomeNewDomainCmdService>();
            Assert.IsNotNull(newCmdService2);
            var serviceD = container.GetObject<SomeDomainCmdServiceD>("D");
             
            Assert.Throws<DependencyInjectorGetObjectException>(() => container.GetObject<ISomeDomainCmdService>());
            Assert.Throws<DependencyInjectorGetObjectException>(() => container.GetObject<SomeDomainCmdServiceD>());
            Assert.Throws<DependencyInjectorGetObjectException>(() => container.GetObject<SomeDomainCmdServiceD>("E"));
            Assert.Throws<DependencyInjectorGetObjectException>(() => container.GetObject<INotImplementedInterface>());
        }

        [Test]
        public void TestGetObjectsInheritingFromSuperClassFromContainer()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig());
            container.Reconfigure();
            Assert.IsTrue(container.ContainsObject<ISomeDomainCmdService>());
            Assert.IsTrue(container.ContainsObject<ICommandManager>());
            Assert.IsTrue(container.ContainsObject("SomeDomainCmdServiceB"));
            Assert.IsTrue(container.ContainsObject<IAggregate>());

            List<ISomeDomainCmdService> someDomainCmdServices = container.GetAllObjectsInheriting<ISomeDomainCmdService>();
            Assert.AreEqual(2, someDomainCmdServices.Count);

            List<SomeDomainCmdServiceC> someDomainCmdServicesC = container.GetAllObjectsInheriting<SomeDomainCmdServiceC>();
            Assert.AreEqual(0, someDomainCmdServicesC.Count);
        }

        [Test]
        public void TestCommandserviceC()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new ServiceConfigurationC());
            var service = container.GetObject<ISomeDomainCmdService>("C");
            Assert.IsNotNull(service);
            Assert.IsInstanceOf<SomeDomainCmdServiceC>(service);

        }

        // DependencyInjector.ScanAssemblies(1) will work in normal situations, only in this test the class ServiceConfigurationNotConfigurable
        // will make this method fail. In production settings you would fix the class...
        [Test]
        public void TestScanAssemblies()
        {
            // Scan will load ServiceConfigurationNotConfigurable and thus should fail. As long as this class is in current assembly then it will always fail...
            Assert.Throws<DependencyInjectorConfigurationException>(() => DependencyInjector.ScanAssemblies(1));
        }

        [Test]
        public void TestThrowsExceptionOnUnresolvableConfig()
        {
            Assert.Throws<DependencyInjectorConfigurationException>(() => new DependencyInjector(new ServiceConfigurationNotConfigurable()));
        }

        [Test]
        public void TestGetPrototypeFromContainerGenericArgs()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC()).Reconfigure();

            Prototype prototype = container.GetObject<Prototype>();
            Assert.IsNotNull(prototype);
        }

        [Test]
        public void TestGetPrototypeFromContainerObjArgs()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC()).Reconfigure();

            Prototype prototype = (Prototype) container.GetObject(typeof(Prototype));
            Assert.IsNotNull(prototype);
        }

        [Test]
        public void TestGetPrototypeFromContainerGenericArgsWithId()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC()).Reconfigure();

            Prototype prototype = container.GetObject<Prototype>(typeof(Prototype).FullName);
            Assert.IsNotNull(prototype);
        }

        [Test]
        public void TestGetPrototypeFromContainerObjArgsWithId()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC()).Reconfigure();

            Prototype prototype = (Prototype)container.GetObject(typeof(Prototype).FullName);
            Assert.IsNotNull(prototype);
        }

        [Test]
        public void TestGetObjectsInheritingFromPrototypeSuperClassFromContainer()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig());
            container.Reconfigure();

            List<Prototype> prototypes = container.GetAllObjectsInheriting<Prototype>();
            Assert.AreEqual(1, prototypes.Count);
        }

        [Test]
        public void TestContainsPrototypeWithId()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC()).Reconfigure();

            bool b = container.ContainsObject(typeof (Prototype).FullName);
            Assert.IsTrue(b);
        }

        [Test]
        public void TestContainsPrototypeWithGenericArg()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC()).Reconfigure();

            bool b = container.ContainsObject<Prototype>();
            Assert.IsTrue(b);
        }

        [Test]
        public void TestContainsPrototypeWithObjectArg()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new MoreConfig());
            container.ExpandConfiguration(new ComplexConfig(), new ServiceConfigurationC()).Reconfigure();

            bool b = container.ContainsObject(typeof(Prototype));
            Assert.IsTrue(b);
        }

        [Test, Ignore("We will address this problem later...Need to extend def-methods parameters with attribute hint...")]
        public void TestAmbiguesObjectResolvement()
        {
            var container = new DependencyInjector(new ServiceConfiguration(), new ServiceConfigurationC());
            var composit = container.GetObject<Composit>();
            Assert.IsNotNull(composit);
            Assert.IsInstanceOf<SomeDomainCmdServiceD>(composit.Service);

        }

        // Example usage in a console app...
        /*class Program
        {
            static void Main(string[] args)
            {
                var container = DependencyInjector.ScanAssemblies(1);
                Console.Out.WriteLine("container.ContainsObject<ISomeDomainCmdService>(): " + container.ContainsObject<ISomeDomainCmdService>());
                Console.Out.WriteLine("container.ContainsObject<ICommandManager>(): " + container.ContainsObject<ICommandManager>());
                Console.Out.WriteLine("container.ContainsObject(\"SomeDomainCmdServiceB\"): " + container.ContainsObject("SomeDomainCmdServiceB"));
                Console.Out.WriteLine("container.ContainsObject<IAggregate>(): " + container.ContainsObject<IAggregate>());

                List<ISomeDomainCmdService> someDomainCmdServices = container.GetAllObjectsInheriting<ISomeDomainCmdService>();
                Console.Out.WriteLine("container.GetAllObjectsInheriting<ISomeDomainCmdService>() should give 3: " + someDomainCmdServices.Count);

                Console.Out.WriteLine("Press 'Enter' to exit...");
                Console.In.ReadLine();
            }
        }*/
    }
}
