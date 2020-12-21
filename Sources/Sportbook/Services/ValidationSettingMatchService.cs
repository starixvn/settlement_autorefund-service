using AutoRefund.Exceptions;
using AutoRefund.Models;
using System.Linq;

namespace AutoRefund.Services
{
    public interface IValidationSettingMatchService
    {
        void VerifySettingMatch(MatchChecking matchChecking);
    }

    public class ValidationSettingMatchServiceImp : IValidationSettingMatchService
    {
        public void VerifySettingMatch(MatchChecking matchChecking)
        {
            if (matchChecking.IsSettingOff)
            {
                throw new SettingOffException();
            }

            RemarkSettingModel remark = matchChecking.Remark;

            bool[] conditionsToCheck = { remark.IsMtdHold, remark.IsUndecided, remark.IsPending, remark.IsUnderSettle, remark.IsHoldKey };

            if (!conditionsToCheck.All(item => !item))
            {
                throw new RemarkException();
            }
        }
    }
}