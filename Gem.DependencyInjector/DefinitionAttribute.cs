using System;

namespace Gem.DependencyInjector
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DefinitionAttribute : Attribute
    {
        public string Id { get; set; }
        public Type ProduceType { get; set; }
    }
}