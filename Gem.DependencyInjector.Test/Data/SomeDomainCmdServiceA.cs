namespace Gem.DependencyInjector.Test
{
    public class SomeDomainCmdServiceA : ISomeDomainCmdService
    {
        private ICommandManager commandManager;
        public SomeDomainCmdServiceA(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
        }
    }
}