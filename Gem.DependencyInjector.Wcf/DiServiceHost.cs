using System;

namespace Gem.DependencyInjector.Wcf
{
    public class DiServiceHost : System.ServiceModel.ServiceHost
    {
        private DependencyInjector container;
        private object service;

        public DiServiceHost(DependencyInjector container, Type serviceType, params Uri[] baseAddresses): base(serviceType, baseAddresses)
        {
            this.container = container;
        }

        public DiServiceHost(object service, params Uri[] baseAddresses): base(service.GetType(), baseAddresses)
        {
            this.service = service;
        }

        protected override void OnOpening()
        {
            if(service != null) Description.Behaviors.Add(new DiServiceBehavior(service));
            if(container != null) Description.Behaviors.Add(new DiServiceBehavior(container));

            base.OnOpening();
        }
    }
}