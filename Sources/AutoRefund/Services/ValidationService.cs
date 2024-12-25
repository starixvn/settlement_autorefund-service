namespace AutoRefund.Services;

using Models;

public interface IValidationService
{
    Task VerifyMatchBeforeExecution(Match match);
}

public class ValidationServiceImp(
    IValidationSettingMatchService validationSettingMatchService,
    IValidationTicketMatchService validationTicketMatchService,
    IValidationRelateMatchService validationRelateMatchService,
    IMatchService matchService)
    : IValidationService
{
    public async Task VerifyMatchBeforeExecution(Match match)
    {
        var matchChecking = await matchService.GetMatchInfo(match);
        validationRelateMatchService.VerifyRelateMatch(matchChecking);
        validationSettingMatchService.VerifySettingMatch(matchChecking);
        await validationTicketMatchService.VerifyTicketMatch(match);
    }
}