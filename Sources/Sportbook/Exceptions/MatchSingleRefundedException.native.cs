using System;
using System.Runtime.Serialization;

namespace AutoRefund.Exceptions
{
    [Serializable]
    public class MatchSingleRefundedException : AutoRefundException
    {
        public MatchSingleRefundedException()
        {
            Description = "Auto refund parlay child match failed: match is not settled.";
        }

        public MatchSingleRefundedException(string message)
            : base(message)
        {
        }

        public MatchSingleRefundedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MatchSingleRefundedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
