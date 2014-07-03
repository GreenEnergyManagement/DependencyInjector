using System;
using System.Runtime.Serialization;

namespace Gem.DependencyInjector.Wcf
{
    public class WrappedServiceHostException : Exception
    {
        public WrappedServiceHostException()
        {
        }

        public WrappedServiceHostException(string message) : base(message)
        {
        }

        public WrappedServiceHostException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WrappedServiceHostException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}