using System;
using System.ServiceModel;

namespace Gem.DependencyInjector.Wcf
{
    /// <summary>
    /// This class is the class that we must interact with when instantiating WCF services. The DiServiceHostFactoryIntegration class, 
    /// which this class inherits from, will be used by the WCF framework when resolving the service instance to be wrapped inside a host.
    /// The method, CreateServiceHost(Type serviceType, Uri[] baseAddresses) on DiServiceHostFactoryIntegration is protected and cannot be
    /// invoked directly. This class main responsibility is to expose this method through CreateHostForService(Type serviceType, Uri[] baseAddresses)
    /// so that we can call it at need.
    /// </summary>
    public class DiServiceHostFactory : DiServiceHostFactoryIntegration
    {
        public DiServiceHostFactory(object service) :base(service) { }
        public DiServiceHostFactory(DependencyInjector container) :base(container) { }

        /// <summary>
        /// Method exposing protected CreateServiceHost(serviceType, baseAddresses); method in base class.
        /// </summary>
        /// <param name="serviceType">The type of the service an instance provider must provide an instance of.</param>
        /// <param name="baseAddresses">The base address, example: http://myMachine:54321/ </param>
        /// <returns>A host for exposing the service object.</returns>
        public ServiceHost CreateHostForService(Type serviceType, Uri[] baseAddresses)
        {
            return CreateServiceHost(serviceType, baseAddresses);
        }
    }
}