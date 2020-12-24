using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;
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
                var criteria = new MatchCheckingCriteria();
                criteria.MatchId = match.MatchId;
                criteria.TypeId = match.TypeId;
                repository.Execute(criteria);
                return new Result
                {
                    ErrCode = criteria.ErrCode,
                    ErrMsg = criteria.ErrMsg
                };
            }
        }
    }