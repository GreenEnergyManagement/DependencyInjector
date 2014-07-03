using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Gem.DependencyInjector.Wcf
{
    public class DiServiceBehavior : IServiceBehavior
    {
        private DependencyInjector container;
        private object service;

        public DiServiceBehavior(DependencyInjector container)
        {
            this.container = container;
        }

        public DiServiceBehavior(object service)
        {
            this.service = service;
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher cd = cdb as ChannelDispatcher; 
                if (cd != null)
                {
                    foreach (EndpointDispatcher ed in cd.Endpoints)
                    {
                        if(service != null) ed.DispatchRuntime.InstanceProvider = new DiInstanceProvider(service);
                        if(container!=null) ed.DispatchRuntime.InstanceProvider = new DiInstanceProvider(container, serviceDescription.ServiceType);
                    }
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
    }
}