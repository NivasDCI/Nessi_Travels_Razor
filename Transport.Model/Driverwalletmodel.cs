using System;
using System.Collections.Generic;

namespace Transport.Model
{
    // ── Daily wallet row per driver (one row = one calendar day) ──────────────
    public class DriverWalletDayModel
    {
        public long WalletID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime WalletDate { get; set; }
        public string DisplayWalletDate => WalletDate.ToString("dd-MMM-yyyy");
        public int TotalRides { get; set; }
        public decimal TotalEarned { get; set; }       // sum of Cash from Jobs completed that day
        public decimal TotalHandedOver { get; set; }   // cash handed to admin/office
        public decimal TotalExpenses { get; set; }     // driver expenses that day
        public decimal ClosingBalance { get; set; }    // earned - handedOver - expenses  (single day)
        public decimal RunningBalance { get; set; }    // cumulative across all days (calculated in repo)
        public string DisplayCreatedDate { get; set; }
    }

    // ── Individual job detail for a wallet day ────────────────────────────────
    public class DriverWalletJobDetailModel
    {
        public long JobCode { get; set; }
        public DateTime? JobDate { get; set; }
        public string DisplayJobDate => JobDate.HasValue ? JobDate.Value.ToString("dd-MMM-yyyy") : "";
        public string JobTime { get; set; }
        public string JobFrom { get; set; }
        public string JobTo { get; set; }
        public string CustomerName { get; set; }
        public string VehicleName { get; set; }
        public string JobVendorName { get; set; }
        public string JobStatus { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Credit { get; set; }
        public decimal Earned => Cash ?? 0;            // only cash is tracked in wallet
    }

    // ── Cash handover record (driver → admin) ─────────────────────────────────
    public class DriverHandoverModel
    {
        public long HandoverID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime HandoverDate { get; set; }
        public string DisplayHandoverDate => HandoverDate.ToString("dd-MMM-yyyy");
        public decimal Amount { get; set; }
        public int? HandedToUserID { get; set; }
        public string HandedToName { get; set; }
        public string Remarks { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayCreatedDate => CreatedDate.ToString("dd-MMM-yyyy hh:mm tt");
    }

    // ── Driver expense record (fuel, toll, parking, etc.) ────────────────────
    public class DriverExpenseWalletModel
    {
        public long DriverExpenseID { get; set; }
        public int DriverUserID { get; set; }
        public string DriverName { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string DisplayExpenseDate => ExpenseDate.ToString("dd-MMM-yyyy");
        public string Category { get; set; }           // Fuel | Toll | Parking | Other
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public long? JobCode { get; set; }             // optional: linked job
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayCreatedDate => CreatedDate.ToString("dd-MMM-yyyy hh:mm tt");
    }

    // ── Summary totals for the balance cards at the top ──────────────────────
    public class WalletBalanceSummaryModel
    {
        public decimal TotalEarned { get; set; }
        public decimal TotalHandedOver { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal RunningBalance { get; set; }    // TotalEarned - TotalHandedOver - TotalExpenses
    }
}