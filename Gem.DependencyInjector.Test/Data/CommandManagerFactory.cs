namespace Gem.DependencyInjector.Test
{
    public class CommandManagerFactory
    {
        private ICommandManager commandManager;
        public ICommandManager CreateCommandManager()
        {
            commandManager = new CommandManager();
            return commandManager;
        }
    }
}