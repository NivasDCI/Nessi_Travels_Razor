using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Transport.Model;
using Transport.Repository;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceRepository _invoiceRepo = new InvoiceRepository();
        private readonly IReportsRepository _reportsRepo = new ReportsRepository();

        // ─── Invoice List Page ──────────────────────────────────────────────────
        public ActionResult InvoiceList(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        [HttpGet]
        public JsonResult Invoice_FindAll(string CustomerName, string InvoiceNo, string FromDate, string ToDate)
        {
            DateTime? parsedFrom = null, parsedTo = null;
            string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };

            if (!string.IsNullOrEmpty(FromDate))
            {
                DateTime d; if (DateTime.TryParseExact(FromDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) parsedFrom = d;
            }
            if (!string.IsNullOrEmpty(ToDate))
            {
                DateTime d; if (DateTime.TryParseExact(ToDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) parsedTo = d;
            }

            var list = _invoiceRepo.GetAllInvoices(CustomerName, InvoiceNo, parsedFrom, parsedTo);
            return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
        }

        // ─── Generate & Save Invoice from JobsReport ───────────────────────────
        [HttpPost]
        public JsonResult GenerateInvoice(string CustomerName, string StartDate, string EndDate,
            int? JobVendorCode, int? DrivingBy, int? VehicleCode, string CreditCash, int? CashInHand)
        {
            try
            {
                DateTime? parsedStart = null, parsedEnd = null;
                string[] fmts = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };

                if (!string.IsNullOrEmpty(StartDate))
                {
                    DateTime d; if (DateTime.TryParseExact(StartDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) parsedStart = d;
                }
                if (!string.IsNullOrEmpty(EndDate))
                {
                    DateTime d; if (DateTime.TryParseExact(EndDate, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) parsedEnd = d;
                }

                if (parsedStart == null) parsedStart = CommonRepository.GetTimeZoneDate();
                if (parsedEnd == null) parsedEnd = CommonRepository.GetTimeZoneDate();

                if (JobVendorCode == 0) JobVendorCode = null;
                if (DrivingBy == 0) DrivingBy = null;
                if (VehicleCode == 0) VehicleCode = null;
                if (CashInHand == 0) CashInHand = null;
                if (string.IsNullOrWhiteSpace(CustomerName)) CustomerName = null;

                int totalCount = 0;
                var jobs = _reportsRepo.Job_FindAll(1, parsedStart, parsedEnd, VehicleCode, JobVendorCode,
                    CustomerName, null, DrivingBy, CashInHand, 500, null, null, out totalCount);

                if (!string.IsNullOrEmpty(CreditCash))
                {
                    if (CreditCash == "Credit") jobs = jobs.Where(o => o.Credit.HasValue).ToList();
                    else if (CreditCash == "Cash") jobs = jobs.Where(o => o.Cash.HasValue).ToList();
                }

                if (jobs == null || !jobs.Any())
                    return Json(new { success = false, message = "No jobs found for the selected filters." });

                // Build header
                var firstJob = jobs.First();
                var header = new InvoiceHeaderModel
                {
                    CustomerName = string.IsNullOrEmpty(CustomerName) ? firstJob.CustomerName : CustomerName,
                    JobVendorCode = JobVendorCode,
                    JobVendorName = firstJob.JobVendorName,
                    DrivingBy = DrivingBy,
                    DrivingByName = firstJob.DrivingByName,
                    VehicleCode = VehicleCode,
                    VehicleName = firstJob.VehicleName,
                    CashInHand = CashInHand,
                    CashInHandName = firstJob.CashInHandName,
                    StartDate = parsedStart,
                    EndDate = parsedEnd,
                    CreditCash = CreditCash,
                    TotalAmount = jobs.Sum(o => o.Credit ?? o.Cash ?? 0),
                    IsCredit = jobs.Any(o => o.Credit.HasValue),
                    CreatedBy = SessionExpire.GetUserID()
                };

                // Build details
                var details = jobs.Select(o => new InvoiceDetailModel
                {
                    JobCode = o.JobCode,
                    JobDate = o.JobDate,
                    JobTime = o.JobTime,
                    JobFrom = o.JobFrom,
                    JobTo = o.JobTo,
                    CustomerName = o.CustomerName,
                    VehicleName = o.VehicleName,
                    DrivingByName = o.DrivingByName,
                    JobVendorName = o.JobVendorName,
                    Credit = o.Credit,
                    Cash = o.Cash,
                    Amount = o.Credit ?? o.Cash
                }).ToList();

                long invoiceId = _invoiceRepo.SaveInvoice(header, details);
                return Json(new { success = true, invoiceId = invoiceId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── View Invoice (printable) ───────────────────────────────────────────
        public ActionResult ViewInvoice(long invoiceId)
        {
            var header = _invoiceRepo.GetInvoiceById(invoiceId);
            var details = _invoiceRepo.GetInvoiceDetails(invoiceId);

            if (header == null) return Content("Invoice not found.");

            header.Details = details;

            decimal gross = details.Sum(d => d.Amount ?? 0);
            ViewBag.Header = header;
            ViewBag.Details = details;
            ViewBag.GrossAmount = gross;
            ViewBag.NetAmount = gross;
            ViewBag.AmountInWords = ConvertToCamelCase(DecimalToWords(gross));

            return View();
        }

        // ─── View Receipt (job sheet table - like screenshot 1) ─────────────────
        public ActionResult ViewReceipt(long invoiceId)
        {
            var header = _invoiceRepo.GetInvoiceById(invoiceId);
            var details = _invoiceRepo.GetInvoiceDetails(invoiceId);

            if (header == null) return Content("Invoice not found.");

            header.Details = details;
            ViewBag.Header = header;
            ViewBag.Details = details;

            return View();
        }

        // ─── Delete Invoice ─────────────────────────────────────────────────────
        [HttpPost]
        public JsonResult DeleteInvoice(long invoiceId)
        {
            bool ok = _invoiceRepo.DeleteInvoice(invoiceId);
            return Json(new { success = ok });
        }

        // ─── Helpers ────────────────────────────────────────────────────────────
        private string DecimalToWords(decimal number)
        {
            if (number == 0) return "zero";
            if (number < 0) return "minus " + DecimalToWords(Math.Abs(number));

            int intPortion = (int)number;
            int decPortion = (int)((number - intPortion) * 100);
            string words = NumberToWords(intPortion);
            if (decPortion > 0) words += " and " + NumberToWords(decPortion) + " cents";
            return words;
        }

        private string NumberToWords(int number)
        {
            if (number == 0) return "zero";
            if (number < 0) return "minus " + NumberToWords(Math.Abs(number));
            string words = "";
            if ((number / 1000000) > 0) { words += NumberToWords(number / 1000000) + " million "; number %= 1000000; }
            if ((number / 1000) > 0) { words += NumberToWords(number / 1000) + " thousand "; number %= 1000; }
            if ((number / 100) > 0) { words += NumberToWords(number / 100) + " hundred "; number %= 100; }
            if (number > 0)
            {
                if (words != "") words += "and ";
                var units = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tens = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
                if (number < 20) words += units[number];
                else { words += tens[number / 10]; if ((number % 10) > 0) words += "-" + units[number % 10]; }
            }
            return words;
        }

        private string ConvertToCamelCase(string input)
        {
            var words = input.Split(' ');
            for (int i = 1; i < words.Length; i++)
                if (words[i].Length > 0) words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            return string.Join(" ", words);
        }
    }
}