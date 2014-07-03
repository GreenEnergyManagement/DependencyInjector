namespace Gem.DependencyInjector
{
    public interface IDependencyInjectorAware
    {
        DependencyInjector Container { get; set; }
    }
}