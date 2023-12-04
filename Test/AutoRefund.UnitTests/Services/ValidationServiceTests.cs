using AutoRefund.Models;
using AutoRefund.Services;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class ValidationServiceTests
{
    private readonly IValidationSettingMatchService validationSettingMatchService;

    private readonly IValidationTicketMatchService validationTicketMatchService;

    private readonly IValidationRelateMatchService validationRelateMatchService;

    private readonly IValidationService service;

    public ValidationServiceTests()
    {
        var matchService = Substitute.For<IMatchService>();
        validationSettingMatchService = Substitute.For<IValidationSettingMatchService>();
        validationTicketMatchService = Substitute.For<IValidationTicketMatchService>();
        validationRelateMatchService = Substitute.For<IValidationRelateMatchService>();
        service = new ValidationServiceImp(validationSettingMatchService, validationTicketMatchService, validationRelateMatchService, matchService);
    }

    [Fact]
    public async Task VerifyMatchBeforeExecution_MatchIsRefundSingle_InvokeThreeMethods()
    {
        // Arrange
        var match = new Match();

        // Act
        await service.VerifyMatchBeforeExecution(match);

        // Assert
        validationRelateMatchService.Received().VerifyRelateMatch(Arg.Any<MatchChecking>());
        validationSettingMatchService.Received().VerifySettingMatch(Arg.Any<MatchChecking>());
        _ = validationTicketMatchService.Received().VerifyTicketMatch(Arg.Any<Match>());
    }
}