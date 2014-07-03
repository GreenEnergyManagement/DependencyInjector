using System;

namespace Gem.DependencyInjector.Test
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MyCustomAttribute : Attribute { }
}