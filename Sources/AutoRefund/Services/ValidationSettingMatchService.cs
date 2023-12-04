namespace AutoRefund.Services;

using Exceptions;
using Models;

public interface IValidationSettingMatchService
{
    void VerifySettingMatch(MatchChecking matchChecking);
}

public class ValidationSettingMatchServiceImp : IValidationSettingMatchService
{
    public void VerifySettingMatch(MatchChecking matchChecking)
    {
        var remark = matchChecking.Remark;
        bool[] conditionsToCheck = { remark.IsMtdHold, remark.IsUndecided, remark.IsPending, remark.IsUnderSettle, remark.IsHoldKey };

        if (!remark.IsRefundAll)
        {
            throw new RefundAllException();
        }

        if (Array.Exists(conditionsToCheck, item => item) || matchChecking.IsHoldResourceId)
        {
            throw new RemarkException();
        }
    }
}