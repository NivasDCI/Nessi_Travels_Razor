using Transport.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transport.Repository
{
    public interface IJobRepository
    {
        #region "Job"
        List<JobModel> Job_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, int? CashInHand, string CustomerName, string ContactNo, int? DrivingBy, int? limit, string sortBy, string direction, out int TotalCount);
        List<JobModel> CreditJob_FindAll(int? page, DateTime? StartDate, DateTime? EndDate, int? VehicleCode, int? JobVendorCode, string CustomerName, string ContactNo, int? DrivingBy, int? limit, string sortBy, string direction, out int TotalCount);
        
        JobModel Job_Edit(long? JobCode);
        JobRequestModel Job_RequestEdit(long? JobRequestCode);
        List<JobStatusModel> JobStatus_FindAll(long? JobCode);
        ReturnMessageModel Job_Save(JobModel objEJob);
        ReturnMessageModel Job_Insert(JobModel objEJob);
        ReturnMessageModel Job_Update(JobModel objEJob);
        ReturnMessageModel JobStatus_Update(long? JobCode, string JobStatus, int LastModifiedBy);
        ReturnMessageModel JobStatus_Delete(long? JobStatusCode);
        ReturnMessageModel JobStatus_ReAssign(long? JobCode, int LastModifiedBy);
        ReturnMessageModel JobStatus_Cancel(long? JobCode, int LastModifiedBy);
        List<MyJobModel> MyJob_FindAll(int? page, int? limit, int? DrivingBy, string sortBy, string direction, out int TotalCount);
        List<MyJobModel> MyJob_FindAllHistory(int? page, int? limit, int? DrivingBy, string sortBy, string direction, out int TotalCount);
        List<JobRequestModel> JobRequest_FindAll(int? page, string CustomerName, string ContactNo, int? limit, string sortBy, string direction, out int TotalCount);
        ReturnMessageModel JobRequest_Insert(JobRequestModel objEJobRequest);
        bool IsRequestedCheck();
        int IsRequestedCount();
        void SendMail(string ToAddress, string Content, string Subject);
        #endregion


    }
}
