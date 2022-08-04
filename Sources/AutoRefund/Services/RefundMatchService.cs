using System;
using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;
using System.Threading.Tasks;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

namespace AutoRefund.Services
{
    public interface IRefundMatchService
    {
        Task<Result> Push(Match match);
    }

    public class RefundMatchServiceImp : IRefundMatchService
    {
        private readonly IDynamicRepository repository;

        private readonly AppSettingsKey appSettingsKey;

        private readonly ScaleOutSettleService scaleOutSettleService;

        private const string Mode = "ALL";

        public RefundMatchServiceImp(
            IDynamicRepository repository,
            ScaleOutSettleService scaleOutSettleService,
            IOptions<AppSettingsKey> appSettingsKey)
        {
            this.appSettingsKey = appSettingsKey.Value;
            this.repository = repository;
            this.scaleOutSettleService = scaleOutSettleService;
        }

        public async Task<Result> Push(Match match)
        {
            var criteria = new RefundPushCriteria
            {
                MatchId = match.MatchId,
                BroadcastSessionId = Guid.NewGuid().ToString()
            };

            var matchChecking = await ValidateBeforeExecute(criteria);
            if (matchChecking.ErrCode > 0)
            {
                return matchChecking;
            }

            await UpdateMatchInfo(criteria);
            Execute(criteria);

            return new Result
            {
                ErrCode = criteria.ErrCode,
                ErrMsg = criteria.ErrMsg
            };
        }

        private void Execute(RefundPushCriteria criteria)
        {
            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                repository.Execute(criteria);
            }
        }

        private async Task<Result> ValidateBeforeExecute(RefundPushCriteria criteria)
        {
            var request = new EventTicketRequest
            {
                MatchId = criteria.MatchId,
                Action = criteria.Action,
                SpName = criteria.GetSettingKey(),
                IsAuto = true,
                Mode = Mode
            };
            var resultMessage = await scaleOutSettleService.ValidateEventAndTicket(request);
            return new Result
            {
                ErrCode = resultMessage.ErrorCode,
                ErrMsg = resultMessage.ErrorMessage
            };
        }

        private async Task UpdateMatchInfo(RefundPushCriteria criteria)
        {
            await scaleOutSettleService.UpdateUnderSettlementFlag(criteria.MatchId, criteria.AdminId, criteria.Action);
            var requestUpdateInfo = new UpdateMatchInfoRequest
            {
                MatchId = criteria.MatchId,
                Action = criteria.Action,
                AdminId = criteria.AdminId,
            };

            await scaleOutSettleService.UpdateMatchInfo(requestUpdateInfo);
        }
    }
}