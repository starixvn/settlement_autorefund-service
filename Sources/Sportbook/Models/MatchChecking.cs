using System;

namespace AutoRefund.Models
{
    public class MatchChecking
    {
        private const int ToBase = 2;

        public byte SettlementStatus { get; set; }

        public RemarkSettingModel Remark => new RemarkSettingModel
        {
            SettlementStatus = Convert.ToString(SettlementStatus, ToBase),
        };

        public bool IsSettingOff { get; set; }

        public bool IsExistMarkedTicket { get; set; }

        public bool IsExistSettledTicket { get; set; }

        public bool IsMainMatchSettled { get; set; }

        public bool IsHoldResourceId { get; set; }
    }
}