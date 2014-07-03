using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Gem.DependencyInjector.Wcf.Test
{
    public class ServiceUtil<T, I> : IDisposable where T : class
    {
        private ServiceHost host;
        private readonly Type serviceImplType = typeof (T);
        private readonly string url;
        private readonly string hostMachine = "localhost";
        private readonly int? port;
        private readonly Binding binding;
        private ChannelFactory<I> factory;
        private I channel;

        public ServiceUtil() : this(new NetTcpBinding())
        {
        }

        public ServiceUtil(Binding binding) : this(binding, DependencyInjector.ScanAssemblies(1))
        {
        }

        public ServiceUtil(Binding binding, DependencyInjector di)
        {
            this.binding = binding;
            string machineAndPort = hostMachine;
            if (port.HasValue) machineAndPort += ":" + port.Value;

            url = string.Format("{0}://{1}/{2}", this.binding.Scheme, machineAndPort, Guid.NewGuid());
            DiServiceHostFactory serviceHostFactory = new DiServiceHostFactory(di);
            host = serviceHostFactory.CreateHostForService(serviceImplType, new Uri[] {new Uri(url),});

            try
            {
                host.Open();
            }
            catch (TimeoutException timeoutEx)
            {
                Console.WriteLine("Failed to close the service host - Exception: " + timeoutEx.Message);
                throw;
            }
            catch (CommunicationException communicationEx)
            {
                Console.WriteLine("Failed to close the service host - Exception: " + communicationEx.Message);
                throw;
            }
        }

        public I CreateClientProxy()
        {
            factory = new ChannelFactory<I>(binding);
            channel = factory.CreateChannel(new EndpointAddress(url));

            return channel;
        }

        public void Dispose()
        {
            if (factory != null)
            {
                factory.Close();
                factory = null;
            }

            DisposeService();
        }

        public void DisposeService()
        {
            if (host != null)
            {
                try
                {
                    if (host.State == CommunicationState.Opened || host.State == CommunicationState.Opening)
                    {
                        host.Close();
                    }
                }
                catch (TimeoutException timeoutEx)
                {
                    Console.WriteLine("Failed to close the service host - Exception: " + timeoutEx.Message);
                }
                catch (CommunicationException communicationEx)
                {
                    Console.WriteLine("Failed to close the service host - Exception: " + communicationEx.Message);
                }
                finally
                {
                    host = null;
                }
            }
        }
    }
}