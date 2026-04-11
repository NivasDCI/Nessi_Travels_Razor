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
    public class JobModel
    {
        public long JobCode { get; set; }
        public DateTime? JobDate { get; set; }
        public string JobTime { get; set; }
        public string JobFrom { get; set; }
        public string JobTo { get; set; }
        public long? JobVendorCode { get; set; }
        public string JobVendorName { get; set; }
        public long? VehicleCode { get; set; }
        public string VehicleName { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> DrivingBy { get; set; }
        public string DrivingByName { get; set; }
        public string OutSource { get; set; }
        public string CustomerName { get; set; }
        public string ContactNo { get; set; }
        public decimal? Cost { get; set; }
        public int? NoOfPaxs { get; set; }
        public int? NoOfBags { get; set; }
        public decimal? Credit { get; set; }
        public decimal? OutSourceAmount { get; set; }
        public decimal? OutSourceAmountGiven { get; set; }
        public decimal? Cash { get; set; }
        public Nullable<int> CashInHand { get; set; }
        public string CashInHandName { get; set; }
        public string JobStatus { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }        
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<DateTime> UpdatedDate { get; set; }
        public string DisplayLastModifiedBy { get; set; }
        public string DisplayCreatedBy { get; set; }
        public string DisplayCreatedDate { get; set; }
        public string DisplayLastModifiedDate { get; set; }
        public string DisplaySubmissionDate { get; set; }
        public List<JobStatusModel> JobStatusModel { get; set; }
        public long JobRequestCode { get; set; }

        //public string PaymentStatus { get; set; }
        //public int? PaymentTypeCode { get; set; }
        //public string PaymentTypeName { get; set; }
        //public double? PaymentAmount { get; set; }

    }    

    public class JobModelFiltered
    {
        [ExcelColumn(Name = "CODE")]
        public long? JobCode { get; set; }

        [ExcelColumn(Name = "DATE")]
        public DateTime? JobDate { get; set; }

        [ExcelColumn(Name = "TIME")]
        public string JobTime { get; set; }

        [ExcelColumn(Name = "VEHICLE NO")]
        public string VehicleName { get; set; }

        [ExcelColumn(Name = "FROM")]
        public string JobFrom { get; set; }

        [ExcelColumn(Name = "TO")]
        public string JobTo { get; set; }

        [ExcelColumn(Name = "JOB GIVEN BY")]
        public string JobVendorName { get; set; } 
        
        [ExcelColumn(Name = "DRIVING BY")]
        public string DrivingByName { get; set; }

        [ExcelColumn(Name = "CUSTOMER NAME")]
        public string CustomerName { get; set; }

        [ExcelColumn(Name = "CONTACT NO")]
        public string ContactNo { get; set; }

        [ExcelColumn(Name = "COST")]
        public decimal? Cost { get; set; }
        //public string Cost { get; set; }

        [ExcelColumn(Name = "NO OF PAXS")]
        public decimal? NoOfPaxs { get; set; }

        [ExcelColumn(Name = "NO OF BAGS")]
        public decimal? NoOfBags { get; set; }

        [ExcelColumn(Name = "CREDIT")]
        public decimal? Credit { get; set; }
        //public string Credit { get; set; }

        [ExcelColumn(Name = "CASH")]
        public decimal? Cash { get; set; }       
        //public string Cash { get; set; }

        [ExcelColumn(Name = "CASH IN HAND")]
        public string CashInHandName { get; set; }

        [ExcelColumn(Name = "OUTSOURCE")]
        public string OutSource { get; set; }

        [ExcelColumn(Name = "OUTSOURCE AMOUNT")]
        //public decimal? OutSourceAmount { get; set; }    
        public string OutSourceAmount { get; set; }
        [ExcelColumn(Name = "OUTSOURCE AMOUNT GIVEN")]
        //public decimal? OutSourceAmount { get; set; }    
        public string OutSourceAmountGiven { get; set; }

        [ExcelColumn(Name = "JOB STATUS")]
        public string JobStatus { get; set; }

        [ExcelColumn(Name = "REMARKS")]
        public string Remarks { get; set; }

        [ExcelColumn(Name = "CREATED BY")]
        public string DisplayCreatedBy { get; set; }

        [ExcelColumn(Name = "CREATED DATE")]
        public string DisplayCreatedDate { get; set; }             

        [ExcelColumn(Name = "UPDATED BY")]
        public string DisplayLastModifiedBy { get; set; }

        [ExcelColumn(Name = "UPDATED DATE")]
        public string DisplayLastModifiedDate { get; set; }


    }

    public class JobStatusModel
    {
        public long? JobStatusCode { get; set; }
        public long? JobCode { get; set; }
        public string JobStatus { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<DateTime> UpdatedDate { get; set; }
        public string DisplayLastModifiedBy { get; set; }
        public string DisplayLastModifiedDate { get; set; }
    }

    public class MyJobModel
    {
        public long JobCode { get; set; }
        public DateTime? JobDate { get; set; }
        public string JobTime { get; set; }
        public string JobFrom { get; set; }
        public string JobTo { get; set; }
        public string VehicleName { get; set; }
        public Nullable<int> DrivingBy { get; set; }
        public string DrivingByName { get; set; }
        public string CustomerName { get; set; }
        public string ContactNo { get; set; }
        public decimal? Cash { get; set; }
        public int? NoOfPaxs { get; set; }
        public int? NoOfBags { get; set; }
        public decimal? Credit { get; set; }
        public string JobStatus { get; set; }
        public Nullable<DateTime> UpdatedDate { get; set; }
        public Nullable<int> CashInHand { get; set; }

    }

    public class JobRequestModel
    {
        public long JobRequestCode { get; set; }
        public DateTime? JobRequestDate { get; set; }
        public string JobRequestTime { get; set; }
        public string JobRequestFrom { get; set; }
        public string JobRequestTo { get; set; }
        public string CustomerName { get; set; }
        public string ContactNo { get; set; }
        public int? NoOfPaxs { get; set; }
        public int? NoOfBags { get; set; }
        public string JobRequestStatus { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public string DisplayCreatedDate { get; set; }
        public decimal? Cost { get; set; }

    }

}
