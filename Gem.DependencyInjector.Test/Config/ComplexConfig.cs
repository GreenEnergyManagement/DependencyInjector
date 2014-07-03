namespace Gem.DependencyInjector.Test
{
    [Configuration]
    public class ComplexConfig : IConfigureDependencies
    {
        [Definition]
        public virtual IAggregate ConfigureSomeDomainCmdServiceB(SomeDomainCmdServiceA a, SomeDomainCmdServiceB b)
        {
            return new Aggregate(a, b);
        }

        [Definition(ProduceType = typeof(Prototype))]
        public virtual PrototypeFactory ConfigurePrototype()
        {
            return new PrototypeFactory();
        }
    }
}