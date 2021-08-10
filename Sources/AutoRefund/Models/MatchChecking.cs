using System;

namespace AutoRefund.Models
{
    public class MatchChecking
    {
        private const int ToBase = 2;

        private const int Calculated = 4;

        private const string Settled = "completed";

        public byte SettlementStatus { get; set; }

        public RemarkSettingModel Remark
        {
            get
            {
                char[] status = Convert.ToString(SettlementStatus, ToBase).ToCharArray();
                Array.Reverse(status);
                return new RemarkSettingModel
                {
                    SettlementStatus = new string(status),
                };
            }
        }

        public int ParentSettlementStatusID { get; set; }

        public int SettlementStatusID { get; set; }

        public string EventStatus { get; set; }

        public bool IsExistMarkedTicket { get; set; }

        public bool IsMainMatchSettled => ParentSettlementStatusID == Calculated;

        public bool IsSettled => EventStatus.Equals(Settled, StringComparison.OrdinalIgnoreCase);

        public bool IsRefundedSingle { get; set; }

        public bool IsExistSettledTicket { get; set; }

        public bool IsHoldResourceId { get; set; }

        public int ParentHomeValue { get; set; }

        public int ParentAwayValue { get; set; }
    }
}