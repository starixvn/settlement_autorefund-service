namespace AutoRefund.Criteria
{
    public class RefundMPPushCriteria : RefundPushCriteria
    {
        public override string GetSettingKey()
        {
            return "NAP_Admin_Settlement_Refund_MP_Push";
        }
    }
}
