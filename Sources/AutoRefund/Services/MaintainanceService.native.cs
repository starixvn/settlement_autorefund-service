using ONEbook.UM;
using ONEbook.UM.Services;

namespace AutoRefund.Services
{
    public interface MaintainanceService
    {
        bool IsMaintainance { get; }
    }

    public class MaintainanceServiceImp : MaintainanceService
    {
        public bool IsMaintainance
        {
            get
            {
                var maintenanceService = new MaintenanceService(
                    UMClientManager.RestClient,
                    UMClientManager.CacheService,
                    UMClientManager.UMClientConfigs);

                var downTime = maintenanceService.GetUnderMaintenanceTime();

                return downTime.IsUnderMaintenanceTime;
            }
        }
    }
}