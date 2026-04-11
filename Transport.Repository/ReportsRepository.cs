using Transport.Repository.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Transport.Model;
using Transport.Entity;
using System.Security.Cryptography;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading;

namespace Transport.Repository
{
    public class ReportsRepository : IReportsRepository
    {
        TransportEntities db = new TransportEntities();
        CommonRepository ObjCom = new CommonRepository();

        #region "Dashboard"
        public DashboardModel Dashboard_FindAll()
        {           
            DashboardModel ObjExpenses = (from PU in db.sp_frm_get_Dashboard_Daily(null)

                                         select new DashboardModel
                                         {
                                              Requests = PU.Requests,
                                              PendingJobs = PU.PendingJobs,
                                              OnGoingJobs = PU.OnGoingJobs,
                                              CompletedJobs = PU.CompletedJobs,
                                              TotalCredit = PU.TotalCredit,
                                              TotalCash = PU.TotalCash
                                          }).FirstOrDefault();

            return ObjExpenses;
        }        
        #endregion

        #region "Jobs"
        public List<JobModel> Job_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand, int? limit, string sortBy, string direction, out int TotalCount)
        {
            
            List<JobModel> ObjJobs = (from PU in db.sp_frm_get_Jobs_Report(VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand, StartDate, EndDate)

                                      select new JobModel
                                      {
                                          JobCode = PU.JobCode,
                                          JobDate = PU.JobDate,
                                          JobTime = PU.JobTime,
                                          VehicleName = PU.VehicleName,
                                          JobFrom = PU.JobFrom,
                                          JobTo = PU.JobTo,
                                          JobVendorName = PU.JobGivenBy,
                                          DrivingByName = PU.DrivingBy,
                                          CustomerName = PU.CustomerName,
                                          ContactNo = PU.ContactNo,                                         
                                          Credit = PU.Credit,
                                          Cash = PU.Cash,
                                          OutSource = PU.OutSource,
                                          OutSourceAmount = PU.OutSourceAmount,
                                          JobStatus = PU.JobStatus,
                                          CashInHandName = PU.CashInHandName,
                                          DisplayCreatedBy = PU.CreatedBy,
                                          DisplayLastModifiedBy = PU.UpdatedBy,
                                          UpdatedDate = PU.UpdatedDate,
                                          DisplayCreatedDate = PU.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplayLastModifiedDate = PU.UpdatedDate.Value.ToString("MMM dd yyyy hh:mm tt")
                                      }).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjJobs = ObjJobs.OrderBy(q => q.VehicleName).ToList();
                            break;

                        case "JobVendorName":
                            ObjJobs = ObjJobs.OrderBy(q => q.JobVendorName).ToList();
                            break;

                    }
                }
                else
                {
                    // step 7 applying sorting desc

                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjJobs = ObjJobs.OrderByDescending(q => q.VehicleName).ToList();
                            break;

                        case "JobVendorName":
                            ObjJobs = ObjJobs.OrderByDescending(q => q.JobVendorName).ToList();
                            break;
                    }
                }
            }
            else
            {
                ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ToList();
            }

            TotalCount = ObjJobs.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobs = ObjJobs.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobs;
        }
        public List<JobModel> JobsReport(DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand)
        {
            List<JobModel> ObjJobs = (from PU in db.sp_frm_get_Jobs_Report(VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand, StartDate, EndDate)

                                      select new JobModel
                                      {
                                          JobCode = PU.JobCode,
                                          JobDate = PU.JobDate,
                                          JobTime = PU.JobTime,
                                          VehicleName = PU.VehicleName,
                                          JobFrom = PU.JobFrom,
                                          JobTo = PU.JobTo,
                                          JobVendorName = PU.JobGivenBy,
                                          DrivingByName = PU.DrivingBy,
                                          CustomerName = PU.CustomerName,
                                          ContactNo = PU.ContactNo,
                                          Cost = PU.Cost,   
                                          NoOfPaxs = PU.NoOfPaxs,
                                          NoOfBags = PU.NoOfBags,
                                          Credit = PU.Credit,
                                          Cash = PU.Cash,
                                          Remarks = PU.Remarks, 
                                          OutSource = PU.OutSource,
                                          OutSourceAmount = PU.OutSourceAmount,
                                          JobStatus = PU.JobStatus,
                                          CashInHandName = PU.CashInHandName,
                                          DisplayCreatedBy = PU.CreatedBy,
                                          DisplayLastModifiedBy = PU.UpdatedBy,
                                          UpdatedDate = PU.UpdatedDate,
                                          DisplayCreatedDate = PU.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplayLastModifiedDate = PU.UpdatedDate.Value.ToString("MMM dd yyyy hh:mm tt")
                                      }).ToList();

            return ObjJobs;
        }

        public List<TotalReportModel> TotalReport(DateTime? StartDate, DateTime? EndDate)
        {
            List<TotalReportModel> ObjJobs = (from PU in db.sp_frm_get_Total_Report(StartDate, EndDate)

                                      select new TotalReportModel
                                      {
                                          CashOrder = PU.CashOrder,
                                          CreditJob = PU.CreditJob,
                                          TotalOrder = PU.TotalOrder
                                      }).ToList();

            return ObjJobs;
        }
        public List<VehicleReportModel> VehicleReport(DateTime? StartDate, DateTime? EndDate)
        {
            List<VehicleReportModel> ObjJobs = (from PU in db.sp_frm_get_Vehicle_Report(StartDate, EndDate)

                                              select new VehicleReportModel
                                              {
                                                  CashOrder = PU.CashOrder,
                                                  CreditJob = PU.CreditJob,
                                                  TotalOrder = PU.TotalOrder,
                                                  VehicleName = PU.VehicleName
                                              }).ToList();

            return ObjJobs;
        }
        public List<CashInHandReportModel> CashInHandReport(DateTime? StartDate, DateTime? EndDate)
        {
            List<CashInHandReportModel> ObjJobs = (from PU in db.sp_frm_get_CashInHand_Report(StartDate, EndDate)

                                              select new CashInHandReportModel
                                              {
                                                  CashOrder = PU.CashOrder,
                                                  CreditJob = PU.CreditJob,
                                                  TotalOrder = PU.TotalOrder,
                                                  CashInHand = PU.CashInHand
                                              }).ToList();

            return ObjJobs;
        }
        public List<CreditReportModel> CreditReport(DateTime? StartDate, DateTime? EndDate)
        {
            List<CreditReportModel> ObjJobs = (from PU in db.sp_frm_get_Credit_Report(StartDate, EndDate)

                                              select new CreditReportModel
                                              {
                                                  //CashOrder = PU.CashOrder,
                                                  CreditJob = PU.CreditJob,
                                                  //TotalOrder = PU.TotalOrder
                                                  JobVendorName = PU.JobVendorName
                                              }).ToList();

            return ObjJobs;
        }
        public List<TotalReportModel> VehicleTotalReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate)
        {
            List<TotalReportModel> ObjJobs = (from PU in db.sp_frm_get_Vehicle_Total_Report(StartDate, EndDate, VehicleCode)

                                              select new TotalReportModel
                                              {
                                                  CashOrder = PU.CashOrder,
                                                  CreditJob = PU.CreditJob,
                                                  TotalOrder = PU.TotalOrder,
                                                  VehicleName = PU.VehicleName
                                              }).ToList();

            return ObjJobs;
        }
        public List<ExpenseModel> VehicleExpenseReport(int? VehicleCode, DateTime? StartDate, DateTime? EndDate)
        {
            List<ExpenseModel> ObjJobs = (from PU in db.sp_frm_get_Vehicle_Expense_Report(StartDate, EndDate, VehicleCode)

                                              select new ExpenseModel
                                              {
                                                  VehicleName = PU.VehicleName,
                                                  ServiceTypeName = PU.ServiceTypeName,
                                                  Charge = PU.Charge
                                              }).ToList();

            return ObjJobs;
        }
        public List<CreditCustomersReportModel> CreditCustomersReport(DateTime? StartDate, DateTime? EndDate)
        {
            List<CreditCustomersReportModel> ObjJobs = (from PU in db.sp_frm_get_CreditCustomers_Report(StartDate, EndDate)

                                               select new CreditCustomersReportModel
                                               {
                                                   //CashOrder = PU.CashOrder,
                                                   CreditJob = PU.CreditJob,
                                                   //TotalOrder = PU.TotalOrder
                                                   CustomerName = PU.Customerame
                                               }).ToList();

            return ObjJobs;
        }

        public List<JobModel> OutSourceJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand, int? limit, string sortBy, string direction, out int TotalCount)
        {

            List<JobModel> ObjJobs = (from PU in db.sp_frm_get_OutSourceJobs_Report(VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand, StartDate, EndDate)

                                      select new JobModel
                                      {
                                          JobCode = PU.JobCode,
                                          JobDate = PU.JobDate,
                                          JobTime = PU.JobTime,
                                          VehicleName = PU.VehicleName,
                                          JobFrom = PU.JobFrom,
                                          JobTo = PU.JobTo,
                                          JobVendorName = PU.JobGivenBy,
                                          DrivingByName = PU.DrivingBy,
                                          CustomerName = PU.CustomerName,
                                          ContactNo = PU.ContactNo,
                                          Credit = PU.Credit,
                                          Cash = PU.Cash,
                                          OutSource = PU.OutSource,
                                          OutSourceAmount = PU.OutSourceAmount,
                                          OutSourceAmountGiven = PU.OutSourceAmountGiven,
                                          JobStatus = PU.JobStatus,
                                          CashInHandName = PU.CashInHandName,
                                          DisplayCreatedBy = PU.CreatedBy,
                                          DisplayLastModifiedBy = PU.UpdatedBy,
                                          UpdatedDate = PU.UpdatedDate,
                                          DisplayCreatedDate = PU.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplayLastModifiedDate = PU.UpdatedDate.Value.ToString("MMM dd yyyy hh:mm tt")
                                      }).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjJobs = ObjJobs.OrderBy(q => q.VehicleName).ToList();
                            break;

                        case "JobVendorName":
                            ObjJobs = ObjJobs.OrderBy(q => q.JobVendorName).ToList();
                            break;

                    }
                }
                else
                {
                    // step 7 applying sorting desc

                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjJobs = ObjJobs.OrderByDescending(q => q.VehicleName).ToList();
                            break;

                        case "JobVendorName":
                            ObjJobs = ObjJobs.OrderByDescending(q => q.JobVendorName).ToList();
                            break;
                    }
                }
            }
            else
            {
                ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ToList();
            }

            TotalCount = ObjJobs.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobs = ObjJobs.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobs;
        }
        public List<JobModel> OutSourceJobsReport(DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? CashInHand)
        {
            List<JobModel> ObjJobs = (from PU in db.sp_frm_get_OutSourceJobs_Report(VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand, StartDate, EndDate)

                                      select new JobModel
                                      {
                                          JobCode = PU.JobCode,
                                          JobDate = PU.JobDate,
                                          JobTime = PU.JobTime,
                                          VehicleName = PU.VehicleName,
                                          JobFrom = PU.JobFrom,
                                          JobTo = PU.JobTo,
                                          JobVendorName = PU.JobGivenBy,
                                          DrivingByName = PU.DrivingBy,
                                          CustomerName = PU.CustomerName,
                                          ContactNo = PU.ContactNo,
                                          Cost = PU.Cost,
                                          NoOfPaxs = PU.NoOfPaxs,
                                          NoOfBags = PU.NoOfBags,
                                          Credit = PU.Credit,
                                          Cash = PU.Cash,
                                          Remarks = PU.Remarks,
                                          OutSource = PU.OutSource,
                                          OutSourceAmount = PU.OutSourceAmount,
                                          OutSourceAmountGiven = PU.OutSourceAmountGiven,
                                          JobStatus = PU.JobStatus,
                                          CashInHandName = PU.CashInHandName,
                                          DisplayCreatedBy = PU.CreatedBy,
                                          DisplayLastModifiedBy = PU.UpdatedBy,
                                          UpdatedDate = PU.UpdatedDate,
                                          DisplayCreatedDate = PU.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplayLastModifiedDate = PU.UpdatedDate.Value.ToString("MMM dd yyyy hh:mm tt")
                                      }).ToList();

            return ObjJobs;
        }
        #endregion

    }
}

