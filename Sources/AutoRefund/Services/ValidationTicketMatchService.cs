using AutoRefund.Exceptions;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Services
{
    public interface IValidationTicketMatchService
    {
        void VerifyTicketMatch(MatchChecking matchChecking);
    }

    public class ValidationTicketServiceImp : IValidationTicketMatchService
    {
        public void VerifyTicketMatch(MatchChecking matchChecking)
        {
            if (matchChecking.IsExistMarkedTicket)
            {
                throw new MarkedTicketException();
            }

            if (matchChecking.IsExistSettledTicket)
            {
                throw new SettledTicketException();
            }
        }
    }
}