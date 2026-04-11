using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MiniExcelLibs.Attributes;

namespace Transport.Model
{    
    public class ExpenseModel
    {
        public long? ExpenseCode { get; set; }
        public long? VehicleCode { get; set; }
        public long? ServiceTypeCode { get; set; }
        public long? SupplierTypeCode { get; set; }       
        public int? Qty { get; set; }
        public decimal? Charge { get; set; }        
        public string VehicleName { get; set; }
        public string ServiceTypeName { get; set; }
        public string SupplierTypeName { get; set; }
        public string Remarks { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public int? PaymentBy { get; set; }
        public string PaymentByName { get; set; }
        public int? PaidTo { get; set; }
        public string PaidToName { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }        
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<DateTime> UpdatedDate { get; set; }
        public string DisplayLastModifiedBy { get; set; }
        public string DisplayCreatedBy { get; set; }
        public string DisplayCreatedDate { get; set; }
        public string DisplayLastModifiedDate { get; set; }        

    }
    


}
