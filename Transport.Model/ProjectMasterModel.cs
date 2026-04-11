using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Transport.Model
{
    public class RunningNumberModel
    {
        public int RunningNumber { get; set; }
        public int ReturnRunningNumber { get; set; }
    }

    public class DefaultImageModel
    {
        public string HeaderViewID { get; set; }
        public string DetailViewID { get; set; }
        public string DefaultHeader { get; set; }
        public string Title { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }

    } 

    #region Project Master
    public class VehicleMasterModel
    {
        public long VehicleCode { get; set; }
        public string VehicleName { get; set; }        
        public bool? Status { get; set; }       

    }
    public class JobVendorMasterModel
    {
        public long JobVendorCode { get; set; }
        public string JobVendorName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string ContactNo { get; set; }
        public bool? Status { get; set; }

    }

    #endregion

}
