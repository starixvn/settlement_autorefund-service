using Fanex.Settlement.ScaleOut.Criteria.Normal;

namespace AutoEarlyRefund.Features.CheckEventAndTicket;

public class ValidateRefundTicketCriteria : ValidateTicketCriteria
{
    public int AutoType { get; set; } = 7;
}
