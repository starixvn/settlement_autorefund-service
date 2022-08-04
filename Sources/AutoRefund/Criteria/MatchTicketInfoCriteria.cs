using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    internal class MatchTicketInfoCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int TypeId { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override bool IsValid()
        {
            return MatchId > 0;
        }

        public override string GetSettingKey()
        {
            return $"NAP_Set_AutoRefund_TicketInfo_{DbServer}";
        }
    }
}