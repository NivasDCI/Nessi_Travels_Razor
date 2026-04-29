using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Transport.Model;

namespace Transport.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly string _connectionString;

        public WalletRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public decimal GetDriverWalletBalance(int driverUserID)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM WalletTransactions WHERE UserID = @UserID AND UserType = 'Driver'", connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", driverUserID);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToDecimal(result);
                }
            }
        }

        public decimal GetCompanyWalletBalance()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM WalletTransactions WHERE UserType = 'Admin'", connection))
                {
                    var result = cmd.ExecuteScalar();
                    return Convert.ToDecimal(result);
                }
            }
        }

        public bool AddJobPaymentToWallet(int driverUserID, decimal amount, long jobCode, int createdBy)
        {
            // Simplified implementation for testing
            return true;
        }

        public bool AddDriverExpense(int driverUserID, decimal amount, string category, string remarks, long? jobCode, int createdBy)
        {
            return true;
        }

        public bool HandoverCashToAdmin(int driverUserID, decimal amount, int handedToUserID, string handedToName, string remarks, int createdBy)
        {
            return true;
        }

        public bool AddCompanyExpense(string category, decimal amount, string paidTo, string remarks, int createdBy)
        {
            return true;
        }

        public List<DriverWalletViewModel> GetAllDriverWallets()
        {
            return new List<DriverWalletViewModel>();
        }

        public DriverWalletDetailViewModel GetDriverWalletDetail(int driverUserID)
        {
            return new DriverWalletDetailViewModel();
        }

        public List<TransactionHistoryViewModel> GetWalletTransactions(int? userID = null, string userType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return new List<TransactionHistoryViewModel>();
        }

        public List<CashHandoverViewModel> GetCashHandovers(int? driverUserID = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return new List<CashHandoverViewModel>();
        }

        public List<DriverExpenseViewModel> GetDriverExpenses(int? driverUserID = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return new List<DriverExpenseViewModel>();
        }

        public List<CompanyExpenseViewModel> GetCompanyExpenses(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return new List<CompanyExpenseViewModel>();
        }

        public WalletDashboardViewModel GetWalletDashboard()
        {
            var result = new WalletDashboardViewModel();

            try
            {
                // Initialize collections
                result.DriverWallets = new List<DriverWalletViewModel>();
                result.RecentTransactions = new List<TransactionHistoryViewModel>();
                result.AdminWallet = new AdminWalletViewModel();

                using (var connection = GetConnection())
                {
                    // Get admin wallet info
                    result.AdminWallet.CurrentBalance = GetCompanyWalletBalance();
                    result.AdminWallet.TotalCashReceived = GetWalletTransactions(null, "Admin", null, null)
                        .Where(t => t.Category == "CashReceived").Sum(t => t.Amount);
                    result.AdminWallet.TotalExpenses = GetWalletTransactions(null, "Admin", null, null)
                        .Where(t => t.TransactionType == "Debit").Sum(t => t.Amount);
                    result.AdminWallet.NetBalance = result.AdminWallet.CurrentBalance;
                    result.AdminWallet.LastUpdated = DateTime.Now;

                    // Get driver wallets
                    result.DriverWallets = GetAllDriverWallets() ?? new List<DriverWalletViewModel>();
                    result.TotalDriverBalance = result.DriverWallets.Sum(d => d.CurrentBalance);
                    result.TotalCompanyCash = result.AdminWallet.CurrentBalance;
                    result.TotalSystemCash = result.TotalDriverBalance + result.TotalCompanyCash;
                    result.RecentTransactions = GetWalletTransactions(null, null, null, null).Take(20).ToList() ?? new List<TransactionHistoryViewModel>();
                }
            }
            catch (Exception ex)
            {
                // Log error if you have logging
                System.Diagnostics.Debug.WriteLine("Error in GetWalletDashboard: " + ex.Message);
            }

            return result;
        }

        public List<DriverDropdownModel> GetDrivers()
        {
            return new List<DriverDropdownModel>();
        }

        public List<UserDropdownModel> GetAdmins()
        {
            return new List<UserDropdownModel>();
        }
    }
}