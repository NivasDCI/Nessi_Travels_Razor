using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using System.Web.Mvc;
using Transport.Model;
using Transport.Repository;
using System.IO;
using System.Reflection;
using MiniExcelLibs;
using System.Globalization;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class ReportsController : Controller
    {
        IReportsRepository _objReportsRepository = new ReportsRepository();

        #region "Dashboard"
        public ActionResult OverallReport(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult AccountsReport(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult MyAccountsReport(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult TransactionsReport(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult StaffAccountsReport(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult VehiclesReport(string HeaderViewID, string DetailViewID) { return View(); }

        [HttpGet]
        public ActionResult Dashboard_FindAll()
        {
            DashboardModel Dashboardlist = _objReportsRepository.Dashboard_FindAll();
            return Json(Dashboardlist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "Jobs"
        public ActionResult JobsReport(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult MyJobsReport(string HeaderViewID, string DetailViewID) { return View(); }

        [HttpGet]
        public ActionResult MyJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            if (StartDate == null) { StartDate = firstDayOfMonth; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            int? CashInHand = SessionExpire.GetUserID();
            List<JobModel> Jobslist = _objReportsRepository.Job_FindAll(page, StartDate, EndDate, null, null, null, null, null, CashInHand, limit, sortBy, direction, out TotalCount);
            return Json(new { records = Jobslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Trxn_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? UserID, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            if (StartDate == null) { StartDate = firstDayOfMonth; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            DataSet ds = ReturnTrxnDetails(UserID, StartDate, EndDate);
            List<TrxnsReportModel> trxnList = new List<TrxnsReportModel>();
            if (ds.Tables[0].Rows.Count > 0)
            {
                trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new TrxnsReportModel
                {
                    TrxnID = dataRow.Field<int>("TrxnID"),
                    TrxnDate = dataRow.Field<DateTime>("TrxnDate"),
                    ServiceName = dataRow.Field<string>("ServiceName"),
                    Debit = dataRow.Field<decimal>("Debit"),
                    Credit = dataRow.Field<decimal>("Credit")
                }).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc") { switch (sortBy.Trim()) { case "ServiceName": trxnList = trxnList.OrderBy(q => q.ServiceName).ToList(); break; } }
                else { switch (sortBy.Trim()) { case "ServiceName": trxnList = trxnList.OrderByDescending(q => q.ServiceName).ToList(); break; } }
            }
            TotalCount = trxnList.Count;
            if (page.HasValue && limit.HasValue) { int start = (page.Value - 1) * limit.Value; trxnList = trxnList.Skip(start).Take(limit.Value).ToList(); }
            return Json(new { records = trxnList, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MyTrxn_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            if (StartDate == null) { StartDate = firstDayOfMonth; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            int? UserID = SessionExpire.GetUserID();
            DataSet ds = ReturnTrxnDetails(UserID, StartDate, EndDate);
            List<TrxnsReportModel> trxnList = new List<TrxnsReportModel>();
            if (ds.Tables[0].Rows.Count > 0)
            {
                trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new TrxnsReportModel
                {
                    TrxnID = dataRow.Field<int>("TrxnID"),
                    TrxnDate = dataRow.Field<DateTime>("TrxnDate"),
                    ServiceName = dataRow.Field<string>("ServiceName"),
                    Debit = dataRow.Field<decimal>("Debit"),
                    Credit = dataRow.Field<decimal>("Credit")
                }).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc") { switch (sortBy.Trim()) { case "ServiceName": trxnList = trxnList.OrderBy(q => q.ServiceName).ToList(); break; } }
                else { switch (sortBy.Trim()) { case "ServiceName": trxnList = trxnList.OrderByDescending(q => q.ServiceName).ToList(); break; } }
            }
            TotalCount = trxnList.Count;
            if (page.HasValue && limit.HasValue) { int start = (page.Value - 1) * limit.Value; trxnList = trxnList.Skip(start).Take(limit.Value).ToList(); }
            return Json(new { records = trxnList, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        private DataSet ReturnTrxnDetails(int? UserID, DateTime? TransactionFrom, DateTime? TransactionTo)
        {
            DataSet dsQuery = new DataSet();
            try
            {
                SqlParameter[] sqlParams = new SqlParameter[3];
                sqlParams[0] = new SqlParameter("@UserID", SqlDbType.Int); sqlParams[0].Value = UserID;
                sqlParams[1] = new SqlParameter("@TransactionFrom", SqlDbType.DateTime);
                if (TransactionFrom != null) sqlParams[1].Value = TransactionFrom; else sqlParams[1].Value = DBNull.Value;
                sqlParams[2] = new SqlParameter("@TransactionTo", SqlDbType.DateTime);
                if (TransactionFrom != null) sqlParams[2].Value = TransactionTo; else sqlParams[2].Value = DBNull.Value;
                dsQuery = FetchRS(CommandType.StoredProcedure, "sp_frm_get_Transaction_Report", sqlParams);
            }
            catch (Exception ex) { ModelState.AddModelError(string.Empty, string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : ex.Message); }
            return dsQuery;
        }

        public static DataSet FetchRS(CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                string txtpath = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
                string connectionvalue = "";
                using (StreamReader sr = new StreamReader(txtpath))
                {
                    while (sr.Peek() >= 0) { connectionvalue = sr.ReadLine(); }
                }
                conn.ConnectionString = connectionvalue;
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                try
                {
                    conn.Open(); cmd.Connection = conn; cmd.CommandText = cmdText; cmd.CommandType = cmdType;
                    if (cmdParams != null) { foreach (SqlParameter param in cmdParams) cmd.Parameters.Add(param); }
                    da.SelectCommand = cmd; da.Fill(ds); cmd.Parameters.Clear(); return ds;
                }
                catch { conn.Close(); throw; }
            }
        }

        [HttpGet]
        public ActionResult Job_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            if (StartDate == null) { StartDate = firstDayOfMonth; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<JobModel> Jobslist = _objReportsRepository.Job_FindAll(page, StartDate, EndDate, VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand, limit, sortBy, direction, out TotalCount);
            return Json(new { records = Jobslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        public FileStreamResult JobsExcelExport(int? Length, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, DateTime? StartDate, DateTime? EndDate, int? CashInHand)
        {
            var memoryStream = new MemoryStream();
            try
            {
                List<JobModel> Jobslist = _objReportsRepository.JobsReport(StartDate, EndDate, VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand);
                List<JobModelFiltered> JobslistFiltered = Jobslist.ToList().Select(o => new JobModelFiltered
                {
                    JobCode = o.JobCode,
                    JobDate = o.JobDate,
                    JobTime = o.JobTime,
                    VehicleName = o.VehicleName,
                    JobFrom = o.JobFrom,
                    JobTo = o.JobTo,
                    JobVendorName = o.JobVendorName,
                    DrivingByName = o.DrivingByName,
                    CustomerName = o.CustomerName,
                    ContactNo = o.ContactNo,
                    Cost = o.Cost.HasValue ? o.Cost : 0,
                    Credit = o.Credit.HasValue ? o.Credit : 0,
                    Cash = o.Cash.HasValue ? o.Cash : 0,
                    OutSource = o.OutSource,
                    OutSourceAmount = o.OutSourceAmount.HasValue ? String.Format("{0:N2}", o.OutSourceAmount) : string.Empty,
                    CashInHandName = o.CashInHandName,
                    JobStatus = o.JobStatus,
                    Remarks = o.Remarks,
                    DisplayCreatedBy = o.DisplayCreatedBy,
                    DisplayCreatedDate = o.DisplayCreatedDate,
                    DisplayLastModifiedBy = o.DisplayLastModifiedBy,
                    DisplayLastModifiedDate = o.DisplayLastModifiedDate
                }).ToList();
                memoryStream.SaveAs(JobslistFiltered);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex) { }
            return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = "JobsReport_" + DateTime.Now.ToString() + ".xlsx" };
        }

        [HttpGet] public ActionResult TotalReport(DateTime? StartDate, DateTime? EndDate) { if (StartDate == null) { StartDate = DateTime.Now; } if (EndDate == null) { EndDate = DateTime.Now; } List<TotalReportModel> Totallist = _objReportsRepository.TotalReport(StartDate, EndDate); return Json(new { records = Totallist }, JsonRequestBehavior.AllowGet); }
        [HttpGet] public ActionResult VehicleReport(DateTime? StartDate, DateTime? EndDate) { if (StartDate == null) { StartDate = DateTime.Now; } if (EndDate == null) { EndDate = DateTime.Now; } List<VehicleReportModel> Vehiclelist = _objReportsRepository.VehicleReport(StartDate, EndDate); return Json(new { records = Vehiclelist }, JsonRequestBehavior.AllowGet); }
        [HttpGet] public ActionResult CashInHandReport(DateTime? StartDate, DateTime? EndDate) { if (StartDate == null) { StartDate = DateTime.Now; } if (EndDate == null) { EndDate = DateTime.Now; } List<CashInHandReportModel> CashInHandlist = _objReportsRepository.CashInHandReport(StartDate, EndDate); return Json(new { records = CashInHandlist }, JsonRequestBehavior.AllowGet); }
        [HttpGet] public ActionResult CreditReport(DateTime? StartDate, DateTime? EndDate) { if (StartDate == null) { StartDate = DateTime.Now; } if (EndDate == null) { EndDate = DateTime.Now; } List<CreditReportModel> Creditlist = _objReportsRepository.CreditReport(StartDate, EndDate); return Json(new { records = Creditlist }, JsonRequestBehavior.AllowGet); }
        [HttpGet] public ActionResult CreditCustomersReport(DateTime? StartDate, DateTime? EndDate) { if (StartDate == null) { StartDate = DateTime.Now; } if (EndDate == null) { EndDate = DateTime.Now; } List<CreditCustomersReportModel> CreditCustomerslist = _objReportsRepository.CreditCustomersReport(StartDate, EndDate); return Json(new { records = CreditCustomerslist }, JsonRequestBehavior.AllowGet); }
        [HttpGet] public ActionResult VehicleTotalReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate) { if (StartDate == null) { StartDate = DateTime.Now; } if (EndDate == null) { EndDate = DateTime.Now; } List<TotalReportModel> Totallist = _objReportsRepository.VehicleTotalReport(VehicleCode, StartDate, EndDate); return Json(new { records = Totallist }, JsonRequestBehavior.AllowGet); }
        [HttpGet] public ActionResult VehicleExpenseReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate) { if (StartDate == null) { StartDate = DateTime.Now; } if (EndDate == null) { EndDate = DateTime.Now; } List<ExpenseModel> Totallist = _objReportsRepository.VehicleExpenseReport(VehicleCode, StartDate, EndDate); return Json(new { records = Totallist }, JsonRequestBehavior.AllowGet); }

        public ActionResult OutSourceJobsReport(string HeaderViewID, string DetailViewID) { return View(); }

        [HttpGet]
        public ActionResult OutSourceJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            if (StartDate == null) { StartDate = firstDayOfMonth; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<JobModel> Jobslist = _objReportsRepository.OutSourceJob_FindAll(page, StartDate, EndDate, VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand, limit, sortBy, direction, out TotalCount);
            return Json(new { records = Jobslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        public FileStreamResult OutSourceJobsExcelExport(int? Length, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, DateTime? StartDate, DateTime? EndDate, int? CashInHand)
        {
            var memoryStream = new MemoryStream();
            try
            {
                List<JobModel> Jobslist = _objReportsRepository.OutSourceJobsReport(StartDate, EndDate, VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand);
                List<JobModelFiltered> JobslistFiltered = Jobslist.ToList().Select(o => new JobModelFiltered
                {
                    JobCode = o.JobCode,
                    JobDate = o.JobDate,
                    JobTime = o.JobTime,
                    VehicleName = o.VehicleName,
                    JobFrom = o.JobFrom,
                    JobTo = o.JobTo,
                    JobVendorName = o.JobVendorName,
                    DrivingByName = o.DrivingByName,
                    CustomerName = o.CustomerName,
                    ContactNo = o.ContactNo,
                    Cost = o.Cost.HasValue ? o.Cost : 0,
                    Credit = o.Credit.HasValue ? o.Credit : 0,
                    Cash = o.Cash.HasValue ? o.Cash : 0,
                    OutSource = o.OutSource,
                    OutSourceAmount = o.OutSourceAmount.HasValue ? String.Format("{0:N2}", o.OutSourceAmount) : string.Empty,
                    OutSourceAmountGiven = o.OutSourceAmountGiven.HasValue ? String.Format("{0:N2}", o.OutSourceAmountGiven) : string.Empty,
                    CashInHandName = o.CashInHandName,
                    JobStatus = o.JobStatus,
                    Remarks = o.Remarks,
                    DisplayCreatedBy = o.DisplayCreatedBy,
                    DisplayCreatedDate = o.DisplayCreatedDate,
                    DisplayLastModifiedBy = o.DisplayLastModifiedBy,
                    DisplayLastModifiedDate = o.DisplayLastModifiedDate
                }).ToList();
                memoryStream.SaveAs(JobslistFiltered);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex) { }
            return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = "JobsReport_" + DateTime.Now.ToString() + ".xlsx" };
        }
        #endregion

        #region "Invoice"

        private SqlConnection GetRawConnection()
        {
            string efConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["TransportEntities"].ConnectionString;
            string marker = "provider connection string=";
            int mi = efConnStr.IndexOf(marker, System.StringComparison.OrdinalIgnoreCase);
            string raw = string.Empty;
            if (mi >= 0) { int o = efConnStr.IndexOf('"', mi) + 1; int c = efConnStr.IndexOf('"', o); raw = efConnStr.Substring(o, c - o); }
            if (string.IsNullOrEmpty(raw))
            {
                string m2 = "provider connection string=&quot;";
                int m2i = efConnStr.IndexOf(m2, System.StringComparison.OrdinalIgnoreCase);
                if (m2i >= 0) { int o2 = m2i + m2.Length; int c2 = efConnStr.IndexOf("&quot;", o2, System.StringComparison.OrdinalIgnoreCase); raw = efConnStr.Substring(o2, c2 - o2); }
            }
            return new SqlConnection(raw);
        }

        private string GenerateInvoiceNo(SqlConnection conn, SqlTransaction tran)
        {
            var cmd = new SqlCommand("SELECT 'NTT-INV-' + RIGHT('00000' + CAST(ISNULL(MAX(InvoiceID),0)+1 AS VARCHAR),5) FROM InvoiceHeaders", conn, tran);
            object r = cmd.ExecuteScalar();
            return r != null ? r.ToString() : "NTT-INV-00001";
        }

        public ActionResult InvoiceList(string HeaderViewID, string DetailViewID) { return View(); }

        [HttpGet]
        public JsonResult InvoiceDashboard(string VendorName)
        {
            try
            {
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        SELECT
                            ISNULL(SUM(h.TotalAmount), 0)       AS TotalInvoiced,
                            ISNULL(SUM(ISNULL(pay.Paid, 0)), 0) AS TotalCollected,
                            COUNT(*)                             AS TotalInvoices
                        FROM InvoiceHeaders h
                        LEFT JOIN (
                            SELECT InvoiceID, SUM(PaidAmount) AS Paid
                            FROM InvoicePayments GROUP BY InvoiceID
                        ) pay ON pay.InvoiceID = h.InvoiceID
                        WHERE (@VendorName IS NULL OR h.JobVendorName LIKE '%'+@VendorName+'%')", conn);
                    cmd.Parameters.AddWithValue("@VendorName", string.IsNullOrEmpty(VendorName) ? (object)DBNull.Value : VendorName);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            decimal invoiced = Convert.ToDecimal(r["TotalInvoiced"]);
                            decimal collected = Convert.ToDecimal(r["TotalCollected"]);
                            int count = Convert.ToInt32(r["TotalInvoices"]);
                            return Json(new { success = true, TotalInvoiced = invoiced, TotalCollected = collected, TotalPending = invoiced - collected, TotalInvoices = count }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet); }
            return Json(new { success = true, TotalInvoiced = 0, TotalCollected = 0, TotalPending = 0, TotalInvoices = 0 }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetVendorInfo(int vendorCode)
        {
            try
            {
                using (var db2 = new Transport.Entity.TransportEntities())
                {
                    var v = db2.JobVendors.FirstOrDefault(x => x.JobVendorCode == vendorCode);
                    if (v == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    var addr = string.Join("\n", new[] { v.Address1, v.Address2, v.Address3 }.Where(a => !string.IsNullOrWhiteSpace(a)));
                    return Json(new { success = true, name = v.JobVendorName, address = addr }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpGet]
        public JsonResult InvoiceSummary(string VendorName, string FromDate, string ToDate)
        {
            try
            {
                DateTime? pFrom = null, pTo = null;
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                DateTime dt;
                if (!string.IsNullOrEmpty(FromDate) && DateTime.TryParseExact(FromDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pFrom = dt;
                if (!string.IsNullOrEmpty(ToDate) && DateTime.TryParseExact(ToDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pTo = dt;

                using (var conn = GetRawConnection())
                {
                    conn.Open();

                    // Previous balance
                    decimal prevBalance = 0;
                    string prevInvDate = "";
                    if (pFrom.HasValue)
                    {
                        var prevCmd = new SqlCommand(@"
                            SELECT ISNULL(SUM(sub.Balance), 0), MAX(sub.InvDate)
                            FROM (
                                SELECT h.TotalAmount - ISNULL(pay.Paid, 0) AS Balance,
                                       CONVERT(VARCHAR(10), h.InvoiceDate, 103) AS InvDate
                                FROM InvoiceHeaders h
                                LEFT JOIN (SELECT InvoiceID, SUM(PaidAmount) AS Paid FROM InvoicePayments GROUP BY InvoiceID) pay ON pay.InvoiceID = h.InvoiceID
                                WHERE CAST(h.InvoiceDate AS DATE) < CAST(@FromDate AS DATE)
                                  AND (@VendorName IS NULL OR h.JobVendorName LIKE '%'+@VendorName+'%')
                                  AND ISNULL(pay.Paid, 0) < h.TotalAmount
                            ) sub", conn);
                        prevCmd.Parameters.AddWithValue("@FromDate", pFrom.Value);
                        prevCmd.Parameters.AddWithValue("@VendorName", string.IsNullOrEmpty(VendorName) ? (object)DBNull.Value : VendorName);
                        using (var pr = prevCmd.ExecuteReader())
                        {
                            if (pr.Read())
                            {
                                prevBalance = pr[0] == DBNull.Value ? 0 : Convert.ToDecimal(pr[0]);
                                prevInvDate = pr[1] == DBNull.Value ? "" : pr[1].ToString();
                            }
                        }
                    }

                    // Invoice rows with payment details
                    var rows = new List<object>();
                    var rowCmd = new SqlCommand(@"
                        SELECT h.InvoiceID, h.InvoiceNo,
                               CONVERT(VARCHAR(10), h.InvoiceDate, 103) AS InvoiceDate,
                               h.TotalAmount,
                               ISNULL(pay.Paid, 0) AS Paid
                        FROM InvoiceHeaders h
                        LEFT JOIN (SELECT InvoiceID, SUM(PaidAmount) AS Paid FROM InvoicePayments GROUP BY InvoiceID) pay ON pay.InvoiceID = h.InvoiceID
                        WHERE (@VendorName IS NULL OR h.JobVendorName LIKE '%'+@VendorName+'%')
                          AND (@FromDate IS NULL OR CAST(h.InvoiceDate AS DATE) >= CAST(@FromDate AS DATE))
                          AND (@ToDate   IS NULL OR CAST(h.InvoiceDate AS DATE) <= CAST(@ToDate   AS DATE))
                        ORDER BY h.InvoiceDate, h.InvoiceID", conn);
                    rowCmd.Parameters.AddWithValue("@VendorName", string.IsNullOrEmpty(VendorName) ? (object)DBNull.Value : VendorName);
                    rowCmd.Parameters.AddWithValue("@FromDate", pFrom.HasValue ? (object)pFrom.Value : DBNull.Value);
                    rowCmd.Parameters.AddWithValue("@ToDate", pTo.HasValue ? (object)pTo.Value : DBNull.Value);

                    decimal totalInvoiced = 0, totalPaid = 0;
                    decimal runningBalance = prevBalance;
                    var invoiceIds = new List<long>();

                    using (var r = rowCmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            decimal invAmt = Convert.ToDecimal(r["TotalAmount"]);
                            decimal paid = Convert.ToDecimal(r["Paid"]);
                            decimal balance = invAmt - paid;
                            runningBalance = runningBalance + balance;
                            string remarks = paid <= 0 ? "Pending" : balance <= 0 ? "Paid in Full" : "Partial Payment";
                            totalInvoiced += invAmt;
                            totalPaid += paid;
                            long invId = Convert.ToInt64(r["InvoiceID"]);
                            invoiceIds.Add(invId);
                            rows.Add(new
                            {
                                InvoiceID = invId,
                                InvoiceDate = r["InvoiceDate"].ToString(),
                                InvoiceNo = r["InvoiceNo"].ToString(),
                                TotalAmount = invAmt,
                                Paid = paid,
                                Balance = runningBalance,
                                Remarks = remarks
                            });
                        }
                    }

                    // Payment receipts for each invoice
                    // Use Dictionary<long, List<Dictionary<string,string>>> so Json() serializes correctly
                    var allPayments = new System.Collections.Generic.Dictionary<long, List<System.Collections.Generic.Dictionary<string, string>>>();
                    if (invoiceIds.Count > 0)
                    {
                        string idList = string.Join(",", invoiceIds);
                        var payCmd = new SqlCommand(@"
                            SELECT InvoiceID, PaymentDate, PaidAmount, PaymentMode,
                                   AccountName, ReceivedByName, Remarks
                            FROM InvoicePayments
                            WHERE InvoiceID IN (" + idList + @")
                            ORDER BY InvoiceID, PaymentDate, PaymentID", conn);
                        using (var pr = payCmd.ExecuteReader())
                        {
                            while (pr.Read())
                            {
                                long iid = Convert.ToInt64(pr["InvoiceID"]);
                                if (!allPayments.ContainsKey(iid))
                                    allPayments[iid] = new List<System.Collections.Generic.Dictionary<string, string>>();
                                var payRow = new System.Collections.Generic.Dictionary<string, string>();
                                payRow["PaymentDate"] = pr["PaymentDate"] == DBNull.Value ? "" : Convert.ToDateTime(pr["PaymentDate"]).ToString("dd/MM/yyyy");
                                payRow["PaidAmount"] = pr["PaidAmount"] == DBNull.Value ? "0" : Convert.ToDecimal(pr["PaidAmount"]).ToString("F2");
                                payRow["PaymentMode"] = pr["PaymentMode"] == DBNull.Value ? "" : pr["PaymentMode"].ToString();
                                payRow["AccountName"] = pr["AccountName"] == DBNull.Value ? "" : pr["AccountName"].ToString();
                                payRow["ReceivedByName"] = pr["ReceivedByName"] == DBNull.Value ? "" : pr["ReceivedByName"].ToString();
                                payRow["Remarks"] = pr["Remarks"] == DBNull.Value ? "" : pr["Remarks"].ToString();
                                allPayments[iid].Add(payRow);
                            }
                        }
                    }

                    // Attach payments to rows - use concrete typed list for proper JSON serialization
                    var rowsWithPayments = rows.Select(rowObj => {
                        dynamic row = rowObj;
                        long iid = (long)row.InvoiceID;
                        var pays = allPayments.ContainsKey(iid)
                            ? allPayments[iid]
                            : new List<System.Collections.Generic.Dictionary<string, string>>();
                        return new
                        {
                            InvoiceID = iid,
                            InvoiceDate = (string)row.InvoiceDate,
                            InvoiceNo = (string)row.InvoiceNo,
                            TotalAmount = (decimal)row.TotalAmount,
                            Paid = (decimal)row.Paid,
                            Balance = (decimal)row.Balance,
                            Remarks = (string)row.Remarks,
                            Payments = pays
                        };
                    }).ToList();

                    // Vendor info
                    string vendorAddr = "", vendorContact = "";
                    if (!string.IsNullOrEmpty(VendorName))
                    {
                        var vCmd = new SqlCommand("SELECT TOP 1 ISNULL(Address1,'') AS A1, ISNULL(Address2,'') AS A2, ISNULL(Address3,'') AS A3, ISNULL(ContactNo,'') AS Phone FROM JobVendors WHERE JobVendorName LIKE '%'+@VN+'%'", conn);
                        vCmd.Parameters.AddWithValue("@VN", VendorName);
                        using (var vr = vCmd.ExecuteReader())
                        {
                            if (vr.Read())
                            {
                                vendorAddr = string.Join(", ", new[] { vr["A1"].ToString(), vr["A2"].ToString(), vr["A3"].ToString() }.Where(a => !string.IsNullOrWhiteSpace(a)));
                                vendorContact = vr["Phone"].ToString();
                            }
                        }
                    }

                    decimal periodBalance = totalInvoiced - totalPaid;
                    return Json(new
                    {
                        success = true,
                        VendorName = string.IsNullOrEmpty(VendorName) ? "All Vendors" : VendorName,
                        VendorAddress = vendorAddr,
                        VendorContact = vendorContact,
                        FromDate = pFrom.HasValue ? pFrom.Value.ToString("dd/MM/yyyy") : "",
                        ToDate = pTo.HasValue ? pTo.Value.ToString("dd/MM/yyyy") : "",
                        GeneratedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"),
                        PrevBalance = prevBalance,
                        PrevDate = prevInvDate,
                        Rows = rowsWithPayments,
                        TotalInvoiced = totalInvoiced,
                        TotalPaid = totalPaid,
                        TotalPending = periodBalance,
                        GrandTotal = prevBalance + periodBalance
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        [HttpGet]
        public JsonResult Invoice_FindAll(string BillToName, string InvoiceNo, string FromDate, string ToDate, string PaymentStatus, string VendorName)
        {
            var list = new List<InvoiceHeaderModel>();
            try
            {
                DateTime? pFrom = null, pTo = null;
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                DateTime dt;
                if (!string.IsNullOrEmpty(FromDate) && DateTime.TryParseExact(FromDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pFrom = dt;
                if (!string.IsNullOrEmpty(ToDate) && DateTime.TryParseExact(ToDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pTo = dt;

                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        SELECT h.*,
                               (SELECT COUNT(*) FROM InvoiceDetails d WHERE d.InvoiceID=h.InvoiceID) AS TotalJobs,
                               ISNULL((SELECT SUM(p.PaidAmount) FROM InvoicePayments p WHERE p.InvoiceID=h.InvoiceID),0) AS TotalPaid
                        FROM InvoiceHeaders h
                        WHERE (@BillToName IS NULL OR h.BillToName   LIKE '%'+@BillToName+'%')
                          AND (@InvoiceNo  IS NULL OR h.InvoiceNo    LIKE '%'+@InvoiceNo+'%')
                          AND (@VendorName IS NULL OR h.JobVendorName LIKE '%'+@VendorName+'%')
                          AND (@FromDate   IS NULL OR CAST(h.InvoiceDate AS DATE) >= CAST(@FromDate AS DATE))
                          AND (@ToDate     IS NULL OR CAST(h.InvoiceDate AS DATE) <= CAST(@ToDate   AS DATE))
                          AND (@PayStatus  IS NULL
                               OR (@PayStatus='Paid'    AND ISNULL((SELECT SUM(p.PaidAmount) FROM InvoicePayments p WHERE p.InvoiceID=h.InvoiceID),0) >= h.TotalAmount)
                               OR (@PayStatus='Partial' AND ISNULL((SELECT SUM(p.PaidAmount) FROM InvoicePayments p WHERE p.InvoiceID=h.InvoiceID),0) > 0
                                   AND ISNULL((SELECT SUM(p.PaidAmount) FROM InvoicePayments p WHERE p.InvoiceID=h.InvoiceID),0) < h.TotalAmount)
                               OR (@PayStatus='Unpaid'  AND ISNULL((SELECT SUM(p.PaidAmount) FROM InvoicePayments p WHERE p.InvoiceID=h.InvoiceID),0) = 0))
                        ORDER BY h.InvoiceDate DESC, h.InvoiceID DESC", conn);

                    cmd.Parameters.AddWithValue("@BillToName", string.IsNullOrEmpty(BillToName) ? (object)DBNull.Value : BillToName);
                    cmd.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(InvoiceNo) ? (object)DBNull.Value : InvoiceNo);
                    cmd.Parameters.AddWithValue("@FromDate", pFrom.HasValue ? (object)pFrom.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", pTo.HasValue ? (object)pTo.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PayStatus", string.IsNullOrEmpty(PaymentStatus) ? (object)DBNull.Value : PaymentStatus);
                    cmd.Parameters.AddWithValue("@VendorName", string.IsNullOrEmpty(VendorName) ? (object)DBNull.Value : VendorName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var h = MapHeader(reader);
                            h.TotalPaid = reader["TotalPaid"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["TotalPaid"]);
                            list.Add(h);
                        }
                    }
                }
            }
            catch (Exception ex) { return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet); }
            var flat = list.Select(h => new {
                InvoiceID = h.InvoiceID,
                InvoiceNo = h.InvoiceNo,
                DisplayInvoiceDate = h.DisplayInvoiceDate,
                BillToName = h.BillToName,
                BillToAddress = h.BillToAddress,
                JobVendorName = h.JobVendorName,
                CustomerName = h.CustomerName,
                DisplayStartDate = h.DisplayStartDate,
                DisplayEndDate = h.DisplayEndDate,
                TotalJobs = h.TotalJobs,
                TotalAmount = h.TotalAmount,
                TotalPaid = h.TotalPaid,
                BalanceAmount = h.TotalAmount - h.TotalPaid,
                PaymentStatus = h.TotalPaid <= 0 ? "Unpaid" : h.TotalPaid >= h.TotalAmount ? "Paid" : "Partial",
                IsManual = h.IsManual,
                IsCredit = h.IsCredit,
                CreditCash = h.CreditCash
            }).ToList();
            return Json(new { records = flat, total = flat.Count }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetInvoicePayments(long InvoiceID)
        {
            var list = new List<object>();
            try
            {
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"SELECT PaymentID, InvoiceID, PaymentDate, PaidAmount, PaymentMode, AccountName, ReceivedByUserID, ReceivedByName, Remarks, CreatedDate FROM InvoicePayments WHERE InvoiceID=@ID ORDER BY PaymentDate, PaymentID", conn);
                    cmd.Parameters.AddWithValue("@ID", InvoiceID);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new
                            {
                                PaymentID = Convert.ToInt64(r["PaymentID"]),
                                PaymentDate = r["PaymentDate"] == DBNull.Value ? "" : Convert.ToDateTime(r["PaymentDate"]).ToString("dd-MMM-yyyy"),
                                PaidAmount = r["PaidAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["PaidAmount"]),
                                PaymentMode = r["PaymentMode"] == DBNull.Value ? "" : r["PaymentMode"].ToString(),
                                AccountName = r["AccountName"] == DBNull.Value ? "" : r["AccountName"].ToString(),
                                ReceivedByName = r["ReceivedByName"] == DBNull.Value ? "" : r["ReceivedByName"].ToString(),
                                Remarks = r["Remarks"] == DBNull.Value ? "" : r["Remarks"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet); }
            return Json(new { records = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddPayment(long InvoiceID, string PaymentDate, decimal PaidAmount, string PaymentMode, string AccountName, int? ReceivedByUserID, string ReceivedByName, string Remarks)
        {
            try
            {
                if (PaidAmount <= 0) return Json(new { success = false, message = "Amount must be greater than 0." });
                DateTime? pDate = null;
                DateTime dt;
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                if (!string.IsNullOrEmpty(PaymentDate) && DateTime.TryParseExact(PaymentDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pDate = dt;
                if (!pDate.HasValue) pDate = CommonRepository.GetTimeZoneDate();

                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var checkCmd = new SqlCommand(@"SELECT h.TotalAmount, ISNULL((SELECT SUM(p.PaidAmount) FROM InvoicePayments p WHERE p.InvoiceID=h.InvoiceID),0) AS AlreadyPaid FROM InvoiceHeaders h WHERE h.InvoiceID=@ID", conn);
                    checkCmd.Parameters.AddWithValue("@ID", InvoiceID);
                    decimal totalAmount = 0, alreadyPaid = 0;
                    using (var r = checkCmd.ExecuteReader()) { if (r.Read()) { totalAmount = Convert.ToDecimal(r["TotalAmount"]); alreadyPaid = Convert.ToDecimal(r["AlreadyPaid"]); } }
                    decimal remaining = totalAmount - alreadyPaid;
                    if (PaidAmount > remaining) return Json(new { success = false, message = "Payment amount (" + PaidAmount.ToString("N2") + ") exceeds remaining balance (" + remaining.ToString("N2") + ")." });

                    var cmd = new SqlCommand(@"INSERT INTO InvoicePayments(InvoiceID,PaymentDate,PaidAmount,PaymentMode,AccountName,ReceivedByUserID,ReceivedByName,Remarks,CreatedBy,CreatedDate)VALUES(@InvoiceID,@PaymentDate,@PaidAmount,@PaymentMode,@AccountName,@ReceivedByUserID,@ReceivedByName,@Remarks,@CreatedBy,GETDATE());SELECT SCOPE_IDENTITY();", conn);
                    cmd.Parameters.AddWithValue("@InvoiceID", InvoiceID);
                    cmd.Parameters.AddWithValue("@PaymentDate", pDate.Value);
                    cmd.Parameters.AddWithValue("@PaidAmount", PaidAmount);
                    cmd.Parameters.AddWithValue("@PaymentMode", (object)PaymentMode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountName", (object)AccountName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReceivedByUserID", (object)ReceivedByUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReceivedByName", (object)ReceivedByName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remarks", (object)Remarks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", SessionExpire.GetUserID());
                    long newPaymentId = Convert.ToInt64(cmd.ExecuteScalar());

                    // ── NEW: Route payment to the correct wallet ─────────────────────────────
                    // sp_frm_wallet_OnInvoicePayment reads the row we just inserted and
                    // routes it: Cash → ReceivedByUserID's wallet, Account → Company Wallet
                    try
                    {
                        var walletCmd = new SqlCommand("sp_frm_wallet_OnInvoicePayment", conn)
                        {
                            CommandType = System.Data.CommandType.StoredProcedure
                        };
                        walletCmd.Parameters.AddWithValue("@PaymentID", newPaymentId);
                        walletCmd.ExecuteNonQuery();
                    }
                    catch (Exception walletEx)
                    {
                        // Error-ஐ response-ல் திருப்பி அனுப்புங்க — temporarily
                        return Json(new
                        {
                            success = true,
                            walletError = walletEx.Message,  // இதை UI-ல் console-ல் பாருங்க
                            paymentId = newPaymentId
                        });
                    }
                    // ─────────────────────────────────────────────────────────────────────────

                    decimal newTotal = alreadyPaid + PaidAmount;
                    decimal newBal = totalAmount - newTotal;
                    return Json(new { success = true, paymentId = newPaymentId, totalPaid = newTotal, balance = newBal });
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult DeletePayment(long PaymentID)
        {
            try
            {
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // Step 1: Get payment details from InvoicePayments
                            decimal paidAmount = 0;
                            string paymentMode = "";
                            int? receivedByUID = null;
                            DateTime? paymentDate = null;

                            var getCmd = new SqlCommand(
                                "SELECT PaidAmount, PaymentMode, ReceivedByUserID, CAST(PaymentDate AS DATE) AS PaymentDate FROM InvoicePayments WHERE PaymentID=@ID",
                                conn, tran);
                            getCmd.Parameters.AddWithValue("@ID", PaymentID);
                            using (var r = getCmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    paidAmount = Convert.ToDecimal(r["PaidAmount"]);
                                    paymentMode = r["PaymentMode"] == DBNull.Value ? "" : r["PaymentMode"].ToString();
                                    receivedByUID = r["ReceivedByUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ReceivedByUserID"]);
                                    paymentDate = r["PaymentDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["PaymentDate"]);
                                }
                            }

                            // Step 1b: If ReceivedByUserID is null, find from WalletTransaction
                            // (sp_frm_wallet_OnInvoicePayment may not have saved ReceivedByUserID)
                            if (receivedByUID == null && paidAmount > 0)
                            {
                                bool isCashMode = string.Equals(paymentMode, "Cash", StringComparison.OrdinalIgnoreCase);
                                if (isCashMode)
                                {
                                    var findCmd = new SqlCommand(
                                        @"SELECT TOP 1 UserID, CAST(CreatedDate AS DATE) AS TxDate
                                          FROM WalletTransaction
                                          WHERE Source = 'Invoice' AND SourceID = @PayID
                                            AND TransactionType = 'CREDIT'",
                                        conn, tran);
                                    findCmd.Parameters.AddWithValue("@PayID", PaymentID);
                                    using (var fr = findCmd.ExecuteReader())
                                    {
                                        if (fr.Read())
                                        {
                                            receivedByUID = Convert.ToInt32(fr["UserID"]);
                                            if (!paymentDate.HasValue && fr["TxDate"] != DBNull.Value)
                                                paymentDate = Convert.ToDateTime(fr["TxDate"]);
                                        }
                                    }
                                }
                            }

                            // Step 2: Wallet reverse
                            if (paidAmount > 0)
                            {
                                // Case-insensitive payment mode check
                                bool isCash = string.Equals(paymentMode, "Cash", StringComparison.OrdinalIgnoreCase);
                                bool isAccount = string.Equals(paymentMode, "Account", StringComparison.OrdinalIgnoreCase);

                                if (isCash && receivedByUID.HasValue)
                                {
                                    // Step 2a: Update UserWallet balance first
                                    new SqlCommand(
                                        "UPDATE UserWallet SET WalletBalance = WalletBalance - @Amt, LastUpdated = GETDATE() WHERE UserID = @UID",
                                        conn, tran)
                                    {
                                        Parameters = {
                                            new SqlParameter("@Amt", paidAmount),
                                            new SqlParameter("@UID", receivedByUID.Value)
                                        }
                                    }.ExecuteNonQuery();

                                    // Step 2b: Insert WalletTransaction DEBIT (read balance AFTER update)
                                    new SqlCommand(
                                        @"INSERT INTO WalletTransaction
                                            (UserID, TransactionType, Amount, BalanceAfter, Source, SourceID, Remarks, CreatedBy, CreatedDate)
                                          SELECT @UID, 'DEBIT', @Amt,
                                                 ISNULL(WalletBalance, 0),
                                                 'Invoice', @PayID,
                                                 'Payment Deleted - Reversed', @CreatedBy, GETDATE()
                                          FROM   UserWallet WHERE UserID = @UID",
                                        conn, tran)
                                    {
                                        Parameters = {
                                            new SqlParameter("@UID",       receivedByUID.Value),
                                            new SqlParameter("@Amt",       paidAmount),
                                            new SqlParameter("@PayID",     PaymentID),
                                            new SqlParameter("@CreatedBy", SessionExpire.GetUserID())
                                        }
                                    }.ExecuteNonQuery();

                                    // Step 2c: Re-sync DriverWallet daily row via SP
                                    if (paymentDate.HasValue)
                                    {
                                        var syncCmd = new SqlCommand("sp_frm_sync_DriverWallet", conn, tran)
                                        { CommandType = System.Data.CommandType.StoredProcedure };
                                        syncCmd.Parameters.AddWithValue("@UserID", receivedByUID.Value);
                                        syncCmd.Parameters.AddWithValue("@WalletDate", paymentDate.Value.Date);
                                        syncCmd.ExecuteNonQuery();
                                    }
                                }
                                else if (isAccount)
                                {
                                    // Account payment reversed → use sp_frm_companyWallet_Debit
                                    var debitCmd = new SqlCommand("sp_frm_companyWallet_Debit", conn, tran)
                                    { CommandType = System.Data.CommandType.StoredProcedure };
                                    debitCmd.Parameters.AddWithValue("@Amount", paidAmount);
                                    debitCmd.Parameters.AddWithValue("@Source", "Invoice");
                                    debitCmd.Parameters.AddWithValue("@SourceID", PaymentID);
                                    debitCmd.Parameters.AddWithValue("@SourceRef", "Payment Deleted - Reversed");
                                    debitCmd.Parameters.AddWithValue("@CreatedBy", SessionExpire.GetUserID());
                                    debitCmd.ExecuteNonQuery();
                                }
                            }

                            // Step 3: Payment delete
                            new SqlCommand("DELETE FROM InvoicePayments WHERE PaymentID=@ID", conn, tran)
                            {
                                Parameters = { new SqlParameter("@ID", PaymentID) }
                            }.ExecuteNonQuery();

                            tran.Commit();
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public JsonResult GenerateInvoice(string CustomerName, string StartDate, string EndDate, int? JobVendorCode, int? DrivingBy, int? VehicleCode, string CreditCash, int? CashInHand, string BillToName, string BillToAddress)
        {
            try
            {
                DateTime? pStart = null, pEnd = null;
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                DateTime dt;
                if (!string.IsNullOrEmpty(StartDate) && DateTime.TryParseExact(StartDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pStart = dt;
                if (!string.IsNullOrEmpty(EndDate) && DateTime.TryParseExact(EndDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pEnd = dt;
                if (pStart == null) pStart = CommonRepository.GetTimeZoneDate();
                if (pEnd == null) pEnd = CommonRepository.GetTimeZoneDate();
                if (JobVendorCode == 0) JobVendorCode = null;
                if (DrivingBy == 0) DrivingBy = null;
                if (VehicleCode == 0) VehicleCode = null;
                if (CashInHand == 0) CashInHand = null;
                if (string.IsNullOrWhiteSpace(CustomerName)) CustomerName = null;

                int tc = 0;
                var jobs = _objReportsRepository.Job_FindAll(1, pStart, pEnd, VehicleCode, JobVendorCode, CustomerName, null, DrivingBy, CashInHand, 500, null, null, out tc);
                if (!string.IsNullOrEmpty(CreditCash))
                {
                    if (CreditCash == "Credit") jobs = jobs.Where(o => o.Credit.HasValue && o.Credit > 0).ToList();
                    else if (CreditCash == "Cash") jobs = jobs.Where(o => o.Cash.HasValue && o.Cash > 0).ToList();
                }
                if (jobs == null || !jobs.Any()) return Json(new { success = false, message = "No jobs found." });

                var first = jobs.First();
                decimal total = jobs.Sum(o => o.Credit ?? o.Cash ?? 0);
                long invoiceId = 0;

                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            string invNo = GenerateInvoiceNo(conn, tran);
                            var hCmd = new SqlCommand(@"
                                INSERT INTO InvoiceHeaders(InvoiceNo,InvoiceDate,CustomerName,JobVendorCode,JobVendorName,DrivingBy,DrivingByName,VehicleCode,VehicleName,CashInHand,CashInHandName,StartDate,EndDate,CreditCash,TotalAmount,IsCredit,CreatedBy,CreatedDate,BillToName,BillToAddress,IsManual)
                                VALUES(@InvoiceNo,@InvoiceDate,@CustomerName,@JobVendorCode,@JobVendorName,@DrivingBy,@DrivingByName,@VehicleCode,@VehicleName,@CashInHand,@CashInHandName,@StartDate,@EndDate,@CreditCash,@TotalAmount,@IsCredit,@CreatedBy,GETDATE(),@BillToName,@BillToAddress,0);
                                SELECT SCOPE_IDENTITY();", conn, tran);
                            hCmd.Parameters.AddWithValue("@InvoiceNo", invNo);
                            hCmd.Parameters.AddWithValue("@InvoiceDate", DateTime.Now);
                            hCmd.Parameters.AddWithValue("@CustomerName", (object)(string.IsNullOrEmpty(CustomerName) ? first.CustomerName : CustomerName) ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@JobVendorCode", (object)JobVendorCode ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@JobVendorName", (object)first.JobVendorName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@DrivingBy", (object)DrivingBy ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@DrivingByName", (object)first.DrivingByName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@VehicleCode", (object)VehicleCode ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@VehicleName", (object)first.VehicleName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@CashInHand", (object)CashInHand ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@CashInHandName", (object)first.CashInHandName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@StartDate", (object)pStart ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@EndDate", (object)pEnd ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@CreditCash", (object)CreditCash ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@TotalAmount", total);
                            hCmd.Parameters.AddWithValue("@IsCredit", jobs.Any(o => o.Credit.HasValue && o.Credit > 0));
                            hCmd.Parameters.AddWithValue("@CreatedBy", SessionExpire.GetUserID());
                            hCmd.Parameters.AddWithValue("@BillToName", (object)BillToName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@BillToAddress", (object)BillToAddress ?? DBNull.Value);
                            invoiceId = Convert.ToInt64(hCmd.ExecuteScalar());

                            foreach (var job in jobs)
                            {
                                var dCmd = new SqlCommand(@"INSERT INTO InvoiceDetails(InvoiceID,JobCode,JobDate,JobTime,JobFrom,JobTo,CustomerName,VehicleName,DrivingByName,JobVendorName,Credit,Cash,Amount)VALUES(@IID,@JC,@JD,@JT,@JF,@JTO,@CN,@VN,@DB,@JV,@CR,@CA,@AM)", conn, tran);
                                dCmd.Parameters.AddWithValue("@IID", invoiceId);
                                dCmd.Parameters.AddWithValue("@JC", job.JobCode);
                                dCmd.Parameters.AddWithValue("@JD", (object)job.JobDate ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JT", (object)job.JobTime ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JF", (object)job.JobFrom ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JTO", (object)job.JobTo ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@CN", (object)job.CustomerName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@VN", (object)job.VehicleName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@DB", (object)job.DrivingByName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JV", (object)job.JobVendorName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@CR", (object)job.Credit ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@CA", (object)job.Cash ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@AM", (object)(job.Credit ?? job.Cash) ?? DBNull.Value);
                                dCmd.ExecuteNonQuery();
                            }
                            tran.Commit();
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
                return Json(new { success = true, invoiceId = invoiceId });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult CreateManualInvoice(string BillToName, string BillToAddress, string InvoiceDate, string StartDate, string EndDate, decimal TotalAmount, string Terms, string Description)
        {
            try
            {
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                DateTime dt;
                DateTime invDate = CommonRepository.GetTimeZoneDate();
                DateTime? pStart = null, pEnd = null;
                if (!string.IsNullOrEmpty(InvoiceDate) && DateTime.TryParseExact(InvoiceDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) invDate = dt;
                if (!string.IsNullOrEmpty(StartDate) && DateTime.TryParseExact(StartDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pStart = dt;
                if (!string.IsNullOrEmpty(EndDate) && DateTime.TryParseExact(EndDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) pEnd = dt;

                long invoiceId = 0;
                string invNo = "";
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            invNo = GenerateInvoiceNo(conn, tran);
                            var cmd = new SqlCommand(@"
                                INSERT INTO InvoiceHeaders(InvoiceNo,InvoiceDate,BillToName,BillToAddress,StartDate,EndDate,CreditCash,TotalAmount,IsCredit,CreatedBy,CreatedDate,IsManual,PaymentRemarks)
                                VALUES(@InvoiceNo,@InvoiceDate,@BillToName,@BillToAddress,@StartDate,@EndDate,@Terms,@TotalAmount,1,@CreatedBy,GETDATE(),1,@Description);
                                SELECT SCOPE_IDENTITY();", conn, tran);
                            cmd.Parameters.AddWithValue("@InvoiceNo", invNo);
                            cmd.Parameters.AddWithValue("@InvoiceDate", invDate);
                            cmd.Parameters.AddWithValue("@BillToName", (object)BillToName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@BillToAddress", (object)BillToAddress ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@StartDate", (object)pStart ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@EndDate", (object)pEnd ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Terms", string.IsNullOrEmpty(Terms) ? "Credit" : Terms);
                            cmd.Parameters.AddWithValue("@TotalAmount", TotalAmount);
                            cmd.Parameters.AddWithValue("@CreatedBy", SessionExpire.GetUserID());
                            cmd.Parameters.AddWithValue("@Description", (object)Description ?? DBNull.Value);
                            invoiceId = Convert.ToInt64(cmd.ExecuteScalar());
                            tran.Commit();
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
                return Json(new { success = true, invoiceId = invoiceId, invoiceNo = invNo });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        public ActionResult ViewInvoice(long invoiceId)
        {
            var header = GetInvoiceHeader(invoiceId);
            var details = GetInvoiceDetails(invoiceId);
            if (header == null) return Content("Invoice not found.");
            header.Details = details;
            decimal gross = header.IsManual ? header.TotalAmount : details.Sum(d => d.Amount ?? 0);
            ViewBag.Header = header;
            ViewBag.Details = details;
            ViewBag.GrossAmount = gross;
            ViewBag.NetAmount = gross;
            ViewBag.AmountInWords = InvConvertToCamelCase(InvDecimalToWords(gross));
            return View();
        }

        public ActionResult ViewReceipt(long invoiceId)
        {
            var header = GetInvoiceHeader(invoiceId);
            var details = GetInvoiceDetails(invoiceId);
            if (header == null) return Content("Invoice not found.");
            header.Details = details;
            ViewBag.Header = header;
            ViewBag.Details = details;
            return View();
        }


        // ── GET Invoice ID from Payment ID ──────────────────────────────────
        [HttpGet]
        public JsonResult GetInvoiceByPaymentID(long PaymentID)
        {
            try
            {
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var cmd = new System.Data.SqlClient.SqlCommand(
                        "SELECT InvoiceID FROM InvoicePayments WHERE PaymentID = @ID", conn);
                    cmd.Parameters.AddWithValue("@ID", PaymentID);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Json(new { invoiceId = Convert.ToInt64(result) }, JsonRequestBehavior.AllowGet);
                }
            }
            catch { }
            return Json(new { invoiceId = (long?)null }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteInvoice(long invoiceId)
        {
            try
            {
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            new SqlCommand("DELETE FROM InvoicePayments WHERE InvoiceID=@ID", conn, tran) { Parameters = { new SqlParameter("@ID", invoiceId) } }.ExecuteNonQuery();
                            new SqlCommand("DELETE FROM InvoiceDetails  WHERE InvoiceID=@ID", conn, tran) { Parameters = { new SqlParameter("@ID", invoiceId) } }.ExecuteNonQuery();
                            new SqlCommand("DELETE FROM InvoiceHeaders  WHERE InvoiceID=@ID", conn, tran) { Parameters = { new SqlParameter("@ID", invoiceId) } }.ExecuteNonQuery();
                            tran.Commit();
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        private InvoiceHeaderModel GetInvoiceHeader(long invoiceId)
        {
            try
            {
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"SELECT h.*, (SELECT COUNT(*) FROM InvoiceDetails d WHERE d.InvoiceID=h.InvoiceID) AS TotalJobs, ISNULL((SELECT SUM(p.PaidAmount) FROM InvoicePayments p WHERE p.InvoiceID=h.InvoiceID),0) AS TotalPaid FROM InvoiceHeaders h WHERE h.InvoiceID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", invoiceId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            var h = MapHeader(r);
                            h.TotalPaid = r["TotalPaid"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalPaid"]);
                            return h;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private List<InvoiceDetailModel> GetInvoiceDetails(long invoiceId)
        {
            var list = new List<InvoiceDetailModel>();
            try
            {
                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT * FROM InvoiceDetails WHERE InvoiceID=@ID ORDER BY JobDate,JobTime", conn);
                    cmd.Parameters.AddWithValue("@ID", invoiceId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new InvoiceDetailModel
                            {
                                InvoiceDetailID = Convert.ToInt64(r["InvoiceDetailID"]),
                                InvoiceID = Convert.ToInt64(r["InvoiceID"]),
                                JobCode = Convert.ToInt64(r["JobCode"]),
                                JobDate = r["JobDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["JobDate"]),
                                JobTime = r["JobTime"] == DBNull.Value ? "" : r["JobTime"].ToString(),
                                JobFrom = r["JobFrom"] == DBNull.Value ? "" : r["JobFrom"].ToString(),
                                JobTo = r["JobTo"] == DBNull.Value ? "" : r["JobTo"].ToString(),
                                CustomerName = r["CustomerName"] == DBNull.Value ? "" : r["CustomerName"].ToString(),
                                VehicleName = r["VehicleName"] == DBNull.Value ? "" : r["VehicleName"].ToString(),
                                DrivingByName = r["DrivingByName"] == DBNull.Value ? "" : r["DrivingByName"].ToString(),
                                JobVendorName = r["JobVendorName"] == DBNull.Value ? "" : r["JobVendorName"].ToString(),
                                Credit = r["Credit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Credit"]),
                                Cash = r["Cash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Cash"]),
                                Amount = r["Amount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Amount"])
                            });
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        private InvoiceHeaderModel MapHeader(System.Data.SqlClient.SqlDataReader r)
        {
            Func<string, string> str = col => { try { return r[col] == DBNull.Value ? "" : r[col].ToString(); } catch { return ""; } };
            Func<string, decimal> dec = col => { try { return r[col] == DBNull.Value ? 0m : Convert.ToDecimal(r[col]); } catch { return 0m; } };
            Func<string, DateTime?> ndt = col => { try { return r[col] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[col]); } catch { return null; } };
            Func<string, bool> boo = col => { try { return r[col] != DBNull.Value && Convert.ToBoolean(r[col]); } catch { return false; } };
            Func<string, int> cnt = col => { try { return r[col] == DBNull.Value ? 0 : Convert.ToInt32(r[col]); } catch { return 0; } };

            return new InvoiceHeaderModel
            {
                InvoiceID = Convert.ToInt64(r["InvoiceID"]),
                InvoiceNo = str("InvoiceNo"),
                InvoiceDate = Convert.ToDateTime(r["InvoiceDate"]),
                CustomerName = str("CustomerName"),
                JobVendorName = str("JobVendorName"),
                DrivingByName = str("DrivingByName"),
                VehicleName = str("VehicleName"),
                StartDate = ndt("StartDate"),
                EndDate = ndt("EndDate"),
                CreditCash = str("CreditCash"),
                TotalAmount = dec("TotalAmount"),
                IsCredit = boo("IsCredit"),
                CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                TotalJobs = cnt("TotalJobs"),
                BillToName = str("BillToName"),
                BillToAddress = str("BillToAddress"),
                IsManual = boo("IsManual")
            };
        }

        private string InvDecimalToWords(decimal n)
        {
            if (n == 0) return "zero";
            if (n < 0) return "minus " + InvDecimalToWords(Math.Abs(n));
            int i = (int)n, d = (int)((n - i) * 100);
            string w = InvNumberToWords(i);
            if (d > 0) w += " and " + InvNumberToWords(d) + " cents";
            return w;
        }

        private string InvNumberToWords(int n)
        {
            if (n == 0) return "zero";
            if (n < 0) return "minus " + InvNumberToWords(Math.Abs(n));
            string w = "";
            if (n / 1000000 > 0) { w += InvNumberToWords(n / 1000000) + " million "; n %= 1000000; }
            if (n / 1000 > 0) { w += InvNumberToWords(n / 1000) + " thousand "; n %= 1000; }
            if (n / 100 > 0) { w += InvNumberToWords(n / 100) + " hundred "; n %= 100; }
            if (n > 0)
            {
                if (w != "") w += "and ";
                var u = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var t = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
                if (n < 20) w += u[n];
                else { w += t[n / 10]; if (n % 10 > 0) w += "-" + u[n % 10]; }
            }
            return w;
        }

        private string InvConvertToCamelCase(string s)
        {
            var words = s.Split(' ');
            for (int i = 1; i < words.Length; i++)
                if (words[i].Length > 0) words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            return string.Join(" ", words);
        }

        #endregion
    }
}