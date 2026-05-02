using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Transport.Model;

namespace Transport.Repository
{
    // ─── INTERFACE ────────────────────────────────────────────────────────────
    public interface IWalletRepository
    {
        // Sync
        void SyncDriverWallet(int driverUserID, DateTime date);

        // Driver wallet
        List<DriverWalletDayModel> GetDriverWalletSummary(int driverUserID, DateTime? from, DateTime? to);
        DriverWalletBalanceModel GetDriverWalletBalance(int driverUserID);
        List<DriverCashHistoryModel> GetDriverCashHistory(int driverUserID, DateTime? from, DateTime? to);

        // Handover
        bool SaveHandover(DriverHandoverModel model);
        List<DriverHandoverModel> GetHandovers(int? driverUserID, DateTime? from, DateTime? to);
        bool DeleteHandover(long handoverID);

        // Driver expense
        bool SaveDriverExpense(DriverExpenseModel model);
        List<DriverExpenseModel> GetDriverExpenses(int? driverUserID, DateTime? from, DateTime? to);
        bool DeleteDriverExpense(long driverExpenseID);

        // Admin wallet
        List<AdminWalletRowModel> GetAdminWallet(int adminUserID, DateTime? from, DateTime? to);
        AdminWalletBalanceModel GetAdminWalletBalance(int adminUserID);

        // Company wallet
        CompanyWalletSummaryModel GetCompanyWalletSummary();
        List<CompanyWalletDriverRowModel> GetCompanyWalletDriverBreakdown();
        bool SaveCompanyExpense(CompanyExpenseModel model);
        List<CompanyExpenseModel> GetCompanyExpenses(DateTime? from, DateTime? to, string category);
        bool DeleteCompanyExpense(long companyExpenseID);
    }

    // ─── IMPLEMENTATION ───────────────────────────────────────────────────────
    public class WalletRepository : IWalletRepository
    {
        //private string Conn()
        //{
        //    string path = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
        //    string val = "";
        //    using (var sr = new StreamReader(path))
        //        while (sr.Peek() >= 0) val = sr.ReadLine();
        //    return val;
        //}

        private string Conn()
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
            string val = "";
            using (var sr = new StreamReader(path))
                while (sr.Peek() >= 0) val = sr.ReadLine();
            return val;
        }

