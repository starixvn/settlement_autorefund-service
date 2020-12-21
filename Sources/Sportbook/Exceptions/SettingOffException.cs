using System;
using System.Runtime.Serialization;

namespace AutoRefund.Exceptions
{
    [Serializable]
    public class SettingOffException : AutoRefundException
    {
        public SettingOffException()
        {
            Description = "Failed: Cannot execute Auto Refund calculate due to setting turned off.";
        }

        public SettingOffException(string message)
            : base(message)
        {
        }

        public SettingOffException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SettingOffException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}