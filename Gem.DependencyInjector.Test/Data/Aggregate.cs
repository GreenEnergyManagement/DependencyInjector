using System;

namespace Gem.DependencyInjector.Test
{
    public class Aggregate : IAggregate
    {
        private ISomeDomainCmdService someDomainCmdServiceA;
        private ISomeDomainCmdService someDomainCmdServiceB;

        public Aggregate(ISomeDomainCmdService someDomainCmdServiceA, ISomeDomainCmdService someDomainCmdServiceB)
        {
            this.someDomainCmdServiceA = someDomainCmdServiceA;
            this.someDomainCmdServiceB = someDomainCmdServiceB;
        }
    }

    /// <summary>
    /// Will be produced each time
    /// </summary>
    public class Prototype : IDisposable
    {
        private IAggregate aggregate;

        public Prototype(IAggregate aggregate)
        {
            this.aggregate = aggregate;
        }

        public void Dispose()
        {
            aggregate = null;
        }
    }

    public class PrototypeFactory : IDependencyInjectorAware, IObjectFactory
    {
        public DependencyInjector Container { get; set; }

        public PrototypeFactory()
        {
            //Container = container;
        }

        public object Create()
        {
            return new Prototype(Container.GetObject<IAggregate>());
        }
    }
}