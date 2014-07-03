namespace Gem.DependencyInjector.Test
{
    [Configuration]
    public class ServiceConfigurationNotConfigurable : IConfigureDependencies
    {
        [Definition(Id = "NotConfigurable")]
        public virtual ISomeDomainCmdService ConfigureSomeDomainCmdServiceC(ICommandManager commandManager, INotImplementedInterface noInstanceCanBeCreated)
        {
            return new SomeDomainCmdServiceC(commandManager);
        }
    }
}