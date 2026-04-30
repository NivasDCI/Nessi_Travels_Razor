using System;
using System.Globalization;
using System.Web.Mvc;
using Transport.Model;
using Transport.Repository;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class WalletController : Controller
    {
        private readonly IWalletRepository _repo = new WalletRepository();

        private static readonly string[] Fmts =
            { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };

        private static DateTime? Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            DateTime d;
            return DateTime.TryParseExact(s.Trim(), Fmts,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                ? d : (DateTime?)null;
        }

        // ════════════════════════════════════════════════════════════════════
        // PAGE ACTIONS (return views)
        // ════════════════════════════════════════════════════════════════════

        // ADMIN pages
        public ActionResult AdminWallet(string HeaderViewID, string DetailViewID)
        {
            ViewBag.CurrentUID = SessionExpire.GetUserID().ToString();
            return View();
        }

        public ActionResult DriverWallet(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        public ActionResult CompanyWallet(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        public ActionResult AddExpenses(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        // DRIVER pages
        public ActionResult MyWallet(string HeaderViewID, string DetailViewID)
        {
            ViewBag.CurrentUID = SessionExpire.GetUserID().ToString();
            return View();
        }

        public ActionResult MyAddExpenses(string HeaderViewID, string DetailViewID)
        {
            ViewBag.CurrentUID = SessionExpire.GetUserID().ToString();
            return View();
        }

        // ════════════════════════════════════════════════════════════════════
        // DRIVER WALLET - JSON ACTIONS
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult DriverWallet_FindAll(int DriverUserID, string FromDate, string ToDate)
        {
            var list = _repo.GetDriverWalletSummary(DriverUserID, Parse(FromDate), Parse(ToDate));
            var balance = _repo.GetDriverWalletBalance(DriverUserID);
            return Json(new { records = list, total = list.Count, balance = balance },
                        JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DriverCashHistory_FindAll(int DriverUserID, string FromDate, string ToDate)
        {
            var list = _repo.GetDriverCashHistory(DriverUserID, Parse(FromDate), Parse(ToDate));
            return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
        }

        // Driver's own wallet (uses session UID)
        [HttpGet]
        public JsonResult MyWallet_FindAll(string FromDate, string ToDate)
        {
            int uid = SessionExpire.GetUserID();
            var list = _repo.GetDriverWalletSummary(uid, Parse(FromDate), Parse(ToDate));
            var balance = _repo.GetDriverWalletBalance(uid);
            return Json(new { records = list, total = list.Count, balance = balance },
                        JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MyCashHistory_FindAll(string FromDate, string ToDate)
        {
            int uid = SessionExpire.GetUserID();
            var list = _repo.GetDriverCashHistory(uid, Parse(FromDate), Parse(ToDate));
            return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
        }

        // ════════════════════════════════════════════════════════════════════
        // HANDOVER
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult Handover_FindAll(int? DriverUserID, string FromDate, string ToDate)
        {
            // If driver role, always scope to self
            int? uid = (DriverUserID.HasValue && DriverUserID.Value > 0)
                       ? DriverUserID : (int?)null;
            var list = _repo.GetHandovers(uid, Parse(FromDate), Parse(ToDate));
            return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Handover_Save(int DriverUserID, string HandoverDate,
            decimal Amount, int? HandedToUserID, string HandedToName, string Remarks)
        {
            try
            {
                int duid = DriverUserID > 0 ? DriverUserID : SessionExpire.GetUserID();
                var dt = Parse(HandoverDate) ?? DateTime.Today;
                bool ok = _repo.SaveHandover(new DriverHandoverModel
                {
                    DriverUserID = duid,
                    HandoverDate = dt,
                    Amount = Amount,
                    HandedToUserID = (HandedToUserID.HasValue && HandedToUserID.Value > 0) ? HandedToUserID : null,
                    HandedToName = HandedToName,
                    Remarks = Remarks,
                    CreatedBy = SessionExpire.GetUserID()
                });
                return Json(new { success = ok, message = ok ? "" : "Failed to save handover." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult Handover_Delete(long HandoverID)
        {
            return Json(new { success = _repo.DeleteHandover(HandoverID) });
        }

        // ════════════════════════════════════════════════════════════════════
        // DRIVER EXPENSE
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult DriverExpense_FindAll(int? DriverUserID, string FromDate, string ToDate)
        {
            int? uid = (DriverUserID.HasValue && DriverUserID.Value > 0)
                       ? DriverUserID : (int?)null;
            var list = _repo.GetDriverExpenses(uid, Parse(FromDate), Parse(ToDate));
            return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DriverExpense_Save(int DriverUserID, string ExpenseDate,
            string Category, decimal Amount, string Remarks, long? JobCode)
        {
            try
            {
                int duid = DriverUserID > 0 ? DriverUserID : SessionExpire.GetUserID();
                var dt = Parse(ExpenseDate) ?? DateTime.Today;
                bool ok = _repo.SaveDriverExpense(new DriverExpenseModel
                {
                    DriverUserID = duid,
                    ExpenseDate = dt,
                    Category = Category,
                    Amount = Amount,
                    Remarks = Remarks,
                    JobCode = (JobCode.HasValue && JobCode.Value > 0) ? JobCode : null,
                    CreatedBy = SessionExpire.GetUserID()
                });
                return Json(new { success = ok, message = ok ? "" : "Failed to save expense." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult DriverExpense_Delete(long DriverExpenseID)
        {
            return Json(new { success = _repo.DeleteDriverExpense(DriverExpenseID) });
        }

        // ════════════════════════════════════════════════════════════════════
        // ADMIN WALLET
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult AdminWallet_FindAll(int? AdminUserID, string FromDate, string ToDate)
        {
            int uid = (AdminUserID.HasValue && AdminUserID.Value > 0)
                      ? AdminUserID.Value : SessionExpire.GetUserID();
            var list = _repo.GetAdminWallet(uid, Parse(FromDate), Parse(ToDate));
            var balance = _repo.GetAdminWalletBalance(uid);
            return Json(new { records = list, total = list.Count, balance = balance },
                        JsonRequestBehavior.AllowGet);
        }

        // ════════════════════════════════════════════════════════════════════
        // COMPANY WALLET
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult CompanyWallet_FindAll()
        {
            var summary = _repo.GetCompanyWalletSummary();
            var breakdown = _repo.GetCompanyWalletDriverBreakdown();
            return Json(new { summary = summary, records = breakdown, total = breakdown.Count },
                        JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CompanyExpense_FindAll(string FromDate, string ToDate, string Category)
        {
            var list = _repo.GetCompanyExpenses(Parse(FromDate), Parse(ToDate), Category);
            return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CompanyExpense_Save(string ExpenseDate, string Category,
            decimal Amount, string Remarks, int? DriverUserID)
        {
            try
            {
                var dt = Parse(ExpenseDate) ?? DateTime.Today;
                bool ok = _repo.SaveCompanyExpense(new CompanyExpenseModel
                {
                    ExpenseDate = dt,
                    Category = Category,
                    Amount = Amount,
                    Remarks = Remarks,
                    DriverUserID = (DriverUserID.HasValue && DriverUserID.Value > 0) ? DriverUserID : null,
                    CreatedBy = SessionExpire.GetUserID()
                });
                return Json(new { success = ok, message = ok ? "" : "Failed to save expense." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult CompanyExpense_Delete(long CompanyExpenseID)
        {
            return Json(new { success = _repo.DeleteCompanyExpense(CompanyExpenseID) });
        }
    }
}