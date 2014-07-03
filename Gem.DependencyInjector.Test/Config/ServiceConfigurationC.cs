namespace Gem.DependencyInjector.Test
{
    [Configuration]
    public class ServiceConfigurationC : IConfigureDependencies
    {

        [Definition(Id = "C")]
        public virtual ISomeDomainCmdService ConfigureSomeDomainCmdServiceC(ICommandManager commandManager)
        {
            return new SomeDomainCmdServiceC(commandManager);
        }

        [Definition(Id = "D")]
        public virtual ISomeDomainCmdService ConfigureSomeDomainCmdServiceD(ICommandManager commandManager)
        {
            return new SomeDomainCmdServiceD(commandManager);
        }

        [Definition(Id = "DE")]
        public virtual ISomeDomainCmdService ConfigureSomeDomainCmdServiceDE(ICommandManager commandManager)
        {
            return new SomeDomainCmdServiceD(commandManager);
        }

        [Definition(Id = "NEW")]
        public virtual ISomeNewDomainCmdService ConfigureSomeNewDomainCmdService(ICommandManager commandManager)
        {
            return new SomeNewDomainCmdService(commandManager);
        }

        /*[Definition]
        public virtual Composit GetComposit(SomeDomainCmdServiceD service)
        {
            return new Composit(service);
        }*/
    }

    public class Composit
    {
        public SomeDomainCmdServiceD Service { get; set; }

        public Composit(SomeDomainCmdServiceD service)
        {
            Service = service;
        }
    }
}