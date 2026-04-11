using MiniExcelLibs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Transport.Model
{   
    public class DashboardModel
	{            
        public Nullable<int> Requests { get; set; }
        public Nullable<int> PendingJobs { get; set; }
        public Nullable<int> OnGoingJobs { get; set; }
        public Nullable<int> CompletedJobs { get; set; }
        public Nullable<double> TotalCredit { get; set; }
        public Nullable<double> TotalCash { get; set; }

    }

    public class TotalReportModel
    {
        public Nullable<decimal> CashOrder { get; set; }
        public Nullable<decimal> CreditJob { get; set; }
        public Nullable<decimal> TotalOrder { get; set; }
        public string VehicleName { get; set; }

    }
    public class VehicleReportModel
    {
        public Nullable<decimal> CashOrder { get; set; }
        public Nullable<decimal> CreditJob { get; set; }
        public Nullable<decimal> TotalOrder { get; set; }
        public string VehicleName { get; set; }

    }
    public class CashInHandReportModel
    {
        public Nullable<decimal> CashOrder { get; set; }
        public Nullable<decimal> CreditJob { get; set; }
        public Nullable<decimal> TotalOrder { get; set; }
        public string CashInHand { get; set; }

    }
    public class CreditReportModel
    {
        public Nullable<decimal> CashOrder { get; set; }
        public Nullable<decimal> CreditJob { get; set; }
        public Nullable<decimal> TotalOrder { get; set; }
        public string JobVendorName { get; set; }

    }
    public class CreditCustomersReportModel
    {
        public Nullable<decimal> CashOrder { get; set; }
        public Nullable<decimal> CreditJob { get; set; }
        public Nullable<decimal> TotalOrder { get; set; }
        public string CustomerName { get; set; }

    }

    public class TrxnsReportModel
    {
        public int TrxnID { get; set; }
        public DateTime TrxnDate { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public string ServiceName { get; set; }

    }
}
