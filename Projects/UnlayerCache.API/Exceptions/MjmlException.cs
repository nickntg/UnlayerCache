using System;
using System.Runtime.Serialization;

namespace UnlayerCache.API.Exceptions
{
    [Serializable]
    internal class MjmlException : Exception
    {
        public MjmlException()
        {
        }

        public MjmlException(string message) : base(message)
        {
        }

        public MjmlException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MjmlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}