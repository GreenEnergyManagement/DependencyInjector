namespace Gem.DependencyInjector.Test
{
    [Configuration]
    public class MoreConfig : IConfigureDependencies
    {
        [Definition(Id = "SomeDomainCmdServiceB")]
        public virtual ISomeDomainCmdService ConfigureSomeDomainCmdServiceB(ICommandManager commandManager)
        {
            ISomeDomainCmdService service = new SomeDomainCmdServiceB(commandManager);
            return service;
        }
    }
}