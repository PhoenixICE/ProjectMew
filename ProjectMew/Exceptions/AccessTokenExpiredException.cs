using System;
using System.Runtime.Serialization;

namespace ProjectMew.Exceptions
{
    [Serializable]
    internal class AccessTokenExpiredException : Exception
    {
        private object error;

        public AccessTokenExpiredException()
        {
        }

        public AccessTokenExpiredException(string message) : base(message)
        {
        }

        public AccessTokenExpiredException(object error)
        {
            this.error = error;
        }

        public AccessTokenExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AccessTokenExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}