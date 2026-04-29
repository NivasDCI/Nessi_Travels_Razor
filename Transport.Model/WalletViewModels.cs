using System;
using System.Collections.Generic;

namespace Transport.Model
{
    public class DriverWalletViewModel
    {
        public int UserID { get; set; }
        public string DriverName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalHandedOver { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class AdminWalletViewModel
    {
        public decimal CurrentBalance { get; set; }
        public decimal TotalCashReceived { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetBalance { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CashHandoverViewModel
    {
        public long? HandoverID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public decimal HandoverAmount { get; set; }
        public int HandedToUserID { get; set; }
        public string HandedToName { get; set; }
        public DateTime HandoverDate { get; set; }
        public string Remarks { get; set; }
        public List<DriverDropdownModel> Drivers { get; set; }
        public List<UserDropdownModel> Admins { get; set; }
    }

    public class DriverExpenseViewModel
    {
        public long? ExpenseID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public long? JobCode { get; set; }
        public List<DriverDropdownModel> Drivers { get; set; }
        public List<ExpenseCategoryModel> Categories { get; set; }
    }

    public class CompanyExpenseViewModel
    {
        public long? CompanyExpenseID { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string PaidTo { get; set; }
        public string Remarks { get; set; }
        public List<ExpenseCategoryModel> Categories { get; set; }
    }

    public class TransactionHistoryViewModel
    {
        public long TransactionID { get; set; }
        public string TransactionCode { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string TransactionType { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CreatedByName { get; set; }
    }

    public class DriverWalletDetailViewModel
    {
        public DriverWalletViewModel DriverInfo { get; set; }
        public List<TransactionHistoryViewModel> Transactions { get; set; }
        public List<CashHandoverViewModel> Handovers { get; set; }
        public List<DriverExpenseViewModel> Expenses { get; set; }
        public decimal TotalJobEarnings { get; set; }
        public decimal TotalHandovers { get; set; }
        public decimal TotalDriverExpenses { get; set; }
    }

    public class DriverDropdownModel
    {
        public int UserID { get; set; }
        public string DriverName { get; set; }
    }

    public class UserDropdownModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }

    public class ExpenseCategoryModel
    {
        public string CategoryValue { get; set; }
        public string CategoryText { get; set; }
    }

    public class WalletDashboardViewModel
    {
        public AdminWalletViewModel AdminWallet { get; set; }
        public List<DriverWalletViewModel> DriverWallets { get; set; }
        public decimal TotalDriverBalance { get; set; }
        public decimal TotalCompanyCash { get; set; }
        public decimal TotalSystemCash { get; set; }
        public List<TransactionHistoryViewModel> RecentTransactions { get; set; }
    }
}