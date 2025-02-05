using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckEventAndTicket;

public class CheckEventAndTicketHandler : IValidateHandler
{
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly IValidateHandler next;


    public CheckEventAndTicketHandler(IValidateHandler next, ScaleOutSettleService scaleOutSettleService)
    {
        this.scaleOutSettleService = scaleOutSettleService;
        this.next = next;
    }

    public async Task<bool> HandleAsync(Match match)
    {
        var request = new EventTicketRequest
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            Mode = "EAR",
            SpName = "NAP_Admin_Settlement_Auto_1s_Push",
            IsAuto = true,
            SelectedResources = match.ResourcesXml
        };

        //var result = await scaleOutSettleService.ValidateEventAndTicket(request);

        //if (result.ErrorCode == 0 && next != null)
        {
            return await next.HandleAsync(match);
        }

        //return result.ErrorCode == 0;
    }
}
