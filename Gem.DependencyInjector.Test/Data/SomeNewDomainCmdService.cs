namespace Gem.DependencyInjector.Test
{
    public class SomeNewDomainCmdService : ISomeNewDomainCmdService
    {
        public ICommandManager commandManager;
        public SomeNewDomainCmdService(ICommandManager commandManager)
        {
            this.commandManager = commandManager;
        }

        /*public virtual void Execute()
        {

        }*/
    }
}