using Transport.Entity;
using Transport.Model;
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

namespace Transport.Repository
{
    public class SystemMasterRepository : ISystemMasterRepository
    {
        CommonRepository ObjCom = new CommonRepository();
        TransportEntities db = new TransportEntities();


        #region "Profile Master" 
         

        public ReturnMessageModel UserProfile_Save(UserMasterModel ObjUserModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                string EncryptedPassword = "", OldPassword = "";
                string EncryptedNewPassword = "", NewPassword = "";
                if (ObjUserModel.Password != null)
                {
                    OldPassword = ObjUserModel.Password.Trim();
                    EncryptedPassword = ObjCom.EncryptString(ObjUserModel.Password.Trim());
                }
                if (ObjUserModel.NewPassword != null)
                {
                    NewPassword = ObjUserModel.NewPassword.Trim();
                    EncryptedNewPassword = ObjCom.EncryptString(ObjUserModel.NewPassword.Trim());
                }

                var objModule = (from tblSMod in db.TblUserMasters
                                 where tblSMod.UserID == ObjUserModel.UserID
                                 select tblSMod).ToList();
                if (objModule != null && objModule.Count > 0)
                { 
                    if (objModule[0].Password != EncryptedNewPassword && EncryptedNewPassword != "")
                    {
                        objModule[0].Password = EncryptedNewPassword;
                    } 
                   
                    objModule[0].LastModifiedBy = ObjUserModel.CreatedBy;
                    objModule[0].LastModifiedDate = CommonRepository.GetTimeZoneDate(); 
                    db.SaveChanges();

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }
                else
                {
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.NORECORD;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "UserProfile_Save");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "UserProfile_Save");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }

