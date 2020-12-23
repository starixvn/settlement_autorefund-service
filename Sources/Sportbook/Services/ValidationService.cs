using AutoRefund.Constants;
using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;
using System.Threading.Tasks;

namespace AutoRefund.Services
{
    public interface IValidationService
    {
        Result VerifyMatchBeforeExecution(Match match);
    }

    public class ValidationServiceImp : IValidationService
    {
        private readonly IDynamicRepository repository;


        public ValidationServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public Result VerifyMatchBeforeExecution(Match match)
        {
            if(match.TypeId == MatchType.AutoRefund)
            {
                var criteria = new MatchCheckingCriteria();
                repository.Execute(criteria);
                return new Result
                {
                    ErrCode = criteria.ErrCode,
                    ErrMsg = criteria.ErrMsg
                };
            }
                
           return new Result();
        }
    }
}