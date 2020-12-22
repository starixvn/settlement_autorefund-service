using AutoRefund.Constants;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class UpdateStatusCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int RefundSetatus { get; } = MatchStatus.Failed;

        public int SourceId { get; } = 250;

        public override string GetSettingKey()
        {
            return "NAP_Set_AutoRefundMatch_Ins";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}
