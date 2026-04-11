using Transport.Model;
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
    public class SystemMasterController : Controller
    {
        ISystemMasterRepository ObjMasterRepository = new SystemMasterRepository();
        ICommonRepository ObjCommRepository = new CommonRepository();

        #region "Profile Master"

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ChangePassword()
        {
            ProfileMasterModel ObjProfile = new ProfileMasterModel();
            var ProfileUserID = SessionExpire.GetUserID();

            List<ProfileMasterModel> obj = ObjCommRepository.Load_ProfileInformationOnUserID(ProfileUserID);

            if (obj != null && obj.Count > 0)
            {
                ObjProfile.UserID = ProfileUserID;  
                ObjProfile.OldPassword = ObjCommRepository.DecryptPassword(obj[0].Password.Trim());
            }  

            return View(ObjProfile);
        }

        [HttpPost]
        public JsonResult UserProfile_Save(UserMasterModel ObjUserModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            ObjUserModel.CreatedBy = SessionExpire.GetUserID();

            ObjMessage = ObjMasterRepository.UserProfile_Save(ObjUserModel);

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "User Master"

        public ActionResult UserMasterDetail(string HeaderViewID, string DetailViewID)
        {
           return View();
        }

        [HttpGet]
        public bool Get_UserPOSStatus(string RoleID)
        {
            bool IsPOS = ObjMasterRepository.Get_UserPOSStatus(RoleID);

            return IsPOS;
        }

        public ActionResult AddUserMasterDetail(string HeaderViewID, string DetailViewID, int? UserID)
        {
            UserMasterModel Obj = new UserMasterModel();
            Obj.UserID = UserID??0;
            Obj.LoadUserRoleData = new SelectList(ObjCommRepository.Load_UserRole().ToList(), "ID", "Name");
            return View(Obj);
        }

        [HttpGet]
        public JsonResult UserMaster_Edit(int UserID)
        {
            UserMasterModel objUser = ObjMasterRepository.UserMaster_Edit(UserID);
            return Json(objUser, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult UserMaster_FindAll(int? page, int? limit, string sortBy, string direction,string LoginName,string DisplayName, bool? Status )
        { 
            int TotalCount; 
            List<UserMasterDetailModel> ObjUser = ObjMasterRepository.UserMaster_FindAll(page, limit,  LoginName, DisplayName, Status,sortBy, direction, out TotalCount);

            return Json(new { records = ObjUser, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UserMaster_Save(UserMasterModel ObjUserModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjUserModel.CreatedBy = SessionExpire.GetUserID();

            ObjMessage = ObjMasterRepository.UserMaster_Save(ObjUserModel);

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult UserMaster_Delete(int UserID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = ObjMasterRepository.UserMaster_Delete(UserID, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Reset User Master"

        [HttpGet]
        public JsonResult ResetUserLogin_FindAll(int? page, int? limit, string sortBy, string direction)
        {

            int TotalCount;

            List<UserMasterDetailModel> ObjUser = ObjMasterRepository.ResetUserLogin_FindAll(page, limit, sortBy, direction, out TotalCount);
            
            return Json(new { records = ObjUser, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ResetUserLogin_Update(int UserID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = ObjMasterRepository.ResetUserLogin_Update(UserID);

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Menu Master Header"       

        [HttpGet]
        public JsonResult MenuMasterHeader_FindAll(int? page, int? limit, string MenuName,string sortBy, string direction)
        {
            int TotalCount;

            List<MenuMasterHeader> ObjMenus = ObjMasterRepository.MenuMasterHeader_FindAll(page, limit, MenuName,  sortBy, direction, out TotalCount);
            return Json(new { records = ObjMenus, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MenuMasterHeader_Save(MenuMasterHeader ObjMenuMasterHeaderModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMenuMasterHeaderModel.CreatedBy = SessionExpire.GetUserID();
            ObjMessage = ObjMasterRepository.MenuMasterHeader_Save(ObjMenuMasterHeaderModel);

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MenuHeaderMaster_Edit(string HeaderViewID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            MenuMasterHeader objmenu = ObjMasterRepository.MenuHeaderMaster_Edit(HeaderViewID);

            return Json(objmenu, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MenuMasterHeader_Delete(string HeaderViewID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = ObjMasterRepository.MenuMasterHeader_Delete(HeaderViewID, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MenuMasterDetail_FindAll(string MenuMasterHeaderID, int? page, int? limit, string SearchColumn, string searchString, string sortBy, string direction)
        {
            int TotalCount;

            List<MenuMasterDetail> ObjProject = ObjMasterRepository.MenuMasterDetail_FindAll(MenuMasterHeaderID, page, limit, SearchColumn, searchString, sortBy, direction, out TotalCount);
            return Json(new { records = ObjProject, total = TotalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MenuMasterDetail_Edit(string HeaderViewID, string DetailViewID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            MenuMasterDetail objmenudetail = ObjMasterRepository.MenuMasterDetail_Edit(HeaderViewID, DetailViewID);

            return Json(objmenudetail, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MenuMasterDetail_Save(MenuMasterDetail objMenuDetailModel, List<LoadItems_String> SelectedAccess)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            objMenuDetailModel.CreatedBy = SessionExpire.GetUserID();
            ObjMessage = ObjMasterRepository.MenuMasterDetail_Save(objMenuDetailModel, SelectedAccess);

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MenuMasterDetail_Delete(string DetailViewID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            ObjMessage = ObjMasterRepository.MenuMasterDetail_Delete(DetailViewID, SessionExpire.GetUserID());

            return Json(ObjMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}