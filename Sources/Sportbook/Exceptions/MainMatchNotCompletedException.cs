using System;
using System.Runtime.Serialization;

namespace AutoRefund.Exceptions
{
    [Serializable]
    public class MainMatchNotCompletedException : AutoRefundException
    {
        public MainMatchNotCompletedException()
        {
            Description = "Auto refund child match failed: main match not completed.";
        }

        public MainMatchNotCompletedException(string message)
            : base(message)
        {
        }

        public MainMatchNotCompletedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MainMatchNotCompletedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
