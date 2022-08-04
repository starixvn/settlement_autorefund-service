using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class RefundPushCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int AdminId { get; }

        public string Action { get; } = "REFUND";

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public int IsAutoRefund { get; } = 1;

        public string BroadcastSessionId { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override string GetSettingKey()
        {
            return $"NAP_Admin_Settlement_Refund_1_Push_{DbServer}";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}