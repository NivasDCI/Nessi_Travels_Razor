using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transport.Repository.Resource;

namespace Transport.Repository
{
    public static class WEBPAGEBUTTON
    {
        public static string CLEAR
        {
            get { return Button.Button_Clear; }
        }
        public static string SEARCH
        {
            get { return Button.Button_Search; }
        } 
        public static string ADD
        {
            get { return Button.Button_Add; }
        }
        public static string SAVE
        {
            get { return Button.Button_Save; }
        }
        public static string DELETE
        {
            get { return Button.Button_Delete; }
        }
        public static string CLOSE
        {
            get { return Button.Button_Close; }
        } 
        public static string EDIT
        {
            get { return Button.Button_Edit; }
        }
         
        public static string UPDATE
        {
            get { return Button.Button_Update; }
        } 
        public static string UPLOAD
        {
            get { return Button.Button_Upload; }
        }
        public static string DOWNLOAD
        {
            get { return Button.Button_Download; }
        }
        public static string DownloadTemplate
        {
            get { return Button.Button_DownloadTemplate; }
        }

        public static string RESET
        {
            get { return Button.Button_Reset; }
        }

        public static string PAYMENT
        {
            get { return Button.Button_Payment; }
        } 

        public static string CANCEL
        {
            get { return Button.Button_Cancel; }
        }

        public static string CLOSED
        {
            get { return Button.Button_TodaySalesClosed; }
        }

        public static string PDF
        {
            get { return Button.Button_Pdf; }
        }

        public static string EXCEL
        {
            get { return Button.Button_Excel; }
        }

        public static string PDF1
        {
            get { return Button.Button_Pdf1; }
        }

        public static string EXCEL1
        {
            get { return Button.Button_Excel1; }
        }
        public static string APPROVAL
        {
            get { return Button.Button_Approval; }
        }
        public static string DELIVERED
        {
            get { return Button.Button_GoodReceipt; }
        }

        public static string MERGE
        {
            get { return Button.Button_Merge; }
        }
    }


    public static class SystemConfig
    {       
        public const int SystemConfig_DefaultYear_DOB = 10; 
    }

    public static class WEBCONSTANTMESSAGECODE
    {
        public const string INSERT = "INSERT";
        public const string NOSTOCK = "NOSTOCK";
        public const string UPDATE = "UPDATE";
        public const string ERROR = "ERROR";
        public const string DELETE = "DELETE";
        public const string UPLOAD = "UPLOAD";
        public const string DUPLICATEBARCODE = "DUPLICATE BARCODE";
        public const string DUPLICATE = "DUPLICATE";
        public const string DUPLICATEORDER = "DUPLICATE ORDER";  
        public const string Approval_Approved = "Approved"; 
        public const string Approval_Rejected = "Rejected"; 
        public const string FILEDUPLICATE = "FileDuplicate"; 
        public const string NOTAVAILABLE = "Product Not Available";
        public const string Transferred = "Product Transferred";
        public const string PurchaseReceived = "Purchase Received";  
        public const string MULTIRATES = "MULTIRATES"; 
        public const string SalesPaymentStatus = "Sales";
        public const string SalesReturnPaymentStatus = "Sales Return";
        public const string PurchasePaymentStatus = "Purchase";
        public const string PurchaseReturnPaymentStatus = "Purchase Return";
        public const string HSNLocal = "L";
        public const string HSNInternational = "I";
        public const string SalesReportTypePOS = "Retail";
        public const string SalesReportTypeDISTRIBUTION = "Distribution";
        public const string InclusiveTax = "Inclusive Tax";
        public const string ExclusiveTax = "Exclusive Tax";
        public const string DELIVERED = "DELIVERED";
    }
    public static class SYSTEMCONFIG
    { 
        public const string SystemConfig_DefaultCategory_ViewID = "JA8LS92KXO01";
    }
    public static class DEFAULTMASTER
    {  
        public const string DefaultMaster_DefaultStatus_ViewID = "U3NN8B8V9B1Q";
        public const string DefaultHeaderMaster_PurchaseStatus_ViewID = "DEFPURCSTATUS";
        public const string DefaultHeaderMaster_PaymentStatus_ViewID = "JO10SL3MA93L";
        public const string DefaultHeaderMaster_PaymentMode_ViewID = "DEFPYMTSTATUS";
        //Purchase
        public const string DefaultDetailMaster_CancelledPurchaseStatus_ViewID = "7PS43KQ4OA1L";
        public const string DefaultDetailMaster_ReceivedPurchasedStatus_ViewID = "HWH9BORJ7SGN";
        public const string DefaultDetailMaster_PartialReceivedPurchasedStatus_ViewID = "PARTIALPURCHASE";
        public const string DefaultDetailMaster_RequestedPurchasedStatus_ViewID = "RJJZN29H6D6J";
        public const string DefaultDetailMaster_ApprovedPurchasedStatus_ViewID = "XKAMK4F315ID";
        //Payment Status
        public const string DefaultDetailMaster_PaymentStatusPaid_ViewID = "YO6JJ0W6KRNO";
        public const string DefaultDetailMaster_PaymentStatusDue_ViewID = "EB1X1ENM67ZX";
        public const string DefaultDetailMaster_PaymentStatusPartial_ViewID = "G49V64RQW8UV";
        public const string DefaultDetailMaster_PaymentStatusReturn_ViewID = "X2WA974G8OFZ";
        public const string DefaultDetailMaster_PaymentStatusCancelled_ViewID = "9D4R14HKSLFJ";
        //Payment By
        public const string DefaultDetailMaster_ChequePaymentStatus_ViewID = "WLVNER4EHWTW";
        public const string DefaultDetailMaster_NETSPaymentStatus_ViewID = "K1PZCWZV38QX";
        public const string DefaultDetailMaster_FundTransferPaymentStatus_ViewID = "K6BSWBI7VYJ7";
        public const string DefaultDetailMaster_PaymentModeCash_ViewID = "TTS5QKQB0YTL";
    }

