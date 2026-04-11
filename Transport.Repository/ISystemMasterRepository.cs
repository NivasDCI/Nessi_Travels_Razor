using Transport.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transport.Repository
{
    public interface ISystemMasterRepository
    {
        #region "User Master"
        List<UserMasterDetailModel> UserMaster_FindAll(int? page, int? limit, string LoginName, string DisplayName, bool? Status, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel UserMaster_Save(UserMasterModel ObjUserModel);
        ReturnMessageModel UserMaster_Delete(int UserID, int deletedBy);
        UserMasterModel UserMaster_Edit(int UserID);
        ReturnMessageModel UserProfile_Save(UserMasterModel ObjUserModel);
        bool Get_UserPOSStatus(string RoleID);

        #endregion

        #region "ResetUser Master"
        List<UserMasterDetailModel> ResetUserLogin_FindAll(int? page, int? limit, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel ResetUserLogin_Update(int UserID);
        #endregion

        
        //Menu Header Master
        List<MenuMasterHeader> MenuMasterHeader_FindAll(int? page, int? limit, string MenuName, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel MenuMasterHeader_Save(MenuMasterHeader ObjMenuMasterHeaderModel);
        ReturnMessageModel MenuMasterHeader_Delete(string HeaderViewID, int LastModifiedBy);
        
        MenuMasterHeader MenuHeaderMaster_Edit(string HeaderViewID);
       

        //Menu Detail Master
        List<MenuMasterDetail> MenuMasterDetail_FindAll(string MenuMasterHeaderID, int? page, int? limit, string SearchColumn, string searchString, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel MenuMasterDetail_Save(MenuMasterDetail ObjMenuMasterDetailModel, List<LoadItems_String> SelectedAccess);
        ReturnMessageModel MenuMasterDetail_Delete(string DetailViewID, int LastModifiedBy); 
        MenuMasterDetail MenuMasterDetail_Edit(string HeaderViewID, string DetailViewID);

    }
}
