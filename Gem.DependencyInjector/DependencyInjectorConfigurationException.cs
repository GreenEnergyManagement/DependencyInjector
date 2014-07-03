using System;
using System.Runtime.Serialization;

namespace Gem.DependencyInjector
{
    public class DependencyInjectorConfigurationException : Exception
    {
        public DependencyInjectorConfigurationException()
        {
        }

        public DependencyInjectorConfigurationException(string message) : base(message)
        {
        }

        public DependencyInjectorConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DependencyInjectorConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DependencyInjectorGetObjectException : Exception
    {
        public string[] ObjectDefinitionIds { get; private set; }

        public DependencyInjectorGetObjectException() {}

        public DependencyInjectorGetObjectException(string message, string[] objectDefinitiosnIds) : base(message)
        {
            ObjectDefinitionIds = objectDefinitiosnIds;
        }

        public DependencyInjectorGetObjectException(string message) : base(message) {}

        public DependencyInjectorGetObjectException(string message, Exception innerException) : base(message, innerException) {}

        protected DependencyInjectorGetObjectException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}