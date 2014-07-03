using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gem.DependencyInjector.Wcf.Test
{
    [TestFixture]
    public class WcfDependencyInjectorIntegrationTest
    {
        [Test]
        public void TestDiContainerUsingWcfFactory()
        {
            var injector = new DependencyInjector(new Configuration());
            ITestService service = injector.GetObject<ITestService>();

            Assert.IsNotNull(service);
        }

        [Test]
        public void TestExposingPocoServiceObjectAsWcfHostedService()
        {
            using (var server = new ServiceUtil<TestService, ITestService>(new BasicHttpBinding(), new DependencyInjector(new Configuration())))
            {
                ITestService proxy = server.CreateClientProxy();
                DateTime date = DateTime.Now.Date;
                proxy.SetCurrentDate(date);
                DateTime actual = proxy.GetCurrentDate();
                Assert.AreEqual(date, actual);
            }
        }

        [Test]
        public void TestThrowExceptionIfDependencyInjectorIsNull()
        {
            Assert.Throws<DiServiceHostFactoryIntegrationException>(() => new ServiceUtil<TestService, ITestService>(new BasicHttpBinding(), null));
        }

        [Test]
        public void TestWcfServiceFactoryAndWcfClientProxyFactory()
        {
            var injector = new DependencyInjector(new Configuration());
            ITestService service = injector.GetObject<ITestService>();
            
            // Creating a service host...
            HostWrappingServiceFactory<ITestService> factory = new HostWrappingServiceFactory<ITestService>(service, new BasicHttpBinding(), "localhost");
            ServiceHost host = factory.Create();

            // Creating a client...
            WcfClientProxyFactory<ITestService> client = new WcfClientProxyFactory<ITestService>(new BasicHttpBinding(), "localhost");
            ITestService testService = client.Create();
            DateTime date = new DateTime(2012, 1, 1);
            using (testService  as IDisposable)
            {
                testService.SetCurrentDate(date);
                DateTime currentDate = testService.GetCurrentDate();

                Assert.AreEqual(date, currentDate);
            }

            host.Close();
            factory.Dispose();
            client.Dispose();
        }

        [Test]
        public void TestWcfServiceFactoryAndWcfClientProxyFactory_ThreadsUsingNewChannelFactory()
        {
            var injector = new DependencyInjector(new Configuration());
            ITestService service = injector.GetObject<ITestService>();

            // Creating a service host...
            HostWrappingServiceFactory<ITestService> factory = new HostWrappingServiceFactory<ITestService>(service, new BasicHttpBinding(), "localhost");
            ServiceHost host = factory.Create();
            

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 10, i =>
            {
                DateTime date = new DateTime(2012, 1, 1);
                Console.WriteLine("i = {0}, thread = {1}", i, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1);

                // Creating a client...
                WcfClientProxyFactory<ITestService> client = new WcfClientProxyFactory<ITestService>(new BasicHttpBinding(), "localhost");
                ITestService testService = client.Create();
                date = date.AddDays(i);    
                using (testService as IDisposable)
                {
                    try
                    {
                        testService.SetCurrentDate(date);
                        DateTime currentDate = testService.GetCurrentDate();
                        Console.WriteLine("Are not same: " + (date == currentDate) + " | For Run: "+ i +" | For thread: " + Thread.CurrentThread.ManagedThreadId);
                    } catch(Exception e)
                    {
                        
                    }
                }
                client.Dispose();
            });
            watch.Stop();
            Console.WriteLine("Time used: " + watch.ElapsedMilliseconds);
            host.Close();
            factory.Dispose();
            
        }

        [Test]
        public void TestWcfServiceFactoryAndWcfClientProxyFactory_ThreadsUsingSameChannelFactory()
        {
            var injector = new DependencyInjector(new Configuration());
            ITestService service = injector.GetObject<ITestService>();

            // Creating a service host...
            HostWrappingServiceFactory<ITestService> factory = new HostWrappingServiceFactory<ITestService>(service, new BasicHttpBinding(), "localhost");
            ServiceHost host = factory.Create();
            // Creating a client...
            WcfClientProxyFactory<ITestService> client = new WcfClientProxyFactory<ITestService>(new BasicHttpBinding(), "localhost");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 10, i =>
            {
                DateTime date = new DateTime(2012, 1, 1);
                Console.WriteLine("i = {0}, thread = {1}", i, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1);

                
                ITestService testService = client.Create();
                date = date.AddDays(i);
                using (testService as IDisposable)
                {
                    try
                    {
                        testService.SetCurrentDate(date);
                        DateTime currentDate = testService.GetCurrentDate();
                        Console.WriteLine("Are not same: " + (date == currentDate) + " | For Run: " + i + " | For thread: " + Thread.CurrentThread.ManagedThreadId);
                    }
                    catch (Exception e)
                    {

                    }
                }
            });
            watch.Stop();
            Console.WriteLine("Time used: "+watch.ElapsedMilliseconds);
            host.Close();
            factory.Dispose();
            client.Dispose();
        }

        [Test]
        public void TestWcfServiceFactoryAndWcfClientProxyFactory_ThreadsUsingSameChannelFactory_WhenServiceIsFaulted()
        {
            var injector = new DependencyInjector(new Configuration());
            ITestService service = injector.GetObject<ITestService>();

            // Creating a service host...
            HostWrappingServiceFactory<ITestService> factory = new HostWrappingServiceFactory<ITestService>(service, new BasicHttpBinding(), "localhost");
            ServiceHost host = factory.Create();
            DateTime date = new DateTime(2012, 1, 1);
            // Creating a client...
            WcfClientProxyFactory<ITestService> client = new WcfClientProxyFactory<ITestService>(new BasicHttpBinding(), "localhost");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 10, i =>
            {
                Console.WriteLine("i = {0}, thread = {1}", i, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1);


                ITestService testService = client.Create();
                date = date.AddDays(i-(i*2));
                using (testService as IDisposable)
                {
                    try
                    {
                        testService.SetCurrentDate(date);
                        DateTime currentDate = testService.GetCurrentDate();
                        Console.WriteLine("Are equal: "+(date == currentDate));
                    }catch(Exception e)
                    {
                        ICommunicationObject a = (ICommunicationObject)testService;
                        Console.WriteLine("a.State: " + a.State);
                    }
                    
                    //Assert.AreEqual(date, currentDate);
                }
            });
            watch.Stop();
            Console.WriteLine("Time used: " + watch.ElapsedMilliseconds);
            host.Close();
            factory.Dispose();
            client.Dispose();
        }

        [Test]
        public void TestWcfServiceFactoryAndWcfClientProxyFactory_WhenServiceIsFaulted()
        {
            var injector = new DependencyInjector(new Configuration());
            ITestService service = injector.GetObject<ITestService>();

            // Creating a service host...
            HostWrappingServiceFactory<ITestService> factory = new HostWrappingServiceFactory<ITestService>(service, new BasicHttpBinding(), "localhost");
            ServiceHost host = factory.Create();
            DateTime date = new DateTime(2012, 1, 1);
            // Creating a client...
            WcfClientProxyFactory<ITestService> client = new WcfClientProxyFactory<ITestService>(new BasicHttpBinding(), "localhost");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            ITestService testService = client.Create();
            using (testService as IDisposable)
            {
                try
                {
                    testService.SetCurrentDate(date);
                    date = date.AddDays(-2);
                    testService.SetCurrentDate(date);
                    DateTime currentDate = testService.GetCurrentDate();

                    Console.WriteLine("Are equal: " + (date == currentDate));
                } catch(Exception e)
                {
                    ICommunicationObject a = (ICommunicationObject)testService;
                    Console.WriteLine("a.State: " + a.State);
                    a.Abort();
                    Console.WriteLine("a.State: " + a.State);
                }
                //Assert.AreEqual(date, currentDate);
            }

            testService = client.Create();
            using (testService as IDisposable)
            {
                ICommunicationObject a = (ICommunicationObject)testService;
                a.Open();
                Console.WriteLine("a2.State: " + a.State);
            }
            watch.Stop();
            Console.WriteLine("Time used: " + watch.ElapsedMilliseconds);
            host.Close();
            factory.Dispose();
            client.Dispose();
        }

        [Test]
        public void TestUrlName()
        {
            WcfClientProxyFactory<ITestService> client = new WcfClientProxyFactory<ITestService>(new BasicHttpBinding(), "localhost");
            string name = client.GetServiceName();
            client.Dispose();
        }

        
    }

    public class WcfClientProxyFactory<T> : IDisposable
    {
        public readonly Binding Binding;
        public readonly string Url;
        public readonly string ServiceEndpointName = string.Empty;
        public readonly Type ServiceInterface = typeof(T);
        private ChannelFactory<T> factory;
        private readonly EndpointAddress address;
        private bool disposed;
        private readonly object synchLock = new object();

        public WcfClientProxyFactory(Binding binding, string url)
        {
            Binding = binding;
            ServiceEndpointName = ServiceInterface.Name;
            if (ServiceEndpointName.StartsWith("I")) ServiceEndpointName = ServiceEndpointName.Substring(1);
            Url = string.Format("{0}://{1}/{2}", Binding.Scheme, url, ServiceEndpointName);
            address = new EndpointAddress(Url);
            factory = new ChannelFactory<T>(Binding);
            factory.Closing += ChannelClosing;
            factory.Closed += ChannelClosed;
            factory.Opening += ChannelOpening;
            factory.Opened += ChannelOpened;
            factory.Faulted += ChannelFaulted;
            factory.Open();
            disposed = false;
        }

        public T Create()
        {
            T channel = factory.CreateChannel(address);
            return channel;
        }

        public string GetServiceName()
        {
            return Url + ".svc";
        }

        private void ChannelFaulted(object sender, EventArgs e)
        {
            // Should we restart the channel when it is faulted?
        }

        private void ChannelOpened(object sender, EventArgs e)
        {

        }

        private void ChannelOpening(object sender, EventArgs e)
        {

        }

        private void ChannelClosed(object sender, EventArgs e)
        {

        }

        private void ChannelClosing(object sender, EventArgs e)
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (synchLock)
            {
                if (!disposed)
                {
                    if (disposing)
                    {   // OK to use any private object references
                        factory.Closing -= ChannelClosing;
                        factory.Closed -= ChannelClosed;
                        factory.Opening -= ChannelOpening;
                        factory.Opened -= ChannelOpened;
                        factory.Faulted -= ChannelFaulted;
                        factory.Close();
                        factory = null;
                    }

                    disposed = true;
                }
            }
        }

        // The finalizer, should only be used when cleaning up unmanaged resources
        // such as a stream, file handle, network connections etc.
        ~WcfClientProxyFactory()
        {
            Dispose(false);
        }
    }
}