using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class UpdateStatusCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int StatusId { get; set; }

        public int TypeId { get; set; }

        public string Remark { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Set_AutoRefund_ResultUpd";
        }

        public override bool IsValid()
        {
            return MatchId > 0 || StatusId > 0;
        }
    }
}