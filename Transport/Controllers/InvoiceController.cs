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

//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Mvc;
//using Transport.Model;
//using Transport.Repository;

//namespace Transport.Controllers
//{
//    [SessionExpire]
//    [NoCacheAttribute]
//    public class InvoiceController : Controller
//    {
//        private readonly IInvoiceRepository _invoiceRepo;
//        private readonly IReportsRepository _reportsRepo;

//        public InvoiceController()
//        {
//            _invoiceRepo = new InvoiceRepository();
//            _reportsRepo = new ReportsRepository();
//        }

//        // Constructor for dependency injection (optional)
//        public InvoiceController(IInvoiceRepository invoiceRepo, IReportsRepository reportsRepo)
//        {
//            _invoiceRepo = invoiceRepo;
//            _reportsRepo = reportsRepo;
//        }

//        // ─── Invoice List Page ──────────────────────────────────────────────────
//        public ActionResult InvoiceList()
//        {
//            return View();
//        }

//        [HttpGet]
//        public async Task<JsonResult> Invoice_FindAll(string customerName, string invoiceNo, string fromDate, string toDate)
//        {
//            try
//            {
//                DateTime? parsedFrom = ParseDate(fromDate);
//                DateTime? parsedTo = ParseDate(toDate);

//                var list = await _invoiceRepo.GetAllInvoicesAsync(customerName, invoiceNo, parsedFrom, parsedTo);
//                return Json(new { success = true, records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
//            }
//        }

//        // ─── Generate & Save Invoice from JobsReport ───────────────────────────
//        [HttpPost]
//        public async Task<JsonResult> GenerateInvoice(GenerateInvoiceRequest request)
//        {
//            try
//            {
//                // Validate request
//                if (request == null)
//                    return Json(new { success = false, message = "Invalid request data" });

//                // Parse dates
//                DateTime? parsedStart = ParseDate(request.StartDate);
//                DateTime? parsedEnd = ParseDate(request.EndDate);

//                // Set default dates if not provided (last 30 days)
//                if (!parsedStart.HasValue && !parsedEnd.HasValue)
//                {
//                    parsedEnd = CommonRepository.GetTimeZoneDate();
//                    parsedStart = parsedEnd.Value.AddDays(-30);
//                }
//                else if (!parsedStart.HasValue)
//                    parsedStart = parsedEnd.Value.AddDays(-30);
//                else if (!parsedEnd.HasValue)
//                    parsedEnd = parsedStart.Value.AddDays(30);

//                // Clean up parameters
//                var filters = new JobFilters
//                {
//                    StartDate = parsedStart,
//                    EndDate = parsedEnd,
//                    VehicleCode = request.VehicleCode,
//                    JobVendorCode = request.JobVendorCode,
//                    CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : request.CustomerName,
//                    DrivingBy = request.DrivingBy,
//                    CashInHand = request.CashInHand,
//                    CreditCash = request.CreditCash
//                };

//                // Get jobs
//                int totalCount = 0;
//                var jobs = await Task.Run(() => _reportsRepo.Job_FindAll(
//                    1, filters.StartDate, filters.EndDate, filters.VehicleCode, filters.JobVendorCode,
//                    filters.CustomerName, null, filters.DrivingBy, filters.CashInHand, 500, null, null, out totalCount));

//                if (jobs == null || !jobs.Any())
//                    return Json(new { success = false, message = "No jobs found for the selected filters." });

//                // Apply Credit/Cash filter if specified
//                if (!string.IsNullOrEmpty(request.CreditCash))
//                {
//                    jobs = request.CreditCash == "Credit"
//                        ? jobs.Where(o => o.Credit.HasValue && o.Credit > 0).ToList()
//                        : jobs.Where(o => o.Cash.HasValue && o.Cash > 0).ToList();
//                }

//                if (!jobs.Any())
//                    return Json(new { success = false, message = $"No {request.CreditCash ?? "jobs"} found for the selected filters." });

//                // Check for duplicate invoice
//                bool exists = await _invoiceRepo.InvoiceExistsForPeriodAsync(parsedStart, parsedEnd, filters.CustomerName);
//                if (exists)
//                    return Json(new { success = false, message = "An invoice already exists for this period and customer." });

//                // Build header
//                var firstJob = jobs.First();
//                var header = BuildInvoiceHeader(firstJob, filters, jobs);

//                // Build details
//                var details = BuildInvoiceDetails(jobs);

//                // Save invoice
//                long invoiceId = await _invoiceRepo.SaveInvoiceAsync(header, details);

//                return Json(new { success = true, invoiceId = invoiceId, message = "Invoice generated successfully!" });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = $"Error generating invoice: {ex.Message}" });
//            }
//        }

//        // ─── View Invoice (printable) ───────────────────────────────────────────
//        public async Task<ActionResult> ViewInvoice(long invoiceId)
//        {
//            try
//            {
//                var header = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId);
//                var details = await _invoiceRepo.GetInvoiceDetailsAsync(invoiceId);

//                if (header == null)
//                    return Content("Invoice not found.");

//                header.Details = details;
//                decimal gross = details.Sum(d => d.Amount ?? 0);

//                ViewBag.Header = header;
//                ViewBag.Details = details;
//                ViewBag.GrossAmount = gross;
//                ViewBag.NetAmount = gross;
//                ViewBag.AmountInWords = ConvertToTitleCase(NumberToWords(gross));

//                return View();
//            }
//            catch (Exception ex)
//            {
//                return Content($"Error loading invoice: {ex.Message}");
//            }
//        }

//        // ─── View Receipt ─────────────────────────────────────────────────────
//        public async Task<ActionResult> ViewReceipt(long invoiceId)
//        {
//            try
//            {
//                var header = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId);
//                var details = await _invoiceRepo.GetInvoiceDetailsAsync(invoiceId);

