using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Transport.Entity;
using Transport.Model;
using Transport.Repository;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class JobController : Controller
    {
        IJobRepository _objJobsRepository = new JobRepository();
        ICommonRepository ObjCommRepository = new CommonRepository();
        ISystemMasterRepository ObjSystemRepository = new SystemMasterRepository();
        IExpenseRepository _objExpensesRepository = new ExpenseRepository();

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
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
        public ActionResult MyJobs(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult JobHistory(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult ListJobRequests(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult CreditJobs(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult Invoice(long? JobCode)
        {
            JobModel model = _objJobsRepository.Job_Edit(JobCode);
            //CustomersMasterModel vendor = ObjMasterRepository.CustomersMaster_Edit(Convert.ToInt32(model.CustomerCode));

            ViewBag.CustomerName = model.CustomerName;
            ViewBag.InvoiceNo = "NTT-T" + Shared.ToString(model.JobCode);
            ViewBag.InvoiceDate = DateTime.Now.ToString("MMM dd yyyy");

            List<JobModel> details = new List<JobModel>();
            details.Add(new JobModel { JobDate = model.JobDate, JobTime = model.JobTime, JobFrom = model.JobFrom, JobTo = model.JobTo, Credit = model.Credit });

            ViewBag.details = details;

            ViewBag.GrossAmount = model.Credit ?? model.Cash;
            ViewBag.NetAmount = model.Credit ?? model.Cash;
            ViewBag.AmountinWords = ConvertToCamelCase(DecimalToWords(Convert.ToDecimal(model.Credit ?? model.Cash)));
            ViewBag.IsCredit = model.Credit.HasValue ? true : false;
            return View();

        }
        public ActionResult Receipt(long? JobCode)
        {
            JobModel model = _objJobsRepository.Job_Edit(JobCode);

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
        #region "Job"

        [HttpGet]
        public ActionResult Job_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, int? CashInHand, string CustomerName, string ContactNo, int? DrivingBy, string CreditCash, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            //if (StartDate == null) { StartDate = DateTime.Now; }
            if (StartDate == null) { StartDate = Shared.ToString(Session["NewJobDate"]).Length > 0 ? DateTime.Parse(Shared.ToString(Session["NewJobDate"])) : StartDate = CommonRepository.GetTimeZoneDate(); }
            if (EndDate == null) { EndDate = Shared.ToString(Session["NewJobDate"]).Length > 0 ? DateTime.Parse(Shared.ToString(Session["NewJobDate"])) : EndDate = CommonRepository.GetTimeZoneDate(); }
            List<JobModel> Jobslist = _objJobsRepository.Job_FindAll(page, StartDate, EndDate, VehicleCode, JobVendorCode, CashInHand, CustomerName, ContactNo, DrivingBy, limit, sortBy, direction, out TotalCount);
            if (!string.IsNullOrEmpty(CreditCash))
            {
                if (CreditCash == "Credit")
                    Jobslist = Jobslist.Where(o => o.Credit.HasValue).ToList();
                else if (CreditCash == "Cash")
                    Jobslist = Jobslist.Where(o => o.Cash.HasValue).ToList();
            }
            return Json(new { records = Jobslist.Take(500), total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Job_Find(long? JobCode)
        {
            int TotalCount = 0;
            List<JobModel> Jobslist = _objJobsRepository.Job_FindAll(1, null, null, null, null, null, null, null, null, null, null, null, out TotalCount);
            JobModel model = Jobslist.Where(o => o.JobCode == JobCode).FirstOrDefault();
            if (model != null)
            {
                return Json(new { success = true, response = model });
            }
            else
                return Json(new { success = true, response = " " });

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
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjMessage = _objJobsRepository.JobStatus_Delete(JobStatusCode);
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
                ReturnMessageModel ObjMessage = new ReturnMessageModel();

                ObjMessage = _objJobsRepository.JobStatus_Update(JobCode, Status, SessionExpire.GetUserID());
                return Json(new { success = true, response = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, response = ex.Message });
            }

        }
        [HttpPost]
        public ActionResult ReAssignJobStatus(long? JobCode)
        {
            try
            {
                ReturnMessageModel ObjMessage = new ReturnMessageModel();

                ObjMessage = _objJobsRepository.JobStatus_ReAssign(JobCode, SessionExpire.GetUserID());
                return Json(new { success = true, response = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, response = ex.Message });
            }

        }
        [HttpPost]
        public ActionResult CancelJob(long? JobCode)
        {
            try
            {
                ReturnMessageModel ObjMessage = new ReturnMessageModel();

                ObjMessage = _objJobsRepository.JobStatus_Cancel(JobCode, SessionExpire.GetUserID());
                return Json(new { success = true, response = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, response = ex.Message });
            }

        }
        public ActionResult CustomerInvoice(long? JobCode)
        {
            //JobModel model = _objJobsRepository.Job_Edit(JobCode);

            //List<EJobPassengerModel> passengerlist = _objJobsRepository.EJobPassenger_FindAll(JobCode);

            //List<EJobItineryModel> itinerylist = _objJobsRepository.EJobItinery_FindAll(JobCode);

            //ViewBag.CustomerName = model.Prefix + model.CustomerName;
            //ViewBag.AirlinePNR = model.PNRNumber;
            //ViewBag.BookingID = model.BookingID;

            //ViewBag.Passengers = passengerlist;

            //var itiList = itinerylist.AsEnumerable().Select(dataRow => new EJobItineryModel
            //{
            //    FlightNo = dataRow.FlightNo,
            //    Depart = dataRow.Depart + "</br>" + string.Format("{0:ddd, MMM d, yyyy}", dataRow.DepartDate) + "</br>" + dataRow.DepartTime + "</br>" + " - " + dataRow.DepartTer,
            //    DisplayDepart = dataRow.DisplayDepart,
            //    DepartTime = dataRow.DepartTime,
            //    Arrive = dataRow.Arrive + "</br>" + string.Format("{0:ddd, MMM d, yyyy}", dataRow.ArriveDate) + "</br>" + dataRow.ArriveTime + "</br>" + " - " + dataRow.ArriveTer,
            //    DisplayArrive = dataRow.DisplayArrive,
            //    ArriveTime = dataRow.ArriveTime,
            //    Class = dataRow.Class,
            //    Baggage = dataRow.Baggage

            //}).ToList();

            //ViewBag.Itineries = itiList;
            //ViewBag.Flight = itinerylist.AsEnumerable().Select(a => a.FlightName).FirstOrDefault();
            //ViewBag.AirlineCode = model.AirlineCode;
            return View();

        }
        public string DecimalToWords(decimal number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + DecimalToWords(Math.Abs(number));

            string words = "";

            int intPortion = (int)number;
            decimal fraction = (number - intPortion) * 100;
            int decPortion = (int)fraction;

            words = NumberToWords(intPortion);
            if (decPortion > 0)
            {
                words += " and ";
                words += NumberToWords(decPortion);
            }
            return words;
        }
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
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
                    context.Database.ExecuteSqlCommand("update JobRequests set [JobRequestStatus] = '" + JobStatus + "' , [Cost] = " + Shared.ToDecimal(Cost) + " where JobRequestCode = '" + JobRequestCode + "'");
                    context.SaveChanges();

                    return Json(new { success = true, response = "" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = ex.Message });
                }
            }

        }
        [HttpPost]
        public ActionResult GenerateLinkContent(long? JobRequestCode, string RequestStatus)
        {
            using (TransportEntities context = new TransportEntities())
            {
                //JobModel ObjJobModel = _objJobsRepository.Job_Edit(JobCode);
                JobRequestModel ObjJobRequestModel = _objJobsRepository.Job_RequestEdit(JobRequestCode);

                string linkcontent = ""; string _number = "";
                if (RequestStatus == "Requested")
                {
                    //linkcontent += "Dear " + Shared.ToString(ObjJobRequestModel.CustomerName) + " ,";
                    //linkcontent += System.Environment.NewLine;
                    //linkcontent += "We have accepted your trip Request.";
                    //if (ObjJobRequestModel.Cost != null)
                    //{
                    //linkcontent += System.Environment.NewLine;
                    //linkcontent += "The Price for your Trip is $" + Shared.ToString(ObjJobRequestModel.Cost);
                    //}
                    //linkcontent += System.Environment.NewLine;
                    //linkcontent += "We will send you the driver details Shortly!";
                    //_number = Shared.ToString(ObjJobRequestModel.ContactNo);
                }
                else if (RequestStatus == "Job Accepted")
                {
                    //var JobCode = context.Jobs.Where(o => o.JobRequestCode == JobRequestCode).Select(o => o.JobCode).FirstOrDefault();
                    //JobModel ObjJobModel = _objJobsRepository.Job_Edit(JobCode);

                    //if (ObjJobModel == null)
                    //{
                    //    linkcontent += "</br>";
                    //    linkcontent += "Job is not created. Add Job to send job details to the Customer!";
                    //    _number = "";
                    //}
                    //else
                    //{
                    //    linkcontent += "Dear " + Shared.ToString(ObjJobRequestModel.CustomerName) + " ,";

                    //    if (Shared.ToString(ObjJobModel.OutSource) != string.Empty)
                    //    {
                    //        linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Date : " + Shared.ToString(ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy")); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Time : " + Shared.ToString(ObjJobModel.JobTime); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Pickup : " + Shared.ToString(ObjJobModel.JobFrom); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Dropoff : " + Shared.ToString(ObjJobModel.JobTo); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Booking Status : Confirmed"; linkcontent += System.Environment.NewLine;
                    //        if (Shared.IsDecimal(ObjJobModel.Cost) == true)
                    //        {
                    //            linkcontent += "Price : $" + Shared.ToString(ObjJobModel.Cost); linkcontent += System.Environment.NewLine;
                    //        }
                    //        linkcontent += "Driver Name : " + Shared.ToString(ObjJobModel.OutSource); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Contact No : "; linkcontent += System.Environment.NewLine;
                    //    }
                    //    if (Shared.ToInt(ObjJobModel.DrivingBy) != 0)
                    //    {
                    //        UserMasterModel ObjUserModel = ObjSystemRepository.UserMaster_Edit(ObjJobModel.DrivingBy.Value);

                    //        linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Date : " + Shared.ToString(ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy")); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Time : " + Shared.ToString(ObjJobModel.JobTime); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Pickup : " + Shared.ToString(ObjJobModel.JobFrom); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Dropoff : " + Shared.ToString(ObjJobModel.JobTo); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Booking Status : Confirmed"; linkcontent += System.Environment.NewLine;
                    //        if (Shared.IsDecimal(ObjJobModel.Cost) == true)
                    //        {
                    //            linkcontent += "Price : $" + Shared.ToString(ObjJobModel.Cost); linkcontent += System.Environment.NewLine;
                    //        }
                    //        linkcontent += "Driver Name : " + Shared.ToString(ObjUserModel.FirstName); linkcontent += System.Environment.NewLine;
                    //        linkcontent += "Contact No : " + Shared.ToString(ObjUserModel.MobileNumber); linkcontent += System.Environment.NewLine;
                    //    }
                    //    _number = Shared.ToString(ObjJobModel.ContactNo);
                    //}
                    linkcontent += "Dear " + Shared.ToString(ObjJobRequestModel.CustomerName) + " ,";
                    linkcontent += System.Environment.NewLine;
                    linkcontent += "We have accepted your trip Request.";
                    if (ObjJobRequestModel.Cost != null)
                    {
                        linkcontent += System.Environment.NewLine;
                        linkcontent += "The Price for your Trip is $" + Shared.ToString(ObjJobRequestModel.Cost);
                    }
                    linkcontent += System.Environment.NewLine;
                    linkcontent += "We will send you the driver details Shortly!";
                    _number = Shared.ToString(ObjJobRequestModel.ContactNo);
                }
                else if (RequestStatus == "Not Available" || RequestStatus == "Cancelled")
                {
                    linkcontent += "Dear " + Shared.ToString(ObjJobRequestModel.CustomerName) + " ,";
                    linkcontent += System.Environment.NewLine;
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
                //JobRequestModel ObjJobRequestModel = _objJobsRepository.Job_RequestEdit(ObjJobModel.JobRequestCode);

                string linkcontent = ""; string _number = "";
                if (JobStatus == "Assigned")
                {
                    linkcontent += "Dear " + Shared.ToString(ObjJobModel.CustomerName) + " ,";

                    if (Shared.ToString(ObjJobModel.OutSource) != string.Empty)
                    {
                        linkcontent += System.Environment.NewLine;
                        linkcontent += "Date : " + Shared.ToString(ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy")); linkcontent += System.Environment.NewLine;
                        linkcontent += "Time : " + Shared.ToString(ObjJobModel.JobTime); linkcontent += System.Environment.NewLine;
                        linkcontent += "Pickup : " + Shared.ToString(ObjJobModel.JobFrom); linkcontent += System.Environment.NewLine;
                        linkcontent += "Dropoff : " + Shared.ToString(ObjJobModel.JobTo); linkcontent += System.Environment.NewLine;
                        linkcontent += "Booking Status : Confirmed"; linkcontent += System.Environment.NewLine;
                        if (Shared.IsDecimal(ObjJobModel.Cash) == true)
                        {
                            linkcontent += "Price : $" + Shared.ToString(ObjJobModel.Cash); linkcontent += System.Environment.NewLine;
                        }
                        linkcontent += "Driver Name : " + Shared.ToString(ObjJobModel.OutSource); linkcontent += System.Environment.NewLine;
                        linkcontent += "Contact No : "; linkcontent += System.Environment.NewLine;

                    }
                    if (Shared.ToInt(ObjJobModel.DrivingBy) != 0)
                    {
                        UserMasterModel ObjUserModel = ObjSystemRepository.UserMaster_Edit(ObjJobModel.DrivingBy.Value);

                        linkcontent += System.Environment.NewLine;
                        linkcontent += "Date : " + Shared.ToString(ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy")); linkcontent += System.Environment.NewLine;
                        linkcontent += "Time : " + Shared.ToString(ObjJobModel.JobTime); linkcontent += System.Environment.NewLine;
                        linkcontent += "Pickup : " + Shared.ToString(ObjJobModel.JobFrom); linkcontent += System.Environment.NewLine;
                        linkcontent += "Dropoff : " + Shared.ToString(ObjJobModel.JobTo); linkcontent += System.Environment.NewLine;
                        linkcontent += "Booking Status : Confirmed"; linkcontent += System.Environment.NewLine;
                        if (Shared.IsDecimal(ObjJobModel.Cash) == true)
                        {
                            linkcontent += "Price : $" + Shared.ToString(ObjJobModel.Cash); linkcontent += System.Environment.NewLine;
                        }
                        linkcontent += "Driver Name : " + Shared.ToString(ObjUserModel.FirstName); linkcontent += System.Environment.NewLine;
                        linkcontent += "Contact No : " + Shared.ToString(ObjUserModel.MobileNumber); linkcontent += System.Environment.NewLine;
                    }
                    _number = Shared.ToString(ObjJobModel.ContactNo);
                }
                else if (JobStatus == "Cancelled")
                {
                    linkcontent += "Dear " + Shared.ToString(ObjJobModel.CustomerName) + " ,";
                    linkcontent += System.Environment.NewLine;
                    linkcontent += "Sorry we cannot accept your trip Request!";
                    _number = Shared.ToString(ObjJobModel.ContactNo);
                }
                else if (JobStatus == "Job Completed")
                {
                    linkcontent += "Dear " + Shared.ToString(ObjJobModel.CustomerName) + " ,";
                    linkcontent += System.Environment.NewLine;
                    linkcontent += "Thank you for choosing Nissi Travels! Have a safe Journey!";
                    _number = Shared.ToString(ObjJobModel.ContactNo);
                }
                return Json(new { data = linkcontent, number = _number });
            }
        }

        [HttpPost]
        public ActionResult GenerateLinkContentMyJob(long? JobCode, string JobStatus)
        {
            using (TransportEntities context = new TransportEntities())
            {
                JobModel ObjJobModel = _objJobsRepository.Job_Edit(JobCode);
                //JobRequestModel ObjJobRequestModel = _objJobsRepository.Job_RequestEdit(ObjJobModel.JobRequestCode);

                string linkcontent = ""; string _number = "+6562920521";
                linkcontent += "Date : " + Shared.ToString(ObjJobModel.JobDate.Value.ToString("dd-MMM-yyyy")); linkcontent += System.Environment.NewLine;
                linkcontent += "Time : " + Shared.ToString(ObjJobModel.JobTime); linkcontent += System.Environment.NewLine;
                linkcontent += "Pickup : " + Shared.ToString(ObjJobModel.JobFrom); linkcontent += System.Environment.NewLine;
                linkcontent += "Dropoff : " + Shared.ToString(ObjJobModel.JobTo); linkcontent += System.Environment.NewLine;
                linkcontent += "Customer Name : " + Shared.ToString(ObjJobModel.CustomerName); linkcontent += System.Environment.NewLine;               
                linkcontent += "Phone Number : " + Shared.ToString(ObjJobModel.ContactNo); linkcontent += System.Environment.NewLine;
                linkcontent += "Job Given By : " + Shared.ToString(ObjJobModel.JobVendorName); linkcontent += System.Environment.NewLine;

                return Json(new { data = linkcontent, number = _number });
            }
        }

        [HttpPost]
        public ActionResult GenerateOutSourceLink(long? JobCode)
        {
            string linkcontent = "";
            JobModel ObjJobModel = _objJobsRepository.Job_Edit(JobCode);
            linkcontent += "Dear " + ObjJobModel.OutSource + ", "; linkcontent += System.Environment.NewLine;
            linkcontent += " You have a received a Job Request!"; linkcontent += System.Environment.NewLine;
            linkcontent += " Click the link to Start your Job!"; linkcontent += System.Environment.NewLine;

            string url = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/Login/Outsource?JobCode=" + Encode(Shared.ToString(JobCode));
            linkcontent += url;

            return Json(new { data = linkcontent });
        }
        [HttpGet]
        public ActionResult CreditJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? limit, string sortBy, string direction)
        {
            int TotalCount = 0;
            //if (StartDate == null) { StartDate = DateTime.Now; }
            //if (EndDate == null) { EndDate = DateTime.Now; }
            List<JobModel> Jobslist = _objJobsRepository.CreditJob_FindAll(page, StartDate, EndDate, VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, limit, sortBy, direction, out TotalCount);
            //List<JobModel> JobslistFiltered = Jobslist.Where(o => o.Credit.HasValue).OrderBy(o => o.JobDate).ToList();
            return Json(new { records = Jobslist, total = Jobslist.Count() }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateCreditStatus(long? JobCode, int? CashInHand)
        {
            using (TransportEntities context = new TransportEntities())
            {
                try
                {
                    context.Database.ExecuteSqlCommand("update Jobs set [Cash] = Credit, [Credit] = null,[CashInHand] = " + Shared.ToInt(CashInHand) + " where JobCode = '" + JobCode + "'");
                    context.SaveChanges();

                    return Json(new { success = true, response = "" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = ex.Message });
                }
            }

        }
        [HttpPost]
        public ActionResult UpdateCashInHand(long? JobCode, int? CashInHand)
        {
            using (TransportEntities context = new TransportEntities())
            {
                try
                {
                    context.Database.ExecuteSqlCommand("update Jobs set [CashInHand] = " + Shared.ToInt(CashInHand) + " where JobCode = '" + JobCode + "'");
                    context.SaveChanges();

                    return Json(new { success = true, response = "" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = ex.Message });
                }
            }

        }

        [HttpPost]
        public ActionResult IsRequestedCheck()
        {
            bool isRequested = false; int _count = 0;
            isRequested = _objJobsRepository.IsRequestedCheck();
            _count = _objJobsRepository.IsRequestedCount();
            return Json(new { success = isRequested, count = _count }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public string Encode(string encodeMe)
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(encodeMe);
            return Convert.ToBase64String(encoded);
        }

        public string ConvertToCamelCase(string input)
        {
            string[] words = input.Split(' ');
            for (int i = 1; i < words.Length; i++)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
            return string.Join("", words);
        }

        [HttpGet]
        public JsonResult CalcStaffTransactions(DateTime? StartDate, DateTime? EndDate)
        {
            int TotalCount = 0;
            List<JobModel> Jobslist = _objJobsRepository.Job_FindAll(1, StartDate, EndDate, null, null, SessionExpire.GetUserID(), null, null, null, null, null, null, out TotalCount);
            List<ExpenseModel> MyExpenseslist = _objExpensesRepository.MyExpense_FindAll(1, null, null, SessionExpire.GetUserID(), StartDate, EndDate, null, out TotalCount);
            var data = new
            {
                CashInHand = Jobslist.Select(o => o.Cash).Sum(),
                Expense = MyExpenseslist.Select(o => o.Charge).Sum()
            };

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
            var data = new
            {
                CashInHand = Jobslist.Select(o => o.Cash).Sum(),
                Expense = MyExpenseslist.Select(o => o.Charge).Sum() + _otherexpense.Select(o => o.Charge).Sum(),
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}