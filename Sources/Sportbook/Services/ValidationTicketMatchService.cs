using AutoRefund.Criteria;
using AutoRefund.Exceptions;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Services
{
    public interface IValidationTicketMatchService
    {
        void VerifyTicketMatch(MatchChecking matchChecking);

        void CheckMarkedTicket(Match match);
    }

    public class ValidationTicketServiceImp : IValidationTicketMatchService
    {
        private readonly IDynamicRepository repository;

        public ValidationTicketServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

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

        public void CheckMarkedTicket(Match match)
        {
            var criteria = new CheckingMarkedTicketsCriteria();
            criteria.MatchId = match.MatchId;

            repository.Execute(criteria);

            if(criteria.IsExistMarkedTicket)
            {
                throw new ExistMarkedTicketException() { StopLoop = true };
            }
        }
    }
}