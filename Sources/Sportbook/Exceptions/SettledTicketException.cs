using System;
using System.Runtime.Serialization;

namespace AutoRefund.Exceptions
{
    [Serializable]
    public class SettledTicketException : AutoRefundException
    {
        public SettledTicketException()
        {
            Description = "Auto refund child match failed: settled tickets exist.";
        }

        public SettledTicketException(string message)
            : base(message)
        {
        }

        public SettledTicketException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SettledTicketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}