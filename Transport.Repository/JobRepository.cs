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
using System.Globalization;
using System.Net.Mail;
using System.ComponentModel;
using System.Threading;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace Transport.Repository
{
    public class JobRepository : IJobRepository
    {
        TransportEntities db = new TransportEntities();
        CommonRepository ObjCom = new CommonRepository();

        #region "Job"
        public List<JobModel> Job_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, int? CashInHand, string CustomerName, string ContactNo, int? DrivingBy, int? limit, string sortBy, string direction, out int TotalCount)
        {
            List<JobModel> ObjJobs = (from PU in db.sp_frm_get_Jobs(VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, CashInHand, StartDate, EndDate)

                                      select new JobModel
                                      {
                                          JobCode = PU.JobCode,
                                          JobDate = PU.JobDate,
                                          //JobTime = timeConversion(PU.JobTime),
                                          JobTime = PU.JobTime,
                                          VehicleName = PU.VehicleName,
                                          JobFrom = PU.JobFrom,
                                          JobTo = PU.JobTo,
                                          JobVendorName = PU.JobGivenBy,
                                          DrivingByName = PU.DrivingBy,
                                          OutSource = PU.OutSource,
                                          CustomerName = PU.CustomerName,
                                          ContactNo = PU.ContactNo,
                                          //Cost = PU.Cost,
                                          Credit = PU.Credit,
                                          Cash = PU.Cash,
                                          OutSourceAmount = PU.OutsourceAmount,
                                          JobStatus = PU.JobStatus,
                                          Remarks = PU.Remarks,
                                          CashInHandName = PU.CashInHandName,
                                          DisplayCreatedBy = PU.CreatedBy,
                                          DisplayLastModifiedBy = PU.UpdatedBy,
                                          UpdatedDate = PU.UpdatedDate,
                                          DisplayCreatedDate = PU.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplayLastModifiedDate = PU.UpdatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplaySubmissionDate = PU.SubmissionDate,
                                          NoOfBags = PU.NoOfBags,
                                          NoOfPaxs = PU.NoOfPaxs

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
                //ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => timeConversion(q.JobTime)).ToList();
                ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => q.JobTime).ToList();
            }

            TotalCount = ObjJobs.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobs = ObjJobs.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobs;
        }

        public List<JobModel> CreditJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? limit, string sortBy, string direction, out int TotalCount)
        {
            List<JobModel> ObjJobs = (from PU in db.sp_frm_get_Jobs(VehicleCode, JobVendorCode, CustomerName, ContactNo, DrivingBy, null, StartDate, EndDate)
                                      where PU.Credit.HasValue
                                      select new JobModel
                                      {
                                          JobCode = PU.JobCode,
                                          JobDate = PU.JobDate,
                                          //JobTime = timeConversion(PU.JobTime),
                                          JobTime = PU.JobTime,
                                          VehicleName = PU.VehicleName,
                                          JobFrom = PU.JobFrom,
                                          JobTo = PU.JobTo,
                                          JobVendorName = PU.JobGivenBy,
                                          DrivingByName = PU.DrivingBy,
                                          OutSource = PU.OutSource,
                                          CustomerName = PU.CustomerName,
                                          ContactNo = PU.ContactNo,
                                          //Cost = PU.Cost,
                                          Credit = PU.Credit,
                                          Cash = PU.Cash,
                                          OutSourceAmount = PU.OutsourceAmount,
                                          JobStatus = PU.JobStatus,
                                          Remarks = PU.Remarks,
                                          CashInHandName = PU.CashInHandName,
                                          DisplayCreatedBy = PU.CreatedBy,
                                          DisplayLastModifiedBy = PU.UpdatedBy,
                                          UpdatedDate = PU.UpdatedDate,
                                          DisplayCreatedDate = PU.CreatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplayLastModifiedDate = PU.UpdatedDate.Value.ToString("MMM dd yyyy hh:mm tt"),
                                          DisplaySubmissionDate = PU.SubmissionDate,
                                          NoOfBags = PU.NoOfBags,
                                          NoOfPaxs = PU.NoOfPaxs

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
                //ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => timeConversion(q.JobTime)).ToList();
                //ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => q.JobTime).ToList();
                //ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ToList();
            }

            TotalCount = ObjJobs.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobs = ObjJobs.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobs;
        }


        public JobModel Job_Edit(long? JobCode)
        {
            JobModel ObjJobs = (from ct in db.Jobs

                                    //join jv in db.JobVendors on  ct.JobVendorCode equals jv.JobVendorCode
                                where ct.JobCode == JobCode
                                let jobvenname = (from jv in db.JobVendors where jv.JobVendorCode == ct.JobVendorCode select jv.JobVendorName).FirstOrDefault()
                                select new JobModel
                                {
                                    JobCode = ct.JobCode,
                                    JobDate = ct.JobDate,
                                    JobTime = ct.JobTime,
                                    VehicleCode = ct.VehicleCode,
                                    JobFrom = ct.JobFrom,
                                    JobTo = ct.JobTo,
                                    JobVendorCode = ct.JobVendorCode,
                                    JobVendorName = jobvenname,
                                    OutSource = ct.OutSource,
                                    CustomerName = ct.CustomerName,
                                    ContactNo = ct.ContactNo,
                                    DrivingBy = ct.DrivingBy,
                                    Remarks = ct.Remarks,
                                    //Cost = ct.Cost,
                                    Credit = ct.Credit,
                                    Cash = ct.Cash,
                                    OutSourceAmount = ct.OutSourceAmount,
                                    OutSourceAmountGiven = ct.OutSourceAmountGiven,
                                    CashInHand = ct.CashInHand,
                                    JobStatus = ct.JobStatus
                                }).FirstOrDefault();


            return ObjJobs;
        }
        public JobRequestModel Job_RequestEdit(long? JobRequestCode)
        {
            JobRequestModel ObjJobs = (from ct in db.JobRequests
                                       where ct.JobRequestCode == JobRequestCode
                                       select new JobRequestModel
                                       {
                                           JobRequestCode = ct.JobRequestCode,
                                           JobRequestDate = ct.JobRequestDate,
                                           JobRequestTime = ct.JobRequestTime,
                                           JobRequestFrom = ct.JobRequestFrom,
                                           JobRequestTo = ct.JobRequestTo,
                                           CustomerName = ct.CustomerName,
                                           ContactNo = ct.ContactNo,
                                           JobRequestStatus = ct.JobRequestStatus,
                                           Cost = ct.Cost
                                       }).FirstOrDefault();


            return ObjJobs;
        }
        public List<JobStatusModel> JobStatus_FindAll(long? JobCode)
        {
            List<JobStatusModel> ObjJobPassenger = null;

            try
            {

                ObjJobPassenger = (from PU in db.JobStatus
                                   join SM in db.TblUserMasters on PU.UpdatedBy equals SM.UserID
                                   where PU.JobCode == JobCode
                                   select new JobStatusModel
                                   {
                                       JobStatusCode = PU.JobStatusCode,
                                       JobCode = PU.JobCode,
                                       JobStatus = PU.JobStatus,
                                       UpdatedBy = PU.UpdatedBy,
                                       DisplayLastModifiedBy = SM.FirstName,
                                       UpdatedDate = PU.UpdatedDate
                                   }).ToList().Select(o =>
                                                      new JobStatusModel
                                                      {
                                                          JobStatusCode = o.JobStatusCode,
                                                          JobCode = o.JobCode,
                                                          JobStatus = o.JobStatus,
                                                          DisplayLastModifiedBy = o.DisplayLastModifiedBy,
                                                          UpdatedDate = o.UpdatedDate,
                                                          DisplayLastModifiedDate = o.UpdatedDate.Value.ToString("MMM dd yyyy hh:mm tt")
                                                      }).ToList();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobStatus_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobStatus_FindAll");
            }
            return ObjJobPassenger;
        }
        public ReturnMessageModel Job_Save(JobModel objEJob)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var obj = (from tblSMod in db.Jobs
                           where tblSMod.JobCode == objEJob.JobCode
                           select tblSMod).SingleOrDefault();

                if (obj == null)
                {
                    ObjMessage = Job_Insert(objEJob);//insert
                }
                else
                {
                    ObjMessage = Job_Update(objEJob);
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Job_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Job_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }
        public ReturnMessageModel Job_Insert(JobModel objEJob)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var entity = new Job
                {
                    JobDate = objEJob.JobDate.Value,
                    JobTime = objEJob.JobTime,
                    VehicleCode = objEJob.VehicleCode,
                    JobFrom = objEJob.JobFrom,
                    JobTo = objEJob.JobTo,
                    JobVendorCode = objEJob.JobVendorCode,
                    DrivingBy = objEJob.DrivingBy,
                    OutSource = objEJob.OutSource,
                    CustomerName = objEJob.CustomerName,
                    ContactNo = objEJob.ContactNo,
                    CashInHand = objEJob.CashInHand,
                    Remarks = objEJob.Remarks,
                    //Cost = objEJob.Cost,
                    Credit = objEJob.Credit,
                    Cash = objEJob.Cash,
                    OutSourceAmount = objEJob.OutSourceAmount,
                    OutSourceAmountGiven = objEJob.OutSourceAmountGiven,
                    JobRequestCode = objEJob.JobRequestCode,
                    CreatedBy = objEJob.CreatedBy,
                    CreatedDate = CommonRepository.GetTimeZoneDate(),
                    UpdatedBy = objEJob.CreatedBy,
                    UpdatedDate = CommonRepository.GetTimeZoneDate()

                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();
                long JobCode = entity.JobCode;

                var CreatedBy = objEJob.CreatedBy ?? 0;

                ObjMessage = JobStatus_Update(JobCode, "Assigned", CreatedBy);
                ////Update Job Request Status
                //var objUpdate = (from ct in db.JobRequests where ct.JobRequestCode == objEJob.JobRequestCode select ct).ToList();

                //if (objUpdate != null && objUpdate.Count == 1)
                //{
                //    objUpdate[0].JobRequestStatus = "Completed";

                //    db.SaveChanges();                   
                //}

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "Job_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Job_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }
        public ReturnMessageModel Job_Update(JobModel objEJob)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objUpdate = (from ct in db.Jobs where ct.JobCode == objEJob.JobCode select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].UpdatedBy = objEJob.CreatedBy;
                    objUpdate[0].UpdatedDate = CommonRepository.GetTimeZoneDate();
                    objUpdate[0].JobDate = objEJob.JobDate.Value;
                    objUpdate[0].JobTime = objEJob.JobTime;
                    objUpdate[0].VehicleCode = objEJob.VehicleCode;
                    objUpdate[0].JobFrom = objEJob.JobFrom;
                    objUpdate[0].JobTo = objEJob.JobTo;
                    objUpdate[0].OutSource = objEJob.OutSource;
                    objUpdate[0].CustomerName = objEJob.CustomerName;
                    objUpdate[0].Remarks = objEJob.Remarks;
                    objUpdate[0].ContactNo = objEJob.ContactNo;
                    objUpdate[0].JobVendorCode = objEJob.JobVendorCode;
                    objUpdate[0].DrivingBy = objEJob.DrivingBy;
                    //objUpdate[0].Cost = objEJob.Cost;
                    objUpdate[0].Credit = objEJob.Credit;
                    objUpdate[0].Cash = objEJob.Cash;
                    objUpdate[0].OutSourceAmount = objEJob.OutSourceAmount;
                    objUpdate[0].OutSourceAmountGiven = objEJob.OutSourceAmountGiven;
                    objUpdate[0].CashInHand = objEJob.CashInHand;

                    db.SaveChanges();

                    //var CreatedBy = objEJob.CreatedBy ?? 0;
                    //ObjMessage = JobStatus_Update(objEJob.JobCode, objEJob.JobStatus, CreatedBy);

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "EJob_update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "EJob_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel JobStatus_Update(long? JobCode, string JobStatus, int LastModifiedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                //Add Job Status to History
                var statusentity = new JobStatu
                {
                    JobCode = JobCode.Value,
                    JobStatus = JobStatus,
                    UpdatedBy = LastModifiedBy,
                    UpdatedDate = CommonRepository.GetTimeZoneDate()

                };
                db.Entry(statusentity).State = EntityState.Added;
                db.SaveChanges();

                //Update Job Status
                var objUpdate = (from ct in db.Jobs where ct.JobCode == JobCode select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].JobStatus = JobStatus;
                    db.SaveChanges();
                }

                if (JobStatus == "Job Completed")
                {
                    // ── Update Job Request status (existing logic ─ keep as-is) ──────────
                    var _requestCode = (from ct in db.Jobs
                                        where ct.JobCode == JobCode
                                        select ct.JobRequestCode).FirstOrDefault();

                    var objReqUpdate = (from ct in db.JobRequests
                                        where ct.JobRequestCode == _requestCode
                                        select ct).ToList();

                    if (objReqUpdate != null && objReqUpdate.Count == 1)
                    {
                        objReqUpdate[0].JobRequestStatus = "Completed";
                        db.SaveChanges();
                    }

                    // ── NEW: Credit CashInHand user's wallet ─────────────────────────────
                    // Safe to call ─ sp_frm_wallet_OnJobComplete has an idempotency guard
                    try
                    {
                        string connPath = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
                        string connStr = System.IO.File.ReadAllText(connPath);

                        using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                        {
                            conn.Open();
                            var walletCmd = new System.Data.SqlClient.SqlCommand(
                                "sp_frm_wallet_OnJobComplete", conn)
                            {
                                CommandType = System.Data.CommandType.StoredProcedure
                            };
                            walletCmd.Parameters.AddWithValue("@JobCode", JobCode.Value);
                            walletCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception walletEx)
                    {
                        // Log but do NOT break the job status update
                        // Use Console.WriteLine or your preferred logging method
                        System.Diagnostics.Debug.WriteLine("Wallet credit error: " + walletEx.Message);
                    }
                }

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobStatus_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobStatus_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel JobStatus_ReAssign(long? JobCode, int LastModifiedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                //Delete all status for the Job
                var objDelete = (from ct in db.JobStatus
                                 where ct.JobCode == JobCode
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count > 0)
                {
                    db.JobStatus.RemoveRange(objDelete);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }

                //Update Job Status
                var objUpdate = (from ct in db.Jobs where ct.JobCode == JobCode select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].JobStatus = "Assigned";
                    objUpdate[0].UpdatedBy = LastModifiedBy;
                    db.SaveChanges();
                }

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobStatus_ReAssign");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobStatus_ReAssign");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel JobStatus_Cancel(long? JobCode, int LastModifiedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                //Delete all status for the Job
                var objDelete = (from ct in db.JobStatus
                                 where ct.JobCode == JobCode
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count > 0)
                {
                    db.JobStatus.RemoveRange(objDelete);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }

                //Update Job Status
                var objUpdate = (from ct in db.Jobs where ct.JobCode == JobCode select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].JobStatus = "Cancelled";
                    objUpdate[0].Cost = 0;
                    objUpdate[0].Credit = 0;
                    objUpdate[0].Cash = 0;
                    objUpdate[0].UpdatedBy = LastModifiedBy;
                    db.SaveChanges();
                }

                //Update Job Request Status
                var _requestCode = (from ct in db.Jobs where ct.JobCode == JobCode select ct.JobRequestCode).FirstOrDefault();
                var objUpdate1 = (from ct in db.JobRequests where ct.JobRequestCode == _requestCode select ct).ToList();

                if (objUpdate1 != null && objUpdate1.Count == 1)
                {
                    objUpdate1[0].JobRequestStatus = "Not Available";
                    db.SaveChanges();
                }
                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobStatus_Cancel");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobStatus_Cancel");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel JobStatus_Delete(long? JobStatusCode)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDelete = (from ct in db.JobStatus
                                 where ct.JobStatusCode == JobStatusCode
                                 select ct).ToList();

                if (objDelete != null && objDelete.Count > 0)
                {
                    db.JobStatus.RemoveRange(objDelete);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobStatus_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "Transaction_Delete");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }

            return ObjMessage;
        }
        #endregion

        #region "My Jobs"
        public List<MyJobModel> MyJob_FindAll(int? page, int? limit, int? DrivingBy, string sortBy, string direction, out int TotalCount)
        {
            List<MyJobModel> ObjJobs = (from PU in db.sp_frm_get_Jobs(null, null, null, null, DrivingBy, null, null, null)

                                        select new MyJobModel
                                        {
                                            JobCode = PU.JobCode,
                                            JobDate = PU.JobDate,
                                            //JobTime = timeConversion(PU.JobTime),
                                            JobTime = PU.JobTime,
                                            VehicleName = PU.VehicleName,
                                            JobFrom = PU.JobFrom,
                                            JobTo = PU.JobTo,
                                            DrivingByName = PU.DrivingBy,
                                            CustomerName = PU.CustomerName,
                                            ContactNo = PU.ContactNo,
                                            Credit = PU.Credit,
                                            Cash = PU.Cash,
                                            JobStatus = PU.JobStatus,
                                            UpdatedDate = PU.UpdatedDate,
                                            CashInHand = PU.CashInHand,
                                            NoOfPaxs = PU.NoOfPaxs,
                                            NoOfBags = PU.NoOfBags

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

                    }
                }
            }
            else
            {
                //ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => timeConversion(q.JobTime)).ToList();
                ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => q.JobTime).ToList();
            }
            ObjJobs = ObjJobs.Where(o => o.JobStatus != "Job Completed").Where(o => o.JobStatus != "Cancelled").Where(o => o.JobStatus != "No Show").ToList();
            TotalCount = ObjJobs.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobs = ObjJobs.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobs;
        }
        public List<MyJobModel> MyJob_FindAllHistory(int? page, int? limit, int? DrivingBy, string sortBy, string direction, out int TotalCount)
        {
            List<MyJobModel> ObjJobs = (from PU in db.sp_frm_get_Jobs(null, null, null, null, DrivingBy, null, null, null)
                                        where PU.JobDate >= DateTime.Now.AddDays(-40)
                                        select new MyJobModel
                                        {
                                            JobCode = PU.JobCode,
                                            JobDate = PU?.JobDate,
                                            //JobTime = timeConversion(PU.JobTime),
                                            JobTime = PU?.JobTime,
                                            VehicleName = PU?.VehicleName,
                                            JobFrom = PU?.JobFrom,
                                            JobTo = PU?.JobTo,
                                            DrivingByName = PU?.DrivingBy,
                                            CustomerName = PU?.CustomerName,
                                            ContactNo = PU?.ContactNo,
                                            Credit = PU?.Credit,
                                            Cash = PU?.Cash,
                                            NoOfPaxs = PU?.NoOfPaxs,
                                            NoOfBags = PU?.NoOfBags,
                                            JobStatus = PU?.JobStatus,
                                            UpdatedDate = PU?.UpdatedDate,
                                            CashInHand = PU?.CashInHand

                                        }).ToList();
            //ObjJobs.Where(o => o.JobStatus == "Job Completed").Where(o => o.UpdatedDate <= DateTime.Now.AddDays(-40)).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjJobs = ObjJobs.OrderBy(q => q.VehicleName).ToList();
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

                    }
                }
            }
            else
            {
                //ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => timeConversion(q.JobTime)).ToList();
                ObjJobs = ObjJobs.OrderBy(q => q.JobDate).ThenBy(q => q.JobTime).ToList();
            }

            TotalCount = ObjJobs.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobs = ObjJobs.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobs;
        }
        #endregion

        #region "Job Requests"
        public List<JobRequestModel> JobRequest_FindAll(int? page, string CustomerName, string ContactNo, int? limit, string sortBy, string direction, out int TotalCount)
        {
            List<JobRequestModel> ObjJobRequests = (from PU in db.JobRequests
                                                    where ((ContactNo == null || ContactNo == string.Empty) || PU.ContactNo.Contains(ContactNo)) &&
                                                          ((CustomerName == string.Empty || CustomerName == null) || PU.CustomerName.Contains(CustomerName))

                                                    select new JobRequestModel
                                                    {
                                                        JobRequestCode = PU.JobRequestCode,
                                                        JobRequestDate = PU.JobRequestDate,
                                                        JobRequestTime = PU.JobRequestTime,
                                                        JobRequestFrom = PU.JobRequestFrom,
                                                        JobRequestTo = PU.JobRequestTo,
                                                        CustomerName = PU.CustomerName,
                                                        ContactNo = PU.ContactNo,
                                                        JobRequestStatus = PU.JobRequestStatus,
                                                        CreatedDate = PU.CreatedDate,
                                                        Cost = PU.Cost,
                                                        NoOfPaxs = PU.NoOfPaxs,
                                                        NoOfBags = PU.NoOfBags

                                                    }).ToList().Select(o =>
                                                           new JobRequestModel
                                                           {
                                                               JobRequestCode = o.JobRequestCode,
                                                               JobRequestDate = o.JobRequestDate,
                                                               JobRequestTime = o.JobRequestTime,
                                                               JobRequestFrom = o.JobRequestFrom,
                                                               JobRequestTo = o.JobRequestTo,
                                                               CustomerName = o.CustomerName,
                                                               ContactNo = o.ContactNo,
                                                               JobRequestStatus = o.JobRequestStatus,
                                                               CreatedDate = o.CreatedDate,
                                                               DisplayCreatedDate = o.CreatedDate.Value.AddHours(15).ToString("MMM dd yyyy hh:mm tt"),
                                                               Cost = o.Cost,
                                                               NoOfPaxs = o.NoOfPaxs,
                                                               NoOfBags = o.NoOfBags
                                                           }).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "CustomerName":
                            ObjJobRequests = ObjJobRequests.OrderBy(q => q.CustomerName).ToList();
                            break;
                        case "JobRequestStatus":
                            ObjJobRequests = ObjJobRequests.OrderBy(q => q.JobRequestStatus).ToList();
                            break;
                    }
                }
                else
                {
                    // step 7 applying sorting desc
                    switch (sortBy.Trim())
                    {
                        case "CustomerName":
                            ObjJobRequests = ObjJobRequests.OrderByDescending(q => q.CustomerName).ToList();
                            break;
                        case "JobRequestStatus":
                            ObjJobRequests = ObjJobRequests.OrderByDescending(q => q.JobRequestStatus).ToList();
                            break;
                    }
                }
            }
            else
            {
                ObjJobRequests = ObjJobRequests.OrderByDescending(q => q.CreatedDate).ToList();
            }

            TotalCount = ObjJobRequests.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobRequests = ObjJobRequests.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobRequests;
        }
        public ReturnMessageModel JobRequest_Insert(JobRequestModel objEJobRequest)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var entity = new JobRequest
                {
                    JobRequestDate = objEJobRequest.JobRequestDate.Value,
                    JobRequestTime = objEJobRequest.JobRequestTime,
                    JobRequestFrom = objEJobRequest.JobRequestFrom,
                    JobRequestTo = objEJobRequest.JobRequestTo,
                    CustomerName = objEJobRequest.CustomerName,
                    ContactNo = objEJobRequest.ContactNo,
                    NoOfBags = objEJobRequest.NoOfBags,
                    NoOfPaxs = objEJobRequest.NoOfPaxs,
                    JobRequestStatus = "Requested",
                    CreatedDate = CommonRepository.GetTimeZoneDate()

                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();
                //Send Email
                // "booking@nissitravels.com"
                string linkcontent = "";
                linkcontent += "<b>Job Request details as follows!</b>"; linkcontent += "<br />"; linkcontent += "<br />";
                linkcontent += "Name : " + objEJobRequest.CustomerName; linkcontent += "<br />";
                linkcontent += "Contact No : " + objEJobRequest.ContactNo; linkcontent += "<br />";
                linkcontent += "Date : " + objEJobRequest.JobRequestDate.Value.ToString("dd-MMM-yyyy"); linkcontent += "<br />";
                linkcontent += "Time : " + objEJobRequest.JobRequestTime; linkcontent += "<br />";
                linkcontent += "Pickup : " + objEJobRequest.JobRequestFrom; linkcontent += "<br />";
                linkcontent += "Dropoff : " + objEJobRequest.JobRequestTo; linkcontent += "<br />";
                linkcontent += "No Of Paxs : " + objEJobRequest.NoOfPaxs; linkcontent += "<br />";
                linkcontent += "No Of Bags : " + objEJobRequest.NoOfBags; linkcontent += "<br />";


                SendMail("booking@nissitravels.com", linkcontent, "New Job Request!");

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobRequest_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobRequest_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }
        public bool IsRequestedCheck()
        {
            return db.JobRequests.Where(o => o.JobRequestStatus == "Requested").Any();
        }
        public int IsRequestedCount()
        {
            return db.JobRequests.Where(o => o.JobRequestStatus == "Requested").Count();
        }
        #endregion

        static string timeConversion(string inputTimeString)
        {
            DateTime outputTime = DateTime.Parse(inputTimeString);
            return outputTime.ToString("HH:mm");
        }

        public void SendMail(string ToAddress, string Content, string Subject)
        {
            //ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                string SmtpServer = "smtp.gmail.com";
                string MailserverLogin = "nissitravels17@gmail.com";
                string MailServerPassword = "rvnt nnse vamp cmhu";
                string MailUserName = "Nissi Transport";

                // SmtpClient
                var client = new SmtpClient(SmtpServer)
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential(MailserverLogin, MailServerPassword),
                    EnableSsl = true
                };

                // Specify the email sender.
                var from = new MailAddress(MailserverLogin, MailUserName, System.Text.Encoding.UTF8);

                // Set destinations for the email message.
                var to = new MailAddress(ToAddress);

                // Specify the message content.
                var message = new MailMessage(@from, to)
                {
                    Body = Content,
                    Subject = Subject,
                    IsBodyHtml = true
                };

                //client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                client.Send(message);

                //clean up.
                message.Dispose();
                client.Dispose();
                //ObjMessage.Message = "Mail Sent!";

            }
            catch (Exception ep)
            {
                //ObjMessage.Message = ep.Message;
            }

            //return ObjMessage;
        }

        // Optional helper method for connection string
        private string Conn()
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
            return System.IO.File.ReadAllText(path);
        }

        //static bool mailSent = false;
        //private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        //{
        //    // Get the unique identifier for this asynchronous operation.
        //    var token = (string)e.UserState;

        //    if (e.Cancelled)
        //    {
        //        Console.WriteLine("[{0}] Send canceled.", token);
        //    }

        //    if (e.Error != null)
        //    {
        //        Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
        //    }
        //    else
        //    {
        //        Console.WriteLine("Message sent.");
        //    }

        //    mailSent = true;
        //}
    }
}