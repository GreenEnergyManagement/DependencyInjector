using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Gem.DependencyInjector.Wcf
{
    public class DiInstanceProvider : IInstanceProvider
    {
        private Type serviceType;
        private DependencyInjector container;
        private object service;

        public DiInstanceProvider(DependencyInjector container, Type serviceType)
        {
            this.serviceType = serviceType;
            this.container = container;
        }

        public DiInstanceProvider(object service)
        {
            this.service = service;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            // If we already have the service instance, then just return it to the WCF stack so it can be wrapped inside the service host.
            if (service != null) return service;

            // Extract the service from the container, the service should be configured as any other POCOs in the configuration of the application.
            var serviz = container.GetObject(serviceType);
            return serviz;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            // should we release the service???
        }
    }
}