    public static class COLORCODE
    {
        public const string DashboardColorCode_1 = "#FF8C00";
        public const string DashboardColorCode_2 = "#d92550";
        public const string DashboardColorCode_3 = "#3ac47d";
        public const string DashboardColorCode_4 = "#6f42c1";
        public const string DashboardColorCode_5 = "#28a745"; 
    }
        public static class DASHBOARDMENU
    {
        public const string DashboarMenu_HeaderViewID_SystemMaster = "HGFDRTKIU18"; 
        public const string DashboarMenu_HeaderViewID_ProjectMaster = "KWO20XL4P4";
       
        public const string DashboarMenu_DetailViewID_UserMaster = "4B8POTRLCDYT";
        public const string DashboarMenu_DetailViewID_RoleMaster = "5B8POTRLCDYT";       

    }

    public static class ExcelUploadConnection
    {
        public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string ExcelOLEDBConnection = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{0}';Extended Properties=Excel 12.0;";
        public const string UploadFilePath = "~//Uploads//{0}{1}"; 
    }
    public static class ExcelUploadMessages
    {

        public static string ExcelUploadDuplicate
        {
            get { return PROMessage.ExcelUpload_Duplicate; }
        }
        public static string ExcelUploadIsNotDecimal
        {
            get { return PROMessage.ExcelUpload_IsNotDecimal; }
        }
        public static string ExcelUploadIsNotNumber
        {
            get { return PROMessage.ExcelUpload_IsNotNumber; }
        }
        public static string ExcelUploadDateLength
        {
            get { return PROMessage.ExcelUpload_DateLength; }
        }
        public static string ExcelUploadIsRequired
        {
            get { return PROMessage.ExcelUpload_IsRequired; }
        }
        public static string ExcelUploadColumnMissing
        {
            get { return PROMessage.ExcelImport_ColumnMissing; }
        }
        public static string ExcelUploadTemplateIsNotVaid
        {
            get { return PROMessage.ExcelImport_TemplateIsNotVaid; }
        }
        public static string ExcelUploadProductAlreadyExist
        {
            get { return PROMessage.ExcelImport_TemplateIsAlreadyExist; }
        }
        public static string ExcelUploadIsNotDate
        {
            get { return PROMessage.ExcelImport_IsNotDate; }
        }
        public static string ExcelImportTemplateRowEmpty
        {
            get { return PROMessage.ExcelImport_TemplateRowEmpty; }
        }
        
    }

