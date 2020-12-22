using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class RefundPushCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int IsAutoRefund { get; } = 1;

        public int AdminId { get; } = 0;

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Admin_Settlement_Refund_All_Push";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}