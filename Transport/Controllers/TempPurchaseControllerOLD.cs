using Distributor.Model;
using Distributor.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DistributorProject.Controllers
{
    public class TempPurchaseController : Controller
    {

        ICommonRepository ObjCommRepository = new CommonRepository();
        ITempPurchaseRepository _ObjDistPurchasesRepository = new TempPurchaseRepository();




        #region "Control Pages Action" 


        

        public ActionResult CompanyPurchaseRequest(string HeaderViewID, string DetailViewID)
        {
            PageAccessModel Obj = new PageAccessModel();

            Obj.IsAdd = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_ADD);
            Obj.IsEdit = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_EDIT);
            Obj.IsDelete = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_DELETE);
            Obj.IsPayment = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_Payment);
            Obj.IsApproval = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_Approval);
            return View(Obj);

        }

        public ActionResult AddCompanyPurchaseRequest()
        {
            TempCompanyPurchaseheaderModel obj = new TempCompanyPurchaseheaderModel();
            return View(obj);
        }
        public ActionResult AddDirectCompanyPurchase()
        {
            TempCompanyPurchaseheaderModel obj = new TempCompanyPurchaseheaderModel();
            return View(obj);
        }
        public ActionResult AddDirectDistributionPurchase(string HeaderViewID, string DetailViewID)
        {
            TempDistributorPurchaseheaderModel obj = new TempDistributorPurchaseheaderModel();
            obj.HeaderViewID = HeaderViewID;
            obj.DetailViewID = "18UG35XQZNWT";
            return View(obj);
        }
        public ActionResult AddEditPurchase()
        {
            PurchasesModel obj = new PurchasesModel();
            return View(obj);
        }


        public ActionResult DistributorPurchaseDetail(string HeaderViewID, string DetailViewID)
        {
            PageAccessModel Obj = new PageAccessModel();

            Obj.IsAdd = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_ADD);
            Obj.IsEdit = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_EDIT);
            Obj.IsDelete = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_DELETE);
            Obj.IsPayment = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_Payment);
            Obj.IsApproval = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_Approval);
            return View(Obj);
        }

        public ActionResult DistributorPurchaseRequest()
        {
            TempDistributorPurchaseheaderModel obj = new TempDistributorPurchaseheaderModel();
            return View(obj);
        }

        [HttpPost]
        public JsonResult Purchase_Approval(long DistributorPurchaseID, int IsApprove)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = _ObjDistPurchasesRepository.Purchase_Approval(DistributorPurchaseID, IsApprove, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreatePO(long DistributorPurchaseID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = _ObjDistPurchasesRepository.Purchase_CreatePO(DistributorPurchaseID, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PurchaseOrderPrint(long DistributorPurchaseID, string WareHouseID)
        {
            PurchaseInvoiceHeaderModel Obj = _ObjDistPurchasesRepository.PurchaseInvoiceHeader(WareHouseID, DistributorPurchaseID);
            return View(Obj);
        }

        public ActionResult DistributorPurchaseAddEdit()
        {
            TempDistributorPurchaseheaderModel obj = new TempDistributorPurchaseheaderModel();
            return View(obj);
        }
        public ActionResult CompanyPurchaseDetail(string HeaderViewID, string DetailViewID)
        {
            PageAccessModel Obj = new PageAccessModel();

            Obj.IsAdd = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_ADD);
            Obj.IsEdit = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_EDIT);
            Obj.IsDelete = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_DELETE);
            Obj.IsPayment = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_Payment);
            Obj.IsApproval = ObjCommRepository.GetSinglePageAccessControl(HeaderViewID, DetailViewID, SessionExpire.GetRoleID(), DEFAULTMASTER.DefaultDetailMaster_PageAccess_Approval);
            return View(Obj);
        }

        public ActionResult CompanyPurchaseRequest()
        {
            TempCompanyPurchaseheaderModel obj = new TempCompanyPurchaseheaderModel();
            return View(obj);
        }

        public ActionResult CompanyPurchaseAddEdit()
        {
            TempCompanyPurchaseheaderModel obj = new TempCompanyPurchaseheaderModel();
            return View(obj);
        }



        [HttpGet]
        public ActionResult DistributorPurchaseRequests_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Distributor, string PurchaseStatus, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<TempDistributorPurchaseheaderModel> Purchaseslist = _ObjDistPurchasesRepository.DistributorPurchaseRequests_FindAll(page, limit, PurchaseDate, PurchaseRefNo, Distributor, PurchaseStatus, sortBy, direction, out TotalCount);
            return Json(new { records = Purchaseslist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult CompanyPurchaseRequests_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Company, string PurchaseStatus, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<TempCompanyPurchaseheaderModel> ObjCompanyPurchases = _ObjDistPurchasesRepository.CompanyPurchaseRequests_FindAll(page, limit, PurchaseDate, PurchaseRefNo, Company, PurchaseStatus, sortBy, direction, out TotalCount);
            return Json(new { records = ObjCompanyPurchases, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PurchaseHeader_RunningNumber()
        {
            PurchaseRunningNumber obj = new PurchaseRunningNumber();
            obj = _ObjDistPurchasesRepository.PurchaseHeader_RunningNumber();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Purchase_CompanyApproval(long CompanyPurchaseID, int IsApprove)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = _ObjDistPurchasesRepository.Purchase_CompanyApproval(CompanyPurchaseID, IsApprove, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CompanyPurchaseHeader_RunningNumber()
        {
            PurchaseRunningNumber obj = new PurchaseRunningNumber();
            obj = _ObjDistPurchasesRepository.CompanyPurchaseHeader_RunningNumber();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DistributorInvoiceDuplicate(string DistributorInvoiceNumber)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            bool IsDistributorInvoiceDuplicate = _ObjDistPurchasesRepository.DistributorInvoiceDuplicate(DistributorInvoiceNumber);
            return Json(new { IsDistributorInvoiceDuplicate = IsDistributorInvoiceDuplicate }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DistributionPurchases_Save(TempDistributorPurchaseheaderModel objPurchases)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            HttpFileCollectionBase files = Request.Files;
            string fname;
            byte[] tempFile = null;

            objPurchases.CreatedBy = SessionExpire.GetUserID();
            // objPurchases.PaidBy = SessionExpire.GetUserName();

            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFileBase file = files[i];

                // Checking for Internet Explorer  
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = file.FileName;
                }

                tempFile = new byte[file.ContentLength];
                file.InputStream.Read(tempFile, 0, file.ContentLength);

                objPurchases.FileName = fname;
                objPurchases.ReferenceAttachment = tempFile;
            }

            ObjMessage = _ObjDistPurchasesRepository.DistributionPurchases_Save(objPurchases, objPurchases.PurchaseDetail);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DistributorPurchases_Edit(long DistPurchaseID)
        {
            TempDistributorPurchaseheaderModel ObjMessage = _ObjDistPurchasesRepository.DistributorPurchases_Edit(DistPurchaseID);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult DistributorPurchaseDetail_FindAll(long DistPurchaseID)
        {
            List<TempDistributorPurchaseDetailModel> Purchaseslist = _ObjDistPurchasesRepository.DistributorPurchaseDetail_FindAll(DistPurchaseID);
            return Json(new { records = Purchaseslist }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult PurchaseProduct_Search(int? page, int? limit, string BarCode, string ProductCode, string PrintName, long? CompanyID)
        {
            int TotalCount = 0;
            List<ProductMasterModel> ObjProduct = _ObjDistPurchasesRepository.PurchaseProduct_Search(page, limit, BarCode, ProductCode, PrintName, CompanyID, out TotalCount);
            return Json(new { records = ObjProduct, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region "Purchase Company Detail"

        [HttpGet]
        public ActionResult CompanyPurchase_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Company, string PurchaseStatus, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<TempCompanyPurchaseheaderModel> ObjCompanyPurchases = _ObjDistPurchasesRepository.CompanyPurchase_FindAll(page, limit, PurchaseDate, PurchaseRefNo, Company, PurchaseStatus, sortBy, direction, out TotalCount);
            return Json(new { records = ObjCompanyPurchases, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CompanyInvoiceDuplicate(string CompanyInvoiceNumber)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            bool IsCompanyInvoiceDuplicate = _ObjDistPurchasesRepository.CompanyInvoiceDuplicate(CompanyInvoiceNumber);
            return Json(new { IsCompanyInvoiceDuplicate = IsCompanyInvoiceDuplicate }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult CompanyPurchases_Save(TempCompanyPurchaseheaderModel ObjPurchase)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            HttpFileCollectionBase files = Request.Files;
            string fname;
            byte[] tempFile = null;

            ObjPurchase.CreatedBy = SessionExpire.GetUserID();
            // objPurchases.PaidBy = SessionExpire.GetUserName();

            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFileBase file = files[i];

                // Checking for Internet Explorer  
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = file.FileName;
                }

                tempFile = new byte[file.ContentLength];
                file.InputStream.Read(tempFile, 0, file.ContentLength);

                ObjPurchase.FileName = fname;
                ObjPurchase.ReferenceAttachment = tempFile;
            }

            ObjMessage = _ObjDistPurchasesRepository.CompanyPurchases_Save(ObjPurchase, ObjPurchase.ObjCompanyPurchaseDetail);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult CompanyPurchases_Edit(long CompanyPurchaseID)
        {
            TempCompanyPurchaseheaderModel ObjMessage = _ObjDistPurchasesRepository.CompanyPurchases_Edit(CompanyPurchaseID);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult CompanyPurchaseDetail_FindAll(long CompanyPurchaseID)
        {
            
            List<TempCompanyPurchaseDetailModel> Purchaseslist = _ObjDistPurchasesRepository.CompanyPurchaseDetail_FindAll(CompanyPurchaseID);
            return Json(new { records = Purchaseslist, total = Purchaseslist.Count }, JsonRequestBehavior.AllowGet);
            //return Json(new { records = Purchaseslist }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Purchase_ProductDetailInsert(TempCompanyPurchaseDetailModel ObjCompanyPurchase)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjCompanyPurchase.CreatedBy = SessionExpire.GetUserID();

            ObjMessage = _ObjDistPurchasesRepository.Purchase_ProductDetailInsert(ObjCompanyPurchase);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult PurchaseProductCompany_Search(int? page, int? limit, string BarCode, string ProductCode, string PrintName, long? CompanyID)
        {
            int TotalCount = 0;
            List<ProductMasterModel> ObjProduct = _ObjDistPurchasesRepository.PurchaseProductCompany_Search(page, limit, BarCode, ProductCode, PrintName, CompanyID, out TotalCount);
            return Json(new { records = ObjProduct, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult PurchaseCompany_Search(string CompanyName, string CompanyCode, string CompanyType, string MobileNo, string GSTNumber)
        {
            List<CompanyMasterModel> ObjCompany = _ObjDistPurchasesRepository.PurchaseCompany_Search(CompanyName, CompanyCode, CompanyType, MobileNo, GSTNumber);
            return Json(new { records = ObjCompany }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult Purchase_CompanyCreatePO(long CompanyPurchaseID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = _ObjDistPurchasesRepository.Purchase_CompanyCreatePO(CompanyPurchaseID, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CompanyPurchases_Delete(long CompanyPurchaseID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjMessage = _ObjDistPurchasesRepository.CompanyPurchases_Delete(CompanyPurchaseID, SessionExpire.GetUserID());
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CompanyPurchaseDetails_Delete(string CompanyPurchaseDetailsIDs)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjMessage = _ObjDistPurchasesRepository.CompanyPurchaseDetails_Delete(CompanyPurchaseDetailsIDs);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PurchasePrintInvoice(long DistributorPurchaseID, string WarehouseID)
        {
            PurchaseInvoiceHeaderModel Obj = _ObjDistPurchasesRepository.PurchaseInvoiceHeader(WarehouseID, DistributorPurchaseID);
            return View(Obj);
        }
        public List<PurchaseInvoiceDetailModel> PurchaseDetails_Invoice(long DistributorPurchaseID, string WareHouseID)
        {
            List<PurchaseInvoiceDetailModel> Purchaselist = _ObjDistPurchasesRepository.PurchaseInvoiceDetails(WareHouseID, DistributorPurchaseID);
            return Purchaselist;
        }

        #endregion

        #region "Company Purchase"

        public ActionResult CompanyPurchasePrintInvoice(long CompanyPurchaseID, string WarehouseID)
        {
            CompanyPurchaseInvoiceHeaderModel Obj = _ObjDistPurchasesRepository.CompanyPurchaseInvoiceHeader(WarehouseID, CompanyPurchaseID);
            return View(Obj);
        }
        public List<CompanyPurchaseInvoiceDetailModel> CompanyPurchaseDetails_Invoice(long CompanyPurchaseID, string WareHouseID)
        {
            List<CompanyPurchaseInvoiceDetailModel> Purchaselist = _ObjDistPurchasesRepository.CompanyPurchaseInvoiceDetails(WareHouseID, CompanyPurchaseID);
            return Purchaselist;
        }

        public ActionResult CompanyPurchaseOrderPrint(long CompanyPurchaseID, string WareHouseID)
        {
            CompanyPurchaseInvoiceHeaderModel Obj = _ObjDistPurchasesRepository.CompanyPurchaseInvoiceHeader(WareHouseID, CompanyPurchaseID);
            return View(Obj);
        }

        #endregion

    }
}