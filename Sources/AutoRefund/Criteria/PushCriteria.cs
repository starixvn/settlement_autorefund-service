using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class PushCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int AdminId { get; } = 0;

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public override string GetSettingKey()
        {
            return "";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}
