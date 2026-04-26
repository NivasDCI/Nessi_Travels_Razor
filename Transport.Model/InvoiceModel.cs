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
        public string BillToName { get; set; }
        public string BillToAddress { get; set; }
        public bool IsManual { get; set; }
        public string PaymentRemarks { get; set; }

        // Payment tracking (computed from InvoicePayments table)
        public decimal TotalPaid { get; set; }
        public decimal BalanceAmount { get { return TotalAmount - TotalPaid; } }
        public string PaymentStatus
        {
            get
            {
                if (TotalPaid <= 0) return "Unpaid";
                if (TotalPaid >= TotalAmount) return "Paid";
                return "Partial";
            }
        }

        public List<InvoiceDetailModel> Details { get; set; }

        public string DisplayInvoiceDate { get { return InvoiceDate.ToString("dd-MMM-yyyy"); } }
        public string DisplayStartDate { get { return StartDate.HasValue ? StartDate.Value.ToString("dd-MMM-yyyy") : ""; } }
        public string DisplayEndDate { get { return EndDate.HasValue ? EndDate.Value.ToString("dd-MMM-yyyy") : ""; } }
        public string DisplayShortStartDate { get { return StartDate.HasValue ? StartDate.Value.ToString("d/M/yy") : ""; } }
        public string DisplayShortEndDate { get { return EndDate.HasValue ? EndDate.Value.ToString("d/M/yy") : ""; } }
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

        public string DisplayJobDate { get { return JobDate.HasValue ? JobDate.Value.ToString("d/M/yyyy") : ""; } }
    }
}

// ════════════════════════════════════════════════════════════════════════════
// ADD these classes at the bottom of InvoiceModel.cs  (inside Transport.Model)
// ════════════════════════════════════════════════════════════════════════════

namespace Transport.Model
{
    // ── One payment row against an invoice (maps to InvoicePayments table) ───
    public class InvoicePaymentModel
    {
        public long PaymentID { get; set; }
        public long InvoiceID { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaidAmount { get; set; }
        public string Remarks { get; set; }

        public string DisplayPaymentDate => PaymentDate.ToString("dd/MM/yyyy");
    }

    // ── One invoice + all its payment rows (one group in the summary table) ──
    public class InvoiceWithPayments
    {
        public int SNo { get; set; }
        public InvoiceHeaderModel Invoice { get; set; }   // reuse existing model (has TotalPaid, BalanceAmount, PaymentStatus)
        public List<InvoicePaymentModel> Payments { get; set; } = new List<InvoicePaymentModel>();
        public decimal BalanceAfterInvoice { get; set; }   // running balance right after invoice row
    }

    // ── Full view-model for VendorAccountSummary page ────────────────────────
    public class VendorAccountSummaryViewModel
    {
        // ── Filter / header info ─────────────────────────────────────────────
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime GeneratedOn { get; set; }

        // ── Opening balance (unpaid amount carried over before StartDate) ────
        public decimal OpeningBalance { get; set; }

        // ── Grouped rows: each item = one invoice + N receipt sub-rows ───────
        public List<InvoiceWithPayments> InvoiceGroups { get; set; } = new List<InvoiceWithPayments>();

        // ── Footer totals ────────────────────────────────────────────────────
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalAmountCollected { get; set; }
        public decimal CurrentPeriodBalance { get; set; }   // TotalInvoiceAmount - TotalAmountCollected
        public decimal ClosingBalance { get; set; }   // OpeningBalance + CurrentPeriodBalance

        // ── Display helpers ──────────────────────────────────────────────────
        public string DisplayStartDate => StartDate.HasValue ? StartDate.Value.ToString("dd/MM/yyyy") : "-";
        public string DisplayEndDate => EndDate.HasValue ? EndDate.Value.ToString("dd/MM/yyyy") : "-";
        public bool HasData => InvoiceGroups != null && InvoiceGroups.Count > 0;
    }
}