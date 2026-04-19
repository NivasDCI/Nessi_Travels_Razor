using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Transport.Entity;
using Transport.Model;
using Transport.Repository;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class JobController : Controller
    {
        #region "Private Variables"
        IJobRepository _objJobsRepository = new JobRepository();
        ICommonRepository ObjCommRepository = new CommonRepository();
        ISystemMasterRepository ObjSystemRepository = new SystemMasterRepository();
        IExpenseRepository _objExpensesRepository = new ExpenseRepository();
        private static Random random = new Random();
        #endregion

        #region "Helper Methods"
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string Encode(string encodeMe)
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(encodeMe);
            return Convert.ToBase64String(encoded);
        }

        public string ConvertToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] words = input.Split(' ');
            for (int i = 1; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
            return string.Join("", words);
        }

        private string NumberToWords(int number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";
            if ((number / 1000000) > 0) { words += NumberToWords(number / 1000000) + " Million "; number %= 1000000; }
            if ((number / 1000) > 0) { words += NumberToWords(number / 1000) + " Thousand "; number %= 1000; }
            if ((number / 100) > 0) { words += NumberToWords(number / 100) + " Hundred "; number %= 100; }
            if (number > 0)
            {
                if (words != "") words += "and ";
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
                if (number < 20) words += unitsMap[number];
                else { words += tensMap[number / 10]; if ((number % 10) > 0) words += "-" + unitsMap[number % 10]; }
            }
            return words;
        }

        public string DecimalToWords(decimal number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + DecimalToWords(Math.Abs(number));

            int intPortion = (int)number;
            int decPortion = (int)((number - intPortion) * 100);
            string words = NumberToWords(intPortion);
            if (decPortion > 0) { words += " and "; words += NumberToWords(decPortion); }
            return words;
        }
        #endregion

        #region "View Actions"
        public ActionResult ListJobs(string HeaderViewID, string DetailViewID)
        {
            if (Request.Params["JobDate"] != string.Empty) { string jobdate = Request.Params["JobDate"]; }
            return View();
        }
        public ActionResult AddJob(string JobCde, string HeaderViewID, string DetailViewID)
        {
            JobModel obj = new JobModel();
            return View(obj);
        }
        public ActionResult MyJobs(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult JobHistory(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult ListJobRequests(string HeaderViewID, string DetailViewID) { return View(); }
        public ActionResult CreditJobs(string HeaderViewID, string DetailViewID) { return View(); }
        #endregion

        #region "Invoice & Receipt"
        public ActionResult CustomerInvoiceAll(string CustomerName, string StartDate, string EndDate,
            int? JobVendorCode, int? DrivingBy, int? VehicleCode, string CreditCash, int? CashInHand)
        {
            try
            {
                int TotalCount = 0;
                DateTime? parsedStart = null;
                DateTime? parsedEnd = null;
                string[] fmts = new[] { "dd-MMM-yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };

                // FIX: declare variables separately before TryParseExact (C# 5/6 compatible)
                DateTime s;
                if (!string.IsNullOrEmpty(StartDate) &&
                    DateTime.TryParseExact(StartDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out s))
                    parsedStart = s;

                DateTime e;
                if (!string.IsNullOrEmpty(EndDate) &&
                    DateTime.TryParseExact(EndDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out e))
                    parsedEnd = e;

                if (parsedStart == null) parsedStart = DateTime.Now.Date;
                if (parsedEnd == null) parsedEnd = parsedStart;

                if (JobVendorCode == 0) JobVendorCode = null;
                if (DrivingBy == 0) DrivingBy = null;
                if (VehicleCode == 0) VehicleCode = null;
                if (CashInHand == 0) CashInHand = null;
                if (string.IsNullOrWhiteSpace(CustomerName)) CustomerName = null;

                List<JobModel> allJobs = _objJobsRepository.Job_FindAll(
                    1, parsedStart, parsedEnd, VehicleCode, JobVendorCode,
                    CashInHand, CustomerName, null, DrivingBy, 500, null, null, out TotalCount);

                if (!string.IsNullOrEmpty(CreditCash))
                {
                    if (CreditCash == "Credit") allJobs = allJobs.Where(o => (o.Credit ?? 0) > 0).ToList();
                    else if (CreditCash == "Cash") allJobs = allJobs.Where(o => (o.Cash ?? 0) > 0).ToList();
                }

                if (allJobs == null || allJobs.Count == 0)
                    return Content("<script>alert('No jobs found');window.close();</script>");

                decimal grossAmount = allJobs.Sum(o => (o.Credit ?? o.Cash ?? 0));
                ViewBag.CustomerName = allJobs.FirstOrDefault() != null ? allJobs.FirstOrDefault().CustomerName : "Guest";
                ViewBag.InvoiceNo = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                ViewBag.InvoiceDate = DateTime.Now.ToString("dd-MMM-yyyy");
                ViewBag.details = allJobs;
                ViewBag.GrossAmount = grossAmount;
                ViewBag.NetAmount = grossAmount;
                ViewBag.AmountinWords = ConvertToCamelCase(DecimalToWords(grossAmount));
                ViewBag.IsCredit = allJobs.Any(o => (o.Credit ?? 0) > 0);
                return View("Invoice");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        public ActionResult Invoice(long? JobCode)
        {
            try
            {
                JobModel model = _objJobsRepository.Job_Edit(JobCode);
                if (model == null) return Content("Job not found.");

                ViewBag.CustomerName = model.CustomerName;
                ViewBag.InvoiceNo = "NTT-T" + Shared.ToString(model.JobCode);
                ViewBag.InvoiceDate = DateTime.Now.ToString("MMM dd yyyy");

                List<JobModel> details = new List<JobModel>();
                details.Add(new JobModel { JobDate = model.JobDate, JobTime = model.JobTime, JobFrom = model.JobFrom, JobTo = model.JobTo, Credit = model.Credit });

                ViewBag.details = details;
                ViewBag.GrossAmount = model.Credit ?? model.Cash;
                ViewBag.NetAmount = model.Credit ?? model.Cash;
                ViewBag.AmountinWords = ConvertToCamelCase(DecimalToWords(Convert.ToDecimal(model.Credit ?? model.Cash ?? 0)));
                ViewBag.IsCredit = model.Credit.HasValue;
                return View();
            }
            catch (Exception ex) { return Content("Error: " + ex.Message); }
        }

        public ActionResult Receipt(long? JobCode)
        {
            try
            {
                JobModel model = _objJobsRepository.Job_Edit(JobCode);
                if (model == null) return Content("Job not found.");

                ViewBag.CustomerName = model.CustomerName;
                ViewBag.InvoiceNo = "NTT-T" + Shared.ToString(model.JobCode);
                ViewBag.InvoiceDate = DateTime.Now.ToString("MMM dd yyyy");
                ViewBag.JobDate = model.JobDate;
                ViewBag.JobTime = model.JobTime;
                ViewBag.JobFrom = model.JobFrom;
                ViewBag.JobTo = model.JobTo;
                ViewBag.GrossAmount = model.Credit ?? model.Cash;
                ViewBag.NetAmount = model.Credit ?? model.Cash;
                return View();
            }
            catch (Exception ex) { return Content("Error: " + ex.Message); }
        }

        public ActionResult CustomerInvoice(long? JobCode) { return View(); }
        #endregion

        #region "Job CRUD Operations"
        [HttpGet]
        public ActionResult Job_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, int? CashInHand, string CustomerName, string ContactNo, int? DrivingBy, string CreditCash, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            if (StartDate == null)
            {
                DateTime sessionDate;
                if (Session["NewJobDate"] != null && DateTime.TryParse(Session["NewJobDate"].ToString(), out sessionDate))
                    StartDate = sessionDate;
                else
                    StartDate = CommonRepository.GetTimeZoneDate();
            }
            if (EndDate == null) EndDate = StartDate;

            List<JobModel> Jobslist = _objJobsRepository.Job_FindAll(page, StartDate, EndDate, VehicleCode, JobVendorCode, CashInHand, CustomerName, ContactNo, DrivingBy, limit, sortBy, direction, out TotalCount);

            if (!string.IsNullOrEmpty(CreditCash))
            {
                if (CreditCash == "Credit") Jobslist = Jobslist.Where(o => o.Credit.HasValue && o.Credit > 0).ToList();
                else if (CreditCash == "Cash") Jobslist = Jobslist.Where(o => o.Cash.HasValue && o.Cash > 0).ToList();
            }
            return Json(new { records = Jobslist.Take(500), total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Job_Find(long? JobCode)
        {
            int TotalCount = 0;
            List<JobModel> Jobslist = _objJobsRepository.Job_FindAll(1, null, null, null, null, null, null, null, null, null, null, null, out TotalCount);
            JobModel model = Jobslist.Where(o => o.JobCode == JobCode).FirstOrDefault();
            if (model != null) return Json(new { success = true, response = model });
            else return Json(new { success = true, response = " " });
        }

        [HttpGet]
        public JsonResult Job_Edit(long? JobCode)
        {
            JobModel ObjMessage = _objJobsRepository.Job_Edit(JobCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetRequest(long? JobRequestCode)
        {
            JobRequestModel ObjMessage = _objJobsRepository.Job_RequestEdit(JobRequestCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Job_Save(JobModel objJob)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            objJob.CreatedBy = SessionExpire.GetUserID();
            ObjMessage = _objJobsRepository.Job_Save(objJob);
            Session["NewJobDate"] = objJob.JobDate;
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult JobStatus_Delete(long? JobStatusCode)
        {
            ReturnMessageModel ObjMessage = _objJobsRepository.JobStatus_Delete(JobStatusCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult JobStatus_FindAll(long? JobCode)
        {
            List<JobStatusModel> Jobslist = _objJobsRepository.JobStatus_FindAll(JobCode);
            return Json(new { records = Jobslist }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateJobStatus(long? JobCode, string Status)
        {
            try
            {
                ReturnMessageModel ObjMessage = _objJobsRepository.JobStatus_Update(JobCode, Status, SessionExpire.GetUserID());
                return Json(new { success = true, response = "" });
            }
            catch (Exception ex) { return Json(new { success = false, response = ex.Message }); }
        }

        [HttpPost]
        public ActionResult ReAssignJobStatus(long? JobCode)
        {
            try
            {
                ReturnMessageModel ObjMessage = _objJobsRepository.JobStatus_ReAssign(JobCode, SessionExpire.GetUserID());
                return Json(new { success = true, response = "" });
            }
            catch (Exception ex) { return Json(new { success = false, response = ex.Message }); }
        }

        [HttpPost]
        public ActionResult CancelJob(long? JobCode)
        {
            try
            {
                ReturnMessageModel ObjMessage = _objJobsRepository.JobStatus_Cancel(JobCode, SessionExpire.GetUserID());
                return Json(new { success = true, response = "" });
            }
            catch (Exception ex) { return Json(new { success = false, response = ex.Message }); }
        }
        #endregion

        #region "Job Requests"
        [HttpGet]
        public ActionResult JobRequest_FindAll(int? page, string CustomerName, string ContactNo, int? DrivingBy, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<JobRequestModel> Jobslist = _objJobsRepository.JobRequest_FindAll(page, CustomerName, ContactNo, limit, sortBy, direction, out TotalCount);
            return Json(new { records = Jobslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateRequestStatus(long? JobRequestCode, string JobStatus, decimal? Cost)
        {
            using (TransportEntities context = new TransportEntities())
            {
                try
                {
                    context.Database.ExecuteSqlCommand(
                        "UPDATE JobRequests SET JobRequestStatus = @status, Cost = @cost WHERE JobRequestCode = @code",
                        new System.Data.SqlClient.SqlParameter("@status", JobStatus),
                        new System.Data.SqlClient.SqlParameter("@cost", Cost.HasValue ? (object)Cost.Value : DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@code", JobRequestCode));
                    context.SaveChanges();
                    return Json(new { success = true, response = "" });
                }
                catch (Exception ex) { return Json(new { success = false, response = ex.Message }); }
            }
        }

        [HttpPost]
        public ActionResult IsRequestedCheck()
        {
            bool isRequested = _objJobsRepository.IsRequestedCheck();
            int _count = _objJobsRepository.IsRequestedCount();
            return Json(new { success = isRequested, count = _count }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "My Jobs & History"
        [HttpGet]
        public ActionResult MyJob_FindAll(int? page, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<MyJobModel> Jobslist = _objJobsRepository.MyJob_FindAll(page, limit, SessionExpire.GetUserID(), sortBy, direction, out TotalCount);
            return Json(new { records = Jobslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult JobHistory_FindAll(int? page, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<MyJobModel> Jobslist = _objJobsRepository.MyJob_FindAllHistory(page, limit, SessionExpire.GetUserID(), sortBy, direction, out TotalCount);
            return Json(new { records = Jobslist.Where(o => o.JobStatus == "Job Completed"), total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "Credit Jobs"
        [HttpGet]
        public ActionResult CreditJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<JobModel> Jobslist = _objJobsRepository.CreditJob_FindAll(page, StartDate, EndDate, VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, limit, sortBy, direction, out TotalCount);
            return Json(new { records = Jobslist, total = Jobslist.Count() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateCreditStatus(long? JobCode, int? CashInHand)
        {
            using (TransportEntities context = new TransportEntities())
            {
                try
                {
                    context.Database.ExecuteSqlCommand(
                        "UPDATE Jobs SET Cash = Credit, Credit = NULL, CashInHand = @cashInHand WHERE JobCode = @jobCode",
                        new System.Data.SqlClient.SqlParameter("@cashInHand", Shared.ToInt(CashInHand)),
                        new System.Data.SqlClient.SqlParameter("@jobCode", JobCode));
                    context.SaveChanges();
                    return Json(new { success = true, response = "" });
                }
                catch (Exception ex) { return Json(new { success = false, response = ex.Message }); }
            }
        }

        [HttpPost]
        public ActionResult UpdateCashInHand(long? JobCode, int? CashInHand)
        {
            using (TransportEntities context = new TransportEntities())
            {
                try
                {
                    context.Database.ExecuteSqlCommand(
                        "UPDATE Jobs SET CashInHand = @cashInHand WHERE JobCode = @jobCode",
                        new System.Data.SqlClient.SqlParameter("@cashInHand", Shared.ToInt(CashInHand)),
                        new System.Data.SqlClient.SqlParameter("@jobCode", JobCode));
                    context.SaveChanges();
                    return Json(new { success = true, response = "" });
                }
                catch (Exception ex) { return Json(new { success = false, response = ex.Message }); }
            }
        }
        #endregion

        #region "WhatsApp/SMS Links"
        [HttpPost]
        public ActionResult GenerateLinkContent(long? JobRequestCode, string RequestStatus)
        {
            using (TransportEntities context = new TransportEntities())
            {
                JobRequestModel ObjJobRequestModel = _objJobsRepository.Job_RequestEdit(JobRequestCode);
                string linkcontent = ""; string _number = "";
                if (RequestStatus == "Job Accepted")
                {
                    linkcontent += "Dear " + Shared.ToString(ObjJobRequestModel.CustomerName) + " ,";
                    linkcontent += Environment.NewLine;
                    linkcontent += "We have accepted your trip Request.";
                    if (ObjJobRequestModel.Cost != null) { linkcontent += Environment.NewLine; linkcontent += "The Price for your Trip is $" + Shared.ToString(ObjJobRequestModel.Cost); }
                    linkcontent += Environment.NewLine;
                    linkcontent += "We will send you the driver details Shortly!";
                    _number = Shared.ToString(ObjJobRequestModel.ContactNo);
                }
                else if (RequestStatus == "Not Available" || RequestStatus == "Cancelled")
                {
                    linkcontent += "Dear " + Shared.ToString(ObjJobRequestModel.CustomerName) + " ,";
                    linkcontent += Environment.NewLine;
                    linkcontent += "Sorry we cannot accept your trip Request!";
                    _number = Shared.ToString(ObjJobRequestModel.ContactNo);
                }
                return Json(new { data = linkcontent, number = _number });
            }
        }

        [HttpPost]
        public ActionResult GenerateLinkContentJob(long? JobCode, string JobStatus)
        {
            using (TransportEntities context = new TransportEntities())
            {
                JobModel ObjJobModel = _objJobsRepository.Job_Edit(JobCode);
                string linkcontent = ""; string _number = "";
                if (JobStatus == "Assigned")
                {
                    linkcontent += "Dear " + Shared.ToString(ObjJobModel.CustomerName) + " ,";
                    if (Shared.ToString(ObjJobModel.OutSource) != string.Empty)
                    {
                        linkcontent += Environment.NewLine + "Date : " + Shared.ToString(ObjJobModel.JobDate.HasValue ? ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy") : "");
                        linkcontent += Environment.NewLine + "Time : " + Shared.ToString(ObjJobModel.JobTime);
                        linkcontent += Environment.NewLine + "Pickup : " + Shared.ToString(ObjJobModel.JobFrom);
                        linkcontent += Environment.NewLine + "Dropoff : " + Shared.ToString(ObjJobModel.JobTo);
                        linkcontent += Environment.NewLine + "Booking Status : Confirmed";
                        if (Shared.IsDecimal(ObjJobModel.Cash)) { linkcontent += Environment.NewLine + "Price : $" + Shared.ToString(ObjJobModel.Cash); }
                        linkcontent += Environment.NewLine + "Driver Name : " + Shared.ToString(ObjJobModel.OutSource);
                        linkcontent += Environment.NewLine + "Contact No : ";
                    }
                    if (Shared.ToInt(ObjJobModel.DrivingBy) != 0)
                    {
                        UserMasterModel ObjUserModel = ObjSystemRepository.UserMaster_Edit(ObjJobModel.DrivingBy.Value);
                        linkcontent += Environment.NewLine + "Date : " + Shared.ToString(ObjJobModel.JobDate.HasValue ? ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy") : "");
                        linkcontent += Environment.NewLine + "Time : " + Shared.ToString(ObjJobModel.JobTime);
                        linkcontent += Environment.NewLine + "Pickup : " + Shared.ToString(ObjJobModel.JobFrom);
                        linkcontent += Environment.NewLine + "Dropoff : " + Shared.ToString(ObjJobModel.JobTo);
                        linkcontent += Environment.NewLine + "Booking Status : Confirmed";
                        if (Shared.IsDecimal(ObjJobModel.Cash)) { linkcontent += Environment.NewLine + "Price : $" + Shared.ToString(ObjJobModel.Cash); }
                        linkcontent += Environment.NewLine + "Driver Name : " + Shared.ToString(ObjUserModel.FirstName);
                        linkcontent += Environment.NewLine + "Contact No : " + Shared.ToString(ObjUserModel.MobileNumber);
                    }
                    _number = Shared.ToString(ObjJobModel.ContactNo);
                }
                else if (JobStatus == "Cancelled") { linkcontent += "Dear " + Shared.ToString(ObjJobModel.CustomerName) + " ,"; linkcontent += Environment.NewLine + "Sorry we cannot accept your trip Request!"; _number = Shared.ToString(ObjJobModel.ContactNo); }
                else if (JobStatus == "Job Completed") { linkcontent += "Dear " + Shared.ToString(ObjJobModel.CustomerName) + " ,"; linkcontent += Environment.NewLine + "Thank you for choosing Nissi Travels! Have a safe Journey!"; _number = Shared.ToString(ObjJobModel.ContactNo); }
                return Json(new { data = linkcontent, number = _number });
            }
        }

        [HttpPost]
        public ActionResult GenerateLinkContentMyJob(long? JobCode, string JobStatus)
        {
            using (TransportEntities context = new TransportEntities())
            {
                JobModel ObjJobModel = _objJobsRepository.Job_Edit(JobCode);
                string linkcontent = ""; string _number = "+6562920521";
                linkcontent += "Date : " + Shared.ToString(ObjJobModel.JobDate.HasValue ? ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy") : "") + Environment.NewLine;
                linkcontent += "Time : " + Shared.ToString(ObjJobModel.JobTime) + Environment.NewLine;
                linkcontent += "Pickup : " + Shared.ToString(ObjJobModel.JobFrom) + Environment.NewLine;
                linkcontent += "Dropoff : " + Shared.ToString(ObjJobModel.JobTo) + Environment.NewLine;
                linkcontent += "Customer Name : " + Shared.ToString(ObjJobModel.CustomerName) + Environment.NewLine;
                linkcontent += "Phone Number : " + Shared.ToString(ObjJobModel.ContactNo) + Environment.NewLine;
                linkcontent += "Job Given By : " + Shared.ToString(ObjJobModel.JobVendorName) + Environment.NewLine;
                return Json(new { data = linkcontent, number = _number });
            }
        }

        [HttpPost]
        public ActionResult GenerateOutSourceLink(long? JobCode)
        {
            string linkcontent = "";
            JobModel ObjJobModel = _objJobsRepository.Job_Edit(JobCode);
            linkcontent += "Dear " + ObjJobModel.OutSource + ", " + Environment.NewLine;
            linkcontent += "You have received a Job Request!" + Environment.NewLine;
            linkcontent += "Click the link to Start your Job!" + Environment.NewLine;
            string url = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/Login/Outsource?JobCode=" + Encode(Shared.ToString(JobCode));
            linkcontent += url;
            return Json(new { data = linkcontent });
        }
        #endregion

        #region "Transactions & Calculations"
        [HttpGet]
        public JsonResult CalcStaffTransactions(DateTime? StartDate, DateTime? EndDate)
        {
            int TotalCount = 0;
            List<JobModel> Jobslist = _objJobsRepository.Job_FindAll(1, StartDate, EndDate, null, null, SessionExpire.GetUserID(), null, null, null, null, null, null, out TotalCount);
            List<ExpenseModel> MyExpenseslist = _objExpensesRepository.MyExpense_FindAll(1, null, null, SessionExpire.GetUserID(), StartDate, EndDate, null, out TotalCount);
            var data = new { CashInHand = Jobslist.Select(o => o.Cash).Sum(), Expense = MyExpenseslist.Select(o => o.Charge).Sum() };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CalcTransactions(DateTime? StartDate, DateTime? EndDate, int? PaymentBy)
        {
            int TotalCount = 0;
            List<JobModel> Jobslist = _objJobsRepository.Job_FindAll(1, StartDate, EndDate, null, null, PaymentBy, null, null, null, null, null, null, out TotalCount);
            List<ExpenseModel> MyExpenseslist = _objExpensesRepository.MyExpense_FindAll(1, null, null, PaymentBy, StartDate, EndDate, null, out TotalCount);
            List<ExpenseModel> Expenseslist = _objExpensesRepository.Expense_FindAll(1, null, null, StartDate, EndDate, null, out TotalCount);
            var _otherexpense = Expenseslist.Where(o => o.PaymentBy == PaymentBy).ToList();
            var data = new { CashInHand = Jobslist.Select(o => o.Cash).Sum(), Expense = MyExpenseslist.Select(o => o.Charge).Sum() + _otherexpense.Select(o => o.Charge).Sum() };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}