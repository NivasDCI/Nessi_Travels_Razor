using System;
using System.Collections.Generic;
using Transport.Model;

namespace Transport.Repository
{
    public interface IWalletRepository
    {
        // Driver Wallet Methods
        decimal GetDriverWalletBalance(int driverUserID);
        bool AddJobPaymentToWallet(int driverUserID, decimal amount, long jobCode, int createdBy);
        bool AddDriverExpense(int driverUserID, decimal amount, string category, string remarks, long? jobCode, int createdBy);
        bool HandoverCashToAdmin(int driverUserID, decimal amount, int handedToUserID, string handedToName, string remarks, int createdBy);

        // Admin/Company Wallet Methods
        decimal GetCompanyWalletBalance();
        bool AddCompanyExpense(string category, decimal amount, string paidTo, string remarks, int createdBy);

        // Transaction Methods
        List<TransactionHistoryViewModel> GetWalletTransactions(int? userID = null, string userType = null, DateTime? fromDate = null, DateTime? toDate = null);
        List<DriverWalletViewModel> GetAllDriverWallets();
        DriverWalletDetailViewModel GetDriverWalletDetail(int driverUserID);
        List<CashHandoverViewModel> GetCashHandovers(int? driverUserID = null, DateTime? fromDate = null, DateTime? toDate = null);
        List<DriverExpenseViewModel> GetDriverExpenses(int? driverUserID = null, DateTime? fromDate = null, DateTime? toDate = null);
        List<CompanyExpenseViewModel> GetCompanyExpenses(DateTime? fromDate = null, DateTime? toDate = null);

        // Dashboard
        WalletDashboardViewModel GetWalletDashboard();

        // Helper Methods
        List<DriverDropdownModel> GetDrivers();
        List<UserDropdownModel> GetAdmins();
    }
}