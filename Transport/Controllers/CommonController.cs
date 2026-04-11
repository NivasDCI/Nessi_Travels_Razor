using ClosedXML.Excel;
using Transport.Model;
using System.Data;
using System.IO;
using Transport.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class CommonController : Controller
    {
        ICommonRepository ObjCommonRepository = new CommonRepository();
        IProjectMasterRepository ObjMasterRepository = new ProjectMasterRepository();
        // GET: Common        

        [HttpGet]
        public JsonResult Load_HomePage()
        {
            List<HomePage> PageList = null;

            PageList = ObjCommonRepository.Load_HomePage().ToList();

            return Json(PageList, JsonRequestBehavior.AllowGet);
        }       

        [HttpGet]
        public JsonResult GetIsSession()
        {
            return Json(new { IsSession = SessionExpire.GetUserID() != 0 ? true : false }, JsonRequestBehavior.AllowGet);
        } 

        #region "Menu Master"

        public List<MenuMasterHeader> MenuMasterHeader()
        {
            List<MenuMasterHeader> ObjMenuMasterHeader = null;
            ObjMenuMasterHeader = ObjCommonRepository.MenuMasterHeader(SessionExpire.GetRoleID());
            return ObjMenuMasterHeader;
        }
        
        public List<MenuMasterDetail> MenuMasterDetail(string HeaderViewID)
        {
            List<MenuMasterDetail> ObjMenuMasterDetail = null;
            ObjMenuMasterDetail = ObjCommonRepository.MenuMasterDetail(HeaderViewID, SessionExpire.GetRoleID());
            return ObjMenuMasterDetail;
        }

       
        #endregion

        #region "Common Functions"

        [HttpGet]
        public JsonResult Load_CommonDefaultDetails(string DefaultTypeHeaderID)
        {
            List<DefaultTypeItems> DefaultList = null;

            DefaultList = ObjCommonRepository.Load_CommonDefaultDetails(DefaultTypeHeaderID).ToList();

            return Json(DefaultList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult RoundDown(decimal Amount)
        {
            decimal RoundOffValue = 0;
            if (Amount != 0) RoundOffValue = ObjCommonRepository.RoundDown(Amount);
            return Json(RoundOffValue, JsonRequestBehavior.AllowGet);
        }
              
        [HttpGet]
        public JsonResult Load_CommonDefaultHeaderDetails()
        {
            List<DefaultHeaderNameItems> LevelList = null;
            
            LevelList = ObjCommonRepository.Load_CommonDefaultHeaderDetails().ToList();

            return Json(LevelList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Load_JobStatus()
        {
            List<SelectListItem> PaxType = new List<SelectListItem>();
            PaxType.Add(new SelectListItem() { Text = "Assigned", Value = "Assigned" });
            PaxType.Add(new SelectListItem() { Text = "Job Started", Value = "Job Started" });
            PaxType.Add(new SelectListItem() { Text = "Waiting for Customer", Value = "Waiting for Customer" });
            PaxType.Add(new SelectListItem() { Text = "No Show", Value = "No Show" });
            PaxType.Add(new SelectListItem() { Text = "Passenger on board", Value = "Passenger on board" });
            PaxType.Add(new SelectListItem() { Text = "Job Completed", Value = "Job Completed" });

            //this.ViewBag.PaxType = new SelectList(PaxType, "Value", "Text");
            return Json(PaxType, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Load_JobRequestStatus()
        {
            List<SelectListItem> PaxType = new List<SelectListItem>();
            PaxType.Add(new SelectListItem() { Text = "Job Accepted", Value = "Job Accepted" });
            PaxType.Add(new SelectListItem() { Text = "Pending", Value = "Pending" });
            PaxType.Add(new SelectListItem() { Text = "Not Available", Value = "Not Available" });

            //this.ViewBag.PaxType = new SelectList(PaxType, "Value", "Text");
            return Json(PaxType, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Load_CreditCash()
        {
            List<SelectListItem> CreditCash = new List<SelectListItem>();
            CreditCash.Add(new SelectListItem() { Text = "", Value = "" });
            CreditCash.Add(new SelectListItem() { Text = "Credit", Value = "Credit" });
            CreditCash.Add(new SelectListItem() { Text = "Cash", Value = "Cash" });
           
            return Json(CreditCash, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Load_JobVendors()
        {
            List<LoadItems_BigInt> JobVendorsList = null;

            JobVendorsList = ObjCommonRepository.Load_JobVendors().ToList();

            return Json(JobVendorsList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Load_Vehicles()
        {
            List<LoadItems_BigInt> VehiclesList = null;

            VehiclesList = ObjCommonRepository.Load_Vehicles().ToList();

            return Json(VehiclesList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Load_ServiceTypes()
        {
            List<LoadItems_BigInt> ServiceTypesList = null;

            ServiceTypesList = ObjCommonRepository.Load_ServiceTypes().ToList();

            return Json(ServiceTypesList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Load_ServiceTypesStaff()
        {
            List<LoadItems_BigInt> ServiceTypesList = null;
            ServiceTypesList = ObjCommonRepository.Load_ServiceTypesStaff().ToList();

            return Json(ServiceTypesList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Load_SupplierTypes()
        {
            List<LoadItems_BigInt> SupplierTypesList = null;

            SupplierTypesList = ObjCommonRepository.Load_SupplierTypes().ToList();

            return Json(SupplierTypesList, JsonRequestBehavior.AllowGet);
        }

        
        [HttpGet]
        public JsonResult Load_Status()
        {
            List<LoadItems_Int> StatusList = null;

            StatusList = ObjCommonRepository.Load_Status().ToList();

            return Json(StatusList, JsonRequestBehavior.AllowGet);
        } 

        [HttpGet]
        public JsonResult Load_DefaultType(string DefaultType)
        {
            List<DefaultTypeItems> DefaultTypeList = null;

            DefaultTypeList = ObjCommonRepository.Load_DefaultType(DefaultType).ToList();

            return Json(DefaultTypeList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Load_SystemUserDetail()
        {
            List<LoadItems_BigInt> DefaultTypeList = null;

            DefaultTypeList = ObjCommonRepository.Load_SystemUserDetail();

            return Json(DefaultTypeList, JsonRequestBehavior.AllowGet);
        }  

        [HttpGet]
        public JsonResult Load_DefaultTypeSearch(string DefaultType)
        {
            List<DefaultTypeItems> DefaultTypeList = null;

            DefaultTypeList = ObjCommonRepository.Load_DefaultTypeSearch(DefaultType).ToList();

            return Json(DefaultTypeList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Load_DefaultTypePage(string DefaultType, bool IsPage)
        {
            List<DefaultTypeItems> DefaultTypeList = null;

            DefaultTypeList = ObjCommonRepository.Load_DefaultTypePage(DefaultType, IsPage).ToList();

            return Json(DefaultTypeList, JsonRequestBehavior.AllowGet);
        }              

        [HttpGet]
        public JsonResult DecryptPassword(string vPassword)
        {
            var DecryptPassword = "";
            if (vPassword != null) DecryptPassword = ObjCommonRepository.DecryptPassword(vPassword);

            return Json(DecryptPassword, JsonRequestBehavior.AllowGet);
        } 

        [HttpGet]
        public JsonResult Load_Vehicle()
        {
            List<LoadItems_BigInt> BrandList = null;

            BrandList = ObjCommonRepository.Load_Vehicles().ToList();

            return Json(BrandList, JsonRequestBehavior.AllowGet);
        }
       
        #endregion


    }
}
