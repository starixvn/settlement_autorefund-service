namespace AutoRefund.Services;

using Models;

public interface IValidationService
{
    Task VerifyMatchBeforeExecution(Match match);
}

public class ValidationServiceImp : IValidationService
{
    private readonly IValidationSettingMatchService validationSettingMatchService;

    private readonly IValidationTicketMatchService validationTicketMatchService;

    private readonly IValidationRelateMatchService validationRelateMatchService;

    private readonly IMatchService matchService;

    public ValidationServiceImp(
        IValidationSettingMatchService validationSettingMatchService,
        IValidationTicketMatchService validationTicketMatchService,
        IValidationRelateMatchService validationRelateMatchService,
        IMatchService matchService)
    {
        this.validationSettingMatchService = validationSettingMatchService;
        this.validationTicketMatchService = validationTicketMatchService;
        this.validationRelateMatchService = validationRelateMatchService;
        this.matchService = matchService;
    }

    public async Task VerifyMatchBeforeExecution(Match match)
    {
        var matchChecking = await matchService.GetMatchInfo(match);
        validationRelateMatchService.VerifyRelateMatch(matchChecking);
        validationSettingMatchService.VerifySettingMatch(matchChecking);
        await validationTicketMatchService.VerifyTicketMatch(match);
    }
}