using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Gem.DependencyInjector.Wcf
{
    /// <summary>
    /// There are two usages of this class. The first usage is to provide the service that should be wrapped in a wcf service host
    /// to the ctor of this class. This class and wcf behavior and instance provider will basically pass this service object around
    /// until it is passed into the ServiceHost which wrappes it.
    /// 
    /// The second usage is to pass in a dependency injection container. The container will be queried for an instance of type T. In
    /// the default implementation of the WCF system, the ServiceHostFactory will use the service's default ctor and create an empty
    /// instance of it. This is of course no good. The InstanceProvider will now have a reference to the DI-Container and extract the
    /// instance to be wrapped from there.
    /// </summary>
    public class DiServiceHostFactoryIntegration : ServiceHostFactory
    {
        private static DependencyInjector container;
        private readonly object service;

        public DiServiceHostFactoryIntegration() { /*Keeping this ctor for debugging purpose only, setting breakpoints in here.*/ }

        internal DiServiceHostFactoryIntegration(object service)
        {
            this.service = service;
        }

        public DiServiceHostFactoryIntegration(DependencyInjector container)
        {
            DiServiceHostFactoryIntegration.container = container;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            ServiceHost serviceHost; 
            if (service != null && service.GetType() == serviceType) serviceHost = new DiServiceHost(service, baseAddresses);
            else if (container != null) serviceHost = new DiServiceHost(container, serviceType, baseAddresses);
            else throw new DiServiceHostFactoryIntegrationException("DiServiceHostFactoryIntegration has not been configured, DependencyInjector object is null? "+container);

            return serviceHost;
        }
    }
}