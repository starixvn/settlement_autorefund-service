using AutoRefund.Constants;
using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Services
{
    public interface IRefundMatchService
    {
        Result Push(Match match);
    }

    public class RefundMatchServiceImp : IRefundMatchService
    {
        private readonly IDynamicRepository repository;

        public RefundMatchServiceImp(
            IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public Result Push(Match match)
        {
            var criteria = match.TypeId == MatchType.AutoRefund ? new RefundPushCriteria() : new RefundMPPushCriteria();
            criteria.MatchId = match.MatchId;
            repository.Execute(criteria);
            return new Result
            {
                ErrCode = criteria.ErrCode,
                ErrMsg = criteria.ErrMsg
            };
        }
    }
}