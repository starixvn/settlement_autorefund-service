namespace AutoRefund.Criteria
{
    public class RefundMPPushCriteria : PushCriteria
    {
        public string Action { get; } = "REFUND";

        public override string GetSettingKey()
        {
            return "NAP_Admin_Settlement_Refund_MP_Push";
        }
    }
}
