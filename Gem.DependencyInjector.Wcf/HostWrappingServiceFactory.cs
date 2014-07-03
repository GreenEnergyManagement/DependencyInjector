using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Gem.DependencyInjector.Wcf
{
    /// <summary>
    /// Utility class for wrapping a service inside a WCF service host.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    public class HostWrappingServiceFactory<T> : IDisposable
    {
        internal ServiceHost Host;
        internal readonly Type ServiceImplType = typeof(T); //The type of the service which must be resolved before it can be wrapped inside the ServiceHost.
        internal readonly string ServiceEndpointName = string.Empty;
        internal readonly object Service;
        internal readonly string Url;
        internal readonly string HostMachine = "localhost"; // the assigment is just a default value.
        internal readonly int? Port;
        internal readonly Binding Binding = new BasicHttpBinding();
        private bool disposed;

        /// <summary>
        /// Ctor that wraps a service of a type defined by the serviceImplType parameter. The service instance must be resolved either by
        /// the WCF framework (basically using Activator.CreateInstance, which of course is no good), or by custom code by extending the
        /// pluggable capabilities in WCF (mainly ServiceBehavior and InstanceProvider). Subclassing these classes gives you a chance to pass
        /// in your favourite DI-Container which can resolve the service instance.
        /// </summary>
        /// <param name="binding">The binding providing the connection information.</param>
        /// <param name="hostMachine">The machine serving the service, this information will be embedded into the binding object.</param>
        /// <param name="port">The port that listens for incoming calls, this will become a part of the URL which is embedded into the binding object.</param>
        public HostWrappingServiceFactory(Binding binding, string hostMachine, int port=0)
        {
            if (binding != null) Binding = binding;
            HostMachine = hostMachine;
            if (port > 0) Port = port;

            string machineAndPort = hostMachine;
            if (Port.HasValue) machineAndPort += ":" + Port.Value;

            ServiceEndpointName = ServiceImplType.Name;
            if (ServiceEndpointName.StartsWith("I")) ServiceEndpointName = ServiceEndpointName.Substring(1);
            Url = string.Format("{0}://{1}/{2}", Binding.Scheme, machineAndPort, ServiceImplType.Name);
            disposed = false;
        }

        /// <summary>
        /// Ctor for creating a host that wrappes an already created service object. There is no need to ship in a di-container here.
        /// </summary>
        /// <param name="service">The service instance to be wrapped.</param>
        /// <param name="binding">The binding  to be used by the wrapping host.</param>
        /// <param name="hostMachine">The machine serving the service, this will be part of the URL embedded into the binding object.</param>
        /// <param name="port">At what port the service will be listening for incoming calls, this will also be embedded into the URL and further to the binding object.</param>
        public HostWrappingServiceFactory(object service, Binding binding, string hostMachine, int port=0)
        {
            Service = service;
            if (binding != null) Binding = binding;
            HostMachine = hostMachine;
            if (port > 0) Port = port;

            string machineAndPort = hostMachine;
            if (Port.HasValue) machineAndPort += ":" + Port.Value;

            Url = string.Format("{0}://{1}/{2}", Binding.Scheme, machineAndPort, service.GetType().Name);
            disposed = false;
        }

        /// <summary>
        /// This method will be used when the service instance to be wrapped by the host is already passed into the ctor of this class. There is
        /// nothing to resolve, just instantiate the service host.
        /// </summary>
        /// <returns>A service host wrapping the service instance.</returns>
        public ServiceHost Create()
        {
            return DoCreate(null);
        }

        /// <summary>
        /// This method will be used when only the service type has been passed into this factory object. Then the service type must be resolved
        /// into an instance and that instance must be passed to the service host.
        /// </summary>
        /// <param name="container">A DI-Container holding the instance to be resolved.</param>
        /// <returns>A service host wrapping the resolved service instance.</returns>
        public ServiceHost Create(DependencyInjector container)
        {
            return DoCreate(container);
        }

        protected virtual ServiceHost DoCreate(DependencyInjector container)
        {
            if (Service != null) Host = CreateHostWrappingServiceObject(Service, Url);
            else Host = CreateHostWrappingServiceObjectByQueryingDiContainer(container, ServiceImplType, Url);

            try
            {
                if (Host.State == CommunicationState.Created || Host.State == CommunicationState.Closed || Host.State == CommunicationState.Faulted)
                {
                    Host.Open();
                    SetMaxObjectGraph();
                }
            }
            catch (TimeoutException timeoutEx)
            {
                throw new WrappedServiceHostException("Failed to close the service host, see inner exception - Message: " + timeoutEx.Message, timeoutEx);
            }
            catch (CommunicationException communicationEx)
            {
                throw new WrappedServiceHostException("Failed to close the service host, see inner exception - Message: " + communicationEx.Message, communicationEx);
            }

            return Host;
        }

        private void SetMaxObjectGraph()
        {
            foreach (ServiceEndpoint endPoint in Host.Description.Endpoints)
            {
                foreach (OperationDescription desc in endPoint.Contract.Operations)
                {
                    DataContractSerializerOperationBehavior dcsOperationBehavior = desc.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    dcsOperationBehavior.MaxItemsInObjectGraph = Int32.MaxValue;
                    
                }
            }
        }

        private static ServiceHost CreateHostWrappingServiceObjectByQueryingDiContainer(DependencyInjector container, Type serviceImplType, string url)
        {
            DiServiceHostFactory serviceHostFactory = new DiServiceHostFactory(container);
            return serviceHostFactory.CreateHostForService(serviceImplType, new[] { new Uri(url) });
        }

        private static ServiceHost CreateHostWrappingServiceObject(object serviceInstance, string url)
        {
            DiServiceHostFactory serviceHostFactory = new DiServiceHostFactory(serviceInstance);
            return serviceHostFactory.CreateHostForService(serviceInstance.GetType(), new[] { new Uri(url) });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Host != null)
                    {
                        try
                        {
                            if (Host.State == CommunicationState.Opened || Host.State == CommunicationState.Opening)
                            {
                                Host.Close();
                            }
                        }
                        catch (TimeoutException timeoutEx)
                        {
                            // replace with log
                            Console.WriteLine("Failed to close the service host - Exception: " + timeoutEx.Message);
                        }
                        catch (CommunicationException communicationEx)
                        {
                            // replace with log
                            Console.WriteLine("Failed to close the service host - Exception: " + communicationEx.Message);
                        }
                        finally
                        {
                            Host = null;
                        }
                    }
                }

                disposed = true;
            }
        }

        // The finalizer, should only be used when cleaning up unmanaged resources
        // such as a stream, file handle, network connections etc.
        ~HostWrappingServiceFactory()
        {
            Dispose(false);
        }
    }
}