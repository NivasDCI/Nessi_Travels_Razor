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
        public ActionResult OverallReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        public ActionResult AccountsReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult MyAccountsReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult TransactionsReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult StaffAccountsReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult VehiclesReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }


        [HttpGet]
        public ActionResult Dashboard_FindAll()
        {
            DashboardModel Dashboardlist = _objReportsRepository.Dashboard_FindAll();

            return Json(Dashboardlist, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Jobs"
        public ActionResult JobsReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult MyJobsReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
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
            // int? UserID = SessionExpire.GetUserID();

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
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "ServiceName":
                            trxnList = trxnList.OrderBy(q => q.ServiceName).ToList();
                            break;
                    }
                }
                else
                {
                    // step 7 applying sorting desc

                    switch (sortBy.Trim())
                    {
                        case "ServiceName":
                            trxnList = trxnList.OrderByDescending(q => q.ServiceName).ToList();
                            break;
                    }
                }
            }
            else
            {
                //ObjTickets = ObjTickets.TicketByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = trxnList.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                trxnList = trxnList.Skip(start).Take(limit.Value).ToList();
            }

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
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "ServiceName":
                            trxnList = trxnList.OrderBy(q => q.ServiceName).ToList();
                            break;
                    }
                }
                else
                {
                    // step 7 applying sorting desc

                    switch (sortBy.Trim())
                    {
                        case "ServiceName":
                            trxnList = trxnList.OrderByDescending(q => q.ServiceName).ToList();
                            break;
                    }
                }
            }
            else
            {
                //ObjTickets = ObjTickets.TicketByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = trxnList.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                trxnList = trxnList.Skip(start).Take(limit.Value).ToList();
            }

            return Json(new { records = trxnList, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        private DataSet ReturnTrxnDetails(int? UserID, DateTime? TransactionFrom, DateTime? TransactionTo)
        {
            DataSet dsQuery = new DataSet();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[3];
                sqlParams[0] = new SqlParameter("@UserID", SqlDbType.Int);
                sqlParams[0].Value = UserID;
                sqlParams[1] = new SqlParameter("@TransactionFrom", SqlDbType.DateTime);
                if (TransactionFrom != null)
                    sqlParams[1].Value = TransactionFrom;
                else
                    sqlParams[1].Value = DBNull.Value;
                sqlParams[2] = new SqlParameter("@TransactionTo", SqlDbType.DateTime);
                if (TransactionFrom != null)
                    sqlParams[2].Value = TransactionTo;
                else
                    sqlParams[2].Value = DBNull.Value;

                dsQuery = FetchRS(CommandType.StoredProcedure, "sp_frm_get_Transaction_Report", sqlParams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : ex.Message);
            }

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
                    while (sr.Peek() >= 0)
                    {
                        connectionvalue = sr.ReadLine();
                    }
                }

                //conn.ConnectionString = ToString(ConfigurationManager.ConnectionStrings["RangoonAirEntities"]);
                conn.ConnectionString = connectionvalue;
                SqlCommand cmd = new SqlCommand();

                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();

                try
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = cmdText;
                    cmd.CommandType = cmdType;	//CommandType.Text 

                    if (cmdParams != null)
                    {
                        foreach (SqlParameter param in cmdParams)
                            cmd.Parameters.Add(param);
                    }

                    da.SelectCommand = cmd;
                    da.Fill(ds);

                    cmd.Parameters.Clear();
                    return ds;
                }
                catch
                {
                    conn.Close();
                    throw;
                }
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
                    //Cost = o.Cost.HasValue ? String.Format("{0:N2}", o.Cost) : "0.00",
                    //Credit = o.Credit.HasValue ? String.Format("{0:N2}", o.Credit) : string.Empty,
                    //Cash = o.Cash.HasValue ? String.Format("{0:N2}", o.Cash) : string.Empty,
                    //Cost = o.Cost.HasValue ? String.Format("{0:C}", o.Cost) : string.Empty,
                    //Credit = o.Credit.HasValue ? String.Format("{0:C}", o.Credit) : string.Empty,
                    //Cash = o.Cash.HasValue ? String.Format("{0:C}", o.Cash) : string.Empty,
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
            catch (Exception ex)
            { }

            return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "JobsReport_" + DateTime.Now.ToString() + ".xlsx"
            };
        }

        [HttpGet]
        public ActionResult TotalReport(DateTime? StartDate, DateTime? EndDate)
        {
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<TotalReportModel> Totallist = _objReportsRepository.TotalReport(StartDate, EndDate);
            return Json(new { records = Totallist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult VehicleReport(DateTime? StartDate, DateTime? EndDate)
        {
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<VehicleReportModel> Vehiclelist = _objReportsRepository.VehicleReport(StartDate, EndDate);
            return Json(new { records = Vehiclelist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CashInHandReport(DateTime? StartDate, DateTime? EndDate)
        {
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<CashInHandReportModel> CashInHandlist = _objReportsRepository.CashInHandReport(StartDate, EndDate);
            return Json(new { records = CashInHandlist }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult CreditReport(DateTime? StartDate, DateTime? EndDate)
        {
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<CreditReportModel> Creditlist = _objReportsRepository.CreditReport(StartDate, EndDate);
            return Json(new { records = Creditlist }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult CreditCustomersReport(DateTime? StartDate, DateTime? EndDate)
        {
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<CreditCustomersReportModel> CreditCustomerslist = _objReportsRepository.CreditCustomersReport(StartDate, EndDate);
            return Json(new { records = CreditCustomerslist }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult VehicleTotalReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate)
        {
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<TotalReportModel> Totallist = _objReportsRepository.VehicleTotalReport(VehicleCode, StartDate, EndDate);
            return Json(new { records = Totallist }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult VehicleExpenseReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate)
        {
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<ExpenseModel> Totallist = _objReportsRepository.VehicleExpenseReport(VehicleCode, StartDate, EndDate);
            return Json(new { records = Totallist }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OutSourceJobsReport(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

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
                    //Cost = o.Cost.HasValue ? String.Format("{0:N2}", o.Cost) : "0.00",
                    //Credit = o.Credit.HasValue ? String.Format("{0:N2}", o.Credit) : string.Empty,
                    //Cash = o.Cash.HasValue ? String.Format("{0:N2}", o.Cash) : string.Empty,
                    //Cost = o.Cost.HasValue ? String.Format("{0:C}", o.Cost) : string.Empty,
                    //Credit = o.Credit.HasValue ? String.Format("{0:C}", o.Credit) : string.Empty,
                    //Cash = o.Cash.HasValue ? String.Format("{0:C}", o.Cash) : string.Empty,
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
            catch (Exception ex)
            { }

            return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "JobsReport_" + DateTime.Now.ToString() + ".xlsx"
            };
        }

        #endregion

        // =====================================================================
        // INVOICE ACTIONS
        // =====================================================================
        #region "Invoice"

        private SqlConnection GetRawConnection()
        {
            // Extract raw SQL connection string from Web.config EF connection string
            // Same connection Entity Framework uses - guaranteed to work
            string efConnStr = System.Configuration.ConfigurationManager
                                   .ConnectionStrings["TransportEntities"].ConnectionString;

            // EF stores the SQL connection inside: provider connection string="..."
            // In Web.config it is HTML-encoded as &quot; so we handle both forms
            string marker = "provider connection string=";
            int markerIdx = efConnStr.IndexOf(marker, System.StringComparison.OrdinalIgnoreCase);
            string rawSqlConnStr = string.Empty;

            if (markerIdx >= 0)
            {
                // Move past the marker and the opening quote character
                int openQuote = efConnStr.IndexOf('"', markerIdx) + 1;
                int closeQuote = efConnStr.IndexOf('"', openQuote);
                rawSqlConnStr = efConnStr.Substring(openQuote, closeQuote - openQuote);
            }

            // If the Web.config value was HTML-encoded (e.g. &quot; instead of ")
            // the IndexOf above won't find quotes - fall back to &quot; delimiters
            if (string.IsNullOrEmpty(rawSqlConnStr))
            {
                string marker2 = "provider connection string=&quot;";
                int m2 = efConnStr.IndexOf(marker2, System.StringComparison.OrdinalIgnoreCase);
                if (m2 >= 0)
                {
                    int open2 = m2 + marker2.Length;
                    int close2 = efConnStr.IndexOf("&quot;", open2, System.StringComparison.OrdinalIgnoreCase);
                    rawSqlConnStr = efConnStr.Substring(open2, close2 - open2);
                }
            }

            return new SqlConnection(rawSqlConnStr);
        }

        private string GenerateInvoiceNo(SqlConnection conn, SqlTransaction tran)
        {
            var cmd = new SqlCommand(
                "SELECT 'NTT-INV-' + RIGHT('00000' + CAST(ISNULL(MAX(InvoiceID),0)+1 AS VARCHAR),5) FROM InvoiceHeaders",
                conn, tran);
            object result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : "NTT-INV-00001";
        }

        public ActionResult InvoiceList(string HeaderViewID, string DetailViewID) { return View(); }

        public JsonResult InvoiceTest()
        {
            return Json(new { ok = true, message = "Invoice actions working!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Invoice_FindAll(string CustomerName, string InvoiceNo, string FromDate, string ToDate)
        {
            var list = new List<InvoiceHeaderModel>();
            try
            {
                DateTime? parsedFrom = null, parsedTo = null;
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                DateTime dtmp;
                if (!string.IsNullOrEmpty(FromDate) && DateTime.TryParseExact(FromDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtmp)) parsedFrom = dtmp;
                if (!string.IsNullOrEmpty(ToDate) && DateTime.TryParseExact(ToDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtmp)) parsedTo = dtmp;

                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        SELECT h.*, (SELECT COUNT(*) FROM InvoiceDetails d WHERE d.InvoiceID=h.InvoiceID) AS TotalJobs
                        FROM InvoiceHeaders h
                        WHERE (@CustomerName IS NULL OR h.CustomerName LIKE '%'+@CustomerName+'%')
                          AND (@InvoiceNo    IS NULL OR h.InvoiceNo    LIKE '%'+@InvoiceNo+'%')
                          AND (@FromDate     IS NULL OR CAST(h.InvoiceDate AS DATE) >= CAST(@FromDate AS DATE))
                          AND (@ToDate       IS NULL OR CAST(h.InvoiceDate AS DATE) <= CAST(@ToDate   AS DATE))
                        ORDER BY h.InvoiceID DESC", conn);
                    cmd.Parameters.AddWithValue("@CustomerName", string.IsNullOrEmpty(CustomerName) ? (object)DBNull.Value : CustomerName);
                    cmd.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(InvoiceNo) ? (object)DBNull.Value : InvoiceNo);
                    cmd.Parameters.AddWithValue("@FromDate", parsedFrom.HasValue ? (object)parsedFrom.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", parsedTo.HasValue ? (object)parsedTo.Value : DBNull.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new InvoiceHeaderModel
                            {
                                InvoiceID = Convert.ToInt64(reader["InvoiceID"]),
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]),
                                CustomerName = reader["CustomerName"] == DBNull.Value ? "" : reader["CustomerName"].ToString(),
                                JobVendorName = reader["JobVendorName"] == DBNull.Value ? "" : reader["JobVendorName"].ToString(),
                                DrivingByName = reader["DrivingByName"] == DBNull.Value ? "" : reader["DrivingByName"].ToString(),
                                VehicleName = reader["VehicleName"] == DBNull.Value ? "" : reader["VehicleName"].ToString(),
                                StartDate = reader["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["StartDate"]),
                                EndDate = reader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndDate"]),
                                CreditCash = reader["CreditCash"] == DBNull.Value ? "" : reader["CreditCash"].ToString(),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                IsCredit = Convert.ToBoolean(reader["IsCredit"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                TotalJobs = reader["TotalJobs"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalJobs"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet); }
            return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GenerateInvoice(string CustomerName, string StartDate, string EndDate,
            int? JobVendorCode, int? DrivingBy, int? VehicleCode, string CreditCash, int? CashInHand)
        {
            try
            {
                DateTime? parsedStart = null, parsedEnd = null;
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                DateTime dtmp;
                if (!string.IsNullOrEmpty(StartDate) && DateTime.TryParseExact(StartDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtmp)) parsedStart = dtmp;
                if (!string.IsNullOrEmpty(EndDate) && DateTime.TryParseExact(EndDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtmp)) parsedEnd = dtmp;
                if (parsedStart == null) parsedStart = CommonRepository.GetTimeZoneDate();
                if (parsedEnd == null) parsedEnd = CommonRepository.GetTimeZoneDate();

                if (JobVendorCode == 0) JobVendorCode = null;
                if (DrivingBy == 0) DrivingBy = null;
                if (VehicleCode == 0) VehicleCode = null;
                if (CashInHand == 0) CashInHand = null;
                if (string.IsNullOrWhiteSpace(CustomerName)) CustomerName = null;

                int totalCount = 0;
                var jobs = _objReportsRepository.Job_FindAll(1, parsedStart, parsedEnd, VehicleCode, JobVendorCode,
                    CustomerName, null, DrivingBy, CashInHand, 500, null, null, out totalCount);

                if (!string.IsNullOrEmpty(CreditCash))
                {
                    if (CreditCash == "Credit") jobs = jobs.Where(o => o.Credit.HasValue && o.Credit > 0).ToList();
                    else if (CreditCash == "Cash") jobs = jobs.Where(o => o.Cash.HasValue && o.Cash > 0).ToList();
                }

                if (jobs == null || !jobs.Any())
                    return Json(new { success = false, message = "No jobs found for the selected filters." });

                var first = jobs.First();
                string invCustomer = string.IsNullOrEmpty(CustomerName) ? first.CustomerName : CustomerName;
                bool isCredit = jobs.Any(o => o.Credit.HasValue && o.Credit > 0);
                decimal totalAmount = jobs.Sum(o => o.Credit ?? o.Cash ?? 0);
                int createdBy = SessionExpire.GetUserID();

                long invoiceId = 0;

                using (var conn = GetRawConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            string invoiceNo = GenerateInvoiceNo(conn, tran);

                            var hCmd = new SqlCommand(@"
                                INSERT INTO InvoiceHeaders
                                    (InvoiceNo,InvoiceDate,CustomerName,JobVendorCode,JobVendorName,
                                     DrivingBy,DrivingByName,VehicleCode,VehicleName,CashInHand,CashInHandName,
                                     StartDate,EndDate,CreditCash,TotalAmount,IsCredit,CreatedBy,CreatedDate)
                                VALUES
                                    (@InvoiceNo,@InvoiceDate,@CustomerName,@JobVendorCode,@JobVendorName,
                                     @DrivingBy,@DrivingByName,@VehicleCode,@VehicleName,@CashInHand,@CashInHandName,
                                     @StartDate,@EndDate,@CreditCash,@TotalAmount,@IsCredit,@CreatedBy,GETDATE());
                                SELECT SCOPE_IDENTITY();", conn, tran);

                            hCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                            hCmd.Parameters.AddWithValue("@InvoiceDate", DateTime.Now);
                            hCmd.Parameters.AddWithValue("@CustomerName", (object)invCustomer ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@JobVendorCode", (object)JobVendorCode ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@JobVendorName", (object)first.JobVendorName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@DrivingBy", (object)DrivingBy ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@DrivingByName", (object)first.DrivingByName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@VehicleCode", (object)VehicleCode ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@VehicleName", (object)first.VehicleName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@CashInHand", (object)CashInHand ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@CashInHandName", (object)first.CashInHandName ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@StartDate", (object)parsedStart ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@EndDate", (object)parsedEnd ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@CreditCash", (object)CreditCash ?? DBNull.Value);
                            hCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            hCmd.Parameters.AddWithValue("@IsCredit", isCredit);
                            hCmd.Parameters.AddWithValue("@CreatedBy", (object)createdBy);

                            invoiceId = Convert.ToInt64(hCmd.ExecuteScalar());

                            foreach (var job in jobs)
                            {
                                var dCmd = new SqlCommand(@"
                                    INSERT INTO InvoiceDetails
                                        (InvoiceID,JobCode,JobDate,JobTime,JobFrom,JobTo,
                                         CustomerName,VehicleName,DrivingByName,JobVendorName,Credit,Cash,Amount)
                                    VALUES
                                        (@InvoiceID,@JobCode,@JobDate,@JobTime,@JobFrom,@JobTo,
                                         @CustomerName,@VehicleName,@DrivingByName,@JobVendorName,@Credit,@Cash,@Amount)",
                                    conn, tran);
                                dCmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                dCmd.Parameters.AddWithValue("@JobCode", job.JobCode);
                                dCmd.Parameters.AddWithValue("@JobDate", (object)job.JobDate ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JobTime", (object)job.JobTime ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JobFrom", (object)job.JobFrom ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JobTo", (object)job.JobTo ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@CustomerName", (object)job.CustomerName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@VehicleName", (object)job.VehicleName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@DrivingByName", (object)job.DrivingByName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@JobVendorName", (object)job.JobVendorName ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@Credit", (object)job.Credit ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@Cash", (object)job.Cash ?? DBNull.Value);
                                dCmd.Parameters.AddWithValue("@Amount", (object)(job.Credit ?? job.Cash) ?? DBNull.Value);
                                dCmd.ExecuteNonQuery();
                            }

                            tran.Commit();
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }

                return Json(new { success = true, invoiceId = invoiceId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult ViewInvoice(long invoiceId)
        {
            var header = GetInvoiceHeader(invoiceId);
            var details = GetInvoiceDetails(invoiceId);
            if (header == null) return Content("Invoice not found.");
            header.Details = details;
            decimal gross = details.Sum(d => d.Amount ?? 0);
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
                            var c1 = new SqlCommand("DELETE FROM InvoiceDetails WHERE InvoiceID=@ID", conn, tran);
                            c1.Parameters.AddWithValue("@ID", invoiceId); c1.ExecuteNonQuery();
                            var c2 = new SqlCommand("DELETE FROM InvoiceHeaders WHERE InvoiceID=@ID", conn, tran);
                            c2.Parameters.AddWithValue("@ID", invoiceId); c2.ExecuteNonQuery();
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
                    var cmd = new SqlCommand(@"
                        SELECT h.*, (SELECT COUNT(*) FROM InvoiceDetails d WHERE d.InvoiceID=h.InvoiceID) AS TotalJobs
                        FROM InvoiceHeaders h WHERE h.InvoiceID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", invoiceId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new InvoiceHeaderModel
                            {
                                InvoiceID = Convert.ToInt64(r["InvoiceID"]),
                                InvoiceNo = r["InvoiceNo"].ToString(),
                                InvoiceDate = Convert.ToDateTime(r["InvoiceDate"]),
                                CustomerName = r["CustomerName"] == DBNull.Value ? "" : r["CustomerName"].ToString(),
                                JobVendorName = r["JobVendorName"] == DBNull.Value ? "" : r["JobVendorName"].ToString(),
                                DrivingByName = r["DrivingByName"] == DBNull.Value ? "" : r["DrivingByName"].ToString(),
                                VehicleName = r["VehicleName"] == DBNull.Value ? "" : r["VehicleName"].ToString(),
                                StartDate = r["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["StartDate"]),
                                EndDate = r["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["EndDate"]),
                                CreditCash = r["CreditCash"] == DBNull.Value ? "" : r["CreditCash"].ToString(),
                                TotalAmount = Convert.ToDecimal(r["TotalAmount"]),
                                IsCredit = Convert.ToBoolean(r["IsCredit"]),
                                CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                                TotalJobs = r["TotalJobs"] == DBNull.Value ? 0 : Convert.ToInt32(r["TotalJobs"])
                            };
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
                                Amount = r["Amount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Amount"]),
                            });
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        private string InvDecimalToWords(decimal number)
        {
            if (number == 0) return "zero";
            if (number < 0) return "minus " + InvDecimalToWords(Math.Abs(number));
            int intPortion = (int)number; int decPortion = (int)((number - intPortion) * 100);
            string words = InvNumberToWords(intPortion);
            if (decPortion > 0) words += " and " + InvNumberToWords(decPortion) + " cents";
            return words;
        }

        private string InvNumberToWords(int number)
        {
            if (number == 0) return "zero"; if (number < 0) return "minus " + InvNumberToWords(Math.Abs(number));
            string words = "";
            if ((number / 1000000) > 0) { words += InvNumberToWords(number / 1000000) + " million "; number %= 1000000; }
            if ((number / 1000) > 0) { words += InvNumberToWords(number / 1000) + " thousand "; number %= 1000; }
            if ((number / 100) > 0) { words += InvNumberToWords(number / 100) + " hundred "; number %= 100; }
            if (number > 0)
            {
                if (words != "") words += "and ";
                var u = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var t = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
                if (number < 20) words += u[number]; else { words += t[number / 10]; if ((number % 10) > 0) words += "-" + u[number % 10]; }
            }
            return words;
        }

        private string InvConvertToCamelCase(string input)
        {
            var words = input.Split(' ');
            for (int i = 1; i < words.Length; i++) if (words[i].Length > 0) words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            return string.Join(" ", words);
        }

        #endregion
    }
}