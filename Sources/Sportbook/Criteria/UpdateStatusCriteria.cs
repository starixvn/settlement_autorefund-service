using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class UpdateStatusCriteria : NonQueryCommand
    {
        public UpdateStatusCriteria(Match match, int statusId)
        {
            MatchId = match.MatchId;
            TypeId = match.TypeId;
            StatusId = statusId;
        }

        public UpdateStatusCriteria(Match match, string remark, int statusId)
        {
            MatchId = match.MatchId;
            TypeId = match.TypeId;
            Remark = remark;
            StatusId = statusId;
        }

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
            return MatchId > 0;
        }
    }
}
