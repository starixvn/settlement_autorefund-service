using AutoRefund.Constants;
using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;
using System;
using System.Threading.Tasks;

namespace AutoRefund.Services
{
    public interface IRefundMatchService
    {
        Task<Result> Push(Match match);
    }

    public class RefundMatchServiceImp : IRefundMatchService
    {
        private readonly IDynamicRepository repository;

        public RefundMatchServiceImp(
            IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Result> Push(Match match)
        {
            var criteria = new RefundPushCriteria
            {
                MatchId = match.MatchId
            };
            await repository.ExecuteAsync(criteria);

            return new Result
            {
                ErrCode = criteria.ErrCode,
                ErrMsg = criteria.ErrMsg
            };
        }
    }
}