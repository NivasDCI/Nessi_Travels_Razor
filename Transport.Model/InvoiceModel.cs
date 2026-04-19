using System;
using System.Collections.Generic;

namespace Transport.Model
{
    public class InvoiceHeaderModel
    {
        public long InvoiceID { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public int? JobVendorCode { get; set; }
        public string JobVendorName { get; set; }
        public int? DrivingBy { get; set; }
        public string DrivingByName { get; set; }
        public int? VehicleCode { get; set; }
        public string VehicleName { get; set; }
        public int? CashInHand { get; set; }
        public string CashInHandName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CreditCash { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCredit { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalJobs { get; set; }
        public List<InvoiceDetailModel> Details { get; set; }

        public string DisplayInvoiceDate
        {
            get { return InvoiceDate.ToString("dd-MMM-yyyy"); }
        }
        public string DisplayStartDate
        {
            get { return StartDate.HasValue ? StartDate.Value.ToString("dd-MMM-yyyy") : ""; }
        }
        public string DisplayEndDate
        {
            get { return EndDate.HasValue ? EndDate.Value.ToString("dd-MMM-yyyy") : ""; }
        }
    }

    public class InvoiceDetailModel
    {
        public long InvoiceDetailID { get; set; }
        public long InvoiceID { get; set; }
        public long JobCode { get; set; }
        public DateTime? JobDate { get; set; }
        public string JobTime { get; set; }
        public string JobFrom { get; set; }
        public string JobTo { get; set; }
        public string CustomerName { get; set; }
        public string VehicleName { get; set; }
        public string DrivingByName { get; set; }
        public string JobVendorName { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Amount { get; set; }

        public string DisplayJobDate
        {
            get { return JobDate.HasValue ? JobDate.Value.ToString("dd-MMM-yyyy") : ""; }
        }
    }
}