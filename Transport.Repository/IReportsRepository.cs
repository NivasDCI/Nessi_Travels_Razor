using Transport.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transport.Repository
{
    public interface IReportsRepository
    {
        DashboardModel Dashboard_FindAll();
        //DashboardModel DashboardReport_FindAll(DateTime? StartDate, DateTime? EndDate);
        List<JobModel> Job_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand, int? limit, string sortBy, string direction, out int TotalCount);
        List<JobModel> JobsReport(DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand);
        List<TotalReportModel> TotalReport(DateTime? StartDate, DateTime? EndDate);
        List<VehicleReportModel> VehicleReport(DateTime? StartDate, DateTime? EndDate);
        List<CashInHandReportModel> CashInHandReport(DateTime? StartDate, DateTime? EndDate);
        List<CreditReportModel> CreditReport(DateTime? StartDate, DateTime? EndDate);
        List<TotalReportModel> VehicleTotalReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate);
        List<ExpenseModel> VehicleExpenseReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate);
        List<CreditCustomersReportModel> CreditCustomersReport(DateTime? StartDate, DateTime? EndDate);
        List<JobModel> OutSourceJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand, int? limit, string sortBy, string direction, out int TotalCount);
        List<JobModel> OutSourceJobsReport(DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand);

    }
}
