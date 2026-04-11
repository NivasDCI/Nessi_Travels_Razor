using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Distributor.Model;

namespace Distributor.Repository
{
    public interface ITempPurchaseRepository
    {
        #region PurchasesCompany 
        ReturnMessageModel Purchase_CompanyApproval(long CompanyPurchaseID, int IsApprove, int ApproveRejectBy);
        ReturnMessageModel Purchase_CompanyCreatePO(long CompanyPurchaseID, int CreatedBy);
        ReturnMessageModel CompanyPurchases_Delete(long CompanyPurchaseID, int LastModifiedBy);
        ReturnMessageModel CompanyPurchaseDetails_Delete(string CompanyPurchaseDetailsIDs);
        ReturnMessageModel Purchase_ProductDetailInsert(TempCompanyPurchaseDetailModel ObjCompanyPurchase);
        List<TempCompanyPurchaseheaderModel> CompanyPurchaseRequests_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Company, string PurchaseStatus, string sortBy, string direction, out int TotalCount);
        CompanyPurchaseInvoiceHeaderModel CompanyPurchaseInvoiceHeader(string WareHouseID, long CompanyPurchaseID);
        List<CompanyPurchaseInvoiceDetailModel> CompanyPurchaseInvoiceDetails(string WareHouseID, long CompanyPurchaseID);
        List<CompanyMasterModel> PurchaseCompany_Search(string CompanyName, string CompanyCode, string CompanyTypeName, string MobileNo, string GSTNumber);
        List<ProductMasterModel> PurchaseProductCompany_Search(int? page, int? limit, string BarCode, string ProductCode, string PrintName, long? CompanyID, out int TotalCount);

        List<ProductMasterModel> PurchaseProduct_Search(int? page, int? limit, string BarCode, string ProductCode, string PrintName, long? CompanyID, out int TotalCount);
        List<TempCompanyPurchaseheaderModel> CompanyPurchase_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Company, string PurchaseStatus, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel CompanyPurchases_Save(TempCompanyPurchaseheaderModel ObjCompanyPurchases, List<TempCompanyPurchaseDetailModel> ArrCompanyPurchaseDetail);
        TempCompanyPurchaseheaderModel CompanyPurchases_Edit(long CompanyPurchaseID);
        List<TempCompanyPurchaseDetailModel> CompanyPurchaseDetail_FindAll(long CompanyPurchaseID);
        bool CompanyInvoiceDuplicate(string CompanyInvoiceNumber);
        PurchaseRunningNumber CompanyPurchaseHeader_RunningNumber();
        List<TempDistributorPurchaseheaderModel> DistributorPurchaseRequests_FindAll(int? page, int? limit, string PurchaseDate, string PurchaseRefNo, string Distributor, string PurchaseStatus, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel DistributionPurchases_Save(TempDistributorPurchaseheaderModel objPurchases, List<TempDistributorPurchaseDetailModel> ObjPurchaseDetail);
        PurchaseRunningNumber PurchaseHeader_RunningNumber();
        ReturnMessageModel Purchase_Approval(long DistributorPurchaseID, int IsApprove, int ApproveRejectBy);
        ReturnMessageModel Purchase_CreatePO(long DistributorPurchaseID, int CreatedBy);

        bool DistributorInvoiceDuplicate(string DistributorInvoiceNumber);
        TempDistributorPurchaseheaderModel DistributorPurchases_Edit(long DistPurchaseID);
        List<TempDistributorPurchaseDetailModel> DistributorPurchaseDetail_FindAll(long DistPurchaseID);
        ReturnMessageModel Purchases_Delete(long CompanyPurchaseID, int LastModifiedBy);

        PurchaseInvoiceHeaderModel PurchaseInvoiceHeader(string WarehouseID, long DistributorPurchaseID);
        PurchaseReturnInvoiceHeaderModel PurchaseReturnInvoiceHeader(string WarehouseID, long PurchaseReturnID);
        List<PurchaseInvoiceDetailModel> PurchaseInvoiceDetails(string WarehouseID, long DistributorPurchaseID);
        List<PurchaseReturnInvoiceDetailModel> PurchaseReturnInvoiceDetails(string WarehouseID, long PurchaseReturnID);

        #endregion
    }
}
