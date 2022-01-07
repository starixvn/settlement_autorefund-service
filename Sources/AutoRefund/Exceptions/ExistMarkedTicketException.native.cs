using System;
using System.Runtime.Serialization;

namespace AutoRefund.Exceptions
{
    [Serializable]
    public class ExistMarkedTicketException : AutoRefundException
    {
        public ExistMarkedTicketException()
        {
            Description = "ERROR! Cannot proceed due to marked tickets. Please unmark them and try again.";
        }

        public ExistMarkedTicketException(bool stopLoop) : this()
        {
            StopLoop = stopLoop;
        }

        public ExistMarkedTicketException(string message)
            : base(message)
        {
        }

        public ExistMarkedTicketException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ExistMarkedTicketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

       
    }
}