    public static class DropDownListSelectValue
    {
        public static string DropDownListSelect_Status
        {
            get { return PROMessage.DropDownListSelect_Status; }
        } 
        public static string DropDownListSelect_Role
        {
            get { return SYSMessage.DropDownListSelect_Role; }
        }
 
        public static string DropDownListSelect_SalesPaymentStatus
        {
            get { return PROMessage.DropDownListSelect_SalesPaymentStatus; }
        }
        public static string DropDownListSelect_PurchasePaymentStatus
        {
            get { return PROMessage.DropDownListSelect_PurchasePaymentStatus; }
        }
        
    }

    public static class WEBCONSTANTMESSAGE
    {
        //Language
        public const string MultiLanguage_English = "en";
        public const string MultiLanguage_Tamil = "ta";

        //Status
        public static string ACTIVE
        {
            get { return PROPageModel.Common_Status_Active; }
        }
        public static string INACTIVE
        {
            get { return PROPageModel.Common_Status_Inactive; }
        }

        public static string NORECORD
        {
            get { return PROMessage.Common_NoRecord; }
        }
        public static string INSERTFAIL
        {
            get { return PROMessage.Common_InsertFail; }
        }
        public static string UPDATEFAIL
        {
            get { return PROMessage.Common_UpdateFail; }
        }
        public static string DELETEFAIL
        {
            get { return PROMessage.Common_DeleteFail; }
        }

        public static string INSERTSUCCESS
        {
            get { return PROMessage.Common_InsertSuccess; }
        }

        public static string UPDATESUCCESS
        {
            get { return PROMessage.Common_UpdateSuccess; }
        }
        public static string DELETESUCCESS
        {
            get { return PROMessage.Common_DeleteSuccess; }
        } 

        public static string UNAUTHINSERTUPDATE
        {
            get { return PROMessage.Common_UnauthInsertUpdate; }
        }
        public static string ADDSUCCESS
        {
            get { return PROMessage.Common_AddSuccess; }
        }
        public static string DUPLICATEBARCODEFAIL
        {
            get { return PROMessage.Common_DuplicateBarcodeFail; }
        }
        public static string DUPLICATEFAIL
        {
            get { return PROMessage.Common_DuplicateFail; }
        }
        public static string DUPLICATEORDERFAIL
        {
            get { return PROMessage.DuplicateOrderFail; }
        }

        public static string ORDERFAIL
        {
            get { return PROMessage.OrderFail; }
        }

        public static string ALREADYAPPROVED
        {
            get { return PROMessage.Common_AlreadyApproved; }
        }

        public static string FILEALREADYUPLADED
        {
            get { return PROMessage.Attachment_AlreadyUploaded; }
        }

        public static string CANCELSUCCESS
        {
            get { return PROMessage.Common_CancelSuccess; }
        }
        public static string PRODUCTNOTAVAILABLE
        {
            get { return PROMessage.Common_PRODUCTNOTAVAILABLE; }
        }
        public static string NOSTOCKSUCCESS
        {
            get { return "Product might be out of stock in the selected store!"; }
        }

        public const string MenuType_Header = "Header";
        public const string MenuType_Detail = "Detail";

        public static string TRANSFERSUCCESS
        {
            get { return PROMessage.Common_Transferred; }
        }

        public static string APPROVALSUCCESS
        {
            get { return PROMessage.Common_Approved; }
        }

        public static string REJECTSUCCESS
        {
            get { return PROMessage.Common_Rejected; }
        }

        public static string APPROVEREJECTFAIL
        {
            get { return PROMessage.Common_ApproveRejectFail; }
        }

        public static string ALREADYSALESCLOSED
        {
            get { return PROMessage.Common_AlreadyTodaySalesClosed; }
        }
        public static string UPLOADSUCCESS
        {
            get { return PROMessage.Excelmport_SuccessfullyUpload; }
        }
        public static string UPLOADFAIL
        {
            get { return PROMessage.ExcelImport_UploadFailed; }

        }

        public static string Atleast1Active
        {
            get { return PROMessage.Atleast1Active; }
        }

        
        public static string DELIVEREDSUCCESS
        {
            get { return PROMessage.DELIVEREDSUCCESS; }
        }

        public static string MULTIRATES
        {
            get { return PROMessage.PRODUCTMULTIRATES; }
        }


    }
}