        // ════════════════════════════════════════════════════════════════════
        // SYNC DRIVER WALLET
        // ════════════════════════════════════════════════════════════════════
        public void SyncDriverWallet(int driverUserID, DateTime date)
        {
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_sync_DriverWallet", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@UserID", driverUserID);
                cmd.Parameters.AddWithValue("@WalletDate", date.Date);
                cmd.ExecuteNonQuery();
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // DRIVER WALLET
        // ════════════════════════════════════════════════════════════════════
        public List<DriverWalletDayModel> GetDriverWalletSummary(int driverUserID, DateTime? from, DateTime? to)
        {
            var list = new List<DriverWalletDayModel>();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_DriverWallet_Summary", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID);
                cmd.Parameters.AddWithValue("@FromDate", from.HasValue ? (object)from.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", to.HasValue ? (object)to.Value.Date : DBNull.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new DriverWalletDayModel
                        {
                            WalletID = Convert.ToInt64(r["WalletID"]),
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"].ToString(),
                            WalletDate = Convert.ToDateTime(r["WalletDate"]),
                            DisplayWalletDate = r["DisplayWalletDate"].ToString(),
                            TotalRides = Convert.ToInt32(r["TotalRides"]),
                            TotalEarned = Convert.ToDecimal(r["TotalEarned"]),
                            TotalHandedOver = Convert.ToDecimal(r["TotalHandedOver"]),
                            TotalExpenses = Convert.ToDecimal(r["TotalExpenses"]),
                            ClosingBalance = Convert.ToDecimal(r["ClosingBalance"]),
                            RunningBalance = Convert.ToDecimal(r["RunningBalance"])
                        });
            }
            return list;
        }

        public DriverWalletBalanceModel GetDriverWalletBalance(int driverUserID)
        {
            var m = new DriverWalletBalanceModel();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_DriverWallet_Balance", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID);
                using (var r = cmd.ExecuteReader())
                    if (r.Read())
                    {
                        m.TotalEarned = Convert.ToDecimal(r["TotalEarned"]);
                        m.TotalHandedOver = Convert.ToDecimal(r["TotalHandedOver"]);
                        m.TotalExpenses = Convert.ToDecimal(r["TotalExpenses"]);
                        m.WalletBalance = Convert.ToDecimal(r["WalletBalance"]);
                    }
            }
            return m;
        }

        public List<DriverCashHistoryModel> GetDriverCashHistory(int driverUserID, DateTime? from, DateTime? to)
        {
            var list = new List<DriverCashHistoryModel>();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_DriverCashHistory", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID);
                cmd.Parameters.AddWithValue("@FromDate", from.HasValue ? (object)from.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", to.HasValue ? (object)to.Value.Date : DBNull.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new DriverCashHistoryModel
                        {
                            JobCode = Convert.ToInt64(r["JobCode"]),
                            JobDate = r["JobDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["JobDate"]),
                            DisplayJobDate = r["DisplayJobDate"].ToString(),
                            JobTime = r["JobTime"].ToString(),
                            JobFrom = r["JobFrom"].ToString(),
                            JobTo = r["JobTo"].ToString(),
                            CustomerName = r["CustomerName"].ToString(),
                            Cash = Convert.ToDecimal(r["Cash"]),
                            Credit = Convert.ToDecimal(r["Credit"]),
                            JobStatus = r["JobStatus"].ToString(),
                            VehicleName = r["VehicleName"].ToString(),
                            JobGivenBy = r["JobGivenBy"].ToString()
                        });
            }
            return list;
        }

        // ════════════════════════════════════════════════════════════════════
        // HANDOVER
        // ════════════════════════════════════════════════════════════════════
        public bool SaveHandover(DriverHandoverModel model)
        {
            try
            {
                using (var conn = new SqlConnection(Conn()))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO DriverHandover(DriverUserID,HandoverDate,Amount,HandedToUserID,HandedToName,Remarks,CreatedBy)" +
                        " VALUES(@DriverUserID,@HandoverDate,@Amount,@HandedToUserID,@HandedToName,@Remarks,@CreatedBy)", conn);
                    cmd.Parameters.AddWithValue("@DriverUserID", model.DriverUserID);
                    cmd.Parameters.AddWithValue("@HandoverDate", model.HandoverDate.Date);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@HandedToUserID", model.HandedToUserID.HasValue ? (object)model.HandedToUserID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@HandedToName", string.IsNullOrEmpty(model.HandedToName) ? (object)DBNull.Value : model.HandedToName);
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(model.Remarks) ? (object)DBNull.Value : model.Remarks);
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy.HasValue ? (object)model.CreatedBy.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                SyncDriverWallet(model.DriverUserID, model.HandoverDate.Date);
                return true;
            }
            catch { return false; }
        }

        public List<DriverHandoverModel> GetHandovers(int? driverUserID, DateTime? from, DateTime? to)
        {
            var list = new List<DriverHandoverModel>();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_DriverHandovers", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID.HasValue ? (object)driverUserID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@FromDate", from.HasValue ? (object)from.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", to.HasValue ? (object)to.Value.Date : DBNull.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new DriverHandoverModel
                        {
                            HandoverID = Convert.ToInt64(r["HandoverID"]),
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"].ToString(),
                            HandoverDate = Convert.ToDateTime(r["HandoverDate"]),
                            DisplayHandoverDate = r["DisplayHandoverDate"].ToString(),
                            Amount = Convert.ToDecimal(r["Amount"]),
                            HandedToUserID = r["HandedToUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["HandedToUserID"]),
                            HandedToName = r["HandedToName"].ToString(),
                            Remarks = r["Remarks"].ToString(),
                            CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                            DisplayCreatedDate = r["DisplayCreatedDate"].ToString()
                        });
            }
            return list;
        }

        public bool DeleteHandover(long handoverID)
        {
            try
            {
                int driverID = 0; DateTime dt = DateTime.Today;
                using (var conn = new SqlConnection(Conn()))
                {
                    conn.Open();
                    var read = new SqlCommand("SELECT DriverUserID,HandoverDate FROM DriverHandover WHERE HandoverID=@ID", conn);
                    read.Parameters.AddWithValue("@ID", handoverID);
                    using (var r = read.ExecuteReader())
                        if (r.Read()) { driverID = Convert.ToInt32(r["DriverUserID"]); dt = Convert.ToDateTime(r["HandoverDate"]); }
                    var del = new SqlCommand("DELETE FROM DriverHandover WHERE HandoverID=@ID", conn);
                    del.Parameters.AddWithValue("@ID", handoverID);
                    del.ExecuteNonQuery();
                }
                if (driverID > 0) SyncDriverWallet(driverID, dt);
                return true;
            }
            catch { return false; }
        }

        // ════════════════════════════════════════════════════════════════════
        // DRIVER EXPENSE
        // ════════════════════════════════════════════════════════════════════
        public bool SaveDriverExpense(DriverExpenseModel model)
        {
            try
            {
                using (var conn = new SqlConnection(Conn()))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO DriverExpense(DriverUserID,ExpenseDate,Category,Amount,Remarks,JobCode,CreatedBy)" +
                        " VALUES(@DriverUserID,@ExpenseDate,@Category,@Amount,@Remarks,@JobCode,@CreatedBy)", conn);
                    cmd.Parameters.AddWithValue("@DriverUserID", model.DriverUserID);
                    cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate.Date);
                    cmd.Parameters.AddWithValue("@Category", string.IsNullOrEmpty(model.Category) ? (object)DBNull.Value : model.Category);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(model.Remarks) ? (object)DBNull.Value : model.Remarks);
                    cmd.Parameters.AddWithValue("@JobCode", model.JobCode.HasValue ? (object)model.JobCode.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy.HasValue ? (object)model.CreatedBy.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                SyncDriverWallet(model.DriverUserID, model.ExpenseDate.Date);
                return true;
            }
            catch { return false; }
        }

        public List<DriverExpenseModel> GetDriverExpenses(int? driverUserID, DateTime? from, DateTime? to)
        {
            var list = new List<DriverExpenseModel>();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_DriverExpenses", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID.HasValue ? (object)driverUserID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@FromDate", from.HasValue ? (object)from.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", to.HasValue ? (object)to.Value.Date : DBNull.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new DriverExpenseModel
                        {
                            DriverExpenseID = Convert.ToInt64(r["DriverExpenseID"]),
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"].ToString(),
                            ExpenseDate = Convert.ToDateTime(r["ExpenseDate"]),
                            DisplayExpenseDate = r["DisplayExpenseDate"].ToString(),
                            Category = r["Category"].ToString(),
                            Amount = Convert.ToDecimal(r["Amount"]),
                            Remarks = r["Remarks"].ToString(),
                            JobCode = r["JobCode"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["JobCode"]),
                            CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                            DisplayCreatedDate = r["DisplayCreatedDate"].ToString()
                        });
            }
            return list;
        }

        public bool DeleteDriverExpense(long driverExpenseID)
        {
            try
            {
                int driverID = 0; DateTime dt = DateTime.Today;
                using (var conn = new SqlConnection(Conn()))
                {
                    conn.Open();
                    var read = new SqlCommand("SELECT DriverUserID,ExpenseDate FROM DriverExpense WHERE DriverExpenseID=@ID", conn);
                    read.Parameters.AddWithValue("@ID", driverExpenseID);
                    using (var r = read.ExecuteReader())
                        if (r.Read()) { driverID = Convert.ToInt32(r["DriverUserID"]); dt = Convert.ToDateTime(r["ExpenseDate"]); }
                    var del = new SqlCommand("DELETE FROM DriverExpense WHERE DriverExpenseID=@ID", conn);
                    del.Parameters.AddWithValue("@ID", driverExpenseID);
                    del.ExecuteNonQuery();
                }
                if (driverID > 0) SyncDriverWallet(driverID, dt);
                return true;
            }
            catch { return false; }
        }

        // ════════════════════════════════════════════════════════════════════
        // ADMIN WALLET
        // ════════════════════════════════════════════════════════════════════
        public List<AdminWalletRowModel> GetAdminWallet(int adminUserID, DateTime? from, DateTime? to)
        {
            var list = new List<AdminWalletRowModel>();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_AdminWallet_Summary", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@AdminUserID", adminUserID);
                cmd.Parameters.AddWithValue("@FromDate", from.HasValue ? (object)from.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", to.HasValue ? (object)to.Value.Date : DBNull.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new AdminWalletRowModel
                        {
                            HandoverID = Convert.ToInt64(r["HandoverID"]),
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"].ToString(),
                            HandoverDate = Convert.ToDateTime(r["HandoverDate"]),
                            DisplayHandoverDate = r["DisplayHandoverDate"].ToString(),
                            Amount = Convert.ToDecimal(r["Amount"]),
                            Remarks = r["Remarks"].ToString(),
                            CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                            DisplayCreatedDate = r["DisplayCreatedDate"].ToString()
                        });
            }
            return list;
        }

        public AdminWalletBalanceModel GetAdminWalletBalance(int adminUserID)
        {
            var m = new AdminWalletBalanceModel();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_AdminWallet_Balance", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@AdminUserID", adminUserID);
                using (var r = cmd.ExecuteReader())
                    if (r.Read())
                        m.TotalReceived = Convert.ToDecimal(r["TotalReceived"]);
            }
            return m;
        }

        // ════════════════════════════════════════════════════════════════════
        // COMPANY WALLET
        // ════════════════════════════════════════════════════════════════════
        public CompanyWalletSummaryModel GetCompanyWalletSummary()
        {
            var m = new CompanyWalletSummaryModel();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_CompanyWallet_Summary", conn)
                { CommandType = CommandType.StoredProcedure };
                using (var r = cmd.ExecuteReader())
                    if (r.Read())
                    {
                        m.TotalDriverEarnings = Convert.ToDecimal(r["TotalDriverEarnings"]);
                        m.TotalHandedOver = Convert.ToDecimal(r["TotalHandedOver"]);
                        m.TotalDriverExpenses = Convert.ToDecimal(r["TotalDriverExpenses"]);
                        m.TotalCompanyExpenses = Convert.ToDecimal(r["TotalCompanyExpenses"]);
                        m.WithDrivers = Convert.ToDecimal(r["WithDrivers"]);
                        m.CompanyBalance = Convert.ToDecimal(r["CompanyBalance"]);
                    }
            }
            return m;
        }

        public List<CompanyWalletDriverRowModel> GetCompanyWalletDriverBreakdown()
        {
            var list = new List<CompanyWalletDriverRowModel>();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_CompanyWallet_DriverBreakdown", conn)
                { CommandType = CommandType.StoredProcedure };
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new CompanyWalletDriverRowModel
                        {
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"].ToString(),
                            TotalEarned = Convert.ToDecimal(r["TotalEarned"]),
                            TotalHandedOver = Convert.ToDecimal(r["TotalHandedOver"]),
                            TotalExpenses = Convert.ToDecimal(r["TotalExpenses"]),
                            Balance = Convert.ToDecimal(r["Balance"])
                        });
            }
            return list;
        }

        public bool SaveCompanyExpense(CompanyExpenseModel model)
        {
            try
            {
                using (var conn = new SqlConnection(Conn()))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO CompanyExpense(ExpenseDate,Category,Amount,Remarks,DriverUserID,CreatedBy)" +
                        " VALUES(@ExpenseDate,@Category,@Amount,@Remarks,@DriverUserID,@CreatedBy)", conn);
                    cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate.Date);
                    cmd.Parameters.AddWithValue("@Category", model.Category);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(model.Remarks) ? (object)DBNull.Value : model.Remarks);
                    cmd.Parameters.AddWithValue("@DriverUserID", model.DriverUserID.HasValue ? (object)model.DriverUserID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy.HasValue ? (object)model.CreatedBy.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch { return false; }
        }

        public List<CompanyExpenseModel> GetCompanyExpenses(DateTime? from, DateTime? to, string category)
        {
            var list = new List<CompanyExpenseModel>();
            using (var conn = new SqlConnection(Conn()))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_frm_get_CompanyExpenses", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@FromDate", from.HasValue ? (object)from.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", to.HasValue ? (object)to.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", string.IsNullOrEmpty(category) ? (object)DBNull.Value : category);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new CompanyExpenseModel
                        {
                            CompanyExpenseID = Convert.ToInt64(r["CompanyExpenseID"]),
                            ExpenseDate = Convert.ToDateTime(r["ExpenseDate"]),
                            DisplayExpenseDate = r["DisplayExpenseDate"].ToString(),
                            Category = r["Category"].ToString(),
                            Amount = Convert.ToDecimal(r["Amount"]),
                            Remarks = r["Remarks"].ToString(),
                            DriverUserID = r["DriverUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"].ToString(),
                            CreatedBy = r["CreatedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["CreatedBy"]),
                            CreatedByName = r["CreatedByName"].ToString(),
                            CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                            DisplayCreatedDate = r["DisplayCreatedDate"].ToString()
                        });
            }
            return list;
        }

        public bool DeleteCompanyExpense(long companyExpenseID)
        {
            try
            {
                using (var conn = new SqlConnection(Conn()))
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM CompanyExpense WHERE CompanyExpenseID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", companyExpenseID);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch { return false; }
        }
    }
}