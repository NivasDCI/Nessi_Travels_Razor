using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Transport.Model;

namespace Transport.Repository
{
    // ── Interface ─────────────────────────────────────────────────────────────
    public interface IDriverWalletRepository
    {
        void SyncWallet(int driverUserID, DateTime date);
        List<DriverWalletDayModel> GetWalletDailySummary(int driverUserID, DateTime? fromDate, DateTime? toDate);
        List<DriverWalletJobDetailModel> GetDayJobDetails(int driverUserID, DateTime walletDate);
        WalletBalanceSummaryModel GetWalletBalanceSummary(int driverUserID);

        bool SaveHandover(DriverHandoverModel model);
        List<DriverHandoverModel> GetHandovers(int? driverUserID, DateTime? fromDate, DateTime? toDate);
        bool DeleteHandover(long handoverID);

        bool SaveExpense(DriverExpenseWalletModel model);
        List<DriverExpenseWalletModel> GetExpenses(int? driverUserID, DateTime? fromDate, DateTime? toDate);
        bool DeleteExpense(long driverExpenseID);
    }

    // ── Implementation ────────────────────────────────────────────────────────
    public class DriverWalletRepository : IDriverWalletRepository
    {
        // ── Connection string (same pattern as InvoiceRepository) ─────────────
        private string GetConnectionString()
        {
            string txtpath = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
            string connectionValue = "";
            using (StreamReader sr = new StreamReader(txtpath))
            {
                while (sr.Peek() >= 0)
                    connectionValue = sr.ReadLine();
            }
            return connectionValue;
        }

        // ═════════════════════════════════════════════════════════════════════
        // SyncWallet  –  Recalculates & UPSERTs one DriverWallet row
        //               for a given driver + date.
        //               Called automatically when a job is completed AND
        //               when a handover / expense is saved / deleted.
        // ═════════════════════════════════════════════════════════════════════
        public void SyncWallet(int driverUserID, DateTime date)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    DECLARE @TotalRides      INT
                    DECLARE @TotalEarned     DECIMAL(18,2)
                    DECLARE @TotalHandedOver DECIMAL(18,2)
                    DECLARE @TotalExpenses   DECIMAL(18,2)
                    DECLARE @ClosingBalance  DECIMAL(18,2)

                    -- Jobs completed that day by this driver that brought in cash
                    SELECT  @TotalRides  = COUNT(*),
                            @TotalEarned = ISNULL(SUM(ISNULL(Cash,0)), 0)
                    FROM    Jobs
                    WHERE   DrivingBy = @DriverUserID
                      AND   CAST(JobDate AS DATE) = CAST(@WalletDate AS DATE)
                      AND   JobStatus = 'Job Completed'
                      AND   Cash IS NOT NULL
                      AND   Cash > 0;

                    SELECT  @TotalHandedOver = ISNULL(SUM(Amount), 0)
                    FROM    DriverHandover
                    WHERE   DriverUserID = @DriverUserID
                      AND   HandoverDate  = CAST(@WalletDate AS DATE);

                    SELECT  @TotalExpenses = ISNULL(SUM(Amount), 0)
                    FROM    DriverExpense
                    WHERE   DriverUserID = @DriverUserID
                      AND   ExpenseDate   = CAST(@WalletDate AS DATE);

                    SET @ClosingBalance = @TotalEarned - @TotalHandedOver - @TotalExpenses;

