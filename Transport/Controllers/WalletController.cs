using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Transport.Model;
using Transport.Repository;

namespace Transport.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletRepository _repository;

        private int CurrentUserID
        {
            get
            {
                try
                {
                    if (Session != null && Session["UserID"] != null)
                        return Convert.ToInt32(Session["UserID"]);
                }
                catch { }
                return 1; // Default user
            }
        }

        public WalletController()
        {
            try
            {
                // Direct connection string from your database
                string connectionString = "data source=localhost,1433;initial catalog=TravelsDatabase;user id=sa;password=Rthinfotech@123;MultipleActiveResultSets=True;TrustServerCertificate=True;Encrypt=True";

                _repository = new WalletRepository(connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception("Database connection error: " + ex.Message);
            }
        }

        // GET: Wallet/Dashboard
        public ActionResult Dashboard()
        {
            try
            {
                if (_repository == null)
                {
                    TempData["Error"] = "Repository not initialized. Please check database connection.";
                    var emptyModel = new WalletDashboardViewModel
                    {
                        AdminWallet = new AdminWalletViewModel(),
                        DriverWallets = new List<DriverWalletViewModel>(),
                        RecentTransactions = new List<TransactionHistoryViewModel>()
                    };
                    return View(emptyModel);
                }

                var dashboard = _repository.GetWalletDashboard();

                if (dashboard == null)
                {
                    dashboard = new WalletDashboardViewModel
                    {
                        AdminWallet = new AdminWalletViewModel(),
                        DriverWallets = new List<DriverWalletViewModel>(),
                        RecentTransactions = new List<TransactionHistoryViewModel>()
                    };
                }

                if (dashboard.AdminWallet == null)
                {
                    dashboard.AdminWallet = new AdminWalletViewModel();
                }

                if (dashboard.DriverWallets == null)
                {
                    dashboard.DriverWallets = new List<DriverWalletViewModel>();
                }

                return View(dashboard);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading dashboard: " + ex.Message;
                var emptyModel = new WalletDashboardViewModel
                {
                    AdminWallet = new AdminWalletViewModel(),
                    DriverWallets = new List<DriverWalletViewModel>(),
                    RecentTransactions = new List<TransactionHistoryViewModel>()
                };
                return View(emptyModel);
            }
        }

        // GET: Wallet/DriverWallets
        public ActionResult DriverWallets()
        {
            try
            {
                var drivers = _repository.GetAllDriverWallets();
                return View(drivers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading drivers: " + ex.Message;
                return View(new List<DriverWalletViewModel>());
            }
        }

        // GET: Wallet/DriverWalletDetail/{id}
        public ActionResult DriverWalletDetail(int id)
        {
            try
            {
                var detail = _repository.GetDriverWalletDetail(id);
                if (detail.DriverInfo == null)
                {
                    TempData["Error"] = "Driver not found";
                    return RedirectToAction("DriverWallets");
                }
                return View(detail);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DriverWallets");
            }
        }

        // GET: Wallet/HandoverCash
        public ActionResult HandoverCash(int? driverId = null)
        {
            var model = new CashHandoverViewModel
            {
                Drivers = _repository.GetDrivers(),
                Admins = _repository.GetAdmins(),
                HandoverDate = DateTime.Now
            };

            if (driverId.HasValue)
            {
                model.DriverUserID = driverId.Value;
            }

            return View(model);
        }

        // POST: Wallet/HandoverCash
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandoverCash(CashHandoverViewModel model)
        {
            try
            {
                if (model.HandoverAmount <= 0)
                {
                    ModelState.AddModelError("HandoverAmount", "Amount must be greater than zero");
                }

                if (ModelState.IsValid)
                {
                    var handedToName = _repository.GetAdmins().FirstOrDefault(a => a.UserID == model.HandedToUserID)?.UserName ?? "";

                    var result = _repository.HandoverCashToAdmin(
                        model.DriverUserID,
                        model.HandoverAmount,
                        model.HandedToUserID,
                        handedToName,
                        model.Remarks,
                        CurrentUserID
                    );

                    if (result)
                    {
                        TempData["Success"] = $"Successfully handed over {model.HandoverAmount:C} to {handedToName}";
                        return RedirectToAction("DriverWalletDetail", new { id = model.DriverUserID });
                    }
                }

                model.Drivers = _repository.GetDrivers();
                model.Admins = _repository.GetAdmins();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Drivers = _repository.GetDrivers();
                model.Admins = _repository.GetAdmins();
                return View(model);
            }
        }

        // GET: Wallet/DriverExpense
        public ActionResult DriverExpense(int? driverId = null)
        {
            var model = new DriverExpenseViewModel
            {
                Drivers = _repository.GetDrivers(),
                Categories = GetDriverExpenseCategories(),
                ExpenseDate = DateTime.Now
            };

            if (driverId.HasValue)
            {
                model.DriverUserID = driverId.Value;
            }

            return View(model);
        }

        // POST: Wallet/DriverExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DriverExpense(DriverExpenseViewModel model)
        {
            try
            {
                if (model.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Amount must be greater than zero");
                }

                if (string.IsNullOrEmpty(model.Category))
                {
                    ModelState.AddModelError("Category", "Category is required");
                }

                if (ModelState.IsValid)
                {
                    var result = _repository.AddDriverExpense(
                        model.DriverUserID,
                        model.Amount,
                        model.Category,
                        model.Remarks,
                        model.JobCode,
                        CurrentUserID
                    );

                    if (result)
                    {
                        TempData["Success"] = $"Expense of {model.Amount:C} recorded successfully";
                        return RedirectToAction("DriverWalletDetail", new { id = model.DriverUserID });
                    }
                }

                model.Drivers = _repository.GetDrivers();
                model.Categories = GetDriverExpenseCategories();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Drivers = _repository.GetDrivers();
                model.Categories = GetDriverExpenseCategories();
                return View(model);
            }
        }

        // GET: Wallet/CompanyExpense
        public ActionResult CompanyExpense()
        {
            var model = new CompanyExpenseViewModel
            {
                Categories = GetCompanyExpenseCategories(),
                ExpenseDate = DateTime.Now
            };
            return View(model);
        }

        // POST: Wallet/CompanyExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompanyExpense(CompanyExpenseViewModel model)
        {
            try
            {
                if (model.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Amount must be greater than zero");
                }

                if (string.IsNullOrEmpty(model.Category))
                {
                    ModelState.AddModelError("Category", "Category is required");
                }

                if (ModelState.IsValid)
                {
                    var result = _repository.AddCompanyExpense(
                        model.Category,
                        model.Amount,
                        model.PaidTo,
                        model.Remarks,
                        CurrentUserID
                    );

                    if (result)
                    {
                        TempData["Success"] = $"Company expense of {model.Amount:C} recorded successfully";
                        return RedirectToAction("Dashboard");
                    }
                }

                model.Categories = GetCompanyExpenseCategories();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Categories = GetCompanyExpenseCategories();
                return View(model);
            }
        }

        // GET: Wallet/TransactionHistory
        public ActionResult TransactionHistory(int? userId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var transactions = _repository.GetWalletTransactions(userId, null, fromDate, toDate);
                ViewBag.Drivers = _repository.GetDrivers();
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
                ViewBag.SelectedDriver = userId;
                return View(transactions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new List<TransactionHistoryViewModel>());
            }
        }

        // GET: Wallet/AdminWallet
        public ActionResult AdminWallet()
        {
            try
            {
                var adminWallet = new AdminWalletViewModel
                {
                    CurrentBalance = _repository.GetCompanyWalletBalance(),
                    TotalCashReceived = _repository.GetWalletTransactions(null, "Admin", null, null)
                        .Where(t => t.Category == "CashReceived").Sum(t => t.Amount),
                    TotalExpenses = _repository.GetWalletTransactions(null, "Admin", null, null)
                        .Where(t => t.TransactionType == "Debit").Sum(t => t.Amount),
                    NetBalance = _repository.GetCompanyWalletBalance(),
                    LastUpdated = DateTime.Now
                };
                return View(adminWallet);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new AdminWalletViewModel());
            }
        }

        // Helper Methods
        private List<ExpenseCategoryModel> GetDriverExpenseCategories()
        {
            return new List<ExpenseCategoryModel>
            {
                new ExpenseCategoryModel { CategoryValue = "Diesel", CategoryText = "Diesel / Fuel" },
                new ExpenseCategoryModel { CategoryValue = "Parking", CategoryText = "Parking Fee" },
                new ExpenseCategoryModel { CategoryValue = "Toll", CategoryText = "Toll Tax" },
                new ExpenseCategoryModel { CategoryValue = "Repair", CategoryText = "Vehicle Repair" },
                new ExpenseCategoryModel { CategoryValue = "Food", CategoryText = "Food / Meals" },
                new ExpenseCategoryModel { CategoryValue = "Other", CategoryText = "Other Expenses" }
            };
        }

        private List<ExpenseCategoryModel> GetCompanyExpenseCategories()
        {
            return new List<ExpenseCategoryModel>
            {
                new ExpenseCategoryModel { CategoryValue = "Salary", CategoryText = "Employee Salary" },
                new ExpenseCategoryModel { CategoryValue = "Rent", CategoryText = "Office Rent" },
                new ExpenseCategoryModel { CategoryValue = "VehicleDues", CategoryText = "Vehicle Dues / EMI" },
                new ExpenseCategoryModel { CategoryValue = "Maintenance", CategoryText = "Vehicle Maintenance" },
                new ExpenseCategoryModel { CategoryValue = "Utilities", CategoryText = "Electricity / Water / Internet" },
                new ExpenseCategoryModel { CategoryValue = "Marketing", CategoryText = "Marketing / Advertising" },
                new ExpenseCategoryModel { CategoryValue = "Other", CategoryText = "Other Expenses" }
            };
        }
    }
}