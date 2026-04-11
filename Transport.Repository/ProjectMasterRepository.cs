using Transport.Entity;
using Transport.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Web;
using System.IO;
using System.Globalization;
using System.Drawing;

namespace Transport.Repository
{
    public class ProjectMasterRepository : IProjectMasterRepository
    {
        CommonRepository ObjCom = new CommonRepository();
        TransportEntities db = new TransportEntities();

        #region "Default Master"

        public List<DefaultDetailMasterModel> DefaultMasterDetail_FindAll(string DefaultHeaderID, bool? Status, int? page, int? limit, string sortBy, string direction, out int TotalCount)
        {
            TotalCount = 0;
            List<DefaultDetailMasterModel> ObjProject = new List<DefaultDetailMasterModel>();

            try
            {


                ObjProject = (from DMD in db.TblDefaultMasterDetails
                              where DMD.DefaultHeaderID == DefaultHeaderID
                                               && ((Status == null && DMD.Status == true) || DMD.Status == Status)
                              select new DefaultDetailMasterModel
                              {
                                  DefaultDetailID = DMD.DefaultDetailID,
                                  DefaultHeaderID = DMD.DefaultHeaderID,
                                  DefaultValueField = DMD.DefaultValueField,
                                  DefaultTextField = DMD.DefaultTextField,
                                  DefaultOrder = DMD.DefaultOrder,
                                  Status = DMD.Status,
                                  CreatedDate = DMD.CreatedDate,
                                  LastModifiedDate = DMD.LastModifiedDate
                              }).ToList();


                if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
                {
                    if (direction.Trim().ToLower() == "asc")
                    {
                        switch (sortBy.Trim())
                        {
                            case "DefaultTextField":
                                ObjProject = ObjProject.OrderBy(q => q.DefaultTextField).ToList();
                                break;
                            case "DefaultOrder":
                                ObjProject = ObjProject.OrderBy(q => q.DefaultOrder).ToList();
                                break;
                            case "Status":
                                ObjProject = ObjProject.OrderBy(q => q.Status).ToList();
                                break;
                        }
                    }
                    else
                    {
                        switch (sortBy.Trim())
                        {
                            case "DefaultTextField":
                                ObjProject = ObjProject.OrderByDescending(q => q.DefaultTextField).ToList();
                                break;
                            case "DefaultOrder":
                                ObjProject = ObjProject.OrderByDescending(q => q.DefaultOrder).ToList();
                                break;
                            case "Status":
                                ObjProject = ObjProject.OrderByDescending(q => q.Status).ToList();
                                break;
                        }
                    }
                }
                else
                {
                    ObjProject = ObjProject.OrderByDescending(q => q.CreatedDate).ToList();
                }

                TotalCount = ObjProject.Count;

                if (page.HasValue && limit.HasValue)
                {
                    int start = (page.Value - 1) * limit.Value;
                    ObjProject = ObjProject.Skip(start).Take(limit.Value).ToList();
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DefaultDetailMasterModel_FindAll");
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DefaultDetailMasterModel_FindAll");
            }
            return ObjProject;
        }
        
        public int DefaultMaxOrder(string DefaultHeaderID)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            int DefaultMaxOrder = 0;
            try
            {

                DefaultMaxOrder = (from tblDefault in db.TblDefaultMasterDetails
                                   where tblDefault.DefaultHeaderID == DefaultHeaderID  
                                   select tblDefault.DefaultOrder??0).Max();

               
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DefaultMaxOrder");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DefaultMaxOrder");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }

            return DefaultMaxOrder+1;
        }

