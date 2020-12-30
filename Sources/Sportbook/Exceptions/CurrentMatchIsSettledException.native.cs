using System;
using System.Runtime.Serialization;

namespace AutoRefund.Exceptions
{
    [Serializable]
    public class CurrentMatchIsSettledException : AutoRefundException
    {
        public CurrentMatchIsSettledException()
        {
            Description = "Auto refund child match failed: match settled.";
        }

        public CurrentMatchIsSettledException(string message)
            : base(message)
        {
        }

        public CurrentMatchIsSettledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CurrentMatchIsSettledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

