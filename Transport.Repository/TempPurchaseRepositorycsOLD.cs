using Distributor.Repository.Resource;
using Distributor.Entity;
using Distributor.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Web;
using System.IO;
using System.Globalization;
using System.Security.Cryptography;

namespace Distributor.Repository
{
    public class TempPurchaseRepository : ITempPurchaseRepository
    {
        CommonRepository ObjCom = new CommonRepository();
        IndexDistributorEntities db = new IndexDistributorEntities();


        public List<TempDistributorPurchaseheaderModel> DistributorPurchaseRequests_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Distributor, string PurchaseStatus, string sortBy, string direction, out int TotalCount)
        {

            DateTime ObjPurchaseDate = DateTime.Now;
            if (PurchaseDate != "")
            {
                ObjPurchaseDate = Convert.ToDateTime(PurchaseDate);
            }
            List<TempDistributorPurchaseheaderModel> ObjPurchases = (from ct in db.TblTempDistributorPurchaseheaders
                                                                     join SM in db.TblDistributorMasters on ct.SuperStockistID equals SM.DistributorID
                                                                     where
                                                                       ((PurchaseDate == "" || PurchaseDate == null) || ct.PurchaseDate == ObjPurchaseDate)
                                                                     && ((PurchaseRefNo == "" || PurchaseRefNo == null) || ct.ReferenceNO.Contains(PurchaseRefNo))
                                                                     && ((Distributor == "" || Distributor == null) || SM.DistributorName.Contains(Distributor))
                                                                     && ((PurchaseStatus == "" || PurchaseStatus == null) || ct.PurchaseStatus == PurchaseStatus)

                                                                     select new TempDistributorPurchaseheaderModel
                                                                     {
                                                                         DistributorPurchaseID = ct.DistributorPurchaseID,
                                                                         ReferenceNO = ct.ReferenceNO,
                                                                         PurchaseDate = ct.PurchaseDate,
                                                                         SuperStockistID = ct.SuperStockistID,
                                                                         DistributorInvoice = ct.DistributorInvoice,
                                                                         WarehouseID = ct.WarehouseID,
                                                                         GrandTotal = ct.GrandTotal,
                                                                         Paid = ct.Paid ?? 0,
                                                                         Balance = ct.Balance ?? 0,
                                                                         PurchaseStatus = ct.PurchaseStatus,
                                                                         DistributorName = SM.DistributorName,
                                                                         CreatedDate = ct.CreatedDate,
                                                                         WarehouseName = (from DM in db.TblDefaultMasterDetails where DM.DefaultDetailID == ct.WarehouseID select DM.DefaultTextField).FirstOrDefault(),
                                                                         Quantity = (from PM in db.TblTempDistributorPurchaseDetails where PM.DistributorPurchaseID == ct.DistributorPurchaseID select PM.Quantity).Sum(),
                                                                         FreeQuantity = (from PM in db.TblTempDistributorPurchaseDetails where PM.DistributorPurchaseID == ct.DistributorPurchaseID select PM.FreeQuantity ?? 0).DefaultIfEmpty(0).Sum(),
                                                                         PurchaseStatusStr = (from DM in db.TblDefaultMasterDetails where DM.DefaultDetailID == ct.PurchaseStatus select DM.DefaultTextField).FirstOrDefault(),
                                                                         CreatedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.CreatedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                                                                         LastModifiedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.LastModifiedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                                                                         ApprovedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.ApprovedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault()
                                                                     }).Where(a => a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_RequestedPurchasedStatus_ViewID
                                                                            || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_CancelledPurchaseStatus_ViewID
                                                                            || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ApprovedPurchasedStatus_ViewID
                                                                            || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_OrderedPurchasedStatus_ViewID
                                                                            || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "ReferenceNO":
                            ObjPurchases = ObjPurchases.OrderBy(q => q.ReferenceNO).ToList();
                            break;
                        case "PurchaseDate":
                            ObjPurchases = ObjPurchases.OrderBy(q => q.PurchaseDate).ToList();
                            break;
                        case "GrandTotal":
                            ObjPurchases = ObjPurchases.OrderBy(q => q.GrandTotal).ToList();
                            break;
                        case "DistributorName":
                            ObjPurchases = ObjPurchases.OrderBy(q => q.DistributorName).ToList();
                            break;
                        case "PurchaseStatusStr":
                            ObjPurchases = ObjPurchases.OrderBy(q => q.PurchaseStatusStr).ToList();
                            break;
                        case "Quantity":
                            ObjPurchases = ObjPurchases.OrderBy(q => q.Quantity).ToList();
                            break;
                    }
                }
                else
                {

                    switch (sortBy.Trim())
                    {
                        case "ReferenceNO":
                            ObjPurchases = ObjPurchases.OrderByDescending(q => q.ReferenceNO).ToList();
                            break;
                        case "PurchaseDate":
                            ObjPurchases = ObjPurchases.OrderByDescending(q => q.PurchaseDate).ToList();
                            break;
                        case "GrandTotal":
                            ObjPurchases = ObjPurchases.OrderByDescending(q => q.GrandTotal).ToList();
                            break;
                        case "DistributorName":
                            ObjPurchases = ObjPurchases.OrderByDescending(q => q.DistributorName).ToList();
                            break;
                        case "PurchaseStatusStr":
                            ObjPurchases = ObjPurchases.OrderByDescending(q => q.PurchaseStatusStr).ToList();
                            break;
                        case "Quantity":
                            ObjPurchases = ObjPurchases.OrderByDescending(q => q.Quantity).ToList();
                            break;

                    }
                }
            }
            else
            {
                ObjPurchases = ObjPurchases.OrderByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = ObjPurchases.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjPurchases = ObjPurchases.Skip(start).Take(limit.Value).ToList();
            }

            return ObjPurchases;
        }

        public ReturnMessageModel DistributionPurchases_Save(TempDistributorPurchaseheaderModel objPurchases, List<TempDistributorPurchaseDetailModel> ObjPurchaseDetail)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var obj = (from tblSMod in db.TblTempDistributorPurchaseheaders
                           where tblSMod.DistributorPurchaseID != objPurchases.DistributorPurchaseID && tblSMod.RunningNumber == objPurchases.RunningNumber
                           && tblSMod.RunningYear == objPurchases.RunningYear && tblSMod.RunningMonth == objPurchases.RunningMonth
                           select tblSMod).ToList();

                if (obj != null && obj.Count == 0)
                {
                    if (objPurchases.DistributorPurchaseID == 0)
                    {
                        ObjMessage = DistributionPurchases_Insert(objPurchases, ObjPurchaseDetail);////insert
                    }
                    else
                    {
                        ObjMessage = DistributionPurchases_Update(objPurchases, ObjPurchaseDetail);
                    }
                }
                else
                {
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DUPLICATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DUPLICATEFAIL;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DistributionPurchases_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DistributionPurchases_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }
        public ReturnMessageModel DistributionPurchases_Insert(TempDistributorPurchaseheaderModel objPurchases, List<TempDistributorPurchaseDetailModel> PurchaseDetail)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var PaymentStatus = "";

                if (objPurchases.GrandTotal == objPurchases.Paid || objPurchases.GrandTotal <= objPurchases.Paid)
                {
                    objPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPaid_ViewID;
                }
                else if (objPurchases.Paid == 0 || objPurchases.Paid == null)
                {
                    objPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusDue_ViewID;
                }
                else if (objPurchases.Paid != 0)
                {
                    objPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPartial_ViewID;
                }

                if (objPurchases.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID)
                {
                    PaymentStatus = objPurchases.PaymentStatus;
                }

                var entity = new TblTempDistributorPurchaseheader
                {
                    RunningYear = objPurchases.RunningYear,
                    RunningMonth = objPurchases.RunningMonth,
                    RunningNumber = Convert.ToString(objPurchases.RunningNumber),
                    ReferenceNO = objPurchases.ReferenceNO,
                    PurchaseDate = objPurchases.PurchaseDate,
                    SuperStockistID = objPurchases.SuperStockistID,
                    WarehouseID = objPurchases.WarehouseID,
                    BrandID = objPurchases.BrandID,
                    DistributorInvoice = objPurchases.DistributorInvoice,
                    DistributorLocationID = objPurchases.DistributorLocationID,
                    Shipping = objPurchases.Shipping,
                    PaymentTerm = objPurchases.PaymentTerm,
                    TaxType = objPurchases.TaxType,
                    Note = objPurchases.Note,
                    TotalTax = objPurchases.TotalTax,
                    TotalDiscount = objPurchases.TotalDiscount,
                    TotalScheme = objPurchases.TotalScheme,
                    TotalAmount = objPurchases.TotalAmount,
                    FinalDiscountPercent = objPurchases.FinalDiscountPercent,
                    FinalDiscountAmount = objPurchases.FinalDiscountAmount,
                    GrandTotal = objPurchases.GrandTotal,
                    FileName = objPurchases.FileName,
                    ReferenceAttachment = objPurchases.ReferenceAttachment,
                    PurchaseStatus = objPurchases.PurchaseStatus,
                    PaymentStatus = PaymentStatus,
                    CreatedBy = objPurchases.CreatedBy,
                    CreatedDate = CommonRepository.GetTimeZoneDate()
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();

                var vPurchaseID = entity.DistributorPurchaseID;
                var WarehouseID = objPurchases.WarehouseID;
                var CreatedBy = objPurchases.CreatedBy ?? 0;
                var PurchaseStatus = objPurchases.PurchaseStatus;


                ObjMessage = DistributorPurchaseDetail_Save(PurchaseDetail, vPurchaseID, PurchaseStatus, WarehouseID, CreatedBy, objPurchases.SuperStockistID);

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DistributionPurchases_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DistributionPurchases_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel Purchase_Approval(long DistributorPurchaseID, int IsApprove, int ApproveRejectBy)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var ObjPur = (from pur in db.TblTempDistributorPurchaseheaders
                              where pur.DistributorPurchaseID == DistributorPurchaseID
                              select pur).ToList();
                if (ObjPur.Count > 0)
                {
                    ObjPur[0].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_ApprovedPurchasedStatus_ViewID;
                    ObjPur[0].ApprovedBy = ApproveRejectBy;
                    ObjPur[0].ApprovedDate = CommonRepository.GetTimeZoneDate();
                    db.SaveChanges();

                    ObjMessage.Result = true;

                    if (IsApprove == 1)
                    {
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.PurchaseReceived;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVALSUCCESS;
                    }
                    else
                    {
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.Approval_Rejected;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.REJECTSUCCESS;
                    }

                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Purchase_Approval");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVEREJECTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Purchase_Approval");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVEREJECTFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel Purchase_CreatePO(long DistributorPurchaseID, int CreatedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var ObjPur = (from pur in db.TblTempDistributorPurchaseheaders
                              where pur.DistributorPurchaseID == DistributorPurchaseID
                              select pur).ToList();

                if (ObjPur.Count > 0)
                {
                    ObjPur[0].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_OrderedPurchasedStatus_ViewID;
                    ObjPur[0].LastModifiedBy = CreatedBy;
                    ObjPur[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                    db.SaveChanges();

                    ObjMessage.Result = true;

                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.PurchaseReceived;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.ADDSUCCESS;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Purchase_CreatePO");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Purchase_CreatePO");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }

        public PurchaseRunningNumber PurchaseHeader_RunningNumber()
        {
            PurchaseRunningNumber Obj = new PurchaseRunningNumber();
            int CurrentYear = DateTime.Now.Year;
            int CurrentMonth = DateTime.Now.Month;
            string month_name = DateTime.Now.ToString("MMM");
            var objModule = (from tblSMod in db.TblTempDistributorPurchaseheaders
                             where tblSMod.RunningYear == CurrentYear && tblSMod.RunningMonth == CurrentMonth
                             select tblSMod).ToList();
            if (objModule != null && objModule.Count != 0)
            {
                int int_MaxNumber = objModule.Max(x => Convert.ToInt16(x.RunningNumber)) + 1;
                string str_RunningNo = GetNumberFormat(int_MaxNumber, 4).ToString();
                Obj.PurchaseRunningNo = "PUR-" + month_name.ToUpper() + Convert.ToString(CurrentYear) + "/" + str_RunningNo;
                Obj.RunningYear = CurrentYear;
                Obj.RunningMonth = CurrentMonth;
                Obj.RunningNumber = str_RunningNo;
            }
            else if (objModule != null && objModule.Count == 0)
            {
                string str_RunningNo = GetNumberFormat(1, 4).ToString();
                Obj.PurchaseRunningNo = "PUR-" + month_name.ToUpper() + Convert.ToString(CurrentYear) + "/" + str_RunningNo;
                Obj.RunningYear = CurrentYear;
                Obj.RunningMonth = CurrentMonth;
                Obj.RunningNumber = str_RunningNo;
            }


            return Obj;
        }

        public PurchaseRunningNumber CompanyPurchaseHeader_RunningNumber()
        {
            PurchaseRunningNumber Obj = new PurchaseRunningNumber();
            int CurrentYear = DateTime.Now.Year;
            int CurrentMonth = DateTime.Now.Month;
            string month_name = DateTime.Now.ToString("MMM");
            var objModule = (from tblSMod in db.TblTempCompanyPurchaseheaders
                             where tblSMod.RunningYear == CurrentYear && tblSMod.RunningMonth == CurrentMonth
                             select tblSMod).ToList();
            if (objModule != null && objModule.Count != 0)
            {
                int int_MaxNumber = objModule.Max(x => Convert.ToInt16(x.RunningNumber)) + 1;
                string str_RunningNo = GetNumberFormat(int_MaxNumber, 4).ToString();
                Obj.PurchaseRunningNo = "PUR-" + month_name.ToUpper() + Convert.ToString(CurrentYear) + "/" + str_RunningNo;
                Obj.RunningYear = CurrentYear;
                Obj.RunningMonth = CurrentMonth;
                Obj.RunningNumber = str_RunningNo;
            }
            else if (objModule != null && objModule.Count == 0)
            {
                string str_RunningNo = GetNumberFormat(1, 4).ToString();
                Obj.PurchaseRunningNo = "PUR-" + month_name.ToUpper() + Convert.ToString(CurrentYear) + "/" + str_RunningNo;
                Obj.RunningYear = CurrentYear;
                Obj.RunningMonth = CurrentMonth;
                Obj.RunningNumber = str_RunningNo;
            }


            return Obj;
        }


        private string GetNumberFormat(int RunningNumber, int NumberLength)
        {
            string ret = RunningNumber.ToString();
            int length = ret.Length;
            for (int i = 0; i < (NumberLength - length); i++)
            {
                ret = "0" + ret;
            }

            return ret;
        }
        public ReturnMessageModel DistributorPurchaseDetail_Save(List<TempDistributorPurchaseDetailModel> PurchaseDetail, long DistPurchaseID, string PurchaseStatus, string WarehouseID, int CreatedBy, int DistributorID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                if (PurchaseDetail != null && PurchaseDetail.Count > 0)
                {

                    var objPurchase = (from tblSMod in db.TblTempDistributorPurchaseDetails
                                       where tblSMod.DistributorPurchaseID == DistPurchaseID
                                       select tblSMod).ToList();

                    if (objPurchase != null && objPurchase.Count > 0)
                    {
                        foreach (var tblSMod in objPurchase)
                        {
                            db.TblTempDistributorPurchaseDetails.Remove(tblSMod);
                            db.SaveChanges();

                        }

                    }

                    for (int i = 0; i < PurchaseDetail.Count; i++) // Loop through List with for
                    {
                        var vProductID = PurchaseDetail[i].ProductID;
                        var vQuantity = PurchaseDetail[i].QuantityReceived;
                        decimal vCostPrice = PurchaseDetail[i].UnitCost;
                        decimal vSelllingPrice = PurchaseDetail[i].SellingPrice ?? 0;
                        decimal vMRP = PurchaseDetail[i].MRPPrice ?? 0;

                        var entity = new TblTempDistributorPurchaseDetail
                        {
                            DistributorPurchaseID = DistPurchaseID,
                            Quantity = PurchaseDetail[i].Quantity,
                            QuantityReceived = PurchaseDetail[i].QuantityReceived,
                            SellingPrice = PurchaseDetail[i].SellingPrice,
                            MRPPrice = PurchaseDetail[i].MRPPrice,
                            UnitCost = PurchaseDetail[i].UnitCost,
                            HSNID = PurchaseDetail[i].HSNID,
                            HSNType = PurchaseDetail[i].HSNType,
                            TaxType = PurchaseDetail[i].TaxType,
                            ProductID = PurchaseDetail[i].ProductID,
                            ProductCode = PurchaseDetail[i].ProductCode,
                            PrintName = PurchaseDetail[i].PrintName,
                            RackNo = PurchaseDetail[i].RackNo,
                            ProductTax = PurchaseDetail[i].ProductTax,
                            FreeQuantity = PurchaseDetail[i].FreeQuantity,
                            DiscountPercent = PurchaseDetail[i].DiscountPercent,
                            DiscountAmount = PurchaseDetail[i].DiscountAmount,
                            SchemePercent = PurchaseDetail[i].SchemePercent,
                            SchemeAmount = PurchaseDetail[i].SchemeAmount,
                            TotalAmount = PurchaseDetail[i].TotalAmount,
                            PurchaseStatus = PurchaseStatus,
                            WarehouseID = WarehouseID,
                            CreatedBy = CreatedBy,
                            CreatedDate = CommonRepository.GetTimeZoneDate()
                        };

                        db.Entry(entity).State = EntityState.Added;
                        db.SaveChanges();


                        if ((PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID || PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_PartialReceivedPurchasedStatus_ViewID))
                        {

                            var ObjPro = (from tblSMod in db.TblProductMasters
                                          where tblSMod.ProductID == vProductID
                                          select tblSMod).ToList();

                            if (ObjPro[0].DistributorPrice != vCostPrice)
                            {
                                ObjPro[0].DistributorPrice = PurchaseDetail[i].UnitCost;
                                db.SaveChanges();
                            }

                            var ObjProduct = (from tblSMod in db.TblProductWareHouses
                                              where tblSMod.ProductID == vProductID && tblSMod.WareHouseID == WarehouseID
                                              select tblSMod).ToList();

                            if (ObjProduct != null && ObjProduct.Count == 1)
                            {
                                decimal Dec_FreeQuantity = PurchaseDetail[i].FreeQuantity ?? 0;
                                if (ObjProduct[0].SellingPrice != 0 && ObjProduct[0].MRPPrice != 0 && (ObjProduct[0].SellingPrice != vSelllingPrice || ObjProduct[0].MRPPrice != vMRP))
                                {
                                    ObjProduct[0].Status = false;
                                    db.SaveChanges();

                                    var productstore = new TblProductWareHouse
                                    {
                                        ProductID = PurchaseDetail[i].ProductID,
                                        WareHouseID = WarehouseID,
                                        SellingPrice = PurchaseDetail[i].SellingPrice,
                                        MRPPrice = PurchaseDetail[i].MRPPrice,
                                        DistributorPrice = PurchaseDetail[i].UnitCost,
                                        Quantity = PurchaseDetail[i].QuantityReceived,
                                        FreeQuantity = PurchaseDetail[i].FreeQuantity,
                                        Status = true,
                                        CreatedBy = CreatedBy,
                                        CreatedDate = CommonRepository.GetTimeZoneDate()
                                    };

                                    db.Entry(productstore).State = EntityState.Added;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    //Update Unit Cost, Selling Price & MRP
                                    ObjProduct[0].DistributorPrice = vCostPrice;
                                    ObjProduct[0].SellingPrice = PurchaseDetail[i].SellingPrice;
                                    ObjProduct[0].MRPPrice = PurchaseDetail[i].MRPPrice;
                                    ObjProduct[0].LastModifiedBy = CreatedBy;
                                    ObjProduct[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                                    db.SaveChanges();
                                    //Quantity adjustment
                                    PurchaseQuantity_MinusUpdate(vProductID, WarehouseID, PurchaseDetail[i].QuantityReceivedTemp ?? 0, PurchaseDetail[i].FreeQuantityReceivedTemp ?? 0);
                                    PurchaseQuantity_PlusUpdate(vProductID, WarehouseID, PurchaseDetail[i].QuantityReceived ?? 0, Dec_FreeQuantity);
                                }
                            }
                            if (ObjProduct == null || ObjProduct.Count == 0)
                            {

                                var productstore = new TblProductWareHouse
                                {
                                    ProductID = PurchaseDetail[i].ProductID,
                                    WareHouseID = WarehouseID,
                                    DistributorPrice = PurchaseDetail[i].UnitCost,
                                    SellingPrice = PurchaseDetail[i].SellingPrice,
                                    MRPPrice = PurchaseDetail[i].MRPPrice,
                                    Quantity = PurchaseDetail[i].QuantityReceived,
                                    FreeQuantity = PurchaseDetail[i].FreeQuantity,
                                    Status = true,
                                    CreatedBy = CreatedBy,
                                    CreatedDate = CommonRepository.GetTimeZoneDate()
                                };

                                db.Entry(productstore).State = EntityState.Added;
                                db.SaveChanges();

                            }
                            else if (ObjProduct.Count > 1)
                            {
                                decimal Dec_FreeQuantity = PurchaseDetail[i].FreeQuantity ?? 0;
                                //Update Unit Cost, Selling Price & MRP
                                ObjProduct[0].DistributorPrice = vCostPrice;
                                ObjProduct[0].SellingPrice = PurchaseDetail[i].SellingPrice;
                                ObjProduct[0].MRPPrice = PurchaseDetail[i].MRPPrice;
                                ObjProduct[0].LastModifiedBy = CreatedBy;
                                ObjProduct[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                                db.SaveChanges();
                                //Quantity adjustment
                                PurchaseQuantity_MinusUpdate(vProductID, WarehouseID, PurchaseDetail[i].QuantityReceivedTemp ?? 0, PurchaseDetail[i].FreeQuantityReceivedTemp ?? 0);
                                PurchaseQuantity_PlusUpdate(vProductID, WarehouseID, PurchaseDetail[i].QuantityReceived ?? 0, Dec_FreeQuantity);
                            }

                        }

                    }
                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DistributorPurchaseDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DistributorPurchaseDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }

            return ObjMessage;
        }
        public ReturnMessageModel DistributionPurchases_Update(TempDistributorPurchaseheaderModel objPurchases, List<TempDistributorPurchaseDetailModel> ObjPurchaseDetail)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                var objUpdate = (from ct in db.TblTempDistributorPurchaseheaders where ct.DistributorPurchaseID == objPurchases.DistributorPurchaseID select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    var PaymentStatus = "";

                    if (objPurchases.GrandTotal == objPurchases.Paid || objPurchases.GrandTotal <= objPurchases.Paid)
                    {
                        objPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPaid_ViewID;
                    }
                    else if (objPurchases.Paid == 0 || objPurchases.Paid == null)
                    {
                        objPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusDue_ViewID;
                    }
                    else if (objPurchases.Paid != 0)
                    {
                        objPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPartial_ViewID;
                    }

                    if (objPurchases.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID)
                    {
                        PaymentStatus = objPurchases.PaymentStatus;
                    }
                    if (PaymentStatus == "")
                    {
                        objUpdate[0].PaymentStatus = objPurchases.PaymentStatus;
                    }
                    else
                    {
                        objUpdate[0].PaymentStatus = PaymentStatus;
                    }
                    objUpdate[0].PurchaseDate = objPurchases.PurchaseDate;
                    objUpdate[0].SuperStockistID = objPurchases.SuperStockistID;
                    objUpdate[0].WarehouseID = objPurchases.WarehouseID;
                    objUpdate[0].DistributorLocationID = objPurchases.DistributorLocationID;
                    objUpdate[0].BrandID = objPurchases.BrandID;
                    objUpdate[0].TotalAmount = objPurchases.TotalAmount;
                    objUpdate[0].GrandTotal = objPurchases.GrandTotal;
                    objUpdate[0].DistributorInvoice = objPurchases.DistributorInvoice;
                    if (objPurchases.ReferenceAttachment != null)
                    {
                        objUpdate[0].FileName = objPurchases.FileName;
                        objUpdate[0].ReferenceAttachment = objPurchases.ReferenceAttachment;
                    }
                    objUpdate[0].PurchaseStatus = objPurchases.PurchaseStatus;
                    objUpdate[0].TaxType = objPurchases.TaxType;
                    objUpdate[0].TotalDiscount = objPurchases.TotalDiscount;
                    objUpdate[0].FinalDiscountPercent = objPurchases.FinalDiscountPercent;
                    objUpdate[0].FinalDiscountAmount = objPurchases.FinalDiscountAmount;
                    objUpdate[0].TotalScheme = objPurchases.TotalScheme;
                    objUpdate[0].Shipping = objPurchases.Shipping;
                    objUpdate[0].PaymentTerm = objPurchases.PaymentTerm;
                    objUpdate[0].Note = objPurchases.Note;
                    objUpdate[0].LastModifiedBy = objPurchases.LastModifiedBy;
                    objUpdate[0].LastModifiedDate = objPurchases.LastModifiedDate;


                    db.SaveChanges();

                    var WarehouseID = objPurchases.WarehouseID;
                    var CreatedBy = objPurchases.CreatedBy ?? 0;
                    var PurchaseStatus = objPurchases.PurchaseStatus;


                    ObjMessage = DistributorPurchaseDetail_Save(ObjPurchaseDetail, objPurchases.DistributorPurchaseID, PurchaseStatus, WarehouseID, CreatedBy, objPurchases.SuperStockistID);

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;


                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DistributionPurchases_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DistributionPurchases_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel PurchaseQuantity_PlusUpdate(long Lon_ProductID, string WarehouseID, decimal Quantity, decimal FreeQuantity)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var ObjProduct = (from ct in db.TblProductWareHouses where ct.ProductID == Lon_ProductID && ct.WareHouseID == WarehouseID select ct).ToList();

                if (ObjProduct != null && ObjProduct.Count >= 1)
                {
                    decimal Dec_Quantity = ((ObjProduct[0].Quantity ?? 0) + (Quantity));
                    decimal Dec_FreeQty = ((ObjProduct[0].FreeQuantity ?? 0) + (FreeQuantity));
                    ObjProduct[0].Quantity = Dec_Quantity;
                    ObjProduct[0].FreeQuantity = Dec_FreeQty;
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "PurchaseQuantity_PlusUpdate");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "PurchaseQuantity_PlusUpdate");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }
        public ReturnMessageModel PurchaseQuantity_MinusUpdate(long Lon_ProductID, string WarehouseID, decimal Quantity, decimal FreeQuantity)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var ObjProduct = (from ct in db.TblProductWareHouses
                                  where ct.ProductID == Lon_ProductID && ct.WareHouseID == WarehouseID
                                  select ct).ToList();

                if (ObjProduct != null && ObjProduct.Count >= 1)
                {
                    decimal Dec_Quantity = 0;
                    decimal Dec_FreeQty = 0;
                    if (Quantity > 0)
                    {
                        Dec_Quantity = ((ObjProduct[0].Quantity ?? 0) - (Quantity));
                        ObjProduct[0].Quantity = Dec_Quantity;
                    }
                    if (FreeQuantity > 0)
                    {
                        Dec_FreeQty = ((ObjProduct[0].FreeQuantity ?? 0) - (FreeQuantity));
                        ObjProduct[0].FreeQuantity = Dec_FreeQty;
                    }


                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "PurchaseQuantity_MinusUpdate");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "PurchaseQuantity_MinusUpdate");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }

        public List<TempDistributorPurchaseDetailModel> DistributorPurchaseDetail_FindAll(long DistPurchaseID)
        {
            List<TempDistributorPurchaseDetailModel> ObjPurchaseDetail = null;

            try
            {
                ObjPurchaseDetail = (from PM in db.TblTempDistributorPurchaseDetails
                                     where PM.DistributorPurchaseID == DistPurchaseID
                                     select new TempDistributorPurchaseDetailModel
                                     {
                                         DistributorPurchaseDetailsID = PM.DistributorPurchaseDetailsID,
                                         DistributorPurchaseID = PM.DistributorPurchaseID,
                                         ProductID = PM.ProductID,
                                         ProductCode = PM.ProductCode,
                                         PrintName = PM.PrintName,
                                         UnitCost = PM.UnitCost,
                                         CostPrice = PM.UnitCost,
                                         Quantity = PM.Quantity,
                                         MRPPrice = PM.MRPPrice,//(from PS in db.TblProductWareHouses where PS.ProductID == PM.ProductID && PS.WareHouseID == PM.WarehouseID select PS.MRPPrice ?? 0).FirstOrDefault(),
                                         SellingPrice = PM.SellingPrice, //(from PS in db.TblProductWareHouses where PS.ProductID == PM.ProductID && PS.WareHouseID == PM.WarehouseID select PS.SellingPrice ?? 0).FirstOrDefault(),
                                         WarehouseID = PM.WarehouseID,
                                         ProductTax = PM.ProductTax ?? 0,
                                         HSNID = PM.HSNID,
                                         HSNType = PM.HSNType,
                                         RackNo = PM.RackNo,
                                         TaxType = PM.TaxType,
                                         QuantityReceived = PM.QuantityReceived ?? 0,
                                         QuantityReceivedTemp = PM.QuantityReceived ?? 0,
                                         FreeQuantity = PM.FreeQuantity ?? 0,
                                         FreeQuantityReceivedTemp = PM.FreeQuantity ?? 0,
                                         DiscountPercent = PM.DiscountPercent ?? 0,
                                         DiscountAmount = PM.DiscountAmount ?? 0,
                                         SchemePercent = PM.SchemePercent ?? 0,
                                         SchemeAmount = PM.SchemeAmount ?? 0,
                                         ProductName = PM.PrintName,
                                         //  TotalTax = ((((PM.UnitCost * PM.Quantity) - PM.DiscountAmount) * PM.ProductTax ?? 0) / 100),
                                         TotalAmount = PM.TotalAmount ?? 0,
                                         SubTotal = PM.TotalAmount ?? 0
                                     }).ToList();


                for (int i = 0; i < ObjPurchaseDetail.Count; i++)
                {
                    ObjPurchaseDetail[i].Sno = i + 1;
                }


            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "PurchaseDetail_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "PurchaseDetail_FindAll");
            }
            return ObjPurchaseDetail;
        }
        public PurchaseInvoiceHeaderModel PurchaseInvoiceHeader(string WareHouseID, long DistributorPurchaseID)
        {
            PurchaseInvoiceHeaderModel Obj = new PurchaseInvoiceHeaderModel();

            Obj = (from ct in db.TblTempDistributorPurchaseheaders
                   where ct.WarehouseID == WareHouseID && ct.DistributorPurchaseID == DistributorPurchaseID
                   select new PurchaseInvoiceHeaderModel
                   {
                       BillNumber = ct.ReferenceNO,
                       PurchaseDate = ct.PurchaseDate.ToString(),
                       TotalDiscount = ct.TotalDiscount ?? 0,
                       PaymentStatus = (from UMC in db.TblDefaultMasterDetails where UMC.DefaultDetailID == ct.PaymentStatus select UMC.DefaultTextField).FirstOrDefault(),
                       PurchaseStatus = (from DM in db.TblDefaultMasterDetails where DM.DefaultDetailID == ct.PurchaseStatus select DM.DefaultTextField).FirstOrDefault(),
                       ToName = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.SuperStockistID select CM.DistributorName).FirstOrDefault(),
                       ToAddress1 = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.SuperStockistID select CM.AddressLine1).FirstOrDefault(),
                       ToAddress2 = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.SuperStockistID select CM.AddressLine2).FirstOrDefault(),
                       ToAddress3 = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.SuperStockistID select CM.AddressLine3).FirstOrDefault(),
                       ToPhoneNumber = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.SuperStockistID select CM.MobileNo).FirstOrDefault(),
                       ToEmail = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.SuperStockistID select CM.EmailID).FirstOrDefault(),
                       ToGSTNumber = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.SuperStockistID select CM.GSTNumber).FirstOrDefault()
                   }).FirstOrDefault();

            Obj.StoreName = (from ct in db.TblDefaultMasterDetails where ct.DefaultDetailID == WareHouseID select ct.DefaultTextField).FirstOrDefault();

            var ObjPrintSetting = (from ct in db.TblPrintSettingsMasters
                                   where ct.StoreID == WareHouseID
                                   select new SalesPrintModel
                                   {
                                       CompanyName = ct.CompanyName,
                                       Address1 = ct.Address1,
                                       Address2 = ct.Address2,
                                       PinCode = ct.PinCode,
                                       MobileNumber = ct.MobileNumber,
                                       Email = ct.Email,
                                       FooterMessage = ct.FooterMessage,
                                       GSTNo = ct.GSTNumber
                                   }).FirstOrDefault();
            if (ObjPrintSetting != null)
            {
                Obj.FromName = ObjPrintSetting.CompanyName;
                Obj.FromAddress1 = ObjPrintSetting.Address1;
                Obj.FromAddress2 = ObjPrintSetting.Address2;
                Obj.FromPinCode = ObjPrintSetting.PinCode;
                Obj.FromPhoneNumber = ObjPrintSetting.MobileNumber;
                Obj.FromEmail = ObjPrintSetting.Email;
                Obj.FooterMessage = ObjPrintSetting.FooterMessage;
                Obj.FromGSTNumber = ObjPrintSetting.GSTNo;
            }

            Obj.WareHouseID = WareHouseID;
            Obj.DistributorPurchaseID = DistributorPurchaseID;

            return Obj;
        }

        public PurchaseReturnInvoiceHeaderModel PurchaseReturnInvoiceHeader(string WarehouseID, long PurchaseReturnID)
        {
            PurchaseReturnInvoiceHeaderModel Obj = new PurchaseReturnInvoiceHeaderModel();

            Obj = (from ct in db.TblPurchaseReturns
                   where ct.WareHouseID == WarehouseID && ct.PurchaseReturnID == PurchaseReturnID
                   select new PurchaseReturnInvoiceHeaderModel
                   {
                       BillNumber = ct.ReferenceNO,
                       PurchaseReturnDate = ct.PurchaseReturnDate.ToString(),
                       TotalDiscount = ct.TotalDiscount ?? 0,
                       PaymentStatus = (from UMC in db.TblDefaultMasterDetails where UMC.DefaultDetailID == ct.PaymentStatus select UMC.DefaultTextField).FirstOrDefault(),
                       PurchaseStatus = (from DM in db.TblDefaultMasterDetails where DM.DefaultDetailID == ct.PurchaseReturnStatus select DM.DefaultTextField).FirstOrDefault(),
                       ToName = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.DistributorID select CM.DistributorName).FirstOrDefault(),
                       ToAddress1 = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.DistributorID select CM.AddressLine1).FirstOrDefault(),
                       ToAddress2 = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.DistributorID select CM.AddressLine2).FirstOrDefault(),
                       ToAddress3 = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.DistributorID select CM.AddressLine3).FirstOrDefault(),
                       ToPhoneNumber = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.DistributorID select CM.MobileNo).FirstOrDefault(),
                       ToEmail = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.DistributorID select CM.EmailID).FirstOrDefault(),
                       ToGSTNumber = (from CM in db.TblDistributorMasters where CM.DistributorID == ct.DistributorID select CM.GSTNumber).FirstOrDefault()
                   }).FirstOrDefault();

            Obj.WareHouseName = (from ct in db.TblDefaultMasterDetails where ct.DefaultDetailID == WarehouseID select ct.DefaultTextField).FirstOrDefault();

            var ObjPrintSetting = (from ct in db.TblPrintSettingsMasters
                                   where ct.StoreID == WarehouseID
                                   select new SalesPrintModel
                                   {
                                       CompanyName = ct.CompanyName,
                                       Address1 = ct.Address1,
                                       Address2 = ct.Address2,
                                       PinCode = ct.PinCode,
                                       MobileNumber = ct.MobileNumber,
                                       Email = ct.Email,
                                       FooterMessage = ct.FooterMessage,
                                       GSTNo = ct.GSTNumber
                                   }).FirstOrDefault();
            if (ObjPrintSetting != null)
            {
                Obj.FromName = ObjPrintSetting.CompanyName;
                Obj.FromAddress1 = ObjPrintSetting.Address1;
                Obj.FromAddress2 = ObjPrintSetting.Address2;
                Obj.FromPinCode = ObjPrintSetting.PinCode;
                Obj.FromPhoneNumber = ObjPrintSetting.MobileNumber;
                Obj.FromEmail = ObjPrintSetting.Email;
                Obj.FooterMessage = ObjPrintSetting.FooterMessage;
                Obj.FromGSTNumber = ObjPrintSetting.GSTNo;
            }
            Obj.PurchaseReturnID = PurchaseReturnID;
            Obj.StoreID = WarehouseID;

            return Obj;
        }
        public List<PurchaseInvoiceDetailModel> PurchaseInvoiceDetails(string WareHouseID, long DistributorPurchaseID)
        {

            List<PurchaseInvoiceDetailModel> ObjPurchaseDetails = (from ct in db.TblTempDistributorPurchaseDetails
                                                                   where ct.DistributorPurchaseID == DistributorPurchaseID
                                                                   select new PurchaseInvoiceDetailModel
                                                                   {

                                                                       PrintName = ct.PrintName,
                                                                       UnitCost = ct.UnitCost,
                                                                       Quantity = ct.Quantity,
                                                                       HSNName = (from HS in db.TblHSNMasters where HS.HSNID == ct.HSNID select HS.Code).FirstOrDefault(),
                                                                       HSNPerc = (from HS in db.TblHSNMasters where HS.HSNID == ct.HSNID select HS.CGST + HS.SGST).FirstOrDefault(),
                                                                       DiscountPercent = ct.DiscountPercent ?? 0,
                                                                       DiscountAmount = ct.DiscountAmount ?? 0,
                                                                       Shipping = (from PR in db.TblTempDistributorPurchaseheaders where PR.DistributorPurchaseID == ct.DistributorPurchaseID select PR.Shipping ?? 0).FirstOrDefault(),
                                                                       FinalDiscountAmount = (from PR in db.TblTempDistributorPurchaseheaders where PR.DistributorPurchaseID == ct.DistributorPurchaseID select PR.FinalDiscountAmount ?? 0).FirstOrDefault(),
                                                                       TotalAmount = ct.TotalAmount ?? 0,
                                                                       GrandTotal = (from PR in db.TblTempDistributorPurchaseheaders where PR.DistributorPurchaseID == ct.DistributorPurchaseID select PR.GrandTotal).FirstOrDefault(),
                                                                       HSNAmount = 0
                                                                   }).ToList();
            return ObjPurchaseDetails;

        }
        public List<PurchaseReturnInvoiceDetailModel> PurchaseReturnInvoiceDetails(string WarehouseID, long PurchaseReturnID)
        {

            List<PurchaseReturnInvoiceDetailModel> ObjPurchaseDetails = (from ct in db.TblPurchaseReturnDetails
                                                                         where ct.PurchaseReturnID == PurchaseReturnID
                                                                         select new PurchaseReturnInvoiceDetailModel
                                                                         {
                                                                             ProductCode = ct.ProductCode,
                                                                             PrintName = ct.PrintName,
                                                                             UnitCost = ct.UnitCost,
                                                                             QuantityReturned = ct.QuantityReturned ?? 0,
                                                                             HSNName = (from HS in db.TblHSNMasters where HS.HSNID == ct.HSNID select HS.Code).FirstOrDefault(),
                                                                             HSNPerc = (from HS in db.TblHSNMasters where HS.HSNID == ct.HSNID select HS.CGST + HS.SGST).FirstOrDefault(),
                                                                             TotalAmount = ct.TotalAmount ?? 0,

                                                                             HSNAmount = 0
                                                                         }).ToList();
            return ObjPurchaseDetails;

        }







        public List<TempCompanyPurchaseDetailModel> CompanyPurchaseDetail_FindAll(long CompanyPurchaseID)
        {
            List<TempCompanyPurchaseDetailModel> ObjCompanyPurchaseDetail = null;

            try
            {
                int counter = 0;
                ObjCompanyPurchaseDetail = (from PM in db.TblTempCompanyPurchaseDetails
                                            where PM.CompanyPurchaseID == CompanyPurchaseID
                                            select new TempCompanyPurchaseDetailModel()
                                            {
                                                CompanyPurchaseDetailsID = PM.CompanyPurchaseDetailsID,
                                                CompanyPurchaseID = PM.CompanyPurchaseID,
                                                ProductID = PM.ProductID,
                                                ProductCode = PM.ProductCode,
                                                PrintName = PM.PrintName,
                                                CostPrice = PM.UnitCost,
                                                Quantity = PM.Quantity,
                                                MRPPrice = (from PS in db.TblProductWareHouses where PS.ProductID == PM.ProductID && PS.WareHouseID == PM.WarehouseID select PS.MRPPrice ?? 0).FirstOrDefault(),
                                                SellingPrice = (from PS in db.TblProductWareHouses where PS.ProductID == PM.ProductID && PS.WareHouseID == PM.WarehouseID select PS.SellingPrice ?? 0).FirstOrDefault(),
                                                WarehouseID = PM.WarehouseID,
                                                ProductTax = PM.ProductTax ?? 0,
                                                HSNID = PM.HSNID,
                                                HSNType = PM.HSNType,
                                                TaxType = PM.TaxType,
                                                QuantityReceived = PM.QuantityReceived ?? 0,
                                                QuantityReceivedTemp = PM.QuantityReceived ?? 0,
                                                FreeQuantity = PM.FreeQuantity ?? 0,
                                                FreeQuantityReceivedTemp = PM.FreeQuantity ?? 0,
                                                DiscountPercent = PM.DiscountPercent ?? 0,
                                                DiscountAmount = PM.DiscountAmount ?? 0,
                                                SchemePercent = PM.SchemePercent ?? 0,
                                                SchemeAmount = PM.SchemeAmount ?? 0,
                                                ProductName = PM.PrintName,
                                                RackNo = PM.RackNo == null ? "" : PM.RackNo,
                                                //  TotalTax = ((((PM.UnitCost * PM.Quantity) - PM.DiscountAmount) * PM.ProductTax ?? 0) / 100),
                                                SubTotal = PM.TotalAmount ?? 0,
                                                ModeType = "DB"

                                            }).ToList();


                for (int i = 0; i < ObjCompanyPurchaseDetail.Count; i++)
                {
                    ObjCompanyPurchaseDetail[i].Sno = i + 1;
                }


            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "CompanyPurchaseDetail_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "CompanyPurchaseDetail_FindAll");
            }
            return ObjCompanyPurchaseDetail;
        }

        public TempDistributorPurchaseheaderModel DistributorPurchases_Edit(long DistPurchaseID)
        {
            TempDistributorPurchaseheaderModel ObjPurchases = (from ct in db.TblTempDistributorPurchaseheaders
                                                               where ct.DistributorPurchaseID == DistPurchaseID
                                                               select new TempDistributorPurchaseheaderModel
                                                               {
                                                                   DistributorPurchaseID = ct.DistributorPurchaseID,
                                                                   RunningYear = ct.RunningYear,
                                                                   RunningMonth = ct.RunningMonth,
                                                                   RunningNumber = ct.RunningNumber,
                                                                   ReferenceNO = ct.ReferenceNO,
                                                                   PurchaseDate = ct.PurchaseDate,
                                                                   SuperStockistID = ct.SuperStockistID,
                                                                   WarehouseID = ct.WarehouseID,
                                                                   BrandID = ct.BrandID ?? 0,
                                                                   DistributorInvoice = ct.DistributorInvoice,
                                                                   DistributorLocationID = ct.DistributorLocationID,
                                                                   Shipping = ct.Shipping,
                                                                   PaymentTerm = ct.PaymentTerm,
                                                                   PaymentStatus = ct.PaymentStatus,
                                                                   TotalTax = ct.TotalTax,
                                                                   TotalDiscount = ct.TotalDiscount,
                                                                   TotalAmount = ct.TotalAmount,
                                                                   FinalDiscountPercent = ct.FinalDiscountPercent,
                                                                   FinalDiscountAmount = ct.FinalDiscountAmount,
                                                                   TaxType = ct.TaxType,
                                                                   GrandTotal = ct.GrandTotal,
                                                                   PurchaseStatus = ct.PurchaseStatus,
                                                                   Note = ct.Note,
                                                                   FileName = ct.FileName
                                                               }).FirstOrDefault();


            return ObjPurchases;
        }

        public bool DistributorInvoiceDuplicate(string DistributorInvoiceNumber)
        {
            bool IsDistributorInvoiceDuplicate = false;

            var obj = (from ct in db.TblTempDistributorPurchaseheaders
                       where ct.DistributorInvoice == DistributorInvoiceNumber
                       select ct.DistributorInvoice).FirstOrDefault();

            if (obj != null)
            {
                IsDistributorInvoiceDuplicate = true;
            }

            return IsDistributorInvoiceDuplicate;
        }

        public bool CompanyInvoiceDuplicate(string CompanyInvoiceNumber)
        {
            bool IsCompanyInvoiceDuplicate = false;

            var obj = (from ct in db.TblTempCompanyPurchaseheaders
                       where ct.CompanyInvoice == CompanyInvoiceNumber
                       select ct.CompanyInvoice).FirstOrDefault();

            if (obj != null)
            {
                IsCompanyInvoiceDuplicate = true;
            }

            return IsCompanyInvoiceDuplicate;
        }

        public ReturnMessageModel DistributionPurchase_Approval(long DistPurchaseID, int IsApprove, int ApproveRejectBy)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var ObjPur = (from pur in db.TblTempDistributorPurchaseheaders
                              where pur.DistributorPurchaseID == DistPurchaseID
                              select pur).ToList();
                if (ObjPur.Count > 0)
                {
                    ObjPur[0].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_ApprovedPurchasedStatus_ViewID;
                    ObjPur[0].ApprovedBy = ApproveRejectBy;
                    ObjPur[0].ApprovedDate = CommonRepository.GetTimeZoneDate();
                    db.SaveChanges();

                    ObjMessage.Result = true;

                    if (IsApprove == 1)
                    {
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.PurchaseReceived;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVALSUCCESS;
                    }
                    else
                    {
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.Approval_Rejected;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.REJECTSUCCESS;
                    }

                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Promotions_Approval");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVEREJECTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Promotions_Approval");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVEREJECTFAIL;
            }
            return ObjMessage;
        }
        public TempCompanyPurchaseheaderModel CompanyPurchases_Edit(long CompanyPurchaseID)
        {
            TempCompanyPurchaseheaderModel ObjPurchases = (from ct in db.TblTempCompanyPurchaseheaders
                                                           where ct.CompanyPurchaseID == CompanyPurchaseID
                                                           select new TempCompanyPurchaseheaderModel
                                                           {
                                                               CompanyPurchaseID = ct.CompanyPurchaseID,
                                                               RunningYear = ct.RunningYear,
                                                               RunningMonth = ct.RunningMonth,
                                                               RunningNumber = ct.RunningNumber,
                                                               ReferenceNO = ct.ReferenceNO,
                                                               PurchaseDate = ct.PurchaseDate,
                                                               CompanyID = ct.CompanyID,
                                                               WarehouseID = ct.WarehouseID,
                                                               CompanyInvoice = ct.CompanyInvoice,
                                                               DistributionLocationID = ct.DistributionLocationID,
                                                               FinalDiscountPercent = ct.FinalDiscountPercent,
                                                               FinalDiscountAmount = ct.FinalDiscountAmount,
                                                               Shipping = ct.Shipping,
                                                               BrandID = ct.BrandID,
                                                               PaymentTerm = ct.PaymentTerm,
                                                               PaymentStatus = ct.PaymentStatus,
                                                               TotalTax = ct.TotalTax,
                                                               TotalDiscount = ct.TotalDiscount,
                                                               TotalAmount = ct.TotalAmount,
                                                               TaxType = ct.TaxType,
                                                               GrandTotal = ct.GrandTotal,
                                                               PurchaseStatus = ct.PurchaseStatus,
                                                               Note = ct.Note,
                                                               FileName = ct.FileName
                                                           }).FirstOrDefault();


            return ObjPurchases;
        }
        public ReturnMessageModel Purchase_CompanyApproval(long CompanyPurchaseID, int IsApprove, int ApproveRejectBy)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var ObjPur = (from pur in db.TblTempCompanyPurchaseheaders
                              where pur.CompanyPurchaseID == CompanyPurchaseID
                              select pur).ToList();
                if (ObjPur.Count > 0)
                {
                    ObjPur[0].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_ApprovedPurchasedStatus_ViewID;
                    ObjPur[0].ApprovedBy = ApproveRejectBy;
                    ObjPur[0].ApprovedDate = CommonRepository.GetTimeZoneDate();
                    db.SaveChanges();

                    ObjMessage.Result = true;

                    if (IsApprove == 1)
                    {
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.PurchaseReceived;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVALSUCCESS;
                    }
                    else
                    {
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.Approval_Rejected;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.REJECTSUCCESS;
                    }

                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Promotions_Approval");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVEREJECTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Promotions_Approval");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.APPROVEREJECTFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel Purchase_CompanyCreatePO(long CompanyPurchaseID, int CreatedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var ObjPur = (from pur in db.TblTempCompanyPurchaseheaders
                              where pur.CompanyPurchaseID == CompanyPurchaseID
                              select pur).ToList();

                if (ObjPur.Count > 0)
                {
                    ObjPur[0].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_OrderedPurchasedStatus_ViewID;
                    ObjPur[0].LastModifiedBy = CreatedBy;
                    ObjPur[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                    db.SaveChanges();

                    ObjMessage.Result = true;

                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.PurchaseReceived;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.ADDSUCCESS;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Purchase_CreatePO");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Purchase_CreatePO");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }

        public List<TempCompanyPurchaseheaderModel> CompanyPurchase_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Company, string PurchaseStatus, string sortBy, string direction, out int TotalCount)
        {
            DateTime ObjPurchaseDate = DateTime.Now;
            if (PurchaseDate != "")
            {
                ObjPurchaseDate = Convert.ToDateTime(PurchaseDate);
            }
            List<TempCompanyPurchaseheaderModel> ObjCompanyPurchases = (from ct in db.TblTempCompanyPurchaseheaders
                                                                        join SM in db.TblCompanyMasters on ct.CompanyID equals SM.CompanyID
                                                                        where
                                                                              ((PurchaseDate == "" || PurchaseDate == null) || ct.PurchaseDate == ObjPurchaseDate)
                                                                            && ((PurchaseRefNo == "" || PurchaseRefNo == null) || ct.ReferenceNO.Contains(PurchaseRefNo))
                                                                            && ((Company == "" || Company == null) || SM.CompanyName.Contains(Company))
                                                                            && ((PurchaseStatus == "" || PurchaseStatus == null) || ct.PurchaseStatus == PurchaseStatus)

                                                                        select new TempCompanyPurchaseheaderModel
                                                                        {
                                                                            CompanyPurchaseID = ct.CompanyPurchaseID,
                                                                            ReferenceNO = ct.ReferenceNO,
                                                                            PurchaseDate = ct.PurchaseDate,
                                                                            CompanyID = ct.CompanyID,
                                                                            CompanyInvoice = ct.CompanyInvoice,
                                                                            WarehouseID = ct.WarehouseID,
                                                                            GrandTotal = ct.GrandTotal,
                                                                            PurchaseStatus = ct.PurchaseStatus,
                                                                            CompanyName = SM.CompanyName,
                                                                            CreatedDate = ct.CreatedDate,
                                                                            Quantity = (from PM in db.TblTempCompanyPurchaseDetails where PM.CompanyPurchaseID == ct.CompanyPurchaseID select PM.Quantity).DefaultIfEmpty(0).Sum(),
                                                                            FreeQuantity = (from PM in db.TblTempCompanyPurchaseDetails where PM.CompanyPurchaseID == ct.CompanyPurchaseID select PM.FreeQuantity ?? 0).DefaultIfEmpty(0).Sum(),
                                                                            PurchaseStatusStr = (from DM in db.TblDefaultMasterDetails where DM.DefaultDetailID == ct.PurchaseStatus select DM.DefaultTextField).FirstOrDefault(),
                                                                            CreatedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.CreatedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                                                                            LastModifiedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.LastModifiedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                                                                            ApprovedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.ApprovedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault()
                                                                        }).ToList();
            //}).Where(a => a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_RequestedPurchasedStatus_ViewID
            //       || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_CancelledPurchaseStatus_ViewID
            //       || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ApprovedPurchasedStatus_ViewID
            //       || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_OrderedPurchasedStatus_ViewID).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "ReferenceNO":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.ReferenceNO).ToList();
                            break;
                        case "PurchaseDate":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.PurchaseDate).ToList();
                            break;
                        case "GrandTotal":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.GrandTotal).ToList();
                            break;
                        case "CompanyName":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.CompanyName).ToList();
                            break;
                        case "PurchaseStatusStr":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.PurchaseStatusStr).ToList();
                            break;
                        case "Quantity":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.Quantity).ToList();
                            break;
                    }
                }
                else
                {

                    switch (sortBy.Trim())
                    {
                        case "ReferenceNO":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.ReferenceNO).ToList();
                            break;
                        case "PurchaseDate":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.PurchaseDate).ToList();
                            break;
                        case "GrandTotal":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.GrandTotal).ToList();
                            break;
                        case "DistributorName":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.CompanyName).ToList();
                            break;
                        case "PurchaseStatusStr":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.PurchaseStatusStr).ToList();
                            break;
                        case "Quantity":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.Quantity).ToList();
                            break;

                    }
                }
            }
            else
            {
                ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = ObjCompanyPurchases.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjCompanyPurchases = ObjCompanyPurchases.Skip(start).Take(limit.Value).ToList();
            }

            return ObjCompanyPurchases;
        }

        public ReturnMessageModel CompanyPurchases_Save(TempCompanyPurchaseheaderModel ObjCompanyPurchases, List<TempCompanyPurchaseDetailModel> ObjCompanyPurchaseDetail)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var obj = (from tblSMod in db.TblTempCompanyPurchaseheaders
                           where tblSMod.CompanyPurchaseID != ObjCompanyPurchases.CompanyPurchaseID && tblSMod.RunningNumber == ObjCompanyPurchases.RunningNumber
                           && tblSMod.RunningYear == ObjCompanyPurchases.RunningYear && tblSMod.RunningMonth == ObjCompanyPurchases.RunningMonth
                           select tblSMod).ToList();

                if (obj != null && obj.Count == 0)
                {
                    if (ObjCompanyPurchases.CompanyPurchaseID == 0)
                    {
                        ObjMessage = CompanyPurchases_Insert(ObjCompanyPurchases, ObjCompanyPurchaseDetail);////insert
                    }
                    else
                    {
                        ObjMessage = CompanyPurchases_Update(ObjCompanyPurchases, ObjCompanyPurchaseDetail);
                    }
                }
                else
                {
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DUPLICATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DUPLICATEFAIL;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "CompanyPurchases_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "CompanyPurchases_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }

        public ReturnMessageModel CompanyPurchases_Update(TempCompanyPurchaseheaderModel ObjCompanyPurchases, List<TempCompanyPurchaseDetailModel> ObjCompanyPurchaseDetail)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                var objUpdate = (from ct in db.TblTempCompanyPurchaseheaders where ct.CompanyPurchaseID == ObjCompanyPurchases.CompanyPurchaseID select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    var PaymentStatus = "";

                    if (ObjCompanyPurchases.GrandTotal == ObjCompanyPurchases.Paid || ObjCompanyPurchases.GrandTotal <= ObjCompanyPurchases.Paid)
                    {
                        ObjCompanyPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPaid_ViewID;
                    }
                    else if (ObjCompanyPurchases.Paid == 0 || ObjCompanyPurchases.Paid == null)
                    {
                        ObjCompanyPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusDue_ViewID;
                    }
                    else if (ObjCompanyPurchases.Paid != 0)
                    {
                        ObjCompanyPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPartial_ViewID;
                    }

                    if (ObjCompanyPurchases.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID)
                    {
                        PaymentStatus = ObjCompanyPurchases.PaymentStatus;
                    }
                    if (PaymentStatus == "")
                    {
                        objUpdate[0].PaymentStatus = ObjCompanyPurchases.PaymentStatus;
                    }
                    else
                    {
                        objUpdate[0].PaymentStatus = PaymentStatus;
                    }
                    objUpdate[0].PurchaseDate = ObjCompanyPurchases.PurchaseDate;
                    objUpdate[0].CompanyID = ObjCompanyPurchases.CompanyID;
                    objUpdate[0].WarehouseID = ObjCompanyPurchases.WarehouseID;
                    objUpdate[0].BrandID = ObjCompanyPurchases.BrandID;
                    objUpdate[0].DistributionLocationID = ObjCompanyPurchases.DistributionLocationID;
                    objUpdate[0].TotalAmount = ObjCompanyPurchases.TotalAmount;
                    objUpdate[0].GrandTotal = ObjCompanyPurchases.GrandTotal;
                    objUpdate[0].CompanyInvoice = ObjCompanyPurchases.CompanyInvoice;
                    objUpdate[0].WarehouseID = ObjCompanyPurchases.WarehouseID;

                    if (ObjCompanyPurchases.ReferenceAttachment != null)
                    {
                        objUpdate[0].FileName = ObjCompanyPurchases.FileName;
                        objUpdate[0].ReferenceAttachment = ObjCompanyPurchases.ReferenceAttachment;
                    }
                    objUpdate[0].PurchaseStatus = ObjCompanyPurchases.PurchaseStatus;
                    objUpdate[0].TaxType = ObjCompanyPurchases.TaxType;
                    objUpdate[0].FinalDiscountPercent = ObjCompanyPurchases.FinalDiscountPercent;
                    objUpdate[0].FinalDiscountAmount = ObjCompanyPurchases.FinalDiscountAmount;
                    objUpdate[0].TotalDiscount = ObjCompanyPurchases.TotalDiscount;
                    objUpdate[0].TotalScheme = ObjCompanyPurchases.TotalScheme;
                    objUpdate[0].Shipping = ObjCompanyPurchases.Shipping;
                    objUpdate[0].PaymentTerm = ObjCompanyPurchases.PaymentTerm;
                    objUpdate[0].Note = ObjCompanyPurchases.Note;
                    objUpdate[0].LastModifiedBy = ObjCompanyPurchases.LastModifiedBy;
                    objUpdate[0].LastModifiedDate = ObjCompanyPurchases.LastModifiedDate;


                    db.SaveChanges();

                    var WarehouseID = ObjCompanyPurchases.WarehouseID;
                    var CreatedBy = ObjCompanyPurchases.CreatedBy ?? 0;
                    var PurchaseStatus = ObjCompanyPurchases.PurchaseStatus;


                    ObjMessage = CompanyPurchaseDetail_Save(ObjCompanyPurchaseDetail, ObjCompanyPurchases.CompanyPurchaseID, PurchaseStatus, WarehouseID, CreatedBy, ObjCompanyPurchases.CompanyID);

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;


                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "CompanyPurchases_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "CompanyPurchases_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel CompanyPurchases_Insert(TempCompanyPurchaseheaderModel ObjCompanyPurchases, List<TempCompanyPurchaseDetailModel> ObjCompanyPurchaseDetail)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var PaymentStatus = "";

                if (ObjCompanyPurchases.GrandTotal == ObjCompanyPurchases.Paid || ObjCompanyPurchases.GrandTotal <= ObjCompanyPurchases.Paid)
                {
                    ObjCompanyPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPaid_ViewID;
                }
                else if (ObjCompanyPurchases.Paid == 0 || ObjCompanyPurchases.Paid == null)
                {
                    ObjCompanyPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusDue_ViewID;
                }
                else if (ObjCompanyPurchases.Paid != 0)
                {
                    ObjCompanyPurchases.PaymentStatus = DEFAULTMASTER.DefaultDetailMaster_PaymentStatusPartial_ViewID;
                }

                if (ObjCompanyPurchases.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID)
                {
                    PaymentStatus = ObjCompanyPurchases.PaymentStatus;
                }
                if (ObjCompanyPurchases.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_RequestedPurchasedStatus_ViewID) ;
                {
                    PaymentStatus = ObjCompanyPurchases.PaymentStatus;
                }
                var entity = new TblTempCompanyPurchaseheader
                {
                    RunningYear = ObjCompanyPurchases.RunningYear,
                    RunningMonth = ObjCompanyPurchases.RunningMonth,
                    RunningNumber = Convert.ToString(ObjCompanyPurchases.RunningNumber),
                    ReferenceNO = ObjCompanyPurchases.ReferenceNO,
                    PurchaseDate = ObjCompanyPurchases.PurchaseDate,
                    CompanyID = ObjCompanyPurchases.CompanyID,
                    WarehouseID = ObjCompanyPurchases.WarehouseID,
                    BrandID = ObjCompanyPurchases.BrandID,
                    DistributionLocationID = ObjCompanyPurchases.DistributionLocationID,
                    CompanyInvoice = ObjCompanyPurchases.CompanyInvoice,
                    Shipping = ObjCompanyPurchases.Shipping,
                    TaxType = ObjCompanyPurchases.TaxType,
                    Note = ObjCompanyPurchases.Note,
                    TotalTax = ObjCompanyPurchases.TotalTax,
                    TotalDiscount = ObjCompanyPurchases.TotalDiscount,
                    TotalScheme = ObjCompanyPurchases.TotalScheme,
                    TotalAmount = ObjCompanyPurchases.TotalAmount,
                    FinalDiscountPercent = ObjCompanyPurchases.FinalDiscountPercent,
                    FinalDiscountAmount = ObjCompanyPurchases.FinalDiscountAmount,
                    GrandTotal = ObjCompanyPurchases.GrandTotal,
                    FileName = ObjCompanyPurchases.FileName,
                    ReferenceAttachment = ObjCompanyPurchases.ReferenceAttachment,
                    PurchaseStatus = ObjCompanyPurchases.PurchaseStatus,
                    PaymentStatus = PaymentStatus,
                    CreatedBy = ObjCompanyPurchases.CreatedBy,
                    CreatedDate = CommonRepository.GetTimeZoneDate()
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();

                var vPurchaseID = entity.CompanyPurchaseID;
                var WarehouseID = ObjCompanyPurchases.WarehouseID;
                var CreatedBy = ObjCompanyPurchases.CreatedBy ?? 0;
                var PurchaseStatus = ObjCompanyPurchases.PurchaseStatus;


                ObjMessage = CompanyPurchaseDetail_Save(ObjCompanyPurchaseDetail, vPurchaseID, PurchaseStatus, WarehouseID, CreatedBy, ObjCompanyPurchases.CompanyID);

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "CompanyPurchases_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "CompanyPurchases_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;

        }

        public ReturnMessageModel Purchase_ProductDetailInsert(TempCompanyPurchaseDetailModel ObjCompanyPurchase)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                        var entity = new TblTempCompanyPurchaseDetail
                        {
                            CompanyPurchaseID = ObjCompanyPurchase.CompanyPurchaseID,
                            Quantity = ObjCompanyPurchase.Quantity,
                            QuantityReceived = ObjCompanyPurchase.QuantityReceived,
                            SellingPrice = ObjCompanyPurchase.SellingPrice,
                            MRPPrice = ObjCompanyPurchase.MRPPrice,
                            UnitCost = ObjCompanyPurchase.UnitCost,
                            HSNID = ObjCompanyPurchase.HSNID,
                            HSNType = ObjCompanyPurchase.HSNType,
                            TaxType = ObjCompanyPurchase.TaxType,
                            ProductID = ObjCompanyPurchase.ProductID,
                            ProductCode = ObjCompanyPurchase.ProductCode,
                            PrintName = ObjCompanyPurchase.PrintName,
                            ProductTax = ObjCompanyPurchase.ProductTax,
                            FreeQuantity = ObjCompanyPurchase.FreeQuantity,
                            RackNo = ObjCompanyPurchase.RackNo,
                            DiscountPercent = ObjCompanyPurchase.DiscountPercent,
                            DiscountAmount = ObjCompanyPurchase.DiscountAmount,
                            SchemePercent = ObjCompanyPurchase.SchemePercent,
                            SchemeAmount = ObjCompanyPurchase.SchemeAmount,
                            TotalAmount = ObjCompanyPurchase.TotalAmount,
                            PurchaseStatus = ObjCompanyPurchase.PurchaseStatus,
                            WarehouseID = ObjCompanyPurchase.WarehouseID,
                            CreatedBy = ObjCompanyPurchase.CreatedBy,
                            CreatedDate = CommonRepository.GetTimeZoneDate()
                        };

                        db.Entry(entity).State = EntityState.Added;
                        db.SaveChanges();


                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
            }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    ObjCom.LogPageError(e, "CompanyPurchases_Insert");
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
                }
                catch (Exception e)
                {
                    ObjCom.GlobalError(e, "CompanyPurchases_Insert");
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
                }
                return ObjMessage;

            }

        private ReturnMessageModel CompanyPurchaseDetail_Save(List<TempCompanyPurchaseDetailModel> ObjCompanyPurchaseDetail, long CompanyPurchaseID, string PurchaseStatus, string WarehouseID, int CreatedBy, int CompanyID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                if (ObjCompanyPurchaseDetail != null && ObjCompanyPurchaseDetail.Count > 0)
                {

                    var objCompanyPurchase = (from tblSMod in db.TblTempCompanyPurchaseDetails
                                              where tblSMod.CompanyPurchaseID == CompanyPurchaseID
                                              select tblSMod).ToList();

                    if (objCompanyPurchase != null && objCompanyPurchase.Count > 0)
                    {
                        foreach (var tblSMod in objCompanyPurchase)
                        {
                            db.TblTempCompanyPurchaseDetails.Remove(tblSMod);
                            db.SaveChanges();

                        }

                    }

                    for (int i = 0; i < ObjCompanyPurchaseDetail.Count; i++) // Loop through List with for
                    {
                        var vProductID = ObjCompanyPurchaseDetail[i].ProductID;
                        var vQuantity = ObjCompanyPurchaseDetail[i].QuantityReceived;
                        decimal vCostPrice = ObjCompanyPurchaseDetail[i].UnitCost;
                        decimal vSelllingPrice = ObjCompanyPurchaseDetail[i].SellingPrice ?? 0;
                        decimal vMRP = ObjCompanyPurchaseDetail[i].MRPPrice ?? 0;

                        var entity = new TblTempCompanyPurchaseDetail
                        {
                            CompanyPurchaseID = CompanyPurchaseID,
                            Quantity = ObjCompanyPurchaseDetail[i].Quantity,
                            QuantityReceived = ObjCompanyPurchaseDetail[i].QuantityReceived,
                            SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice,
                            MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice,
                            UnitCost = ObjCompanyPurchaseDetail[i].UnitCost,
                            HSNID = ObjCompanyPurchaseDetail[i].HSNID,
                            HSNType = ObjCompanyPurchaseDetail[i].HSNType,
                            TaxType = ObjCompanyPurchaseDetail[i].TaxType,
                            ProductID = ObjCompanyPurchaseDetail[i].ProductID,
                            ProductCode = ObjCompanyPurchaseDetail[i].ProductCode,
                            PrintName = ObjCompanyPurchaseDetail[i].PrintName,
                            ProductTax = ObjCompanyPurchaseDetail[i].ProductTax,
                            FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity,
                            RackNo = ObjCompanyPurchaseDetail[i].RackNo,
                            DiscountPercent = ObjCompanyPurchaseDetail[i].DiscountPercent,
                            DiscountAmount = ObjCompanyPurchaseDetail[i].DiscountAmount,
                            SchemePercent = ObjCompanyPurchaseDetail[i].SchemePercent,
                            SchemeAmount = ObjCompanyPurchaseDetail[i].SchemeAmount,
                            TotalAmount = ObjCompanyPurchaseDetail[i].TotalAmount,
                            PurchaseStatus = PurchaseStatus,
                            WarehouseID = WarehouseID,
                            CreatedBy = CreatedBy,
                            CreatedDate = CommonRepository.GetTimeZoneDate()
                        };

                        db.Entry(entity).State = EntityState.Added;
                        db.SaveChanges();


                        if ((PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID || PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_PartialReceivedPurchasedStatus_ViewID))
                        {

                            var ObjPro = (from tblSMod in db.TblProductMasters
                                          where tblSMod.ProductID == vProductID
                                          select tblSMod).ToList();

                            if (ObjPro[0].DistributorPrice != vCostPrice)
                            {
                                ObjPro[0].DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost;
                                db.SaveChanges();
                            }

                            var ObjProduct = (from tblSMod in db.TblProductWareHouses
                                              where tblSMod.ProductID == vProductID && tblSMod.WareHouseID == WarehouseID
                                              select tblSMod).ToList();

                            if (ObjProduct != null && ObjProduct.Count == 1)
                            {
                                decimal Dec_FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity ?? 0;

                                if ((ObjProduct[0].SellingPrice != vSelllingPrice && vSelllingPrice != 0) || (ObjProduct[0].MRPPrice != vMRP && vMRP != 0))
                                {

                                    var productstore = new TblProductWareHouse
                                    {
                                        ProductID = ObjCompanyPurchaseDetail[i].ProductID,
                                        WareHouseID = WarehouseID,
                                        SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice,
                                        MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice,
                                        DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost,
                                        Quantity = ObjCompanyPurchaseDetail[i].QuantityReceived,
                                        FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity,
                                        Status = true,
                                        CreatedBy = CreatedBy,
                                        CreatedDate = CommonRepository.GetTimeZoneDate()
                                    };

                                    db.Entry(productstore).State = EntityState.Added;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    //Update Unit Cost, Selling Price & MRP
                                    if (ObjCompanyPurchaseDetail[i].UnitCost != 0)
                                    {
                                        ObjProduct[0].DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost;
                                    }
                                    if (ObjCompanyPurchaseDetail[i].SellingPrice != 0)
                                    {
                                        ObjProduct[0].SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice;
                                    }
                                    if (ObjCompanyPurchaseDetail[i].MRPPrice != 0)
                                    {
                                        ObjProduct[0].MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice;
                                    }
                                    db.SaveChanges();
                                    //Quantity adjustment
                                    PurchaseQuantity_MinusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceivedTemp ?? 0, ObjCompanyPurchaseDetail[i].FreeQuantityReceivedTemp ?? 0);
                                    PurchaseQuantity_PlusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceived ?? 0, Dec_FreeQuantity);
                                }
                            }
                            if (ObjProduct == null || ObjProduct.Count == 0)
                            {

                                var productstore = new TblProductWareHouse
                                {
                                    ProductID = ObjCompanyPurchaseDetail[i].ProductID,
                                    WareHouseID = WarehouseID,
                                    DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost,
                                    SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice,
                                    MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice,
                                    Quantity = ObjCompanyPurchaseDetail[i].QuantityReceived,
                                    FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity,
                                    Status = true,
                                    CreatedBy = CreatedBy,
                                    CreatedDate = CommonRepository.GetTimeZoneDate()
                                };

                                db.Entry(productstore).State = EntityState.Added;
                                db.SaveChanges();

                            }
                            else if (ObjProduct.Count > 1)
                            {
                                decimal Dec_FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity ?? 0;
                                //Update Unit Cost, Selling Price & MRP
                                ObjProduct[0].DistributorPrice = vCostPrice;
                                ObjProduct[0].SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice;
                                ObjProduct[0].MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice;
                                ObjProduct[0].LastModifiedBy = CreatedBy;
                                ObjProduct[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                                db.SaveChanges();
                                //Quantity adjustment
                                PurchaseQuantity_MinusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceivedTemp ?? 0, ObjCompanyPurchaseDetail[i].FreeQuantityReceivedTemp ?? 0);
                                PurchaseQuantity_PlusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceived ?? 0, Dec_FreeQuantity);
                            }
                        }

                    }
                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "CompanyPurchaseDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "CompanyPurchaseDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }

            return ObjMessage;
        }

        private ReturnMessageModel CompanyPurchaseDetail_SaveSno(List<TempCompanyPurchaseDetailModel> ObjCompanyPurchaseDetail, long CompanyPurchaseID, string PurchaseStatus, string WarehouseID, int CreatedBy, int CompanyID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                if (ObjCompanyPurchaseDetail != null && ObjCompanyPurchaseDetail.Count > 0)
                {

                   

                    for (int i = 0; i < ObjCompanyPurchaseDetail.Count; i++) // Loop through List with for
                    {

                        var vProductID = ObjCompanyPurchaseDetail[i].ProductID;
                        var vQuantity = ObjCompanyPurchaseDetail[i].QuantityReceived;
                        decimal vCostPrice = ObjCompanyPurchaseDetail[i].UnitCost;
                        decimal vSelllingPrice = ObjCompanyPurchaseDetail[i].SellingPrice ?? 0;
                        decimal vMRP = ObjCompanyPurchaseDetail[i].MRPPrice ?? 0;
                        var vCompanyPurchaseDetailsID=ObjCompanyPurchaseDetail[i].CompanyPurchaseDetailsID;

                        var objCompanyPurchase = (from tblSMod in db.TblTempCompanyPurchaseDetails
                                                  where tblSMod.CompanyPurchaseID == CompanyPurchaseID && tblSMod.CompanyPurchaseDetailsID == vCompanyPurchaseDetailsID
                                                  select tblSMod).ToList();

                        if (objCompanyPurchase != null && objCompanyPurchase.Count > 0)
                        {
                            objCompanyPurchase[0].CompanyPurchaseID = CompanyPurchaseID;
                            objCompanyPurchase[0].Quantity = ObjCompanyPurchaseDetail[i].Quantity;
                            objCompanyPurchase[0].QuantityReceived = ObjCompanyPurchaseDetail[i].QuantityReceived;
                            objCompanyPurchase[0].SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice;
                            objCompanyPurchase[0].MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice;
                            objCompanyPurchase[0].UnitCost = ObjCompanyPurchaseDetail[i].UnitCost;
                            objCompanyPurchase[0].HSNID = ObjCompanyPurchaseDetail[i].HSNID;
                            objCompanyPurchase[0].HSNType = ObjCompanyPurchaseDetail[i].HSNType;
                            objCompanyPurchase[0].TaxType = ObjCompanyPurchaseDetail[i].TaxType;
                            objCompanyPurchase[0].ProductID = ObjCompanyPurchaseDetail[i].ProductID;
                            objCompanyPurchase[0].ProductCode = ObjCompanyPurchaseDetail[i].ProductCode;
                            objCompanyPurchase[0].PrintName = ObjCompanyPurchaseDetail[i].PrintName;
                            objCompanyPurchase[0].ProductTax = ObjCompanyPurchaseDetail[i].ProductTax;
                            objCompanyPurchase[0].FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity;
                            objCompanyPurchase[0].RackNo = ObjCompanyPurchaseDetail[i].RackNo;
                            objCompanyPurchase[0].DiscountPercent = ObjCompanyPurchaseDetail[i].DiscountPercent;
                            objCompanyPurchase[0].DiscountAmount = ObjCompanyPurchaseDetail[i].DiscountAmount;
                            objCompanyPurchase[0].SchemePercent = ObjCompanyPurchaseDetail[i].SchemePercent;
                            objCompanyPurchase[0].SchemeAmount = ObjCompanyPurchaseDetail[i].SchemeAmount;
                            objCompanyPurchase[0].TotalAmount = ObjCompanyPurchaseDetail[i].TotalAmount;
                            objCompanyPurchase[0].PurchaseStatus = PurchaseStatus;
                            objCompanyPurchase[0].WarehouseID = WarehouseID;
                            objCompanyPurchase[0].LastModifiedBy = CreatedBy;
                            objCompanyPurchase[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                            db.SaveChanges();
                        }
                        else
                        {
                            var entity = new TblTempCompanyPurchaseDetail
                            {
                                CompanyPurchaseID = CompanyPurchaseID,
                                Quantity = ObjCompanyPurchaseDetail[i].Quantity,
                                QuantityReceived = ObjCompanyPurchaseDetail[i].QuantityReceived,
                                SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice,
                                MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice,
                                UnitCost = ObjCompanyPurchaseDetail[i].UnitCost,
                                HSNID = ObjCompanyPurchaseDetail[i].HSNID,
                                HSNType = ObjCompanyPurchaseDetail[i].HSNType,
                                TaxType = ObjCompanyPurchaseDetail[i].TaxType,
                                ProductID = ObjCompanyPurchaseDetail[i].ProductID,
                                ProductCode = ObjCompanyPurchaseDetail[i].ProductCode,
                                PrintName = ObjCompanyPurchaseDetail[i].PrintName,
                                ProductTax = ObjCompanyPurchaseDetail[i].ProductTax,
                                FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity,
                                RackNo = ObjCompanyPurchaseDetail[i].RackNo,
                                DiscountPercent = ObjCompanyPurchaseDetail[i].DiscountPercent,
                                DiscountAmount = ObjCompanyPurchaseDetail[i].DiscountAmount,
                                SchemePercent = ObjCompanyPurchaseDetail[i].SchemePercent,
                                SchemeAmount = ObjCompanyPurchaseDetail[i].SchemeAmount,
                                TotalAmount = ObjCompanyPurchaseDetail[i].TotalAmount,
                                PurchaseStatus = PurchaseStatus,
                                WarehouseID = WarehouseID,
                                CreatedBy = CreatedBy,
                                CreatedDate = CommonRepository.GetTimeZoneDate()
                            };

                            db.Entry(entity).State = EntityState.Added;
                            db.SaveChanges();
                        }


                            if ((PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ReceivedPurchasedStatus_ViewID || PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_PartialReceivedPurchasedStatus_ViewID))
                            {

                                var ObjPro = (from tblSMod in db.TblProductMasters
                                              where tblSMod.ProductID == vProductID
                                              select tblSMod).ToList();

                                if (ObjPro[0].DistributorPrice != vCostPrice)
                                {
                                    ObjPro[0].DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost;
                                    db.SaveChanges();
                                }

                                var ObjProduct = (from tblSMod in db.TblProductWareHouses
                                                  where tblSMod.ProductID == vProductID && tblSMod.WareHouseID == WarehouseID
                                                  select tblSMod).ToList();

                                if (ObjProduct != null && ObjProduct.Count == 1)
                                {
                                    decimal Dec_FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity ?? 0;

                                    if ((ObjProduct[0].SellingPrice != vSelllingPrice && vSelllingPrice != 0) || (ObjProduct[0].MRPPrice != vMRP && vMRP != 0))
                                    {

                                        var productstore = new TblProductWareHouse
                                        {
                                            ProductID = ObjCompanyPurchaseDetail[i].ProductID,
                                            WareHouseID = WarehouseID,
                                            SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice,
                                            MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice,
                                            DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost,
                                            Quantity = ObjCompanyPurchaseDetail[i].QuantityReceived,
                                            FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity,
                                            Status = true,
                                            CreatedBy = CreatedBy,
                                            CreatedDate = CommonRepository.GetTimeZoneDate()
                                        };

                                        db.Entry(productstore).State = EntityState.Added;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        //Update Unit Cost, Selling Price & MRP
                                        if (ObjCompanyPurchaseDetail[i].UnitCost != 0)
                                        {
                                            ObjProduct[0].DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost;
                                        }
                                        if (ObjCompanyPurchaseDetail[i].SellingPrice != 0)
                                        {
                                            ObjProduct[0].SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice;
                                        }
                                        if (ObjCompanyPurchaseDetail[i].MRPPrice != 0)
                                        {
                                            ObjProduct[0].MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice;
                                        }
                                        db.SaveChanges();
                                        //Quantity adjustment
                                        PurchaseQuantity_MinusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceivedTemp ?? 0, ObjCompanyPurchaseDetail[i].FreeQuantityReceivedTemp ?? 0);
                                        PurchaseQuantity_PlusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceived ?? 0, Dec_FreeQuantity);
                                    }
                                }
                                if (ObjProduct == null || ObjProduct.Count == 0)
                                {

                                    var productstore = new TblProductWareHouse
                                    {
                                        ProductID = ObjCompanyPurchaseDetail[i].ProductID,
                                        WareHouseID = WarehouseID,
                                        DistributorPrice = ObjCompanyPurchaseDetail[i].UnitCost,
                                        SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice,
                                        MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice,
                                        Quantity = ObjCompanyPurchaseDetail[i].QuantityReceived,
                                        FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity,
                                        Status = true,
                                        CreatedBy = CreatedBy,
                                        CreatedDate = CommonRepository.GetTimeZoneDate()
                                    };

                                    db.Entry(productstore).State = EntityState.Added;
                                    db.SaveChanges();

                                }
                                else if (ObjProduct.Count > 1)
                                {
                                    decimal Dec_FreeQuantity = ObjCompanyPurchaseDetail[i].FreeQuantity ?? 0;
                                    //Update Unit Cost, Selling Price & MRP
                                    ObjProduct[0].DistributorPrice = vCostPrice;
                                    ObjProduct[0].SellingPrice = ObjCompanyPurchaseDetail[i].SellingPrice;
                                    ObjProduct[0].MRPPrice = ObjCompanyPurchaseDetail[i].MRPPrice;
                                    ObjProduct[0].LastModifiedBy = CreatedBy;
                                    ObjProduct[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                                    db.SaveChanges();
                                    //Quantity adjustment
                                    PurchaseQuantity_MinusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceivedTemp ?? 0, ObjCompanyPurchaseDetail[i].FreeQuantityReceivedTemp ?? 0);
                                    PurchaseQuantity_PlusUpdate(vProductID, WarehouseID, ObjCompanyPurchaseDetail[i].QuantityReceived ?? 0, Dec_FreeQuantity);
                                }
                            }
                    }


                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "CompanyPurchaseDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "CompanyPurchaseDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }

            return ObjMessage;
        }
        public List<CompanyMasterModel> PurchaseCompany_Search(string CompanyName, string CompanyCode, string CompanyTypeName, string MobileNo, string GSTNumber)
        {
            List<CompanyMasterModel> ObjCompany = new List<CompanyMasterModel>();

            ObjCompany = (from SM in db.TblCompanyMasters
                          where SM.Status == true
                          &&
                          (CompanyName == "" || SM.CompanyName.Contains(CompanyName))
                           &&
                          (CompanyCode == "" || SM.CompanyCode.Contains(CompanyCode))
                          &&
                          (CompanyTypeName == "" || SM.CompanyType.Contains(CompanyTypeName))
                          &&
                          (MobileNo == "" || SM.MobileNo.Contains(MobileNo))
                          &&
                          (GSTNumber == "" || SM.GSTNumber.Contains(GSTNumber))


                          select new CompanyMasterModel
                          {
                              CompanyID = SM.CompanyID,
                              CompanyName = SM.CompanyName,
                              CompanyCode = SM.CompanyCode,
                              CompanyType = SM.CompanyType,
                              CompanyTypeName = (from UMC in db.TblDefaultMasterDetails where UMC.DefaultDetailID == SM.CompanyType select UMC.DefaultTextField).FirstOrDefault(),

                              GSTNumber = SM.GSTNumber,
                              MobileNo = SM.MobileNo
                          }).ToList();

            return ObjCompany;
        }
        public List<ProductMasterModel> PurchaseProduct_Search(int? page, int? limit, string BarCode, string ProductCode, string PrintName, long? CompanyID, out int TotalCount)
        {
            List<ProductMasterModel> ObjPSM = new List<ProductMasterModel>();

            ObjPSM = (from PM in db.TblProductMasters
                      where PM.Status == true
                      &&
                      (BarCode == "" || BarCode == null || PM.BarCode.Contains(BarCode))
                       &&
                      (ProductCode == "" || ProductCode == null || PM.ProductCode.Contains(ProductCode))
                      &&
                      (PrintName == "" || PrintName == null || PM.PrintName.Contains(PrintName))

                      select new ProductMasterModel
                      {
                          ProductID = PM.ProductID,
                          BarCode = PM.BarCode,
                          ProductCode = PM.ProductCode,
                          PrintName = PM.PrintName,
                          CompanyName = (from PS in db.TblProductCompanies
                                         join SM in db.TblCompanyMasters on PS.CompanyID equals SM.CompanyID
                                         where PS.ProductID == PM.ProductID
                                         select SM.CompanyName).FirstOrDefault()
                      }).ToList();

            if (CompanyID != 0 && CompanyID != null)
            {
                ObjPSM = ObjPSM.Where(o => (from PS in db.TblProductCompanies
                                            where PS.CompanyID == CompanyID
                                            select PS.ProductID).Contains(o.ProductID)).ToList();
            }

            TotalCount = ObjPSM.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjPSM = ObjPSM.Skip(start).Take(limit.Value).ToList();
            }


            return ObjPSM;
        }

        public List<ProductMasterModel> PurchaseProductCompany_Search(int? page, int? limit, string BarCode, string ProductCode, string PrintName, long? CompanyID, out int TotalCount)
        {
            List<ProductMasterModel> ObjPSM = new List<ProductMasterModel>();

            ObjPSM = (from PM in db.TblProductMasters
                      where PM.Status == true
                      &&
                      (BarCode == "" || BarCode == null || PM.BarCode.Contains(BarCode))
                       &&
                      (ProductCode == "" || ProductCode == null || PM.ProductCode.Contains(ProductCode))
                      &&
                      (PrintName == "" || PrintName == null || PM.PrintName.Contains(PrintName))
                      select new ProductMasterModel
                      {
                          ProductID = PM.ProductID,
                          BarCode = PM.BarCode,
                          ProductCode = PM.ProductCode,
                          PrintName = PM.PrintName,
                          CompanyName = (from PS in db.TblProductCompanies
                                         join
                                         SM in db.TblCompanyMasters on PS.CompanyID equals SM.CompanyID
                                         where PS.ProductID == PM.ProductID
                                         select SM.CompanyName).FirstOrDefault()
                      }).ToList();

            if (CompanyID != 0 && CompanyID != null)
            {
                ObjPSM = ObjPSM.Where(o => (from PS in db.TblProductCompanies
                                            where PS.CompanyID == CompanyID
                                            select PS.ProductID).Contains(o.ProductID)).ToList();
            }

            TotalCount = ObjPSM.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjPSM = ObjPSM.Skip(start).Take(limit.Value).ToList();
            }


            return ObjPSM;
        }

        public ReturnMessageModel Purchases_Delete(long CompanyPurchaseID, int LastModifiedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDelete = (from ct in db.TblTempCompanyPurchaseheaders
                                 where ct.CompanyPurchaseID == CompanyPurchaseID
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count == 1)
                {

                    var objDel = (from ct in db.TblTempCompanyPurchaseDetails
                                  where ct.CompanyPurchaseID == CompanyPurchaseID
                                  select ct).ToList();

                    if (objDel != null && objDel.Count == 1)
                    {
                        for (int i = 0; i < objDel.Count; i++)
                        {
                            var vQuantity = objDel[i].Quantity;
                            var vProductID = objDel[i].ProductID;

                            objDel[i].LastModifiedBy = LastModifiedBy;
                            objDel[i].LastModifiedDate = DateTime.Now;
                            objDel[i].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_CancelledPurchaseStatus_ViewID;
                            db.SaveChanges();

                            var objProduct = (from tblSMod in db.TblProductStores
                                              where tblSMod.ProductID == vProductID
                                              select tblSMod).ToList();

                            if (objProduct != null && objProduct.Count > 0)
                            {
                                var Qty = objProduct[0].Quantity ?? 0;
                                if (Qty != 0)
                                    Qty = (Qty - vQuantity);
                                else
                                    Qty = 0;

                                objProduct[0].Quantity = Qty;
                                objProduct[0].LastModifiedBy = LastModifiedBy;
                                objProduct[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                                db.SaveChanges();
                            }

                        }
                    }

                    objDelete[0].LastModifiedBy = LastModifiedBy;
                    objDelete[0].LastModifiedDate = DateTime.Now;
                    objDelete[0].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_CancelledPurchaseStatus_ViewID;

                    db.SaveChanges();

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Purchases_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Purchases_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel CompanyPurchases_Delete(long CompanyPurchaseID, int LastModifiedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDelete = (from ct in db.TblTempCompanyPurchaseheaders
                                 where ct.CompanyPurchaseID == CompanyPurchaseID
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count == 1)
                {

                    var objDel = (from ct in db.TblTempCompanyPurchaseDetails
                                  where ct.CompanyPurchaseID == CompanyPurchaseID
                                  select ct).ToList();

                    if (objDel != null && objDel.Count == 1)
                    {
                        for (int i = 0; i < objDel.Count; i++)
                        {
                            var vQuantity = objDel[i].Quantity;
                            var vProductID = objDel[i].ProductID;

                            objDel[i].LastModifiedBy = LastModifiedBy;
                            objDel[i].LastModifiedDate = DateTime.Now;
                            objDel[i].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_CancelledPurchaseStatus_ViewID;
                            db.SaveChanges();

                            var objProduct = (from tblSMod in db.TblProductStores
                                              where tblSMod.ProductID == vProductID
                                              select tblSMod).ToList();

                            if (objProduct != null && objProduct.Count > 0)
                            {
                                var Qty = objProduct[0].Quantity ?? 0;
                                if (Qty != 0)
                                    Qty = (Qty - vQuantity);
                                else
                                    Qty = 0;

                                objProduct[0].Quantity = Qty;
                                objProduct[0].LastModifiedBy = LastModifiedBy;
                                objProduct[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                                db.SaveChanges();
                            }

                        }
                    }

                    objDelete[0].LastModifiedBy = LastModifiedBy;
                    objDelete[0].LastModifiedDate = DateTime.Now;
                    objDelete[0].PurchaseStatus = DEFAULTMASTER.DefaultDetailMaster_CancelledPurchaseStatus_ViewID;

                    db.SaveChanges();

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "CompanyPurchases_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "CompanyPurchases_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel CompanyPurchaseDetails_Delete(string CompanyPurchaseDetailsIDs)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                string[] SplitResult;
                long vCompanyPurchaseDetailsID = 0;

                SplitResult = CompanyPurchaseDetailsIDs.Split(',');
                for (int j = 0; j < SplitResult.Length; j++)
                {
                    vCompanyPurchaseDetailsID = Convert.ToInt32(SplitResult[j]);
                    if (vCompanyPurchaseDetailsID != 0)
                    {

                        var objDel = (from tblProject in db.TblTempCompanyPurchaseDetails
                                          where tblProject.CompanyPurchaseDetailsID == vCompanyPurchaseDetailsID
                                          select tblProject).ToList();


                        var Detail = db.TblTempCompanyPurchaseDetails.Find(objDel[0].CompanyPurchaseDetailsID);
                        if (Detail != null)
                        {
                            db.TblTempCompanyPurchaseDetails.Remove(Detail);
                            db.SaveChanges();
                        }
                    }
                }
                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "ProjectMaster_Update");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "ProjectMaster_Update");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }


            return ObjMessage;
        }


        public List<TempCompanyPurchaseheaderModel> CompanyPurchaseRequests_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Company, string PurchaseStatus, string sortBy, string direction, out int TotalCount)
        {
            DateTime ObjPurchaseDate = DateTime.Now;
            if (PurchaseDate != "")
            {
                ObjPurchaseDate = Convert.ToDateTime(PurchaseDate);
            }
            List<TempCompanyPurchaseheaderModel> ObjCompanyPurchases = (from ct in db.TblTempCompanyPurchaseheaders
                                                                        join SM in db.TblCompanyMasters on ct.CompanyID equals SM.CompanyID
                                                                        where
                                                                          ((PurchaseDate == "" || PurchaseDate == null) || ct.PurchaseDate == ObjPurchaseDate)
                                                                        && ((PurchaseRefNo == "" || PurchaseRefNo == null) || ct.ReferenceNO.Contains(PurchaseRefNo))
                                                                        && ((Company == "" || Company == null) || SM.CompanyName.Contains(Company))
                                                                        && ((PurchaseStatus == "" || PurchaseStatus == null) || ct.PurchaseStatus == PurchaseStatus)

                                                                        select new TempCompanyPurchaseheaderModel
                                                                        {
                                                                            CompanyPurchaseID = ct.CompanyPurchaseID,
                                                                            ReferenceNO = ct.ReferenceNO,
                                                                            PurchaseDate = ct.PurchaseDate,
                                                                            CompanyID = ct.CompanyID,
                                                                            WarehouseID = ct.WarehouseID,
                                                                            GrandTotal = ct.GrandTotal,
                                                                            PurchaseStatus = ct.PurchaseStatus,
                                                                            DistributionLocationID = ct.DistributionLocationID,
                                                                            CompanyName = SM.CompanyName,
                                                                            CreatedDate = ct.CreatedDate,
                                                                            Quantity = (from PM in db.TblPurchaseDetails where PM.PurchaseID == ct.CompanyPurchaseID select PM.Quantity).DefaultIfEmpty(0).Sum(),
                                                                            FreeQuantity = (from PM in db.TblPurchaseDetails where PM.PurchaseID == ct.CompanyPurchaseID select PM.FreeQuantity ?? 0).DefaultIfEmpty(0).Sum(),
                                                                            PurchaseStatusStr = (from DM in db.TblDefaultMasterDetails where DM.DefaultDetailID == ct.PurchaseStatus select DM.DefaultTextField).FirstOrDefault(),
                                                                            CreatedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.CreatedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                                                                            LastModifiedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.LastModifiedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                                                                            ApprovedUser = (from UMC in db.TblUserMasters where UMC.UserID == ct.ApprovedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault()
                                                                        }).Where(a => a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_RequestedPurchasedStatus_ViewID
                                                                               || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_CancelledPurchaseStatus_ViewID
                                                                               || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_ApprovedPurchasedStatus_ViewID
                                                                               || a.PurchaseStatus == DEFAULTMASTER.DefaultDetailMaster_OrderedPurchasedStatus_ViewID).ToList();

            if (
                !string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "ReferenceNO":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.ReferenceNO).ToList();
                            break;
                        case "PurchaseDate":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.PurchaseDate).ToList();
                            break;
                        case "GrandTotal":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.GrandTotal).ToList();
                            break;
                        case "CompanyName":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.CompanyName).ToList();
                            break;
                        case "PurchaseStatusStr":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.PurchaseStatusStr).ToList();
                            break;
                        case "Quantity":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderBy(q => q.Quantity).ToList();
                            break;
                    }
                }
                else
                {

                    switch (sortBy.Trim())
                    {
                        case "ReferenceNO":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.ReferenceNO).ToList();
                            break;
                        case "PurchaseDate":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.PurchaseDate).ToList();
                            break;
                        case "GrandTotal":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.GrandTotal).ToList();
                            break;
                        case "CompanyName":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.CompanyName).ToList();
                            break;
                        case "PurchaseStatusStr":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.PurchaseStatusStr).ToList();
                            break;
                        case "Quantity":
                            ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.Quantity).ToList();
                            break;

                    }
                }
            }
            else
            {
                ObjCompanyPurchases = ObjCompanyPurchases.OrderByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = ObjCompanyPurchases.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjCompanyPurchases = ObjCompanyPurchases.Skip(start).Take(limit.Value).ToList();
            }

            return ObjCompanyPurchases;
        }

        public CompanyPurchaseInvoiceHeaderModel CompanyPurchaseInvoiceHeader(string WareHouseID, long CompanyPurchaseID)
        {
            CompanyPurchaseInvoiceHeaderModel Obj = new CompanyPurchaseInvoiceHeaderModel();

            Obj = (from ct in db.TblTempCompanyPurchaseheaders
                   where ct.WarehouseID == WareHouseID && ct.CompanyPurchaseID == CompanyPurchaseID
                   select new CompanyPurchaseInvoiceHeaderModel
                   {
                       BillNumber = ct.ReferenceNO,
                       PurchaseDate = ct.PurchaseDate.ToString(),
                       TotalDiscount = ct.TotalDiscount ?? 0,
                       PaymentStatus = (from UMC in db.TblDefaultMasterDetails where UMC.DefaultDetailID == ct.PaymentStatus select UMC.DefaultTextField).FirstOrDefault(),
                       PurchaseStatus = (from DM in db.TblDefaultMasterDetails where DM.DefaultDetailID == ct.PurchaseStatus select DM.DefaultTextField).FirstOrDefault(),
                       ToName = (from CM in db.TblCompanyMasters where CM.CompanyID == ct.CompanyID select CM.CompanyName).FirstOrDefault(),
                       ToAddress1 = (from CM in db.TblCompanyMasters where CM.CompanyID == ct.CompanyID select CM.AddressLine1).FirstOrDefault(),
                       ToAddress2 = (from CM in db.TblCompanyMasters where CM.CompanyID == ct.CompanyID select CM.AddressLine2).FirstOrDefault(),
                       ToAddress3 = (from CM in db.TblCompanyMasters where CM.CompanyID == ct.CompanyID select CM.AddressLine3).FirstOrDefault(),
                       ToPhoneNumber = (from CM in db.TblCompanyMasters where CM.CompanyID == ct.CompanyID select CM.MobileNo).FirstOrDefault(),
                       ToEmail = (from CM in db.TblCompanyMasters where CM.CompanyID == ct.CompanyID select CM.EmailID).FirstOrDefault(),
                       ToGSTNumber = (from CM in db.TblCompanyMasters where CM.CompanyID == ct.CompanyID select CM.GSTNumber).FirstOrDefault()
                   }).FirstOrDefault();

            Obj.WarehouseName = (from ct in db.TblDefaultMasterDetails where ct.DefaultDetailID == WareHouseID select ct.DefaultTextField).FirstOrDefault();

            var ObjPrintSetting = (from ct in db.TblPrintSettingsMasters
                                   where ct.StoreID == WareHouseID
                                   select new SalesPrintModel
                                   {
                                       CompanyName = ct.CompanyName,
                                       Address1 = ct.Address1,
                                       Address2 = ct.Address2,
                                       PinCode = ct.PinCode,
                                       MobileNumber = ct.MobileNumber,
                                       Email = ct.Email,
                                       FooterMessage = ct.FooterMessage,
                                       GSTNo = ct.GSTNumber
                                   }).FirstOrDefault();
            if (ObjPrintSetting != null)
            {
                Obj.FromName = ObjPrintSetting.CompanyName;
                Obj.FromAddress1 = ObjPrintSetting.Address1;
                Obj.FromAddress2 = ObjPrintSetting.Address2;
                Obj.FromPinCode = ObjPrintSetting.PinCode;
                Obj.FromPhoneNumber = ObjPrintSetting.MobileNumber;
                Obj.FromEmail = ObjPrintSetting.Email;
                Obj.FooterMessage = ObjPrintSetting.FooterMessage;
                Obj.FromGSTNumber = ObjPrintSetting.GSTNo;
            }

            Obj.WareHouseID = WareHouseID;
            Obj.CompanyPurchaseID = CompanyPurchaseID;

            return Obj;
        }

        public List<CompanyPurchaseInvoiceDetailModel> CompanyPurchaseInvoiceDetails(string WareHouseID, long CompanyPurchaseID)
        {

            List<CompanyPurchaseInvoiceDetailModel> ObjPurchaseDetails = (from ct in db.TblTempCompanyPurchaseDetails
                                                                          where ct.CompanyPurchaseID == CompanyPurchaseID
                                                                          select new CompanyPurchaseInvoiceDetailModel
                                                                          {

                                                                              PrintName = ct.PrintName,
                                                                              UnitCost = ct.UnitCost,
                                                                              Quantity = ct.Quantity,
                                                                              HSNName = (from HS in db.TblHSNMasters where HS.HSNID == ct.HSNID select HS.Code).FirstOrDefault(),
                                                                              HSNPerc = (from HS in db.TblHSNMasters where HS.HSNID == ct.HSNID select HS.CGST + HS.SGST).FirstOrDefault(),
                                                                              DiscountPercent = ct.DiscountPercent ?? 0,
                                                                              DiscountAmount = ct.DiscountAmount ?? 0,
                                                                              TotalAmount = ct.TotalAmount ?? 0,
                                                                              Shipping = (from PR in db.TblTempCompanyPurchaseheaders where PR.CompanyPurchaseID == ct.CompanyPurchaseID select PR.Shipping ?? 0).FirstOrDefault(),
                                                                              FinalDiscountAmount = (from PR in db.TblTempCompanyPurchaseheaders where PR.CompanyPurchaseID == ct.CompanyPurchaseID select PR.FinalDiscountAmount ?? 0).FirstOrDefault(),
                                                                              GrandTotal = (from PR in db.TblTempCompanyPurchaseheaders where PR.CompanyPurchaseID == ct.CompanyPurchaseID select PR.GrandTotal).FirstOrDefault(),
                                                                              HSNAmount = 0
                                                                          }).ToList();
            return ObjPurchaseDetails;

        }
    }
}

