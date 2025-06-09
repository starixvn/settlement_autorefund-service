using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.TriggerConditions;


public interface ITriggerConditionRule
{
    bool IsValid(int homeScore, int awayScore, TargetResourceTrigger trigger);
}

public class DefaultCondition : ITriggerConditionRule
{
    public bool IsValid(int homeScore, int awayScore, TargetResourceTrigger trigger) => true;
}

public class WinLoseCondition : ITriggerConditionRule
{
    public bool IsValid(int homeScore, int awayScore, TargetResourceTrigger trigger) => homeScore != awayScore;
}



