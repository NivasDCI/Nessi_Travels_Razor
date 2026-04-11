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
            
            return Json(Dashboardlist , JsonRequestBehavior.AllowGet);
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

                dsQuery =  FetchRS(CommandType.StoredProcedure, "sp_frm_get_Transaction_Report", sqlParams);
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

            if (StartDate == null) { StartDate = firstDayOfMonth; } if (EndDate == null) { EndDate = DateTime.Now; }
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
                    Cost = o.Cost.HasValue? o.Cost : 0,
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
            List <TotalReportModel> Totallist = _objReportsRepository.TotalReport(StartDate, EndDate);
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

    }
}