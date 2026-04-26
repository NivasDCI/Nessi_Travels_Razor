using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using Transport.Model;
using Transport.Repository;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class DriverWalletController : Controller
    {
        private readonly IDriverWalletRepository _walletRepo = new DriverWalletRepository();
        private readonly IJobRepository _jobRepo = new JobRepository();

        // ── Date parse helper (same formats used throughout the project) ─────
        private static readonly string[] DateFormats =
            { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };

        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            DateTime d;
            return DateTime.TryParseExact(s, DateFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : (DateTime?)null;
        }

        // ════════════════════════════════════════════════════════════════════
        // VIEW ACTIONS
        // ════════════════════════════════════════════════════════════════════

        // Driver's own wallet  (called via GetMenu by driver role)
        public ActionResult DriverWallet(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        // Admin overview of any driver's wallet
        public ActionResult AdminWallet(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        // Expense monitor  (driver sees own; admin can filter by driver)
        public ActionResult Expenses(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        // ════════════════════════════════════════════════════════════════════
        // WALLET SUMMARY
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult WalletSummary_FindAll(int? DriverUserID, string FromDate, string ToDate)
        {
            // If no specific driver requested, use the logged-in user (driver view)
            int uid = (DriverUserID.HasValue && DriverUserID.Value > 0)
                      ? DriverUserID.Value
                      : SessionExpire.GetUserID();

            DateTime? from = ParseDate(FromDate);
            DateTime? to = ParseDate(ToDate);

            var list = _walletRepo.GetWalletDailySummary(uid, from, to);
            var summary = _walletRepo.GetWalletBalanceSummary(uid);

            return Json(new { records = list, total = list.Count, summary = summary },
                        JsonRequestBehavior.AllowGet);
        }

        // ════════════════════════════════════════════════════════════════════
        // DAY DRILL-DOWN  (jobs on a specific wallet date)
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult DayDetails_FindAll(int DriverUserID, string WalletDate)
        {
            DateTime? dt = ParseDate(WalletDate);
            if (dt == null)
                return Json(new { records = new List<DriverWalletJobDetailModel>() },
                            JsonRequestBehavior.AllowGet);

            var list = _walletRepo.GetDayJobDetails(DriverUserID, dt.Value);
            return Json(new { records = list, total = list.Count },
                        JsonRequestBehavior.AllowGet);
        }

        // ════════════════════════════════════════════════════════════════════
        // HANDOVER CRUD
        // ════════════════════════════════════════════════════════════════════

        [HttpPost]
        public JsonResult Handover_Save(int DriverUserID, string HandoverDate,
            decimal Amount, int? HandedToUserID, string HandedToName, string Remarks)
        {
            try
            {
                DateTime? dt = ParseDate(HandoverDate);
                if (dt == null) dt = DateTime.Today;

                var model = new DriverHandoverModel
                {
                    DriverUserID = DriverUserID > 0 ? DriverUserID : SessionExpire.GetUserID(),
                    HandoverDate = dt.Value,
                    Amount = Amount,
                    HandedToUserID = HandedToUserID,
                    HandedToName = HandedToName,
                    Remarks = Remarks,
                    CreatedBy = SessionExpire.GetUserID()
                };

                bool ok = _walletRepo.SaveHandover(model);
                return Json(new { success = ok, message = ok ? "" : "Failed to save handover." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult Handover_FindAll(int? DriverUserID, string FromDate, string ToDate)
        {
            int? uid = (DriverUserID.HasValue && DriverUserID.Value > 0)
                       ? DriverUserID
                       : (int?)null;

            // For driver role, always scope to self
            if (uid == null)
            {
                var self = SessionExpire.GetUserID();
                if (self > 0) uid = self;
            }

            var list = _walletRepo.GetHandovers(uid, ParseDate(FromDate), ParseDate(ToDate));
            return Json(new { records = list, total = list.Count },
                        JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Handover_Delete(long HandoverID)
        {
            bool ok = _walletRepo.DeleteHandover(HandoverID);
            return Json(new { success = ok });
        }

        // ════════════════════════════════════════════════════════════════════
        // EXPENSE CRUD
        // ════════════════════════════════════════════════════════════════════

        [HttpPost]
        public JsonResult Expense_Save(int DriverUserID, string ExpenseDate,
            string Category, decimal Amount, string Remarks, long? JobCode)
        {
            try
            {
                DateTime? dt = ParseDate(ExpenseDate);
                if (dt == null) dt = DateTime.Today;

                var model = new DriverExpenseWalletModel
                {
                    DriverUserID = DriverUserID > 0 ? DriverUserID : SessionExpire.GetUserID(),
                    ExpenseDate = dt.Value,
                    Category = Category,
                    Amount = Amount,
                    Remarks = Remarks,
                    JobCode = (JobCode.HasValue && JobCode.Value > 0) ? JobCode : null,
                    CreatedBy = SessionExpire.GetUserID()
                };

                bool ok = _walletRepo.SaveExpense(model);
                return Json(new { success = ok, message = ok ? "" : "Failed to save expense." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult Expense_FindAll(int? DriverUserID, string FromDate, string ToDate)
        {
            int? uid = (DriverUserID.HasValue && DriverUserID.Value > 0)
                       ? DriverUserID
                       : (int?)null;

            // Driver sees only their own
            if (uid == null)
            {
                var self = SessionExpire.GetUserID();
                if (self > 0) uid = self;
            }

            var list = _walletRepo.GetExpenses(uid, ParseDate(FromDate), ParseDate(ToDate));
            return Json(new { records = list, total = list.Count },
                        JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Expense_Delete(long DriverExpenseID)
        {
            bool ok = _walletRepo.DeleteExpense(DriverExpenseID);
            return Json(new { success = ok });
        }

        // ════════════════════════════════════════════════════════════════════
        // MANUAL RE-SYNC  (admin utility – recalculate any driver's wallet day)
        // ════════════════════════════════════════════════════════════════════

        [HttpPost]
        public JsonResult SyncWallet(int DriverUserID, string WalletDate)
        {
            try
            {
                DateTime? dt = ParseDate(WalletDate);
                if (dt == null) dt = DateTime.Today;
                _walletRepo.SyncWallet(DriverUserID, dt.Value);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}