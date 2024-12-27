using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.CheckEventAndTicket;

public class CheckEventAndTicketHandler
{
    private readonly ScaleOutSettleService scaleOutSettleService;
    public CheckEventAndTicketHandler(ScaleOutSettleService scaleOutSettleService)
    {
        this.scaleOutSettleService = scaleOutSettleService;
    }
    public async Task<bool> HandleAsync(Match match, CancellationToken cancellationToken)
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

        var result = await scaleOutSettleService.ValidateEventAndTicket(request);

        return result.ErrorCode == 0;
    }
}