            return ObjMessage;
        }

        #endregion "Profile Master" 

        #region "Reset Login Master"
        public List<UserMasterDetailModel> ResetUserLogin_FindAll(int? page, int? limit, string sortBy, string direction, out int TotalCount)

        {
            TotalCount = 0;
            List<UserMasterDetailModel> ObjUserList = new List<UserMasterDetailModel>();
            try
            {

                ObjUserList = (from US in db.TblUserMasters
                               where US.PasswordAttemptCount >= 3
                               select new UserMasterDetailModel
                               {
                                   UserID = US.UserID,
                                   LoginName = US.LoginName,
                                   MobileNumber = US.MobileNumber ,
                                   EmailID = US.EmailID
                               }).ToList();


                if (ObjUserList != null)
                    TotalCount = ObjUserList.Count;

                if (page.HasValue && limit.HasValue)
                {
                    int start = (page.Value - 1) * limit.Value;
                    ObjUserList = ObjUserList.Skip(start).Take(limit.Value).ToList();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "ResetUserLogin_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "ResetUserLogin_FindAll");
            }
            return ObjUserList;
        }


        public ReturnMessageModel ResetUserLogin_Update(int UserID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var obj = (from tblSMod in db.TblUserMasters
                           where tblSMod.UserID == UserID
                           select tblSMod).ToList();

                if (obj != null && obj.Count == 1)
                {

                    obj[0].PasswordAttemptCount = 0;
                    db.SaveChanges();
                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;

                }
                else
                {
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "ResetUserLogin_Update");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "ResetUserLogin_Update");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }


            return ObjMessage;
        }

        #endregion

        #region "User Master"

        public bool  Get_UserPOSStatus(string RoleID)
        {
         
            bool objIsPOS = (from RM in db.TblRoleMasters
                                   where RM.RoleID == RoleID
                                   select RM.IsPOS??false).FirstOrDefault();
                
            return objIsPOS;
        }
        public ReturnMessageModel UserMaster_Save(UserMasterModel ObjUserModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var obj = (from tblSMod in db.TblUserMasters
                           where tblSMod.UserID != ObjUserModel.UserID && tblSMod.LoginName == ObjUserModel.LoginName.Trim()
                           && tblSMod.Status == true
                           select tblSMod).ToList();

                if (obj != null && obj.Count == 0)
                {
                    if (ObjUserModel.UserID != 0)
                    {
                        UserMaster_Update(ObjUserModel);
                        ObjMessage.Result = true;
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                    }
                    else
                    {
                        UserMaster_Insert(ObjUserModel);
                        ObjMessage.Result = true;
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
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
                ObjCom.LogPageError(e, "UserMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "UserMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }


            return ObjMessage;
        }

        public ReturnMessageModel UserMaster_Insert(UserMasterModel ObjUserModel)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            string EncryptedPassword = ObjCom.EncryptString(ObjUserModel.Password.Trim());

            try
            {
                var entity = new TblUserMaster
                {
                    LoginName = ObjUserModel.LoginName,
                    RoleID = ObjUserModel.RoleID,
                    FirstName = ObjUserModel.FirstName,
                    LastName = ToString(ObjUserModel.LastName),
                    Password = EncryptedPassword,
                    EmailID = ObjUserModel.EmailID,
                    MobileNumber = ObjUserModel.MobileNumber,               
                    Status = ObjUserModel.Status, 
                    CreatedBy = ObjUserModel.CreatedBy,
                    CreatedDate = CommonRepository.GetTimeZoneDate()
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();

                ObjUserModel.UserID = entity.UserID;
                 

                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "UserMaster_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "UserMaster_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }

            return ObjMessage;
        }

        public ReturnMessageModel UserMaster_Update(UserMasterModel ObjUserModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                string EncryptedPassword = ObjUserModel.Password.Trim();

                var objModule = (from tblSMod in db.TblUserMasters
                                 where tblSMod.UserID == ObjUserModel.UserID
                                 select tblSMod).ToList();
                if (objModule != null && objModule.Count > 0)
                {

                    objModule[0].LoginName = ObjUserModel.LoginName.Trim();
                    objModule[0].RoleID = ObjUserModel.RoleID;
                    objModule[0].FirstName = ObjUserModel.FirstName.Trim();
                    objModule[0].LastName = ToString(ObjUserModel.LastName).Trim();
                    if (objModule[0].Password != EncryptedPassword && EncryptedPassword != "")
                    {
                        string EncryptedPasswordnew = ObjCom.EncryptString(ObjUserModel.Password.Trim());
                        objModule[0].Password = EncryptedPasswordnew;
                    }

                    objModule[0].EmailID = ObjUserModel.EmailID == null ? "" : ObjUserModel.EmailID.Trim();
                    objModule[0].MobileNumber = ObjUserModel.MobileNumber == null ? "" : ObjUserModel.MobileNumber.Trim();
                    objModule[0].LastModifiedBy = ObjUserModel.CreatedBy;
                    objModule[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                    objModule[0].Status = ObjUserModel.Status;
                    objModule[0].PasswordAttemptCount = 0;
                    db.SaveChanges();

                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }
                else
                {
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.NORECORD;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "UserMaster_Update");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "UserMaster_Update");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }

            return ObjMessage;
        }

        public ReturnMessageModel UserMaster_Delete(int UserID, int deletedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                


                var objuserMaster = (from tbluser in db.TblUserMasters
                                       where tbluser.UserID == UserID &&
                                             tbluser.Status == true
                                       select tbluser).ToList();
                        if (objuserMaster.Count > 0)
                        {
                               objuserMaster[0].Status = false;
                             objuserMaster[0].LastModifiedBy = deletedBy;
                              objuserMaster[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                            db.SaveChanges();
                        }

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "UserMaster_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "UserMaster_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            return ObjMessage;
        }
        
        public UserMasterModel UserMaster_Edit(int UserID)
        {
            UserMasterModel Obj = null;
            try
            {

                Obj = (from UM in db.TblUserMasters
                       where UM.UserID == UserID
                       select new UserMasterModel
                       {
                           UserID = UM.UserID,
                           RoleID = UM.RoleID,
                           LoginName = UM.LoginName,
                           FirstName = UM.FirstName,
                           LastName = UM.LastName,
                           Password = UM.Password,
                           EmailID = UM.EmailID,
                           MobileNumber = UM.MobileNumber,
                           Status = UM.Status
                       }).FirstOrDefault();

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "UserMaster_Edit");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "UserMaster_Edit");
            }
            return Obj;
        }

        public List<UserMasterDetailModel> UserMaster_FindAll(int? page, int? limit, string LoginName, string DisplayName, bool? Status, string sortBy, string direction, out int TotalCount)
        {
            TotalCount = 0;
            List<UserMasterDetailModel> ObjUser = new List<UserMasterDetailModel>();
            try
            {


                ObjUser = (from UM in db.TblUserMasters
                           where ((LoginName == "" || LoginName == null) || UM.LoginName.Contains(LoginName))
                           && ((DisplayName == "" || DisplayName == null) || (UM.FirstName + " " + UM.LastName).Contains(DisplayName))
                           && ((Status == null && UM.Status == true) || UM.Status == Status)
                           select new UserMasterDetailModel
                           {
                               UserID = UM.UserID,
                               RoleName = (from RM in db.TblRoleMasters where RM.RoleID == UM.RoleID select RM.RoleName).FirstOrDefault(),
                               DisplayName = UM.FirstName + " " + UM.LastName,
                               LoginName = UM.LoginName,
                               FirstName = UM.FirstName,
                               LastName = UM.LastName, 
                               Password = UM.Password,
                               EmailID = UM.EmailID, 
                               Status = UM.Status,
                               MobileNumber = UM.MobileNumber,
                               CreatedDate = UM.CreatedDate,
                               LastModifiedDate = UM.LastModifiedDate,
                               CreatedUser = (from UMC in db.TblUserMasters where UMC.UserID == UM.CreatedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                               LastModifiedUser = (from UMC in db.TblUserMasters where UMC.UserID == UM.LastModifiedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                               
                           }).ToList();


                if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
                {
                    if (direction.Trim().ToLower() == "asc")
                    {
                        switch (sortBy.Trim())
                        {
                            case "LoginName":
                                ObjUser = ObjUser.OrderBy(q => q.LoginName).ToList();
                                break;
                            case "DisplayName":
                                ObjUser = ObjUser.OrderBy(q => q.DisplayName).ToList();
                                break;
                            case "RoleName":
                                ObjUser = ObjUser.OrderBy(q => q.RoleName).ToList();
                                break;
                            case "CreatedUser":
                                ObjUser = ObjUser.OrderBy(q => q.CreatedUser).ToList();
                                break;

                            case "LastModifiedUser":
                                ObjUser = ObjUser.OrderBy(q => q.LastModifiedUser).ToList();
                                break;
                            case "DisplayCreatedDate":
                                ObjUser = ObjUser.OrderBy(q => q.CreatedDate).ToList();
                                break;
                            case "DisplayModifiedDate":
                                ObjUser = ObjUser.OrderBy(q => q.LastModifiedDate).ToList();
                                break; 
                            case "Status":
                                ObjUser = ObjUser.OrderBy(q => q.Status).ToList();
                                break;
                        }
                    }
                    else
                    {
                        switch (sortBy.Trim())
                        {
                            case "LoginName":
                                ObjUser = ObjUser.OrderByDescending(q => q.LoginName).ToList();
                                break;
                            case "DisplayName":
                                ObjUser = ObjUser.OrderByDescending(q => q.DisplayName).ToList();
                                break;
                            case "RoleName":
                                ObjUser = ObjUser.OrderByDescending(q => q.RoleName).ToList();
                                break;
                            case "CreatedUser":
                                ObjUser = ObjUser.OrderByDescending(q => q.CreatedUser).ToList();
                                break;
                            case "DisplayCreatedDate":
                                ObjUser = ObjUser.OrderByDescending(q => q.CreatedDate).ToList();
                                break;
                            case "DisplayModifiedDate":
                                ObjUser = ObjUser.OrderByDescending(q => q.LastModifiedDate).ToList();
                                break;
                            case "Status":
                                ObjUser = ObjUser.OrderByDescending(q => q.Status).ToList();
                                break;
                        }
                    }
                }
                else
                {
                    ObjUser = ObjUser.OrderByDescending(q => q.CreatedDate).ToList();
                }
                TotalCount = ObjUser.Count;

                if (page.HasValue && limit.HasValue)
                {
                    int start = (page.Value - 1) * limit.Value;
                    ObjUser = ObjUser.Skip(start).Take(limit.Value).ToList();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "UserMaster_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "UserMaster_FindAll");
            }
            return ObjUser;

        }
        #endregion       
        
        #region "Menu Master"
        public List<MenuMasterHeader> MenuMasterHeader_FindAll(int? page, int? limit, string MenuName, string sortBy, string direction, out int TotalCount)
        {
            TotalCount = 0;
            List<MenuMasterHeader> ObjMenu = new List<MenuMasterHeader>();

            try
            {
                ObjMenu = (from RM in db.TblMenuMasterHeaders

                           where ((MenuName == "" || MenuName == null) || RM.MenuName.Contains(MenuName)) 
                           select new MenuMasterHeader
                           {
                               HeaderViewID = RM.HeaderViewID,
                               MenuName = RM.MenuName,
                               OrderByTab = RM.OrderByTab,
                               IconCls = RM.IconCls,
                               Disabled=RM.Disabled,
                               CreatedDate = RM.CreatedDate,
                               LastModifiedDate = RM.LastModifiedDate,
                               CreatedUser = (from UMC in db.TblUserMasters where UMC.UserID == RM.CreatedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                               LastModifiedUser = (from UMC in db.TblUserMasters where UMC.UserID == RM.LastModifiedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                           }).ToList();
                if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
                {
                    if (direction.Trim().ToLower() == "asc")
                    {
                        switch (sortBy.Trim())
                        {
                            case "MenuName":
                                ObjMenu = ObjMenu.OrderBy(q => q.MenuName).ToList();
                                break;
                            case "OrderByTab":
                                ObjMenu = ObjMenu.OrderBy(q => q.OrderByTab).ToList();
                                break;
                            case "Disabled":
                                ObjMenu = ObjMenu.OrderBy(q => q.Disabled).ToList();
                                break;
                            case "DisplayCreatedDate":
                                ObjMenu = ObjMenu.OrderBy(q => q.CreatedDate).ToList();
                                break;
                            case "DisplayModifiedDate":
                                ObjMenu = ObjMenu.OrderBy(q => q.LastModifiedDate).ToList();
                                break;
                            case "CreatedUser":
                                ObjMenu = ObjMenu.OrderBy(q => q.CreatedUser).ToList();
                                break;
                            case "LastModifiedUser":
                                ObjMenu = ObjMenu.OrderBy(q => q.LastModifiedUser).ToList();
                                break;

                        }
                    }
                    else
                    {
                        switch (sortBy.Trim())
                        {
                            case "MenuName":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.MenuName).ToList();
                                break;
                            case "OrderByTab":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.OrderByTab).ToList();
                                break;
                            case "Disabled":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.Disabled).ToList();
                                break;
                            case "DisplayCreatedDate":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.CreatedDate).ToList();
                                break;
                            case "DisplayModifiedDate":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.LastModifiedDate).ToList();
                                break;
                            case "CreatedUser":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.CreatedUser).ToList();
                                break;
                            case "LastModifiedUser":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.LastModifiedUser).ToList();
                                break;
                        }
                    }
                }
                else
                {
                    ObjMenu = ObjMenu.OrderBy(q => q.OrderByTab).ToList();
                }

                TotalCount = ObjMenu.Count;

                if (page.HasValue && limit.HasValue)
                {
                    int start = (page.Value - 1) * limit.Value;
                    ObjMenu = ObjMenu.Skip(start).Take(limit.Value).ToList();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterHeader_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterHeader_FindAll");
            }
            return ObjMenu;
        }

        public MenuMasterHeader MenuHeaderMaster_Edit(string HeaderViewID)
        {
            MenuMasterHeader Obj = null;
            try
            {

                Obj = (from MM in db.TblMenuMasterHeaders
                       where MM.HeaderViewID == HeaderViewID
                       select new MenuMasterHeader
                       {
                           MenuName = MM.MenuName,
                           IconCls = MM.IconCls,
                           OrderByTab = MM.OrderByTab,
                           Disabled = MM.Disabled
                       }).FirstOrDefault();

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMaster_Edit");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMaster_Edit");
            }
            return Obj;
        }
        public ReturnMessageModel MenuMasterHeader_Save(MenuMasterHeader ObjMenuMasterHeaderModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                    var obj = (from tblMenuHeader in db.TblMenuMasterHeaders
                               where tblMenuHeader.MenuName == ObjMenuMasterHeaderModel.MenuName
                                    && tblMenuHeader.HeaderViewID != ObjMenuMasterHeaderModel.HeaderViewID
                                    && tblMenuHeader.OrderByTab == ObjMenuMasterHeaderModel.OrderByTab
                                    && tblMenuHeader.Disabled == false
                               select tblMenuHeader).ToList();

                    if (obj != null && obj.Count == 0)
                    {
                        if (ObjMenuMasterHeaderModel.HeaderViewID != "" && ObjMenuMasterHeaderModel.HeaderViewID != null)
                        {
                            ObjMessage = MenuMasterHeader_Update(ObjMenuMasterHeaderModel);
                        }
                        else
                        {
                            var objOrder = (from tblMenuHeader in db.TblMenuMasterHeaders
                                            where tblMenuHeader.OrderByTab == ObjMenuMasterHeaderModel.OrderByTab
                                            && tblMenuHeader.Disabled == false
                                            select tblMenuHeader).ToList();
                            if (objOrder != null && objOrder.Count == 0)
                            {
                                ObjMessage = MenuMasterHeader_Insert(ObjMenuMasterHeaderModel);
                            }
                            else
                            {
                                ObjMessage.Result = false;
                                ObjMessage.Status = WEBCONSTANTMESSAGECODE.DUPLICATEORDER;
                                ObjMessage.Message = WEBCONSTANTMESSAGE.DUPLICATEORDERFAIL;
                            }
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
                ObjCom.LogPageError(e, "MenuMasterHeader_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterHeader_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }

            return ObjMessage;
        }

        public ReturnMessageModel MenuMasterHeader_Insert(MenuMasterHeader objMenuMasterHeaderModel)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            var MenuHeaderID = CommonRepository.RandomString(12);
            try
            {
                var entity = new TblMenuMasterHeader
                {
                    HeaderViewID = MenuHeaderID,
                    MenuName = objMenuMasterHeaderModel.MenuName,
                    IconCls = objMenuMasterHeaderModel.IconCls,
                    OrderByTab = objMenuMasterHeaderModel.OrderByTab,
                    Disabled = objMenuMasterHeaderModel.Disabled,
                    CreatedBy = objMenuMasterHeaderModel.CreatedBy,
                    CreatedDate = CommonRepository.GetTimeZoneDate(),
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterHeader_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterHeader_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel MenuMasterHeader_Update(MenuMasterHeader ObjMenuMasterHeaderModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objMenuHeader = (from tblMenuHeader in db.TblMenuMasterHeaders
                                     where tblMenuHeader.HeaderViewID == ObjMenuMasterHeaderModel.HeaderViewID
                                     select tblMenuHeader).ToList();

                if (objMenuHeader.Count > 0)
                {
                    objMenuHeader[0].MenuName = ObjMenuMasterHeaderModel.MenuName.Trim();
                    objMenuHeader[0].IconCls = ObjMenuMasterHeaderModel.IconCls == null ? "" : ObjMenuMasterHeaderModel.IconCls.Trim();
                    objMenuHeader[0].Disabled = ObjMenuMasterHeaderModel.Disabled;
                    objMenuHeader[0].OrderByTab = ObjMenuMasterHeaderModel.OrderByTab;
                    objMenuHeader[0].LastModifiedBy = ObjMenuMasterHeaderModel.CreatedBy;
                    objMenuHeader[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                    db.SaveChanges();

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }
                else
                {
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.NORECORD;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterHeader_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterHeader_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }

            return ObjMessage;
        } 

        public List<MenuMasterDetail> MenuMasterDetail_FindAll(string MenuMasterHeaderID, int? page, int? limit, string SearchColumn, string searchString, string sortBy, string direction, out int TotalCount)
        {
            TotalCount = 0;
            List<MenuMasterDetail> ObjMenu = new List<MenuMasterDetail>();

            try
            {
                ObjMenu = (from RM in db.TblMenuMasterDetails
                           where ((MenuMasterHeaderID != "" && MenuMasterHeaderID != null) && RM.HeaderViewID==MenuMasterHeaderID)                            
                           select new MenuMasterDetail
                           {
                               HeaderViewID = RM.HeaderViewID,
                               DetailViewID=RM.DetailViewID,
                               MenuName = RM.MenuName,
                               PageUrl = RM.PageUrl==null?"": RM.PageUrl,
                               OrderByTab = RM.OrderByTab==null?0: RM.OrderByTab,
                               IconCls = RM.IconCls==null?"": RM.IconCls,
                               IsPageAccess = RM.IsPageAccess,
                               Disabled = RM.Disabled,
                               CreatedDate = RM.CreatedDate,
                               LastModifiedDate = RM.LastModifiedDate,
                               CreatedUser = (from UMC in db.TblUserMasters where UMC.UserID == RM.CreatedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                               LastModifiedUser = (from UMC in db.TblUserMasters where UMC.UserID == RM.LastModifiedBy select UMC.FirstName + " " + UMC.LastName).FirstOrDefault(),
                           }).ToList();
                if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
                {
                    if (direction.Trim().ToLower() == "asc")
                    {
                        switch (sortBy.Trim())
                        {
                            case "MenuName":
                                ObjMenu = ObjMenu.OrderBy(q => q.MenuName).ToList();
                                break;
                            case "OrderByTab":
                                ObjMenu = ObjMenu.OrderBy(q => q.OrderByTab).ToList();
                                break;
                            case "Disabled":
                                ObjMenu = ObjMenu.OrderBy(q => q.Disabled).ToList();
                                break;
                            case "DisplayCreatedDate":
                                ObjMenu = ObjMenu.OrderBy(q => q.CreatedDate).ToList();
                                break;
                            case "DisplayModifiedDate":
                                ObjMenu = ObjMenu.OrderBy(q => q.LastModifiedDate).ToList();
                                break;
                            case "CreatedUser":
                                ObjMenu = ObjMenu.OrderBy(q => q.CreatedUser).ToList();
                                break;
                            case "LastModifiedUser":
                                ObjMenu = ObjMenu.OrderBy(q => q.LastModifiedUser).ToList();
                                break;
                        }
                    }
                    else
                    {
                        switch (sortBy.Trim())
                        {
                            case "MenuName":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.MenuName).ToList();
                                break;
                            case "OrderByTab":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.OrderByTab).ToList();
                                break;
                            case "Disabled":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.Disabled).ToList();
                                break;
                            case "DisplayCreatedDate":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.CreatedDate).ToList();
                                break;
                            case "DisplayModifiedDate":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.LastModifiedDate).ToList();
                                break;
                            case "CreatedUser":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.CreatedUser).ToList();
                                break;
                            case "LastModifiedUser":
                                ObjMenu = ObjMenu.OrderByDescending(q => q.LastModifiedUser).ToList();
                                break;
                        }
                    }
                }
                else
                {
                    ObjMenu = ObjMenu.OrderBy(q => q.OrderByTab).ToList();
                }

                TotalCount = ObjMenu.Count;

                if (page.HasValue && limit.HasValue)
                {
                    int start = (page.Value - 1) * limit.Value;
                    ObjMenu = ObjMenu.Skip(start).Take(limit.Value).ToList();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterDetailMasterModel_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterDetailMasterModel_FindAll");
            }
            return ObjMenu;
        }

        public MenuMasterDetail MenuMasterDetail_Edit(string HeaderViewID,string DetailViewID)
        {
            MenuMasterDetail Obj = null;
            try
            {

                Obj = (from MM in db.TblMenuMasterDetails
                       where MM.HeaderViewID == HeaderViewID && MM.DetailViewID == DetailViewID
                       select new MenuMasterDetail
                       {
                           HeaderViewID = HeaderViewID,
                           DetailViewID=DetailViewID,
                           MenuName = MM.MenuName,
                           IconCls = MM.IconCls,
                           OrderByTab = MM.OrderByTab,
                           PageUrl = MM.PageUrl,
                           IsPageAccess = MM.IsPageAccess,
                           Disabled = MM.Disabled
                       }).FirstOrDefault();

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterDetail_Edit");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterDetail_Edit");
            }
            return Obj;
        }
        

        public ReturnMessageModel MenuMasterDetail_Save(MenuMasterDetail objMenuDetailModel, List<LoadItems_String> SelectedAccess)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objOrder = (from tblMenuDetail in db.TblMenuMasterDetails
                                where
                                tblMenuDetail.HeaderViewID==objMenuDetailModel.HeaderViewID
                                &&
                                tblMenuDetail.OrderByTab == objMenuDetailModel.OrderByTab
                                && tblMenuDetail.Disabled == false
                                select tblMenuDetail).ToList();
                if (objOrder != null && objOrder.Count == 0  || (objOrder.Count >0 && objMenuDetailModel.DetailViewID !=null))
                {
                    var obj = (from tblMenu in db.TblMenuMasterDetails
                               where tblMenu.MenuName == objMenuDetailModel.MenuName
                                   && tblMenu.HeaderViewID == objMenuDetailModel.HeaderViewID
                                   && tblMenu.OrderByTab == tblMenu.OrderByTab
                                       && tblMenu.DetailViewID != objMenuDetailModel.DetailViewID
                                   && tblMenu.Disabled != true
                               select tblMenu).ToList();

                    if (obj != null && obj.Count == 0)
                    {
                        if (objMenuDetailModel.DetailViewID != "" && objMenuDetailModel.DetailViewID != null)
                        {
                            MenuMasterDetail_Update(objMenuDetailModel, SelectedAccess);
                            ObjMessage.Result = true;
                            ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                            ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                        }
                        else
                        {
                            ObjMessage.Result = true;
                            MenuMasterDetail_Insert(objMenuDetailModel, SelectedAccess);
                            ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                            ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
                        }
                    }
                    else
                    {
                        ObjMessage.Result = false;
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.DUPLICATE;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.DUPLICATEFAIL;
                    }
                }
                else
                {
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DUPLICATEORDER;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DUPLICATEORDERFAIL;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterDetail_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }

            return ObjMessage;
        }

        public ReturnMessageModel MenuMasterDetail_Insert(MenuMasterDetail objMenuMasterDetailModel, List<LoadItems_String> SelectedAccess)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            var MenuDetailID = CommonRepository.RandomString(12);

            try
            {
                var entity = new TblMenuMasterDetail
                {
                    DetailViewID = MenuDetailID,
                    HeaderViewID = objMenuMasterDetailModel.HeaderViewID,
                    MenuName = objMenuMasterDetailModel.MenuName,
                    PageUrl = objMenuMasterDetailModel.PageUrl,
                    IconCls = objMenuMasterDetailModel.IconCls,
                    OrderByTab = objMenuMasterDetailModel.OrderByTab,
                    Disabled = objMenuMasterDetailModel.Disabled,
                    IsPageAccess = objMenuMasterDetailModel.IsPageAccess,
                    CreatedBy = objMenuMasterDetailModel.CreatedBy,
                    CreatedDate = CommonRepository.GetTimeZoneDate(),
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();               

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterDetail_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterDetail_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel MenuMasterDetail_Update(MenuMasterDetail objMenuMasterDetailModel, List<LoadItems_String> SelectedAccess)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objMenu = (from tblMenu in db.TblMenuMasterDetails
                               where tblMenu.HeaderViewID == objMenuMasterDetailModel.HeaderViewID
                               && tblMenu.DetailViewID == objMenuMasterDetailModel.DetailViewID
                               select tblMenu).ToList();
                if (objMenu != null && objMenu.Count > 0)
                {
                    objMenu[0].MenuName = objMenuMasterDetailModel.MenuName;
                    objMenu[0].PageUrl = objMenuMasterDetailModel.PageUrl;
                    objMenu[0].IconCls = objMenuMasterDetailModel.IconCls;
                    objMenu[0].OrderByTab = objMenuMasterDetailModel.OrderByTab;
                    objMenu[0].Disabled = objMenuMasterDetailModel.Disabled;
                    objMenu[0].IsPageAccess = objMenuMasterDetailModel.IsPageAccess;
                    objMenu[0].LastModifiedBy = objMenuMasterDetailModel.CreatedBy;
                    objMenu[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                    db.SaveChanges();
                }         
                 
                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterDetail_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterDetail_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }

            return ObjMessage;
        }

        public ReturnMessageModel MenuMasterDetailOrder_Change(MenuMasterDetail ObjMenuMasterDetailModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                bool iSChange = false;
                int int_MovingOrder = 0;

                if (ObjMenuMasterDetailModel.ChangeMenuMasterDirection == "DOWN")
                {
                    int_MovingOrder = Convert.ToInt32(ObjMenuMasterDetailModel.OrderByTab) + 1;
                }
                else
                {
                    int_MovingOrder = Convert.ToInt32(ObjMenuMasterDetailModel.OrderByTab) - 1;
                }

                var ObjDownCurrent = (from GD in db.TblMenuMasterDetails
                                      where GD.HeaderViewID == ObjMenuMasterDetailModel.HeaderViewID
                                      && GD.OrderByTab == ObjMenuMasterDetailModel.OrderByTab
                                      select GD).ToList();

                var ObjDownNext = (from GD in db.TblMenuMasterDetails
                                   where GD.HeaderViewID == ObjMenuMasterDetailModel.HeaderViewID
                                   && GD.OrderByTab == int_MovingOrder
                                   select GD).ToList();

                if (ObjDownNext.Count != 0)
                {
                    ObjDownNext[0].OrderByTab = ObjMenuMasterDetailModel.OrderByTab;
                    ObjDownNext[0].LastModifiedBy = ObjMenuMasterDetailModel.LastModifiedBy;
                    ObjDownNext[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                    db.SaveChanges();
                }
                if (ObjDownNext.Count != 0)
                {

                    if (ObjDownCurrent.Count != 0)
                    {
                        ObjDownCurrent[0].OrderByTab = int_MovingOrder;
                        ObjDownCurrent[0].LastModifiedBy = ObjMenuMasterDetailModel.LastModifiedBy;
                        ObjDownCurrent[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                        db.SaveChanges();
                        iSChange = true;
                    }

                }

                if (iSChange)
                {
                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }
                else
                {
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.ORDERFAIL;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterHeaderOrder_Change");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterHeaderOrder_Change");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;

        }

        public ReturnMessageModel MenuMasterHeader_Delete(string HeaderViewID, int LastModifiedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDelete = (from ct in db.TblMenuMasterHeaders
                                 where ct.HeaderViewID == HeaderViewID
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count == 1)
                {

                    var objDel = (from ct in db.TblMenuMasterDetails
                                  where ct.HeaderViewID == HeaderViewID
                                  select ct).ToList();

                    if (objDel != null && objDel.Count == 1)
                    {

                        foreach (var tblSMod in objDel)
                        {
                            db.TblMenuMasterDetails.Remove(tblSMod);
                            db.SaveChanges();
                        }
   
                    }

                    foreach (var tblSMod in objDelete)
                    {
                        db.TblMenuMasterHeaders.Remove(tblSMod);
                        db.SaveChanges();
                    }


                    db.SaveChanges();

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterHeader_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterHeader_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel MenuMasterDetail_Delete(string DetailViewID, int LastModifiedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
 
                var objMenuDetail = (from tblMenu in db.TblMenuMasterDetails
                                     where tblMenu.DetailViewID == DetailViewID
                                     select tblMenu).ToList();
                if (objMenuDetail.Count > 0)
                {
                    foreach (var tblSMod in objMenuDetail)
                    {
                        db.TblMenuMasterDetails.Remove(tblSMod);
                        db.SaveChanges();
                    }
                }

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "MenuMasterDetail_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "MenuMasterDetail_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            return ObjMessage;
        }

        #endregion

        public static string ToString(object o)
        {
            return ToString(o, "");
        }
        public static string ToString(object o, string Default)
        {
            if (o == null || o == DBNull.Value)
                return Default;

            try
            {
                return o.ToString();
            }
            catch //(Exception ex)
            {
                //				return ex.Message;
                return Default;
            }
        }
    }
}









