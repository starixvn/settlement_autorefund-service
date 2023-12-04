namespace AutoRefund.Criteria;

using Models;

public class MatchCheckingCriteria : NonQueryCommand
{
    public MatchCheckingCriteria(Match match)
    {
        MatchId = match.MatchId;
        TypeId = match.TypeId;
    }

    public int MatchId { get; set; }

    public int TypeId { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "NAP_Set_AutoRefund_MatchInfo";
}