        public ReturnMessageModel DefaultMasterDetail_Save(DefaultDetailMasterModel objDefaultDetailModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var objDupOrder = (from tblDefault in db.TblDefaultMasterDetails
                                   where tblDefault.DefaultHeaderID == objDefaultDetailModel.DefaultHeaderID
                                        && tblDefault.DefaultOrder == objDefaultDetailModel.DefaultOrder
                                       && tblDefault.DefaultDetailID != objDefaultDetailModel.DefaultDetailID
                                   && tblDefault.Status == true
                                   select tblDefault).ToList();

                if (objDupOrder != null && objDupOrder.Count == 0)
                {
                    var obj = (from tblDefault in db.TblDefaultMasterDetails
                               where tblDefault.DefaultTextField == objDefaultDetailModel.DefaultTextField
                                   && tblDefault.DefaultHeaderID == objDefaultDetailModel.DefaultHeaderID
                                       && tblDefault.DefaultDetailID != objDefaultDetailModel.DefaultDetailID
                               select tblDefault).ToList();

                    if (obj != null && obj.Count == 0)
                    {
                        if (objDefaultDetailModel.DefaultDetailID != "" && objDefaultDetailModel.DefaultDetailID != null)
                        {
                            ObjMessage = DefaultMasterDetail_Update(objDefaultDetailModel);
                        }
                        else
                        {
                            ObjMessage = DefaultMasterDetail_Insert(objDefaultDetailModel);
                        }
                    }
                    else
                    {
                        ObjMessage.Result = false;
                        ObjMessage.Status = WEBCONSTANTMESSAGECODE.DUPLICATE;
                        ObjMessage.Message = WEBCONSTANTMESSAGE.DUPLICATEFAIL;
                    }
                }
                else
                {
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.DUPLICATEORDER;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.DUPLICATEORDERFAIL;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DefaultDetailMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DefaultDetailMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }

            return ObjMessage;
        }

