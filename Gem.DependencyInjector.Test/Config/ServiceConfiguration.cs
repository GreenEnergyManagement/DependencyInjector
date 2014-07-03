namespace Gem.DependencyInjector.Test
{
    [Configuration]
    public class ServiceConfiguration : IConfigureDependencies
    {

        [Definition]
        public virtual ISomeDomainCmdService ConfigureSomeDomainCmdService(ICommandManager commandManager)
        {
            ISomeDomainCmdService service = new SomeDomainCmdServiceA(commandManager);
            return service;
        }

        [Definition]
        public virtual ICommandManager ConfigureCommandManager()
        {
            CommandManagerFactory factory = new CommandManagerFactory();
            ICommandManager manager = factory.CreateCommandManager();
            return manager;
        }     

    }
}