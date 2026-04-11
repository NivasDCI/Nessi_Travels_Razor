using Transport.Repository.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Transport.Model;
using Transport.Entity;
using System.Security.Cryptography;
using System.Data.Entity;

namespace Transport.Repository
{
    public class ExpenseRepository : IExpenseRepository
    {
        TransportEntities db = new TransportEntities();
        CommonRepository ObjCom = new CommonRepository();

        #region "Expense"
        public List<ExpenseModel> Expense_FindAll(int? page, int? limit, string sortBy, DateTime? StartDate, DateTime? EndDate, string direction, out int TotalCount)
        {
            List<ExpenseModel> ObjExpenses = (from PU in db.Expenses
                                              where (StartDate == null || EndDate == null) || DbFunctions.TruncateTime(PU.ExpenseDate) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(PU.ExpenseDate) <= DbFunctions.TruncateTime(EndDate)
                                              select new ExpenseModel
                                              {
                                                  ExpenseCode = PU.ExpenseCode,
                                                  VehicleCode = PU.VehicleCode,
                                                  ServiceTypeCode = PU.ServiceTypeCode,
                                                  SupplierTypeCode = PU.SupplierTypeCode,
                                                  Qty = PU.Qty, 
                                                  PaidTo = PU.PaidTo,
                                                  PaymentBy = PU.PaymentBy,
                                                  VehicleName = (from PT in db.Vehicles where PT.VehicleCode == PU.VehicleCode select PT.VehicleName).FirstOrDefault(),
                                                  ServiceTypeName = (from PT in db.ServiceTypes where PT.ServiceTypeCode == PU.ServiceTypeCode select PT.ServiceTypeName).FirstOrDefault(),
                                                  SupplierTypeName = (from PT in db.SupplierTypes where PT.SupplierTypeCode == PU.SupplierTypeCode select PT.SupplierTypeName).FirstOrDefault(),
                                                  PaymentByName = (from PT in db.TblUserMasters where PT.UserID == PU.PaymentBy select PT.FirstName).FirstOrDefault(),
                                                  PaidToName = (from PT in db.TblUserMasters where PT.UserID == PU.PaidTo select PT.FirstName).FirstOrDefault(),
                                                  Charge = PU.Charge,
                                                  Remarks = PU.Remarks,
                                                  ExpenseDate = PU.ExpenseDate,
                                                  CreatedDate = PU.CreatedDate,
                                                  DisplayCreatedBy = (from PT in db.TblUserMasters where PT.UserID == PU.CreatedBy select PT.FirstName).FirstOrDefault(),
                                              }).ToList().Select(o =>
                                               new ExpenseModel
                                               {
                                                   ExpenseCode = o.ExpenseCode,
                                                   VehicleCode = o.VehicleCode,
                                                   ServiceTypeCode = o.ServiceTypeCode,
                                                   SupplierTypeCode = o.SupplierTypeCode,
                                                   SupplierTypeName = o.SupplierTypeName,
                                                    PaidTo = o.PaidTo,
                                                    PaidToName = o.PaidToName,
                                                    Qty = o.Qty,
                                                   PaymentBy = o.PaymentBy,
                                                   PaymentByName = o.PaymentByName,
                                                   VehicleName = o.VehicleName,
                                                   ServiceTypeName = o.ServiceTypeName,
                                                   Charge = o.Charge,
                                                   Remarks = o.Remarks,
                                                   ExpenseDate = o.ExpenseDate,
                                                   DisplayCreatedDate = o.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                                   DisplayCreatedBy = o.DisplayCreatedBy
                                               }).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjExpenses = ObjExpenses.OrderBy(q => q.VehicleName).ToList();
                            break;

                    }
                }
                else
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjExpenses = ObjExpenses.OrderByDescending(q => q.VehicleName).ToList();
                            break;
                    }
                }
            }
            else
            {
                ObjExpenses = ObjExpenses.OrderByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = ObjExpenses.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjExpenses = ObjExpenses.Skip(start).Take(limit.Value).ToList();
            }

            return ObjExpenses;
        }

        public ExpenseModel Expense_Edit(long ExpenseCode)
        {
            ExpenseModel ObjExpenses = (from ct in db.Expenses
                                        where ct.ExpenseCode == ExpenseCode
                                        select new ExpenseModel
                                      {
                                            ExpenseCode = ct.ExpenseCode,
                                            VehicleCode = ct.VehicleCode,
                                            ServiceTypeCode = ct.ServiceTypeCode,
                                            SupplierTypeCode = ct.SupplierTypeCode,
                                            Qty = ct.Qty,
                                            PaidTo = ct.PaidTo,
                                            PaymentBy = ct.PaymentBy,
                                            Charge = ct.Charge,
                                            Remarks = ct.Remarks,
                                            ExpenseDate = ct.ExpenseDate
                                      }).FirstOrDefault();


            return ObjExpenses;
        }

        public ReturnMessageModel Expense_Save(ExpenseModel objExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var obj = (from tblSMod in db.Expenses
                           where tblSMod.ExpenseCode == objExpense.ExpenseCode
                           select tblSMod).SingleOrDefault();

                if (obj == null)
                {
                    ObjMessage = Expense_Insert(objExpense);//insert
                }
                else
                {
                    ObjMessage = Expense_Update(objExpense);
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }

        public ReturnMessageModel Expense_Insert(ExpenseModel objExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                if(objExpense.ServiceTypeCode == 2)
                {
                    objExpense.Charge = GetSupplierCost(objExpense.SupplierTypeCode.Value) * objExpense.Qty;
                }
                var entity = new Expense
                {
                    VehicleCode = objExpense.VehicleCode,
                    ServiceTypeCode = objExpense.ServiceTypeCode,
                    SupplierTypeCode = objExpense.SupplierTypeCode,
                    Qty = objExpense.Qty,
                    PaidTo = objExpense.PaidTo,
                    PaymentBy = objExpense.PaymentBy,
                    Charge = objExpense.Charge,
                    Remarks = objExpense.Remarks,
                    ExpenseDate = objExpense.ExpenseDate,
                    CreatedDate = CommonRepository.GetTimeZoneDate(),
                    CreatedBy = objExpense.CreatedBy,
                    UpdatedDate = CommonRepository.GetTimeZoneDate(),
                    UpdatedBy = objExpense.CreatedBy

                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();                

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }

        public ReturnMessageModel Expense_Update(ExpenseModel objExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objUpdate = (from ct in db.Expenses where ct.ExpenseCode == objExpense.ExpenseCode select ct).ToList();
                if (objExpense.ServiceTypeCode == 2)
                {
                    objExpense.Charge = GetSupplierCost(objExpense.SupplierTypeCode.Value) * objExpense.Qty;
                }
                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].UpdatedBy = objExpense.CreatedBy;
                    objUpdate[0].UpdatedDate = CommonRepository.GetTimeZoneDate();
                    objUpdate[0].VehicleCode = objExpense.VehicleCode;
                    objUpdate[0].ServiceTypeCode = objExpense.ServiceTypeCode;
                    objUpdate[0].SupplierTypeCode = objExpense.SupplierTypeCode;
                    objUpdate[0].Qty = objExpense.Qty;
                    objUpdate[0].PaidTo = objExpense.PaidTo;
                    objUpdate[0].PaymentBy = objExpense.PaymentBy;
                    objUpdate[0].Charge = objExpense.Charge;
                    objUpdate[0].Remarks = objExpense.Remarks;
                    objUpdate[0].ExpenseDate = objExpense.ExpenseDate;
                    db.SaveChanges();

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel Expense_Delete(long ExpenseCode)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDelete = (from ct in db.Expenses
                                 where ct.ExpenseCode == ExpenseCode
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count == 1)
                {
                    db.Expenses.RemoveRange(objDelete);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }

            return ObjMessage;
        }

        #endregion

        #region "MyExpense"
        public decimal? GetSupplierCost(long suppliertypecode)
        {
            var SupplierCost = (from c in db.SupplierTypes where c.SupplierTypeCode == suppliertypecode select c.PricePerLr).FirstOrDefault();
            return SupplierCost;
        }
        public List<ExpenseModel> MyExpense_FindAll(int? page, int? limit, string sortBy, int? PaymentBy, DateTime? StartDate, DateTime? EndDate, string direction, out int TotalCount)
        {
            List<ExpenseModel> ObjMyExpenses = (from PU in db.Expenses
                                                where (StartDate == null || EndDate == null) || DbFunctions.TruncateTime(PU.ExpenseDate) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(PU.ExpenseDate) <= DbFunctions.TruncateTime(EndDate)
                                                && PU.PaymentBy == PaymentBy || PU.PaidTo == PaymentBy
                                                select new ExpenseModel
                                                {
                                                    ExpenseCode = PU.ExpenseCode,
                                                    VehicleCode = PU.VehicleCode,
                                                    ServiceTypeCode = PU.ServiceTypeCode,
                                                    SupplierTypeCode = PU.SupplierTypeCode,
                                                    PaymentBy = PU.PaymentBy,
                                                    PaidTo = PU.PaidTo,
                                                    VehicleName = (from PT in db.Vehicles where PT.VehicleCode == PU.VehicleCode select PT.VehicleName).FirstOrDefault(),
                                                    ServiceTypeName = (from PT in db.ServiceTypes where PT.ServiceTypeCode == PU.ServiceTypeCode select PT.ServiceTypeName).FirstOrDefault(),
                                                    SupplierTypeName = (from PT in db.SupplierTypes where PT.SupplierTypeCode == PU.SupplierTypeCode select PT.SupplierTypeName).FirstOrDefault(),
                                                    PaymentByName = (from PT in db.TblUserMasters where PT.UserID == PU.PaymentBy select PT.FirstName).FirstOrDefault(),
                                                    PaidToName = (from PT in db.TblUserMasters where PT.UserID == PU.PaidTo select PT.FirstName).FirstOrDefault(),
                                                    Qty = PU.Qty,
                                                    Charge = PU.Charge,
                                                    Remarks = PU.Remarks,
                                                    ExpenseDate = PU.ExpenseDate,
                                                    CreatedDate = PU.CreatedDate,
                                                    DisplayCreatedBy = (from PT in db.TblUserMasters where PT.UserID == PU.CreatedBy select PT.FirstName).FirstOrDefault(),
                                                }).ToList().Select(o =>
                                                 new ExpenseModel
                                                 {
                                                     ExpenseCode = o.ExpenseCode,
                                                     VehicleCode = o.VehicleCode,
                                                     ServiceTypeCode = o.ServiceTypeCode,
                                                     PaidTo = o.PaidTo,
                                                     PaidToName = o.PaidToName,
                                                     SupplierTypeCode = o.SupplierTypeCode,
                                                     PaymentBy = o.PaymentBy,
                                                     VehicleName = o.VehicleName,
                                                     ServiceTypeName = o.ServiceTypeName,
                                                     SupplierTypeName = o.SupplierTypeName,
                                                     PaymentByName = o.PaymentByName,
                                                     Qty = o.Qty,
                                                     Charge = o.Charge,
                                                     Remarks = o.Remarks,
                                                     ExpenseDate = o.ExpenseDate,
                                                     DisplayCreatedDate = o.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                                     DisplayCreatedBy = o.DisplayCreatedBy
                                                 }).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjMyExpenses = ObjMyExpenses.OrderBy(q => q.VehicleName).ToList();
                            break;

                    }
                }
                else
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjMyExpenses = ObjMyExpenses.OrderByDescending(q => q.VehicleName).ToList();
                            break;
                    }
                }
            }
            else
            {
                ObjMyExpenses = ObjMyExpenses.OrderByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = ObjMyExpenses.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjMyExpenses = ObjMyExpenses.Skip(start).Take(limit.Value).ToList();
            }

            return ObjMyExpenses;
        }

        public ExpenseModel MyExpense_Edit(long ExpenseCode)
        {
            ExpenseModel ObjExpenses = (from ct in db.Expenses
                                        where ct.ExpenseCode == ExpenseCode
                                        select new ExpenseModel
                                        {
                                            ExpenseCode = ct.ExpenseCode,
                                            VehicleCode = ct.VehicleCode,
                                            ServiceTypeCode = ct.ServiceTypeCode,
                                            SupplierTypeCode = ct.SupplierTypeCode,
                                            Qty = ct.Qty,
                                            PaidTo = ct.PaidTo,
                                            PaymentBy = ct.PaymentBy,
                                            Charge = ct.Charge,
                                            Remarks = ct.Remarks,
                                            ExpenseDate = ct.ExpenseDate
                                        }).FirstOrDefault();


            return ObjExpenses;
        }

        public ReturnMessageModel MyExpense_Save(ExpenseModel objExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var obj = (from tblSMod in db.Expenses
                           where tblSMod.ExpenseCode == objExpense.ExpenseCode
                           select tblSMod).SingleOrDefault();

                if (obj == null)
                {
                    ObjMessage = MyExpense_Insert(objExpense);//insert
                }
                else
                {
                    ObjMessage = MyExpense_Update(objExpense);
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }

        public ReturnMessageModel MyExpense_Insert(ExpenseModel objExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                if (objExpense.ServiceTypeCode == 2)
                {
                    objExpense.Charge = GetSupplierCost(objExpense.SupplierTypeCode.Value) * objExpense.Qty;
                }
                var entity = new Expense
                {
                    VehicleCode = objExpense.VehicleCode,
                    ServiceTypeCode = objExpense.ServiceTypeCode,
                    SupplierTypeCode = objExpense.SupplierTypeCode,
                    Qty = objExpense.Qty,
                    PaidTo = null,
                    PaymentBy = objExpense.PaymentBy,
                    Charge = objExpense.Charge,
                    Remarks = objExpense.Remarks,
                    ExpenseDate = objExpense.ExpenseDate,
                    CreatedDate = CommonRepository.GetTimeZoneDate(),
                    CreatedBy = objExpense.CreatedBy,
                    UpdatedDate = CommonRepository.GetTimeZoneDate(),
                    UpdatedBy = objExpense.CreatedBy

                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }

        public ReturnMessageModel MyExpense_Update(ExpenseModel objExpense)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objUpdate = (from ct in db.Expenses where ct.ExpenseCode == objExpense.ExpenseCode select ct).ToList();
                if (objExpense.ServiceTypeCode == 2)
                {
                    objExpense.Charge = GetSupplierCost(objExpense.SupplierTypeCode.Value) * objExpense.Qty;
                }
                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].UpdatedBy = objExpense.CreatedBy;
                    objUpdate[0].UpdatedDate = CommonRepository.GetTimeZoneDate();
                    objUpdate[0].VehicleCode = objExpense.VehicleCode;
                    objUpdate[0].ServiceTypeCode = objExpense.ServiceTypeCode;
                    objUpdate[0].SupplierTypeCode = objExpense.SupplierTypeCode;
                    objUpdate[0].Qty = objExpense.Qty;
                    //objUpdate[0].PaidTo = objExpense.PaidTo;
                    objUpdate[0].PaymentBy = objExpense.PaymentBy;
                    objUpdate[0].Charge = objExpense.Charge;
                    objUpdate[0].Remarks = objExpense.Remarks;
                    objUpdate[0].ExpenseDate = objExpense.ExpenseDate;
                    db.SaveChanges();

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel MyExpense_Delete(long ExpenseCode)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDelete = (from ct in db.Expenses
                                 where ct.ExpenseCode == ExpenseCode
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count == 1)
                {
                    db.Expenses.RemoveRange(objDelete);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Expense_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Expense_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }

            return ObjMessage;
        }

        #endregion

    }
}

