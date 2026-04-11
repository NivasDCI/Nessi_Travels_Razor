using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc; 

namespace Transport.Model
{

    public static class CommonDateFormat
    {
        public static string StringDateFormat
        {
            get { return "dd-MMM-yyyy hh:mm tt"; }
        }
        public static string StringDateonlyFormat
        {
            get { return "dd-MMM-yyyy"; }
        }
    }

    public class ReturnMessageModel
    {
        public long? HeaderID { get; set; }
        public long? DetailID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool Result { get; set; }
        public long? UniqueID { get; set; }
        public string DefaultUniqueID { get; set; }
        public string ConcurrencyDate { get; set; }
    }

    public class LoadItems_String
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public class LoadMenus_String
    {
        public string HeaderViewID { get; set; }
        public string DetailViewID { get; set; }
    }    

    public class LoadItems_Str
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public class LoadItems_Int
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class LoadItems_BigInt
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public int? Order { get; set; }
    }

    public class LoadItems_ToFromDate
    {
        public long ID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class LoadItems_Decimal
    {
        public string Code { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
    }

    public class SessionLoginDetail
    {
        public int UserID { get; set; }
        public string RoleID { get; set; }
        public string StoreID { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsPOS { get; set; }
        public string HomePageViewID { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; } 
        public bool? IsSuperAdmin { get; set; }
        public string HomePageName { get; set; } 
        public string TimeZone { get; set; }
    }

    public class SessionInvalidLogin
    {
        public int UserID { get; set; }
        public bool? IsRoleMapping { get; set; }
        public bool? InValidPassword { get; set; }
    }

    public class DefaultTypeItems
    {
        public string DefaultID { get; set; }
        public string DefaultName { get; set; }
        public string DefaultValue { get; set; }
        public int? DefaultOrder { get; set; }
    }

    public class DefaultHeaderNameItems
    {
        public string DefaultID { get; set; }
        public string DefaultName { get; set; }
        public int? DefaultOrder { get; set; }
    }

    public class Language
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class HomePage
    {
        public string HomePageViewID { get; set; }
        public string HomePageName { get; set; }
    }     

    public class ProfileMasterModel
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string RoleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string usertype { get; set; }
        public string EmailID { get; set; } 
        public string MobileNumber { get; set; } 
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; } 
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; } 
    }

    public class UserMasterModel
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }              
        public Nullable<bool> Status { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedUser { get; set; } 
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public string LastModifiedUser { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }  
        public string RoleID { get; set; }        
        public IEnumerable<SelectListItem> LoadUserRoleData { get; set; }

    }
    
    public class UserMasterDetailModel
    {
        public int UserID { get; set; }  
        public string LoginName { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string RoleName { get; set; }
        public string EmailID { get; set; }  
        public string MobileNumber { get; set; } 
        public string CreatedUser { get; set; }
       
        public string DisplayCreatedDate
        {
            get
            {
                return CreatedDate != null ? CreatedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateonlyFormat) : "";
            }
            set { }
        }

        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string LastModifiedUser { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public string DisplayModifiedDate
        {
            get
            {
                return LastModifiedDate != null ? LastModifiedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateonlyFormat) : "";
            }
            set { }
        }
        public Nullable<bool> Status { get; set; }
        
         
    }

    public class RoleMasterModel
    {
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; } 
        public string ProjectName { get; set; }
        public string HomePageViewID { get; set; }
        public string UserType { get; set; }
        public string UserTypeStr { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public Nullable<bool> IsPOS { get; set; }
        public Nullable<bool> status { get; set; }
        public Nullable<bool> IsAdmin { get; set; }
        public string IsAttachment { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedUser { get; set; }
        public string DisplayCreatedDate
        {
            get
            {
                return CreatedDate != null ? CreatedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateFormat) : "";
            }
            set { }
        }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public string LastModifiedUser { get; set; }
        public string DisplayModifiedDate
        {
            get
            {
                return LastModifiedDate != null ? LastModifiedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateFormat) : "";
            }
            set { }
        }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        
    }

    public class RoleMatrixModel
    { 
        public IEnumerable<SelectListItem> LoadRoleData { get; set; }

        public int RoleMatrixAccessID { get; set; }
        public string RoleID { get; set; } 
        public string HeaderMenu { get; set; }
        public string DetailMenu { get; set; }
        public string HeaderViewID { get; set; }
        public string DetailViewID { get; set; }
        public string PageAccessName { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> IsPageAccess { get; set; }
        public Nullable<bool> IsHeader { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<bool> IsMenuDetailPageAccess { get; set; }
        public int? DisplayOrder { get; set; }
        public int? HeaderDisplayOrder { get; set; }
        public int? MinOrder { get; set; }
        public int? MaxOrder { get; set; }
        public bool IsUpdate { get; set; }

    }

    public class DefaultMasterModel
    {
        public string DefaultHeaderID { get; set; } 
        public string DefaultHeaderName { get; set; }
        public string DisplayColumnValue { get; set; }
        public string DisplayColumnText { get; set; }
        public Nullable<bool> IsTextValue { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
    }

    public class MenuMasterHeader
    {
        public string HeaderViewID { get; set; }
        public string MenuName { get; set; }
        public Nullable<bool> Closable { get; set; }
        public Nullable<bool> Disabled { get; set; }
        public Nullable<int> OrderByTab { get; set; }
        public string IconCls { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string DisplayCreatedDate
        {
            get
            {
                return CreatedDate != null ? CreatedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateFormat) : "";
            }
            set { }
        }
        public Nullable<int> LastModifiedBy { get; set; }
        public string LastModifiedUser { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public string DisplayModifiedDate
        {
            get
            {
                return LastModifiedDate != null ? LastModifiedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateFormat) : "";
            }
            set { }
        }
        public String ChangeMenuMasterDirection { get; set; }
    }

    public class MenuMasterDetail
    {
        public string DetailViewID { get; set; }
        public string HeaderViewID { get; set; }
        public string MenuName { get; set; }
        public string PageUrl { get; set; }
        public string IconCls { get; set; }
        public Nullable<bool> Disabled { get; set; }
        public Nullable<int> OrderByTab { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedUser { get; set; }
        public string DisplayCreatedDate
        {
            get
            {
                return CreatedDate != null ? CreatedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateFormat) : "";
            }
            set { }
        }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public string LastModifiedUser { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public string DisplayModifiedDate
        {
            get
            {
                return LastModifiedDate != null ? LastModifiedDate.Value.ToString(Transport.Model.CommonDateFormat.StringDateFormat) : "";
            }
            set { }
        }
        public String ChangeMenuMasterDirection { get; set; }
      
        public Nullable<bool> IsPageAccess { get; set; }
    }

    public class MenuMasterPageAccess
    {
        public String MenuPageAccessViewID { get; set; }
        public String PageAccessID { get; set; }
        public String MenuMasterID { get; set; }
        public String MenuType { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }

    public class DefaultDetailMasterModel
    {
        public string DefaultDetailID { get; set; }
        public string DefaultHeaderID { get; set; }
        public string DefaultTextField { get; set; }
        public string DefaultValueField { get; set; }
        public Nullable<int> DefaultOrder { get; set; }
        public Nullable<bool> IsSearch { get; set; }
        public Nullable<bool> IsPage { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public byte[] Image { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
    } 

    public  class SystemMasterModel
    {
        public string SystemViewID { get; set; }
        public string ConfigurationName { get; set; }
        public string SystemConfigurationViewID { get; set; }
        public string ConfigurationType { get; set; }
        public string ConfigurationControl { get; set; }
        public string ConfigurationDataSetName { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public string UploadFilepath { get; set; }
        public byte[] UploadData { get; set; }
        public Nullable<bool> BitValue { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
    }

    public class Query
    {
        public long id { get; set; }
        public string label { get; set; }
        public string BrandName { get; set; }
        public string PartName { get; set; }
        public string ItemName { get; set; }
        public Nullable<double> Qty { get; set; }
        public Nullable<decimal> Price { get; set; }
        public long BrandID { get; set; }
        public long PartID { get; set; }
        public long ItemID { get; set; }
    }


}
