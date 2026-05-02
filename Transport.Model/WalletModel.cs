//using System;

//namespace Transport.Model
//{
//    // ─── DRIVER WALLET ────────────────────────────────────────────────────────

//    // One row = one calendar day in DriverWallet table
//    public class DriverWalletDayModel
//    {
//        public long WalletID { get; set; }
//        public int DriverUserID { get; set; }
//        public string DriverName { get; set; }
//        public DateTime WalletDate { get; set; }
//        public string DisplayWalletDate { get; set; }
//        public int TotalRides { get; set; }
//        public decimal TotalEarned { get; set; }
//        public decimal TotalHandedOver { get; set; }
//        public decimal TotalExpenses { get; set; }
//        public decimal ClosingBalance { get; set; }
//        public decimal RunningBalance { get; set; }
//    }

//    // Summary totals for the 4 balance cards
//    public class DriverWalletBalanceModel
//    {
//        public decimal TotalEarned { get; set; }
//        public decimal TotalHandedOver { get; set; }
//        public decimal TotalExpenses { get; set; }
//        public decimal WalletBalance { get; set; }
//    }

//    // Cash collection history (completed cash rides) - driver's MyWallet
//    public class DriverCashHistoryModel
//    {
//        public long JobCode { get; set; }
//        public DateTime? JobDate { get; set; }
//        public string DisplayJobDate { get; set; }
//        public string JobTime { get; set; }
//        public string JobFrom { get; set; }
//        public string JobTo { get; set; }
//        public string CustomerName { get; set; }
//        public decimal Cash { get; set; }
//        public decimal Credit { get; set; }
//        public string JobStatus { get; set; }
//        public string VehicleName { get; set; }
//        public string JobGivenBy { get; set; }
//    }

//    // ─── HANDOVER ─────────────────────────────────────────────────────────────

//    public class DriverHandoverModel
//    {
//        public long HandoverID { get; set; }
//        public int DriverUserID { get; set; }
//        public string DriverName { get; set; }
//        public DateTime HandoverDate { get; set; }
//        public string DisplayHandoverDate { get; set; }
//        public decimal Amount { get; set; }
//        public int? HandedToUserID { get; set; }
//        public string HandedToName { get; set; }
//        public string Remarks { get; set; }
//        public int? CreatedBy { get; set; }
//        public DateTime CreatedDate { get; set; }
//        public string DisplayCreatedDate { get; set; }
//    }

//    // ─── DRIVER EXPENSE ───────────────────────────────────────────────────────

//    public class DriverExpenseModel
//    {
//        public long DriverExpenseID { get; set; }
//        public int DriverUserID { get; set; }
//        public string DriverName { get; set; }
//        public DateTime ExpenseDate { get; set; }
//        public string DisplayExpenseDate { get; set; }
//        public string Category { get; set; }
//        public decimal Amount { get; set; }
//        public string Remarks { get; set; }
//        public long? JobCode { get; set; }
//        public int? CreatedBy { get; set; }
//        public DateTime CreatedDate { get; set; }
//        public string DisplayCreatedDate { get; set; }
//    }

//    // ─── ADMIN WALLET ─────────────────────────────────────────────────────────

//    // Each row = one handover received by the admin
//    public class AdminWalletRowModel
//    {
//        public long HandoverID { get; set; }
//        public int DriverUserID { get; set; }
//        public string DriverName { get; set; }
//        public DateTime HandoverDate { get; set; }
//        public string DisplayHandoverDate { get; set; }
//        public decimal Amount { get; set; }
//        public string Remarks { get; set; }
//        public DateTime CreatedDate { get; set; }
//        public string DisplayCreatedDate { get; set; }
//    }

//    // Admin wallet balance card
//    public class AdminWalletBalanceModel
//    {
//        public decimal TotalReceived { get; set; }
//    }

//    // ─── COMPANY WALLET ───────────────────────────────────────────────────────

//    // Company-level expense record
//    public class CompanyExpenseModel
//    {
//        public long CompanyExpenseID { get; set; }
//        public DateTime ExpenseDate { get; set; }
//        public string DisplayExpenseDate { get; set; }
//        public string Category { get; set; }
//        public decimal Amount { get; set; }
//        public string Remarks { get; set; }
//        public int? DriverUserID { get; set; }
//        public string DriverName { get; set; }
//        public int? CreatedBy { get; set; }
//        public string CreatedByName { get; set; }
//        public DateTime CreatedDate { get; set; }
//        public string DisplayCreatedDate { get; set; }
//    }

