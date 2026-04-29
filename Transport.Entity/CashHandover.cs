using System;

namespace Transport.Entity
{
    public class CashHandover
    {
        public long HandoverID { get; set; }
        public string HandoverCode { get; set; }
        public int DriverUserID { get; set; }
        public decimal HandoverAmount { get; set; }
        public int HandedToUserID { get; set; }
        public string HandedToName { get; set; }
        public DateTime HandoverDate { get; set; }
        public string Remarks { get; set; }
        public long? TransactionID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}