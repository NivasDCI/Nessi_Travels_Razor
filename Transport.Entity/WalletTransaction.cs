using System;

namespace Transport.Entity
{
    public class WalletTransaction
    {
        public long TransactionID { get; set; }
        public string TransactionCode { get; set; }
        public int UserID { get; set; }
        public string UserType { get; set; }
        public string TransactionType { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public long? ReferenceID { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsReversed { get; set; }
        public long? ReversedTransactionID { get; set; }
    }
}