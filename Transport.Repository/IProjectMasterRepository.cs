using Transport.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Transport.Repository
{
    public interface IProjectMasterRepository
    {

        #region "Default Detail Master"
        List<DefaultDetailMasterModel> DefaultMasterDetail_FindAll(string DefaultHeaderID, bool? Status, int? page, int? limit, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel DefaultMasterDetail_Save(DefaultDetailMasterModel ObjDefaultModel);
        ReturnMessageModel DefaultMasterDetail_Delete(string DefaultDetailID, int deletedBy);
        int DefaultMaxOrder(string DefaultHeaderID);

        #endregion 

        #region VehicleMaster        
        List<VehicleMasterModel> VehicleMaster_FindAll(int? page, int? limit, string VehicleName, bool? Status, string sortBy, string direction, out Int32 TotalCount);
        ReturnMessageModel VehicleMaster_Save(VehicleMasterModel objVehicleMaster);
        VehicleMasterModel VehicleMaster_Edit(int VehicleCode);
        #endregion

        #region JobVendors
        List<JobVendorMasterModel> JobVendorsMaster_FindAll(int? page, int? limit, string JobVendorName, bool? Status, string sortBy, string direction, out Int32 TotalCount);
        JobVendorMasterModel JobVendorsMaster_Edit(long JobVendorCode);
        ReturnMessageModel JobVendorsMaster_Save(JobVendorMasterModel objJobVendorsMaster);
        ReturnMessageModel JobVendorsMaster_Insert(JobVendorMasterModel objJobVendorsMaster);
        ReturnMessageModel JobVendorsMaster_Update(JobVendorMasterModel objJobVendorsMaster);
        #endregion


    }
}
