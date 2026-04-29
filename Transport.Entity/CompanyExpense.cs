using System;

namespace Transport.Entity
{
    public class CompanyExpense
    {
        public long CompanyExpenseID { get; set; }
        public string ExpenseCode { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string PaidTo { get; set; }
        public string Remarks { get; set; }
        public long? TransactionID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}