namespace Gem.DependencyInjector.Test
{
    public class SomeDomainCmdServiceC : ISomeDomainCmdService
    {
        public ICommandManager commandManager;
        public SomeDomainCmdServiceC(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
        }

        /*public virtual void Execute()
        {

        }*/
    }
}