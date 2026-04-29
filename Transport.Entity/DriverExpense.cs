using System;

namespace Transport.Entity
{
    public class DriverExpense
    {
        public long ExpenseID { get; set; }
        public string ExpenseCode { get; set; }
        public int DriverUserID { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public long? JobCode { get; set; }
        public long? TransactionID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}