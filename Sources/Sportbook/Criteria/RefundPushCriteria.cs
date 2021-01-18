namespace AutoRefund.Criteria
{
    public class RefundPushCriteria : PushCriteria
    {
        public int IsAutoRefund { get; } = 1;

        public override string GetSettingKey()
        {
            return "NAP_Admin_Settlement_Refund_All_Push";
        }
    }
}