                    MERGE DriverWallet AS target
                    USING (SELECT @DriverUserID AS DU, CAST(@WalletDate AS DATE) AS WD) AS source
                          ON target.DriverUserID = source.DU
                         AND target.WalletDate   = source.WD
                    WHEN MATCHED THEN
                        UPDATE SET
                            TotalRides      = @TotalRides,
                            TotalEarned     = @TotalEarned,
                            TotalHandedOver = @TotalHandedOver,
                            TotalExpenses   = @TotalExpenses,
                            ClosingBalance  = @ClosingBalance,
                            UpdatedDate     = GETDATE()
                    WHEN NOT MATCHED AND (@TotalRides > 0 OR @TotalHandedOver > 0 OR @TotalExpenses > 0) THEN
                        INSERT (DriverUserID, WalletDate, TotalRides, TotalEarned,
                                TotalHandedOver, TotalExpenses, ClosingBalance)
                        VALUES (@DriverUserID, CAST(@WalletDate AS DATE), @TotalRides,
                                @TotalEarned, @TotalHandedOver, @TotalExpenses, @ClosingBalance);";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID);
                cmd.Parameters.AddWithValue("@WalletDate", date.Date);
                cmd.ExecuteNonQuery();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetWalletDailySummary  –  Daily summary rows ordered DESC by date
        //                           with cumulative RunningBalance
        // ═════════════════════════════════════════════════════════════════════
        public List<DriverWalletDayModel> GetWalletDailySummary(int driverUserID, DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<DriverWalletDayModel>();
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT  w.WalletID,
                            w.DriverUserID,
                            u.FirstName + ' ' + u.LastName AS DriverName,
                            w.WalletDate,
                            w.TotalRides,
                            w.TotalEarned,
                            w.TotalHandedOver,
                            w.TotalExpenses,
                            w.ClosingBalance,
                            SUM(w.ClosingBalance) OVER (
                                PARTITION BY w.DriverUserID
                                ORDER BY w.WalletDate
                                ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                            ) AS RunningBalance,
                            CONVERT(VARCHAR, w.CreatedDate, 106) AS DisplayCreatedDate
                    FROM    DriverWallet w
                    LEFT JOIN TblUserMaster u ON w.DriverUserID = u.UserID
                    WHERE   w.DriverUserID = @DriverUserID
                      AND   (@FromDate IS NULL OR w.WalletDate >= CAST(@FromDate AS DATE))
                      AND   (@ToDate   IS NULL OR w.WalletDate <= CAST(@ToDate   AS DATE))
                    ORDER BY w.WalletDate DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID);
                cmd.Parameters.AddWithValue("@FromDate", (object)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object)toDate ?? DBNull.Value);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new DriverWalletDayModel
                        {
                            WalletID = Convert.ToInt64(r["WalletID"]),
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"]?.ToString(),
                            WalletDate = Convert.ToDateTime(r["WalletDate"]),
                            TotalRides = Convert.ToInt32(r["TotalRides"]),
                            TotalEarned = Convert.ToDecimal(r["TotalEarned"]),
                            TotalHandedOver = Convert.ToDecimal(r["TotalHandedOver"]),
                            TotalExpenses = Convert.ToDecimal(r["TotalExpenses"]),
                            ClosingBalance = Convert.ToDecimal(r["ClosingBalance"]),
                            RunningBalance = Convert.ToDecimal(r["RunningBalance"]),
                            DisplayCreatedDate = r["DisplayCreatedDate"]?.ToString()
                        });
                    }
                }
            }
            return list;
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetDayJobDetails  –  All jobs a driver completed on a specific date
        // ═════════════════════════════════════════════════════════════════════
        public List<DriverWalletJobDetailModel> GetDayJobDetails(int driverUserID, DateTime walletDate)
        {
            var list = new List<DriverWalletJobDetailModel>();
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                // Joining Jobs with JobVendors via JobVendorCode for display name
                string sql = @"
                    SELECT  j.JobCode,
                            j.JobDate,
                            j.JobTime,
                            j.JobFrom,
                            j.JobTo,
                            j.CustomerName,
                            v.VehicleName,
                            jv.JobVendorName,
                            j.JobStatus,
                            j.Cash,
                            j.Credit
                    FROM    Jobs j
                    LEFT JOIN Vehicles jvh ON j.VehicleCode = jvh.VehicleCode
                    LEFT JOIN JobVendors jv ON j.JobVendorCode = jv.JobVendorCode
                    LEFT JOIN Vehicles v ON j.VehicleCode = v.VehicleCode
                    WHERE   j.DrivingBy = @DriverUserID
                      AND   CAST(j.JobDate AS DATE) = CAST(@WalletDate AS DATE)
                      AND   j.JobStatus = 'Job Completed'
                    ORDER BY j.JobTime";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID);
                cmd.Parameters.AddWithValue("@WalletDate", walletDate.Date);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new DriverWalletJobDetailModel
                        {
                            JobCode = Convert.ToInt64(r["JobCode"]),
                            JobDate = r["JobDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["JobDate"]),
                            JobTime = r["JobTime"]?.ToString(),
                            JobFrom = r["JobFrom"]?.ToString(),
                            JobTo = r["JobTo"]?.ToString(),
                            CustomerName = r["CustomerName"]?.ToString(),
                            VehicleName = r["VehicleName"]?.ToString(),
                            JobVendorName = r["JobVendorName"]?.ToString(),
                            JobStatus = r["JobStatus"]?.ToString(),
                            Cash = r["Cash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Cash"]),
                            Credit = r["Credit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Credit"])
                        });
                    }
                }
            }
            return list;
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetWalletBalanceSummary  –  Overall totals (all-time for a driver)
        // ═════════════════════════════════════════════════════════════════════
        public WalletBalanceSummaryModel GetWalletBalanceSummary(int driverUserID)
        {
            var summary = new WalletBalanceSummaryModel();
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT  ISNULL(SUM(TotalEarned),     0) AS TotalEarned,
                            ISNULL(SUM(TotalHandedOver), 0) AS TotalHandedOver,
                            ISNULL(SUM(TotalExpenses),   0) AS TotalExpenses,
                            ISNULL(SUM(ClosingBalance),  0) AS RunningBalance
                    FROM    DriverWallet
                    WHERE   DriverUserID = @DriverUserID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DriverUserID", driverUserID);

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        summary.TotalEarned = Convert.ToDecimal(r["TotalEarned"]);
                        summary.TotalHandedOver = Convert.ToDecimal(r["TotalHandedOver"]);
                        summary.TotalExpenses = Convert.ToDecimal(r["TotalExpenses"]);
                        summary.RunningBalance = Convert.ToDecimal(r["RunningBalance"]);
                    }
                }
            }
            return summary;
        }

        // ═════════════════════════════════════════════════════════════════════
        // SaveHandover  –  Insert a handover record, then re-sync that day
        // ═════════════════════════════════════════════════════════════════════
        public bool SaveHandover(DriverHandoverModel model)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO DriverHandover
                            (DriverUserID, HandoverDate, Amount, HandedToUserID, HandedToName, Remarks, CreatedBy)
                        VALUES
                            (@DriverUserID, @HandoverDate, @Amount, @HandedToUserID, @HandedToName, @Remarks, @CreatedBy)";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@DriverUserID", model.DriverUserID);
                    cmd.Parameters.AddWithValue("@HandoverDate", model.HandoverDate.Date);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@HandedToUserID", (object)model.HandedToUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HandedToName", (object)model.HandedToName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", (object)model.CreatedBy ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                // Re-sync wallet for that day
                SyncWallet(model.DriverUserID, model.HandoverDate.Date);
                return true;
            }
            catch { return false; }
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetHandovers  –  List of handover records with optional filters
        // ═════════════════════════════════════════════════════════════════════
        public List<DriverHandoverModel> GetHandovers(int? driverUserID, DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<DriverHandoverModel>();
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT  h.*,
                            d.FirstName + ' ' + d.LastName AS DriverName
                    FROM    DriverHandover h
                    LEFT JOIN TblUserMaster d ON h.DriverUserID = d.UserID
                    WHERE   (@DriverUserID IS NULL OR h.DriverUserID = @DriverUserID)
                      AND   (@FromDate IS NULL OR h.HandoverDate >= CAST(@FromDate AS DATE))
                      AND   (@ToDate   IS NULL OR h.HandoverDate <= CAST(@ToDate   AS DATE))
                    ORDER BY h.HandoverDate DESC, h.HandoverID DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DriverUserID", (object)driverUserID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FromDate", (object)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object)toDate ?? DBNull.Value);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new DriverHandoverModel
                        {
                            HandoverID = Convert.ToInt64(r["HandoverID"]),
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"]?.ToString(),
                            HandoverDate = Convert.ToDateTime(r["HandoverDate"]),
                            Amount = Convert.ToDecimal(r["Amount"]),
                            HandedToUserID = r["HandedToUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["HandedToUserID"]),
                            HandedToName = r["HandedToName"]?.ToString(),
                            Remarks = r["Remarks"]?.ToString(),
                            CreatedBy = r["CreatedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["CreatedBy"]),
                            CreatedDate = Convert.ToDateTime(r["CreatedDate"])
                        });
                    }
                }
            }
            return list;
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteHandover  –  Remove a handover and re-sync that day's wallet
        // ═════════════════════════════════════════════════════════════════════
        public bool DeleteHandover(long handoverID)
        {
            try
            {
                int driverID = 0;
                DateTime handoverDate = DateTime.Today;

                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    // Read before delete so we can sync after
                    var readCmd = new SqlCommand(
                        "SELECT DriverUserID, HandoverDate FROM DriverHandover WHERE HandoverID=@ID", conn);
                    readCmd.Parameters.AddWithValue("@ID", handoverID);
                    using (var r = readCmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            driverID = Convert.ToInt32(r["DriverUserID"]);
                            handoverDate = Convert.ToDateTime(r["HandoverDate"]);
                        }
                    }

                    var delCmd = new SqlCommand(
                        "DELETE FROM DriverHandover WHERE HandoverID=@ID", conn);
                    delCmd.Parameters.AddWithValue("@ID", handoverID);
                    delCmd.ExecuteNonQuery();
                }

                if (driverID > 0)
                    SyncWallet(driverID, handoverDate);

                return true;
            }
            catch { return false; }
        }

        // ═════════════════════════════════════════════════════════════════════
        // SaveExpense  –  Insert driver expense, then re-sync that day
        // ═════════════════════════════════════════════════════════════════════
        public bool SaveExpense(DriverExpenseWalletModel model)
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO DriverExpense
                            (DriverUserID, ExpenseDate, Category, Amount, Remarks, JobCode, CreatedBy)
                        VALUES
                            (@DriverUserID, @ExpenseDate, @Category, @Amount, @Remarks, @JobCode, @CreatedBy)";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@DriverUserID", model.DriverUserID);
                    cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate.Date);
                    cmd.Parameters.AddWithValue("@Category", (object)model.Category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobCode", (object)model.JobCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", (object)model.CreatedBy ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                SyncWallet(model.DriverUserID, model.ExpenseDate.Date);
                return true;
            }
            catch { return false; }
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetExpenses  –  List of driver expense records
        // ═════════════════════════════════════════════════════════════════════
        public List<DriverExpenseWalletModel> GetExpenses(int? driverUserID, DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<DriverExpenseWalletModel>();
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT  e.*,
                            u.FirstName + ' ' + u.LastName AS DriverName
                    FROM    DriverExpense e
                    LEFT JOIN TblUserMaster u ON e.DriverUserID = u.UserID
                    WHERE   (@DriverUserID IS NULL OR e.DriverUserID = @DriverUserID)
                      AND   (@FromDate IS NULL OR e.ExpenseDate >= CAST(@FromDate AS DATE))
                      AND   (@ToDate   IS NULL OR e.ExpenseDate <= CAST(@ToDate   AS DATE))
                    ORDER BY e.ExpenseDate DESC, e.DriverExpenseID DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DriverUserID", (object)driverUserID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FromDate", (object)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object)toDate ?? DBNull.Value);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new DriverExpenseWalletModel
                        {
                            DriverExpenseID = Convert.ToInt64(r["DriverExpenseID"]),
                            DriverUserID = Convert.ToInt32(r["DriverUserID"]),
                            DriverName = r["DriverName"]?.ToString(),
                            ExpenseDate = Convert.ToDateTime(r["ExpenseDate"]),
                            Category = r["Category"]?.ToString(),
                            Amount = Convert.ToDecimal(r["Amount"]),
                            Remarks = r["Remarks"]?.ToString(),
                            JobCode = r["JobCode"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["JobCode"]),
                            CreatedBy = r["CreatedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["CreatedBy"]),
                            CreatedDate = Convert.ToDateTime(r["CreatedDate"])
                        });
                    }
                }
            }
            return list;
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteExpense  –  Remove expense and re-sync that day
        // ═════════════════════════════════════════════════════════════════════
        public bool DeleteExpense(long driverExpenseID)
        {
            try
            {
                int driverID = 0;
                DateTime expenseDate = DateTime.Today;

                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    var readCmd = new SqlCommand(
                        "SELECT DriverUserID, ExpenseDate FROM DriverExpense WHERE DriverExpenseID=@ID", conn);
                    readCmd.Parameters.AddWithValue("@ID", driverExpenseID);
                    using (var r = readCmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            driverID = Convert.ToInt32(r["DriverUserID"]);
                            expenseDate = Convert.ToDateTime(r["ExpenseDate"]);
                        }
                    }

                    var delCmd = new SqlCommand(
                        "DELETE FROM DriverExpense WHERE DriverExpenseID=@ID", conn);
                    delCmd.Parameters.AddWithValue("@ID", driverExpenseID);
                    delCmd.ExecuteNonQuery();
                }

                if (driverID > 0)
                    SyncWallet(driverID, expenseDate);

                return true;
            }
            catch { return false; }
        }
    }
}