        public ReturnMessageModel DefaultMasterDetail_Insert(DefaultDetailMasterModel objDefaultDetailModel)
        {

            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                var entity = new TblDefaultMasterDetail
                {
                    DefaultDetailID = CommonRepository.RandomString(12),
                    DefaultHeaderID = objDefaultDetailModel.DefaultHeaderID,
                    DefaultOrder = objDefaultDetailModel.DefaultOrder,
                    DefaultTextField = objDefaultDetailModel.DefaultTextField,
                    DefaultValueField = objDefaultDetailModel.DefaultValueField,
                    IsPage=true,
                    IsSearch=true,
                    Status = true,
                    CreatedBy = objDefaultDetailModel.CreatedBy,
                    CreatedDate = CommonRepository.GetTimeZoneDate(),
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();

                ObjMessage.Result = true;
                ObjMessage.DefaultUniqueID = entity.DefaultDetailID;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DefaultMasterDetail_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DefaultMasterDetail_Insert");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }

        public ReturnMessageModel DefaultMasterDetail_Update(DefaultDetailMasterModel ObjDefaultDetailModel)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDefault = (from tblDefault in db.TblDefaultMasterDetails
                                  where tblDefault.DefaultHeaderID == ObjDefaultDetailModel.DefaultHeaderID
                                  && tblDefault.DefaultDetailID == ObjDefaultDetailModel.DefaultDetailID
                                  select tblDefault).ToList();
                if (objDefault.Count > 0)
                {
                    // objDefault[0].ColorCode = ObjDefaultDetailModel.ColorCode;
                    objDefault[0].DefaultOrder = ObjDefaultDetailModel.DefaultOrder;
                    objDefault[0].DefaultTextField = ObjDefaultDetailModel.DefaultTextField.Trim();
                    objDefault[0].DefaultValueField = ObjDefaultDetailModel.DefaultValueField != null ? ObjDefaultDetailModel.DefaultValueField.Trim() : "";
                    objDefault[0].LastModifiedBy = ObjDefaultDetailModel.CreatedBy;
                    objDefault[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();

                    db.SaveChanges();
                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }
                else
                {
                    ObjMessage.Result = false;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.NORECORD;
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DefaultDetailMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DefaultDetailMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }

            return ObjMessage;
        }

        public ReturnMessageModel DefaultMasterDetail_Delete(string DefaultDetailID, int deletedBy)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                var objDefaultDetail = (from tblDefault in db.TblDefaultMasterDetails
                                        where tblDefault.DefaultDetailID == DefaultDetailID
                                        select tblDefault).ToList();

                objDefaultDetail[0].Status = false;
                objDefaultDetail[0].LastModifiedBy = deletedBy;
                objDefaultDetail[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                db.SaveChanges();

                ObjMessage.Result = true;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.DELETE;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETESUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "DefaultDetailMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "DefaultDetailMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.DELETEFAIL;
            }

            return ObjMessage;
        }
        #endregion         

        #region Project Master

        #region "VehicleMaster" 
        public List<VehicleMasterModel> VehicleMaster_FindAll(int? page, int? limit, string VehicleName, bool? Status, string sortBy, string direction, out Int32 TotalCount)
        {

            List<VehicleMasterModel> ObjVehicleMaster = (from ct in db.Vehicles
                                                     where ((VehicleName == null || VehicleName == "") || ct.VehicleName.Contains(VehicleName))

                                                     && ((Status == null && ct.Status == true) || ct.Status == Status)
                                                     select new VehicleMasterModel
                                                     {
                                                         VehicleCode = ct.VehicleCode,
                                                         VehicleName = ct.VehicleName,                                                         
                                                         Status = ct.Status                                                        

                                                     }).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjVehicleMaster = ObjVehicleMaster.OrderBy(q => q.VehicleName).ToList();
                            break;
                       
                    }
                }
                else
                {
                    // step 7 applying sorting desc

                    switch (sortBy.Trim())
                    {
                        case "VehicleName":
                            ObjVehicleMaster = ObjVehicleMaster.OrderByDescending(q => q.VehicleName).ToList();
                            break;
                       
                    }
                }
            }
            else
            {
                ObjVehicleMaster = ObjVehicleMaster.OrderBy(q => q.VehicleName).ToList();
            }


            TotalCount = ObjVehicleMaster.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjVehicleMaster = ObjVehicleMaster.Skip(start).Take(limit.Value).ToList();
            }

            return ObjVehicleMaster;
        }
        public VehicleMasterModel VehicleMaster_Edit(int VehicleCode)
        {
            VehicleMasterModel ObjVehicleMaster = (from ct in db.Vehicles
                                               where ct.VehicleCode == VehicleCode
                                               select new VehicleMasterModel
                                               {
                                                   VehicleCode = ct.VehicleCode,
                                                   VehicleName = ct.VehicleName,                                                 
                                                   Status = ct.Status
                                                   
                                               }).FirstOrDefault();


            return ObjVehicleMaster;
        }
        public ReturnMessageModel VehicleMaster_Save(VehicleMasterModel objVehicleMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                if (objVehicleMaster.VehicleCode == 0)
                {
                    ObjMessage = VehicleMaster_Insert(objVehicleMaster);////insert
                }
                else
                {
                    ObjMessage = VehicleMaster_Update(objVehicleMaster);////update
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "VehicleMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "VehicleMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }
        public ReturnMessageModel VehicleMaster_Insert(VehicleMasterModel objVehicleMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var entity = new Vehicle
                {
                    VehicleName = objVehicleMaster.VehicleName,
                    Status = objVehicleMaster.Status,
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();
                ObjMessage.Result = true;
                ObjMessage.UniqueID = entity.VehicleCode;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "VehicleMaster_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "VehicleMaster_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }        
        public ReturnMessageModel VehicleMaster_Update(VehicleMasterModel objVehicleMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                var objUpdate = (from ct in db.Vehicles where ct.VehicleCode == objVehicleMaster.VehicleCode select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].VehicleName = objVehicleMaster.VehicleName;
                    objUpdate[0].Status = objVehicleMaster.Status;

                    db.SaveChanges();
                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "VehicleMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "VehicleMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;
        }

        #endregion

        #region "JobVendorsMaster" 

        public List<JobVendorMasterModel> JobVendorsMaster_FindAll(int? page, int? limit, string JobVendorName, bool? Status, string sortBy, string direction, out Int32 TotalCount)
        {

            List<JobVendorMasterModel> ObjJobVendorsMaster = (from ct in db.JobVendors
                                                             where ((JobVendorName == null || JobVendorName == "") || ct.JobVendorName.Contains(JobVendorName))

                                                         && ((Status == null && ct.Status == true) || ct.Status == Status)
                                                             select new JobVendorMasterModel
                                                             {
                                                                 JobVendorCode = ct.JobVendorCode,
                                                                 JobVendorName = ct.JobVendorName,
                                                                 Address1 = ct.Address1,
                                                                 Address2 = ct.Address2,
                                                                 Address3 = ct.Address3,
                                                                 ContactNo = ct.ContactNo,
                                                                 Status = ct.Status

                                                             }).ToList();

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim())
                    {
                        case "JobVendorName":
                            ObjJobVendorsMaster = ObjJobVendorsMaster.OrderBy(q => q.JobVendorName).ToList();
                            break;

                    }
                }
                else
                {
                    // step 7 applying sorting desc
                    switch (sortBy.Trim())
                    {
                        case "JobVendorName":
                            ObjJobVendorsMaster = ObjJobVendorsMaster.OrderByDescending(q => q.JobVendorName).ToList();
                            break;

                    }
                }
            }
            else
            {
                ObjJobVendorsMaster = ObjJobVendorsMaster.OrderBy(q => q.JobVendorName).ToList();
            }


            TotalCount = ObjJobVendorsMaster.Count;

            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                ObjJobVendorsMaster = ObjJobVendorsMaster.Skip(start).Take(limit.Value).ToList();
            }

            return ObjJobVendorsMaster;
        }
        public JobVendorMasterModel JobVendorsMaster_Edit(long JobVendorCode)
        {
            JobVendorMasterModel ObjJobVendorsMaster = (from ct in db.JobVendors
                                                       where ct.JobVendorCode == JobVendorCode
                                                       select new JobVendorMasterModel
                                                       {
                                                           JobVendorCode = ct.JobVendorCode,
                                                           JobVendorName = ct.JobVendorName,
                                                           Address1 = ct.Address1,
                                                           Address2 = ct.Address2,
                                                           Address3 = ct.Address3,
                                                           ContactNo = ct.ContactNo,
                                                           Status = ct.Status

                                                       }).FirstOrDefault();


            return ObjJobVendorsMaster;
        }
        public ReturnMessageModel JobVendorsMaster_Save(JobVendorMasterModel objJobVendorsMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {
                if (objJobVendorsMaster.JobVendorCode == 0)
                {
                    ObjMessage = JobVendorsMaster_Insert(objJobVendorsMaster);////insert
                }
                else
                {
                    ObjMessage = JobVendorsMaster_Update(objJobVendorsMaster);////update
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobVendorsMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobVendorsMaster_Save");
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;
        }
        public ReturnMessageModel JobVendorsMaster_Insert(JobVendorMasterModel objJobVendorsMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();
            try
            {

                var entity = new JobVendor
                {
                    JobVendorName = objJobVendorsMaster.JobVendorName,
                    Address1 = objJobVendorsMaster.Address1,
                    Address2 = objJobVendorsMaster.Address2,
                    Address3 = objJobVendorsMaster.Address3,
                    ContactNo = objJobVendorsMaster.ContactNo,
                    Status = objJobVendorsMaster.Status,
                };
                db.Entry(entity).State = EntityState.Added;
                db.SaveChanges();
                ObjMessage.Result = true;
                ObjMessage.UniqueID = entity.JobVendorCode;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.INSERT;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTSUCCESS;

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobVendorsMaster_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobVendorsMaster_Insert");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.INSERTFAIL;
            }
            return ObjMessage;
        }
        public ReturnMessageModel JobVendorsMaster_Update(JobVendorMasterModel objJobVendorsMaster)
        {
            ReturnMessageModel ObjMessage = new ReturnMessageModel();

            try
            {
                var objUpdate = (from ct in db.JobVendors where ct.JobVendorCode == objJobVendorsMaster.JobVendorCode select ct).ToList();

                if (objUpdate != null && objUpdate.Count == 1)
                {
                    objUpdate[0].JobVendorName = objJobVendorsMaster.JobVendorName;
                    objUpdate[0].Address1 = objJobVendorsMaster.Address1;
                    objUpdate[0].Address2 = objJobVendorsMaster.Address2;
                    objUpdate[0].Address3 = objJobVendorsMaster.Address3;
                    objUpdate[0].ContactNo = objJobVendorsMaster.ContactNo;
                    objUpdate[0].Status = objJobVendorsMaster.Status;

                    db.SaveChanges();
                    ObjMessage.Result = true;
                    ObjMessage.Status = WEBCONSTANTMESSAGECODE.UPDATE;
                    ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATESUCCESS;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                ObjCom.LogPageError(e, "JobVendorsMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            catch (Exception e)
            {
                ObjCom.GlobalError(e, "JobVendorsMaster_Update");
                ObjMessage.Result = false;
                ObjMessage.Status = WEBCONSTANTMESSAGECODE.ERROR;
                ObjMessage.Message = WEBCONSTANTMESSAGE.UPDATEFAIL;
            }
            return ObjMessage;
        }

        #endregion


        #endregion
    }
}









