using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Services
{
    public interface ILogService
    {
        void ActionLog(Match match, string message);
    }

    public class LogServiceImp : ILogService
    {
        private readonly IDynamicRepository repository;

        public LogServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public void ActionLog(Match match, string message)
        {
            var criteria = new LogCriteria(match, message);
            repository.Execute(criteria);
        }
    }
}