using Transport.Model;
using Transport.Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class ProjectMasterController : Controller
    {
        IProjectMasterRepository ObjMasterRepository = new ProjectMasterRepository();
        ICommonRepository ObjCommRepository = new CommonRepository();

        #region "Control Pages Action" 

        public ActionResult DefaultMasterDetail(string HeaderViewID, string DetailViewID)
        {
            DefaultDetailMasterModel obj = new DefaultDetailMasterModel();
        
            return View(obj);
        }     
        public ActionResult VehicleMaster(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult AddVehicleMaster()
        {
            return View();
        }
        public ActionResult JobVendorsMaster(string HeaderViewID, string DetailViewID)
        {
            return View();
        }
        public ActionResult AddJobVendorsMaster()
        {
            return View();
        }
        public ActionResult DefaultImageMaster(string HeaderViewID, string DetailViewID,string DefaultHeader, string title)
        {
            DefaultImageModel obj = new DefaultImageModel();
            obj.HeaderViewID = HeaderViewID;
            obj.DetailViewID = DetailViewID;
            obj.DefaultHeader = DefaultHeader;
            obj.Title = title;

            return View(obj);
        }

        #endregion

        #region "Default Master"
        [HttpGet]
        public JsonResult DefaultMasterDetail_FindAll(string DefaultHeaderID, bool? Status, int? page, int? limit, string sortBy, string direction)
        {
            int TotalCount;
            List<DefaultDetailMasterModel> ObjProject = ObjMasterRepository.DefaultMasterDetail_FindAll(DefaultHeaderID, Status, page, limit, sortBy, direction, out TotalCount);
            return Json(new { records = ObjProject, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DefaultMasterDetail_LookUp(DefaultDetailMasterModel ObjDefaultDetailModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjDefaultDetailModel.CreatedBy = SessionExpire.GetUserID();
            ObjDefaultDetailModel.DefaultOrder = ObjMasterRepository.DefaultMaxOrder(ObjDefaultDetailModel.DefaultHeaderID);

            ObjMessage = ObjMasterRepository.DefaultMasterDetail_Save(ObjDefaultDetailModel);

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult DefaultMasterDetail_Save(DefaultDetailMasterModel ObjDefaultDetailModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjDefaultDetailModel.CreatedBy = SessionExpire.GetUserID();
            ObjMessage = ObjMasterRepository.DefaultMasterDetail_Save(ObjDefaultDetailModel);

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DefaultMasterDetail_Delete(string DefaultDetailID)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = ObjMasterRepository.DefaultMasterDetail_Delete(DefaultDetailID, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion                 

        #region "Vehicle Master"

        [HttpGet]
        public ActionResult VehicleMaster_FindAll(int? page, int? limit, string VehicleName, bool? Status,string sortBy, string direction)
        {
            int TotalCount = 0;
            List<VehicleMasterModel> VehicleMasterlist = ObjMasterRepository.VehicleMaster_FindAll(page, limit, VehicleName, Status,sortBy, direction, out TotalCount);
            return Json(new { records = VehicleMasterlist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }
       

        [HttpGet]
        public JsonResult VehicleMaster_Edit(int VehicleCode)
        {
            VehicleMasterModel ObjMessage = ObjMasterRepository.VehicleMaster_Edit(VehicleCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult VehicleMaster_Save(VehicleMasterModel objVehicleMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjMessage = ObjMasterRepository.VehicleMaster_Save(objVehicleMaster);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "JobVendors Master"

        [HttpGet]
        public ActionResult JobVendorsMaster_FindAll(int? page, int? limit, string JobVendorName, bool? Status, string sortBy, string direction)
        {
            int TotalCount = 0;
            List<JobVendorMasterModel> JobVendorsMasterlist = ObjMasterRepository.JobVendorsMaster_FindAll(page, limit, JobVendorName, Status, sortBy, direction, out TotalCount);
            return Json(new { records = JobVendorsMasterlist, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult JobVendorsMaster_Edit(long JobVendorCode)
        {
            JobVendorMasterModel ObjMessage = ObjMasterRepository.JobVendorsMaster_Edit(JobVendorCode);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult JobVendorsMaster_Save(JobVendorMasterModel objJobVendorsMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjMessage = ObjMasterRepository.JobVendorsMaster_Save(objJobVendorsMaster);
            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }       

        #endregion

    }

}