using ONEbook.UM.Services;

namespace AutoRefund.Services
{
    public interface MaintainanceService
    {
        bool IsMaintainance { get; }
    }

    public class MaintainanceServiceImp : MaintainanceService
    {
        private readonly IMaintenanceService maintenanceService;

        public MaintainanceServiceImp(IMaintenanceService maintenanceService)
        {
            this.maintenanceService = maintenanceService;
        }

        public bool IsMaintainance
        {
            get
            {
                var downTime = maintenanceService.GetUnderMaintenanceTime();

                return downTime.IsUnderMaintenanceTime;
            }
        }
    }
}