using System;

namespace Gem.DependencyInjector
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConfigurationAttribute : Attribute { }
}