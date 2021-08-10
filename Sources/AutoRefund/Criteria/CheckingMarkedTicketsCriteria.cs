using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class CheckingMarkedTicketsCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Action { get; } = "MP-REFUND";

        public bool IsExistMarkedTicket { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Set_CheckingMarkedTransaction";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}
