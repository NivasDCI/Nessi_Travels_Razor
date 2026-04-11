using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Transport.Model;
using Transport.Repository;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Transport.Controllers
{
    [NoCacheAttribute]
    public class LoginController : Controller
    {
        ICommonRepository ObjCommRepository = new CommonRepository();
        IJobRepository _objJobsRepository = new JobRepository();
        
        #region "Login Master"
        // GET: Login
        public ActionResult Index()
        {
            Session["RequestedBy"] = null;
            Session["fromDate"] = null;
            Session["toDate"] = null;
            
            return View();
        }

        public JsonResult GetLoginAccess(String LoginName, String Password)  
        {
            List<SessionLoginDetail> ObjLoginSession = new List<SessionLoginDetail>();
                                  SessionInvalidLogin ObjInvalidLogin = new SessionInvalidLogin();
            bool IsLogin = false;
            bool IsStore = false;
            string Str_ControllerName = "";
            string Str_ActionName = "";
            int PasswordAttemptCount = ObjCommRepository.GetPasswordAttemptCount(LoginName);

            ObjInvalidLogin = ObjCommRepository.IsRoleMapInvalidPassword(LoginName, Password);

            if (ObjInvalidLogin.UserID == 0)
            {
                return Json(new { IsLogin = false, LoginName = LoginName, UserID = 0, PasswordAttemptCount = PasswordAttemptCount, InValidPassword = true, ISStore = IsStore, ControllerName= Str_ControllerName, ActionName = Str_ActionName }, JsonRequestBehavior.AllowGet);
            }
            else if (ObjInvalidLogin.InValidPassword == true)
            {
                return Json(new { IsLogin = IsLogin, LoginName = LoginName, UserID = ObjInvalidLogin.UserID,  InValidPassword = ObjInvalidLogin.InValidPassword, ISStore = IsStore, ControllerName = Str_ControllerName, ActionName = Str_ActionName }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ObjLoginSession = ObjCommRepository.GetLoginAccess(LoginName, Password);

                if (ObjLoginSession.Count > 0)
                {
                    if (ObjLoginSession.Count == 1)
                    {
                        IsLogin = true;
                    } 

                    if (ObjLoginSession != null && IsLogin == true)
                    {                          
                        Session["USER_ID"] = ObjLoginSession[0].UserID;
                        Session["ROLE_ID"] = ObjLoginSession[0].RoleID;
                       
                        Session["USER_NAME"] = ObjLoginSession[0].UserName;
                        Session["ROLE_NAME"] = ObjLoginSession[0].RoleName;
                       
                        Session["HOME_CONTNAME"] = ObjLoginSession[0].ControllerName;
                        Session["HOME_ACTNAME"] = ObjLoginSession[0].ActionName;

                        Str_ControllerName = ObjLoginSession[0].ControllerName;
                        Str_ActionName = ObjLoginSession[0].ActionName;

                        Session["TIME_ZONE"] = "Singapore Standard Time";
                        if (PasswordAttemptCount != 0 && PasswordAttemptCount <= 3)
                        {
                            ISystemMasterRepository ObjMasterRepository = new SystemMasterRepository();
                            ObjMasterRepository.ResetUserLogin_Update(ObjLoginSession[0].UserID);
                        }
                        if(ObjLoginSession[0].RoleID == "5X6L281Y6W3O")
                        {
                            IsStore = true;
                        }
                    }
                    return Json(new { IsLogin = IsLogin, LoginName = LoginName, UserID = SessionExpire.GetUserID(), InValidPassword = false, ISStore = IsStore, ControllerName = Str_ControllerName, ActionName = Str_ActionName }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { IsLogin = false, LoginName = LoginName, UserID = 0, PasswordAttemptCount = PasswordAttemptCount,  InValidPassword = true, ISStore = IsStore, ControllerName = Str_ControllerName, ActionName = Str_ActionName }, JsonRequestBehavior.AllowGet);
                }
            }
        }
      
        public ActionResult Booking()
        {
            return View();
        }

        public ActionResult OutSource(string JobCode)
        {
            try
            {
                var decryptedData = Decode(JobCode);
                JobModel job = _objJobsRepository.Job_Edit(long.Parse(Shared.ToString(decryptedData)));
                if (job.JobStatus != "Job Completed" && Shared.ToString(job.OutSource).Length > 0)
                {
                    TempData["OutSourceJobCode"] = Shared.ToString(decryptedData);
                    return RedirectToAction("OutSourceJob", "Login");
                }
                else
                    return Redirect("~/JobExpired.html");

            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }
        }

        public ActionResult OutSourceJob()
        {
            try
            {
                if (TempData["OutSourceJobCode"] != null)
                {
                    long? _JobCode = long.Parse(Shared.ToString(TempData["OutSourceJobCode"]));
                    JobModel job = _objJobsRepository.Job_Edit(_JobCode);
                    if (job.JobStatus == "Job Completed") { return Redirect("~/JobExpired.html"); };
                    ViewBag.OutSourceJobCode = _JobCode;
                    ViewBag.JobHeader = "Dear " + Shared.ToString(job.OutSource) + "," + " Your Job details as follows!";

                    string linkcontent = "";
                    linkcontent += "</br>";
                    linkcontent += "Date : " + Shared.ToString(job.JobDate.Value.ToString("dd-MMM-yyyy")); linkcontent += "</br>";
                    linkcontent += "Time : " + Shared.ToString(job.JobTime); linkcontent += "</br>";
                    linkcontent += "Pickup : " + Shared.ToString(job.JobFrom); linkcontent += "</br>";
                    linkcontent += "Dropoff : " + Shared.ToString(job.JobTo); linkcontent += "</br>";
                    linkcontent += "Customer Name : " + Shared.ToString(job.CustomerName); linkcontent += "</br>";
                    linkcontent += "Contact No : " + Shared.ToString(job.ContactNo); linkcontent += "</br>";
                    if (Shared.IsDecimal(job.OutSourceAmount) == true)
                    {
                        linkcontent += "Price : $" + Shared.ToString(job.OutSourceAmount); linkcontent += "</br>";
                    }                   

                    ViewBag.LinkContent = linkcontent ;
                    ViewBag.JobStatus = job.JobStatus;
                    
                }
                return View();
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }
        }
        [HttpPost]
        public ActionResult OutSourceJobUpdate(long? JobCode, string Status)
        {
            try
            {
                ReturnMessageModel ObjMessage = new ReturnMessageModel();

                ObjMessage = _objJobsRepository.JobStatus_Update(JobCode, Status, 1);
                //var icon = (area == 1) ? icon1 : (area == 2) ? icon2 : icon0;
                TempData["OutSourceJobCode"] = JobCode;
                return Json(new { success = true, response = (Status == "Job Started") ? "Job Started Successfully!" : (Status == "Waiting for Customer") ? "Waiting for Customer!" : (Status == "Passenger on board") ? "Passenger on board!" : (Status == "Job Completed") ? "Job Completed Successfully!" : "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, response = ex.Message });
            }

        }

        [HttpPost]
        public ActionResult JobRequest_Save(JobRequestModel objJobRequest)
        {
            try
            {
                ReturnMessageModel ObjMessage = new ReturnMessageModel();
                ObjMessage = _objJobsRepository.JobRequest_Insert(objJobRequest);
                return Json(new { Result = true, Message = "" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }       

        public static string Decode(string decodeMe)
        {
            byte[] encoded = Convert.FromBase64String(decodeMe);
            return System.Text.Encoding.UTF8.GetString(encoded);
        }

        #endregion
    }
}