//                if (header == null)
//                    return Content("Invoice not found.");

//                header.Details = details;
//                ViewBag.Header = header;
//                ViewBag.Details = details;

//                return View();
//            }
//            catch (Exception ex)
//            {
//                return Content($"Error loading receipt: {ex.Message}");
//            }
//        }

//        // ─── Delete Invoice ─────────────────────────────────────────────────────
//        [HttpPost]
//        public async Task<JsonResult> DeleteInvoice(long invoiceId)
//        {
//            try
//            {
//                bool success = await _invoiceRepo.DeleteInvoiceAsync(invoiceId);
//                return Json(new { success = success, message = success ? "Invoice deleted successfully" : "Failed to delete invoice" });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = ex.Message });
//            }
//        }

//        // ─── Helper Methods ────────────────────────────────────────────────────

//        private DateTime? ParseDate(string dateString)
//        {
//            if (string.IsNullOrEmpty(dateString))
//                return null;

//            string[] formats = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd/MM/yyyy" };

//            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
//                return date;

//            if (DateTime.TryParse(dateString, out date))
//                return date;

//            return null;
//        }

//        private InvoiceHeaderModel BuildInvoiceHeader(JobModel firstJob, JobFilters filters, List<JobModel> jobs)
//        {
//            return new InvoiceHeaderModel
//            {
//                CustomerName = string.IsNullOrEmpty(filters.CustomerName) ? firstJob.CustomerName : filters.CustomerName,
//                JobVendorCode = filters.JobVendorCode,
//                JobVendorName = firstJob.JobVendorName,
//                DrivingBy = filters.DrivingBy,
//                DrivingByName = firstJob.DrivingByName,
//                VehicleCode = filters.VehicleCode,
//                VehicleName = firstJob.VehicleName,
//                CashInHand = filters.CashInHand,
//                CashInHandName = firstJob.CashInHandName,
//                StartDate = filters.StartDate,
//                EndDate = filters.EndDate,
//                CreditCash = filters.CreditCash,
//                TotalAmount = jobs.Sum(o => o.Credit ?? o.Cash ?? 0),
//                IsCredit = jobs.Any(o => o.Credit.HasValue && o.Credit > 0),
//                CreatedBy = SessionExpire.GetUserID(),
//                InvoiceDate = DateTime.Now
//            };
//        }

//        private List<InvoiceDetailModel> BuildInvoiceDetails(List<JobModel> jobs)
//        {
//            return jobs.Select(o => new InvoiceDetailModel
//            {
//                JobCode = o.JobCode,
//                JobDate = o.JobDate,
//                JobTime = o.JobTime,
//                JobFrom = o.JobFrom,
//                JobTo = o.JobTo,
//                CustomerName = o.CustomerName,
//                VehicleName = o.VehicleName,
//                DrivingByName = o.DrivingByName,
//                JobVendorName = o.JobVendorName,
//                Credit = o.Credit,
//                Cash = o.Cash,
//                Amount = o.Credit ?? o.Cash
//            }).ToList();
//        }

//        private string NumberToWords(decimal number)
//        {
//            if (number == 0) return "zero";
//            if (number < 0) return "minus " + NumberToWords(Math.Abs(number));

//            int intPortion = (int)number;
//            int decPortion = (int)((number - intPortion) * 100);

//            string words = NumberToWordsInt(intPortion);
//            if (decPortion > 0)
//                words += " and " + NumberToWordsInt(decPortion) + " cents";

//            return words;
//        }

//        private string NumberToWordsInt(int number)
//        {
//            if (number == 0) return "zero";

//            string words = "";

//            if ((number / 1000000) > 0)
//            {
//                words += NumberToWordsInt(number / 1000000) + " million ";
//                number %= 1000000;
//            }
//            if ((number / 1000) > 0)
//            {
//                words += NumberToWordsInt(number / 1000) + " thousand ";
//                number %= 1000;
//            }
//            if ((number / 100) > 0)
//            {
//                words += NumberToWordsInt(number / 100) + " hundred ";
//                number %= 100;
//            }

//            if (number > 0)
//            {
//                if (words != "") words += "and ";

//                var units = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
//                                    "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
//                                    "seventeen", "eighteen", "nineteen" };
//                var tens = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

//                if (number < 20)
//                    words += units[number];
//                else
//                {
//                    words += tens[number / 10];
//                    if ((number % 10) > 0)
//                        words += "-" + units[number % 10];
//                }
//            }

//            return words;
//        }

//        private string ConvertToTitleCase(string input)
//        {
//            if (string.IsNullOrEmpty(input))
//                return input;

//            var words = input.Split(' ');
//            for (int i = 0; i < words.Length; i++)
//            {
//                if (!string.IsNullOrEmpty(words[i]))
//                {
//                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
//                }
//            }
//            return string.Join(" ", words);
//        }
//    }

//    // Request/Response Models
//    public class GenerateInvoiceRequest
//    {
//        public string CustomerName { get; set; }
//        public string StartDate { get; set; }
//        public string EndDate { get; set; }
//        public int? JobVendorCode { get; set; }
//        public int? DrivingBy { get; set; }
//        public int? VehicleCode { get; set; }
//        public string CreditCash { get; set; }
//        public int? CashInHand { get; set; }
//    }

//    public class JobFilters
//    {
//        public DateTime? StartDate { get; set; }
//        public DateTime? EndDate { get; set; }
//        public int? VehicleCode { get; set; }
//        public int? JobVendorCode { get; set; }
//        public string CustomerName { get; set; }
//        public int? DrivingBy { get; set; }
//        public int? CashInHand { get; set; }
//        public string CreditCash { get; set; }
//    }
//}