using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Transport.Model;
using Transport.Repository;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class ExpenseController : Controller
    {
        IExpenseRepository _objExpensesRepository = new ExpenseRepository();
        ICommonRepository ObjCommRepository = new CommonRepository();
        IProjectMasterRepository ObjMasterRepository = new ProjectMasterRepository();

        public ActionResult ListExpense(string HeaderViewID, string DetailViewID)
        {
             return View();
        }

        public ActionResult AddExpense(string ExpenseCode, string HeaderViewID, string DetailViewID)
        {
            ExpenseModel obj = new ExpenseModel();
            return View(obj);
        }

        #region "Expense"

        [HttpGet]
        public ActionResult Expense_FindAll(int? page, int? limit, string sortBy, DateTime? StartDate, DateTime? EndDate, string direction)
        {
            int TotalCount = 0;
            if (StartDate == null) { StartDate = DateTime.Now; }
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<ExpenseModel> Expenseslist = _objExpensesRepository.Expense_FindAll(page, limit, sortBy, StartDate, EndDate, direction, out TotalCount);            
            return Json(new { records = Expenseslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet]
        public JsonResult Expense_Edit(long ExpenseCode)
        {
            ExpenseModel ObjMessage = _objExpensesRepository.Expense_Edit(ExpenseCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Expense_Save(ExpenseModel objExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            objExpense.CreatedBy = SessionExpire.GetUserID();

            ObjMessage = _objExpensesRepository.Expense_Save(objExpense);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Expense_Delete(long ExpenseCode)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjMessage = _objExpensesRepository.Expense_Delete(ExpenseCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult ListMyExpense(string HeaderViewID, string DetailViewID)
        {
            return View();
        }

        public ActionResult AddMyExpense(string MyExpenseCode, string HeaderViewID, string DetailViewID)
        {
            ExpenseModel obj = new ExpenseModel();
            return View(obj);
        }

        #region "MyExpense"

        [HttpGet]
        public ActionResult MyExpense_FindAll(int? page, int? limit, string sortBy, DateTime? StartDate, DateTime? EndDate, string direction)
        {
            int TotalCount = 0;
            if (StartDate == null) { StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);}
            if (EndDate == null) { EndDate = DateTime.Now; }
            List<ExpenseModel> MyExpenseslist = _objExpensesRepository.MyExpense_FindAll(page, limit, sortBy, SessionExpire.GetUserID(), StartDate, EndDate, direction, out TotalCount);
            return Json(new { records = MyExpenseslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MyExpense_Edit(long MyExpenseCode)
        {
            ExpenseModel ObjMessage = _objExpensesRepository.MyExpense_Edit(MyExpenseCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetSupplierCost(int SupplierTypeCode)
        {
            
            decimal? SupplierCost = Shared.ToDecimal(_objExpensesRepository.GetSupplierCost(SupplierTypeCode));
            return Json(SupplierCost, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult MyExpense_Save(ExpenseModel objMyExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            objMyExpense.CreatedBy = SessionExpire.GetUserID();
            objMyExpense.PaymentBy = SessionExpire.GetUserID();

            ObjMessage = _objExpensesRepository.MyExpense_Save(objMyExpense);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MyExpense_Delete(long MyExpenseCode)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjMessage = _objExpensesRepository.MyExpense_Delete(MyExpenseCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion



    }
}