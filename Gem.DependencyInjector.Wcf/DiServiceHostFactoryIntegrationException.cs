using System;
using System.Runtime.Serialization;

namespace Gem.DependencyInjector.Wcf
{
    public class DiServiceHostFactoryIntegrationException : Exception
    {
        public DiServiceHostFactoryIntegrationException() { }

        public DiServiceHostFactoryIntegrationException(string message) : base(message) { }

        public DiServiceHostFactoryIntegrationException(string message, Exception innerException) : base(message, innerException) { }

        protected DiServiceHostFactoryIntegrationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}