//    // Company wallet overall summary (top cards)
//    public class CompanyWalletSummaryModel
//    {
//        public decimal TotalDriverEarnings { get; set; }
//        public decimal TotalHandedOver { get; set; }
//        public decimal TotalDriverExpenses { get; set; }
//        public decimal TotalCompanyExpenses { get; set; }
//        public decimal WithDrivers { get; set; }
//        public decimal CompanyBalance { get; set; }
//    }

//    // Per-driver row in company wallet breakdown grid
//    public class CompanyWalletDriverRowModel
//    {
//        public int DriverUserID { get; set; }
//        public string DriverName { get; set; }
//        public decimal TotalEarned { get; set; }
//        public decimal TotalHandedOver { get; set; }
//        public decimal TotalExpenses { get; set; }
//        public decimal Balance { get; set; }
//    }
//}

using System;

namespace Transport.Model
{
    // ─── DRIVER WALLET ────────────────────────────────────────────────────────

    // One row = one calendar day in DriverWallet table
    public class DriverWalletDayModel
    {
        public long WalletID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime WalletDate { get; set; }
        public string DisplayWalletDate { get; set; }
        public int TotalRides { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalHandedOver { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal RunningBalance { get; set; }
    }

    // Summary totals for the 4 balance cards
    public class DriverWalletBalanceModel
    {
        public decimal TotalEarned { get; set; }
        public decimal TotalHandedOver { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal WalletBalance { get; set; }
    }

    // Cash collection history (completed cash rides) - driver's MyWallet
    public class DriverCashHistoryModel
    {
        public long JobCode { get; set; }
        public DateTime? JobDate { get; set; }
        public string DisplayJobDate { get; set; }
        public string JobTime { get; set; }
        public string JobFrom { get; set; }
        public string JobTo { get; set; }
        public string CustomerName { get; set; }
        public decimal Cash { get; set; }
        public decimal Credit { get; set; }
        public string JobStatus { get; set; }
        public string VehicleName { get; set; }
        public string JobGivenBy { get; set; }
    }

    // ─── HANDOVER ─────────────────────────────────────────────────────────────

    public class DriverHandoverModel
    {
        public long HandoverID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime HandoverDate { get; set; }
        public string DisplayHandoverDate { get; set; }
        public decimal Amount { get; set; }
        public int? HandedToUserID { get; set; }
        public string HandedToName { get; set; }
        public string Remarks { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayCreatedDate { get; set; }
    }

    // ─── DRIVER EXPENSE ───────────────────────────────────────────────────────

    public class DriverExpenseModel
    {
        public long DriverExpenseID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string DisplayExpenseDate { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public long? JobCode { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayCreatedDate { get; set; }
    }

    // ─── ADMIN WALLET ─────────────────────────────────────────────────────────

    // Each row = one handover received by the admin
    public class AdminWalletRowModel
    {
        public long HandoverID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime HandoverDate { get; set; }
        public string DisplayHandoverDate { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayCreatedDate { get; set; }
    }

    // Admin wallet balance card
    public class AdminWalletBalanceModel
    {
        public decimal TotalReceived { get; set; }
    }

    // ─── COMPANY WALLET ───────────────────────────────────────────────────────

    // Company-level expense record
    public class CompanyExpenseModel
    {
        public long CompanyExpenseID { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string DisplayExpenseDate { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public int? DriverUserID { get; set; }
        public string DriverName { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayCreatedDate { get; set; }
    }

    // Company wallet overall summary (top cards)
    public class CompanyWalletSummaryModel
    {
        public decimal TotalDriverEarnings { get; set; }
        public decimal TotalHandedOver { get; set; }
        public decimal TotalDriverExpenses { get; set; }
        public decimal TotalCompanyExpenses { get; set; }
        public decimal WithDrivers { get; set; }
        public decimal CompanyBalance { get; set; }
    }

    // Per-driver row in company wallet breakdown grid
    public class CompanyWalletDriverRowModel
    {
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalHandedOver { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
    }

    // ─── WALLET TRANSACTION HISTORY (UserWallet ledger) ─────────────────────────

    // One row = one credit/debit transaction in UserWallet
    public class WalletTransactionModel
    {
        public long TransactionID { get; set; }
        public int UserID { get; set; }
        public string TxType { get; set; }      // "Credit" or "Debit"
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Source { get; set; }       // "Job", "Handover", "Expense", "Topup", "Payment", etc.
        public long? SourceID { get; set; }
        public string Remarks { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DisplayDate { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
}