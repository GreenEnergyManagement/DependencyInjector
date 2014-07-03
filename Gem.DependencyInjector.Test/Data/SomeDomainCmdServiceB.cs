namespace Gem.DependencyInjector.Test
{
    public class SomeDomainCmdServiceB : ISomeDomainCmdService
    {
        private ICommandManager commandManager;
        public SomeDomainCmdServiceB(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
        }
    }
}