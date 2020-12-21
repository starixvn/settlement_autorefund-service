using System;
using System.Runtime.Serialization;

namespace AutoRefund.Exceptions
{
    [Serializable]
    public class RemarkException : AutoRefundException
    {
        public RemarkException()
        {
            Description = "Auto refund child match failed: match in on hold.";
        }

        public RemarkException(string message)
            : base(message)
        {
        }

        public RemarkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RemarkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}