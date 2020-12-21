using System.Linq;
using System.Threading.Tasks;

namespace AutoRefund.Services
{
    public interface IAutoRefundController
    {
        void ProcessAsync();
    }

    public class AutoRefundControllerImp : IAutoRefundController
    {
        private readonly IMatchPoolService matchPoolService;
        private readonly IMatchRefundController matchRefundController;

        public AutoRefundControllerImp(
            IMatchPoolService matchPoolService,
            IMatchRefundController matchRefundController)
        {
            this.matchPoolService = matchPoolService;
            this.matchRefundController = matchRefundController;
        }

        public void ProcessAsync()
        {
            var newMatches = matchPoolService.PeekNewMatch().ToList();

            newMatches.ForEach(m =>
            {
                Task.Run(() => matchRefundController.Process(m))
                    .ConfigureAwait(false);
            });
        }
    }
}