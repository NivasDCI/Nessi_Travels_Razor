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
        // PAGE ACTIONS
        // ════════════════════════════════════════════════════════════════════

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
        // DIAGNOSTIC — open this URL in browser to see the exact error
        // URL: /Wallet/Diagnostic
        // ════════════════════════════════════════════════════════════════════
        [AllowAnonymous]
        public JsonResult Diagnostic()
        {
            var results = new System.Collections.Generic.List<object>();
            string connStr = "";
            try
            {
                // Test 1: read connection string
                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
                using (var sr = new System.IO.StreamReader(path))
                    while (sr.Peek() >= 0) connStr = sr.ReadLine();
                results.Add(new { test = "ConnectionString", status = "OK", detail = connStr.Substring(0, 30) + "..." });
            }
            catch (Exception ex) { results.Add(new { test = "ConnectionString", status = "FAIL", detail = ex.Message }); }

            // Test 2: check each stored proc
            string[] procs = {
                "sp_frm_sync_DriverWallet",
                "sp_frm_get_DriverWallet_Summary",
                "sp_frm_get_DriverWallet_Balance",
                "sp_frm_get_DriverCashHistory",
                "sp_frm_get_DriverHandovers",
                "sp_frm_get_DriverExpenses",
                "sp_frm_get_AdminWallet_Summary",
                "sp_frm_get_AdminWallet_Balance",
                "sp_frm_get_CompanyExpenses",
                "sp_frm_get_CompanyWallet_Summary",
                "sp_frm_get_CompanyWallet_DriverBreakdown"
            };

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    foreach (var proc in procs)
                    {
                        var cmd = new System.Data.SqlClient.SqlCommand(
                            "SELECT COUNT(1) FROM sys.objects WHERE type='P' AND name=@n", conn);
                        cmd.Parameters.AddWithValue("@n", proc);
                        int exists = (int)cmd.ExecuteScalar();
                        results.Add(new { test = "Proc: " + proc, status = exists > 0 ? "EXISTS" : "MISSING", detail = "" });
                    }

                    // Test tables
                    string[] tables = { "DriverWallet", "DriverHandover", "DriverExpense", "CompanyExpense" };
                    foreach (var tbl in tables)
                    {
                        var cmd = new System.Data.SqlClient.SqlCommand(
                            "SELECT COUNT(1) FROM sysobjects WHERE xtype='U' AND name=@n", conn);
                        cmd.Parameters.AddWithValue("@n", tbl);
                        int exists = (int)cmd.ExecuteScalar();
                        results.Add(new { test = "Table: " + tbl, status = exists > 0 ? "EXISTS" : "MISSING", detail = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add(new { test = "DB Connection", status = "FAIL", detail = ex.Message });
            }

            // Test 3: try calling the actual repo method
            try
            {
                int uid = SessionExpire.GetUserID();
                var list = _repo.GetDriverCashHistory(uid, null, null);
                results.Add(new { test = "GetDriverCashHistory", status = "OK", detail = "Returned " + list.Count + " rows" });
            }
            catch (Exception ex)
            {
                results.Add(new { test = "GetDriverCashHistory", status = "FAIL", detail = ex.Message });
            }

            try
            {
                int uid = SessionExpire.GetUserID();
                var bal = _repo.GetDriverWalletBalance(uid);
                results.Add(new { test = "GetDriverWalletBalance", status = "OK", detail = "Balance=" + bal.WalletBalance });
            }
            catch (Exception ex)
            {
                results.Add(new { test = "GetDriverWalletBalance", status = "FAIL", detail = ex.Message });
            }

            try
            {
                var list = _repo.GetHandovers(null, null, null);
                results.Add(new { test = "GetHandovers", status = "OK", detail = "Returned " + list.Count + " rows" });
            }
            catch (Exception ex)
            {
                results.Add(new { test = "GetHandovers", status = "FAIL", detail = ex.Message });
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        // ════════════════════════════════════════════════════════════════════
        // MY WALLET (driver's own)
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult MyWallet_FindAll(string FromDate, string ToDate)
        {
            try
            {
                int uid = SessionExpire.GetUserID();
                var list = _repo.GetDriverWalletSummary(uid, Parse(FromDate), Parse(ToDate));
                var balance = _repo.GetDriverWalletBalance(uid);
                return Json(new { records = list, total = list.Count, balance = balance },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult MyCashHistory_FindAll(string FromDate, string ToDate)
        {
            try
            {
                int uid = SessionExpire.GetUserID();
                var list = _repo.GetDriverCashHistory(uid, Parse(FromDate), Parse(ToDate));
                return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // DRIVER WALLET (admin view — select any driver)
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult DriverWallet_FindAll(int DriverUserID, string FromDate, string ToDate)
        {
            try
            {
                var list = _repo.GetDriverWalletSummary(DriverUserID, Parse(FromDate), Parse(ToDate));
                var balance = _repo.GetDriverWalletBalance(DriverUserID);
                return Json(new { records = list, total = list.Count, balance = balance },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DriverCashHistory_FindAll(int DriverUserID, string FromDate, string ToDate)
        {
            try
            {
                var list = _repo.GetDriverCashHistory(DriverUserID, Parse(FromDate), Parse(ToDate));
                return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // HANDOVER
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult Handover_FindAll(int? DriverUserID, string FromDate, string ToDate)
        {
            try
            {
                int? uid = (DriverUserID.HasValue && DriverUserID.Value > 0)
                           ? DriverUserID : (int?)null;
                if (uid == null) { var self = SessionExpire.GetUserID(); if (self > 0) uid = self; }
                var list = _repo.GetHandovers(uid, Parse(FromDate), Parse(ToDate));
                return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
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
            try
            {
                int? uid = (DriverUserID.HasValue && DriverUserID.Value > 0)
                           ? DriverUserID : (int?)null;
                if (uid == null) { var self = SessionExpire.GetUserID(); if (self > 0) uid = self; }
                var list = _repo.GetDriverExpenses(uid, Parse(FromDate), Parse(ToDate));
                return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
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
            try
            {
                int uid = (AdminUserID.HasValue && AdminUserID.Value > 0)
                          ? AdminUserID.Value : SessionExpire.GetUserID();
                var list = _repo.GetAdminWallet(uid, Parse(FromDate), Parse(ToDate));
                var balance = _repo.GetAdminWalletBalance(uid);
                return Json(new { records = list, total = list.Count, balance = balance },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // COMPANY WALLET
        // ════════════════════════════════════════════════════════════════════

        [HttpGet]
        public JsonResult CompanyWallet_FindAll()
        {
            try
            {
                var summary = _repo.GetCompanyWalletSummary();
                var breakdown = _repo.GetCompanyWalletDriverBreakdown();
                return Json(new { summary = summary, records = breakdown, total = breakdown.Count },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult CompanyExpense_FindAll(string FromDate, string ToDate, string Category)
        {
            try
            {
                var list = _repo.GetCompanyExpenses(Parse(FromDate), Parse(ToDate), Category);
                return Json(new { records = list, total = list.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { records = new object[0], total = 0, error = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
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
                return Json(new { success = ok, message = ok ? "" : "Failed to save." });
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