namespace Gem.DependencyInjector.Test
{
    [MyCustom]
    public class SomeDomainCmdServiceD : SomeDomainCmdServiceC
    {
        public SomeDomainCmdServiceD(ICommandManager commandManager) :base(commandManager) { }

        /*public override void Execute()
        {

        }*/
    }
}