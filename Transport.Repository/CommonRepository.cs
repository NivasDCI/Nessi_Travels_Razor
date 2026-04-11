using Transport.Entity;
using Transport.Model;
using Transport.Repository.Resource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Transport.Entity;
using WhatsAppApi;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Transport.Repository
{
    public class CommonRepository : ICommonRepository
    {
        TransportEntities db = new TransportEntities();
        #region "Menu Master"
        public List<MenuMasterHeader> MenuMasterHeader(string RoleID)
        {
            List<MenuMasterHeader> ObjMenuMasterHeader = null;

            ObjMenuMasterHeader = (from MenuHeader in db.TblMenuMasterHeaders
                                   join RoleMatrix in db.TblMenuRoleMatrixAccesses on MenuHeader.HeaderViewID equals RoleMatrix.HeaderViewID
                                   where RoleMatrix.RoleID == RoleID
                                         && RoleMatrix.IsPageAccess == true
                                         && MenuHeader.Disabled == false
                                   select new MenuMasterHeader
                                   {
                                       HeaderViewID = MenuHeader.HeaderViewID,
                                       MenuName = MenuHeader.MenuName,
                                       IconCls = MenuHeader.IconCls,
                                       OrderByTab = MenuHeader.OrderByTab
                                   }).Distinct().OrderBy(x => x.OrderByTab).ToList();

            return ObjMenuMasterHeader;
        }
        public List<MenuMasterDetail> MenuMasterDetail(string HeaderViewID, string RoleID)
        {
            List<MenuMasterDetail> ObjMenuMasterHeader = null;

            ObjMenuMasterHeader = (from MenuDetail in db.TblMenuMasterDetails
                                   join RoleMatrix in db.TblMenuRoleMatrixAccesses on MenuDetail.DetailViewID equals RoleMatrix.DetailViewID
                                   where MenuDetail.HeaderViewID == HeaderViewID
                                   && RoleMatrix.HeaderViewID == HeaderViewID
                                   && RoleMatrix.RoleID == RoleID
                                   && RoleMatrix.IsPageAccess == true
                                   && MenuDetail.Disabled == false
                                   select new MenuMasterDetail
                                   {
                                       DetailViewID = MenuDetail.DetailViewID,
                                       HeaderViewID = MenuDetail.HeaderViewID,
                                       MenuName = MenuDetail.MenuName,
                                       PageUrl = MenuDetail.PageUrl,
                                       IconCls = MenuDetail.IconCls,
                                       OrderByTab = MenuDetail.OrderByTab
                                   }).Distinct().OrderBy(x => x.OrderByTab).ToList();

            return ObjMenuMasterHeader;
        }
        #endregion

        public List<HomePage> Load_HomePage()
        {
            List<HomePage> ObjHomePageList = new List<HomePage>();
            ObjHomePageList.Add(new HomePage { HomePageViewID = "", HomePageName = PROMessage.Homepage_SelectHomepage });

            try
            {
                var ObjHomeList = (from HP in db.TblHomePages
                                   where HP.Status == true
                                   select new HomePage
                                   {
                                       HomePageViewID = HP.HomePageViewID,
                                       HomePageName = HP.HomePageName
                                   }).Distinct().OrderBy(x => x.HomePageName).ToList();
                ObjHomePageList.AddRange(ObjHomeList);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_HomePage");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_HomePage");
            }
            return ObjHomePageList;
        }

        #region "Error Master"
        public bool LogPageError(System.Data.Entity.Validation.DbEntityValidationException exe, string str_pagename)
        {
            bool IsError = true;
            string str = "";
            foreach (var eve in exe.EntityValidationErrors)
            {
                str = ("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:" +
                eve.Entry.Entity.GetType().Name + "==" + eve.Entry.State);
                foreach (var ve in eve.ValidationErrors)
                {
                    str = ("- Property: \"{0}\", Error: \"{1}\"" +
                    ve.PropertyName + "==" + ve.ErrorMessage);
                }
            }
            DateTime now = CommonRepository.GetTimeZoneDate();
            string str_path = "";
            StreamWriter obj_sw = null;
            try
            {
                str_path = System.Web.HttpContext.Current.Server.MapPath("~/ErrorLog") + "/Log " + now.ToString(Transport.Model.CommonDateFormat.StringDateonlyFormat) + ".txt";
                if (System.IO.File.Exists(str_path))
                {
                    obj_sw = new StreamWriter(str_path, true);
                }
                else
                {
                    obj_sw = System.IO.File.CreateText(str_path);
                }

                if (str_pagename != "")
                    obj_sw.WriteLine("Page Name          :" + str_pagename);
                obj_sw.WriteLine("Date and Time      :" + now.ToString("dd MMM yyyy h:mm tt"));
                obj_sw.WriteLine("Error Message      :" + str);
                obj_sw.WriteLine();
                obj_sw.WriteLine("----------------------------------------------------------------");
                obj_sw.Close();
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
                IsError = false;
            }
            finally
            {
                if (obj_sw != null)
                {
                    obj_sw = null;
                }
            }

            return IsError;
        }

        public bool GlobalError(Exception obj_exception, string str_pagename)
        {
            bool IsError = true;
            DateTime now = CommonRepository.GetTimeZoneDate();
            string str_path = "";
            StreamWriter obj_sw = null;
            try
            {
                str_path = System.Web.HttpContext.Current.Server.MapPath("~/ErrorLog") + "/Log " + now.ToString(Transport.Model.CommonDateFormat.StringDateonlyFormat) + ".txt";
                if (System.IO.File.Exists(str_path))
                {
                    obj_sw = new StreamWriter(str_path, true);
                }
                else
                {
                    obj_sw = System.IO.File.CreateText(str_path);
                }

                if (str_pagename != "")
                    obj_sw.WriteLine("Page Name          :" + str_pagename);
                obj_sw.WriteLine("Date and Time      :" + now.ToString("dd MMM yyyy h:mm tt"));
                obj_sw.WriteLine("Error Message      :" + obj_exception.Message);
                obj_sw.WriteLine("Stack Trace        :" + obj_exception.StackTrace);
                obj_sw.WriteLine("Error Source       :" + obj_exception.Source);
                obj_sw.WriteLine("Target Site        :" + obj_exception.TargetSite);
                obj_sw.WriteLine();
                obj_sw.WriteLine("----------------------------------------------------------------");
                obj_sw.Close();
                //return RedirectToAction("Index", "Login");
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            { IsError = false; }
            finally
            {
                if (obj_sw != null)
                {
                    obj_sw = null;
                }
            }

            return IsError;
        }
        #endregion

        #region "Common Functions"
        public Decimal RoundDown(Decimal number)
        {
            string pr = number.ToString();
            string[] parts = null;
            parts = pr.Split('.');
            Decimal Dec_Amount = Convert.ToDecimal(parts[0] + ".00");
            if (parts.Count() != 1)
            {
                Decimal Dec_Point = Convert.ToDecimal("0." + parts[1]);
                if (Convert.ToDecimal(0.5) <= Dec_Point)
                {
                    Dec_Amount += 1;
                }
            }
            return Dec_Amount;
        }

        public List<LoadItems_Int> Load_Status()
        {
            List<LoadItems_Int> ObjLoadList = new List<LoadItems_Int>();
            try
            {
                ObjLoadList.Add(new LoadItems_Int { Name = DropDownListSelectValue.DropDownListSelect_Status, ID = -1 });
                ObjLoadList.Add(new LoadItems_Int { Name = WEBCONSTANTMESSAGE.ACTIVE, ID = 1 });
                ObjLoadList.Add(new LoadItems_Int { Name = WEBCONSTANTMESSAGE.INACTIVE, ID = 0 });

            }
            catch (Exception e)
            {
                GlobalError(e, "Load_Status");
            }
            return ObjLoadList;
        }

        public List<LoadItems_BigInt> Load_JobVendors()
        {
            List<LoadItems_BigInt> ObjLoadList = new List<LoadItems_BigInt>();
            ObjLoadList.Add(new LoadItems_BigInt { ID = 0, Name = "Please Select Job Vendor" });
            try
            {
                var ObjList = (from EM in db.JobVendors
                               where EM.Status == true
                               select new LoadItems_BigInt
                               {
                                   ID = EM.JobVendorCode,
                                   Name = EM.JobVendorName
                               }).Distinct().OrderBy(S => S.ID).ToList();

                ObjLoadList.AddRange(ObjList);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_JobVendors");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_JobVendors");
            }
            return ObjLoadList;
        }

        public List<LoadItems_BigInt> Load_Vehicles()
        {
            List<LoadItems_BigInt> ObjLoadList = new List<LoadItems_BigInt>();
            ObjLoadList.Add(new LoadItems_BigInt { ID = 0, Name = "Please Select Vehicle" });
            try
            {
                var ObjList = (from EM in db.Vehicles
                               where EM.Status == true
                               select new LoadItems_BigInt
                               {
                                   ID = EM.VehicleCode,
                                   Name = EM.VehicleName
                               }).Distinct().OrderBy(S => S.ID).ToList();

                ObjLoadList.AddRange(ObjList);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_Vehicles");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_Vehicles");
            }
            return ObjLoadList;
        }
        public List<LoadItems_BigInt> Load_ServiceTypes()
        {
            List<LoadItems_BigInt> ObjLoadList = new List<LoadItems_BigInt>();
            ObjLoadList.Add(new LoadItems_BigInt { ID = 0, Name = "Please Select Service" });
            try
            {
                var ObjList = (from EM in db.ServiceTypes
                               where EM.Status == true
                               select new LoadItems_BigInt
                               {
                                   ID = EM.ServiceTypeCode,
                                   Name = EM.ServiceTypeName
                               }).Distinct().OrderBy(S => S.ID).ToList();

                ObjLoadList.AddRange(ObjList);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_ServiceTypes");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_ServiceTypes");
            }
            return ObjLoadList;
        }

        public List<LoadItems_BigInt> Load_ServiceTypesStaff()
        {
            List<LoadItems_BigInt> ObjLoadList = new List<LoadItems_BigInt>();
            ObjLoadList.Add(new LoadItems_BigInt { ID = 0, Name = "Please Select Service" });
            try
            {
                var ObjList = (from EM in db.ServiceTypes
                               where EM.Status == true
                               select new LoadItems_BigInt
                               {
                                   ID = EM.ServiceTypeCode,
                                   Name = EM.ServiceTypeName
                               }).Where(o => o.ID == 2 || o.ID == 13).Distinct().OrderBy(S => S.ID).ToList();

                ObjLoadList.AddRange(ObjList);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_ServiceTypes");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_ServiceTypes");
            }
            return ObjLoadList;
        }

        public List<LoadItems_BigInt> Load_SupplierTypes()
        {
            List<LoadItems_BigInt> ObjLoadList = new List<LoadItems_BigInt>();
            ObjLoadList.Add(new LoadItems_BigInt { ID = 0, Name = "Please Select Supplier" });
            try
            {
                var ObjList = (from EM in db.SupplierTypes
                               where EM.Status == true
                               select new LoadItems_BigInt
                               {
                                   ID = EM.SupplierTypeCode,
                                   Name = EM.SupplierTypeName
                               }).Distinct().OrderBy(S => S.ID).ToList();

                ObjLoadList.AddRange(ObjList);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_ServiceTypes");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_ServiceTypes");
            }
            return ObjLoadList;
        }
        

        public List<LoadItems_BigInt> Load_SystemUserDetail()
        {
            List<LoadItems_BigInt> ObjLoadList = new List<LoadItems_BigInt>();
            ObjLoadList.Add(new LoadItems_BigInt { ID = 0, Name = @SYSMessage.UserMaster_SelectUser });
            try
            {
                var ObjList = (from UM in db.TblUserMasters
                               where UM.Status == true
                               select new LoadItems_BigInt
                               {
                                   ID = UM.UserID,
                                   Name = UM.FirstName + " " + UM.LastName
                               }).ToList();


                ObjLoadList.AddRange(ObjList);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_SystemUserDetail");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_SystemUserDetail");
            }
            return ObjLoadList;
        }

        public List<LoadItems_String> Load_UserRole()
        {
            List<LoadItems_String> ObjUserRoleList = new List<LoadItems_String>();
            ObjUserRoleList.Add(new LoadItems_String { ID = "", Name = DropDownListSelectValue.DropDownListSelect_Role });
            try
            {
                var Obj = (from RM in db.TblRoleMasters
                           where RM.status == true
                           select new LoadItems_String
                           {
                               ID = RM.RoleID,
                               Name = RM.RoleName
                           }).Distinct().OrderBy(x => x.Name).ToList();

                ObjUserRoleList.AddRange(Obj);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_UserRole");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_UserRole");
            }
            return ObjUserRoleList;
        }

        public List<ProfileMasterModel> Load_ProfileInformationOnUserID(int UserID = 0)
        {
            List<ProfileMasterModel> ObjProfile = new List<ProfileMasterModel>();
            try
            {
                var Obj = (from UM in db.TblUserMasters
                           where UM.Status == true && UM.UserID == UserID
                           select new ProfileMasterModel
                           {
                               UserID = UM.UserID,
                               FirstName = UM.FirstName,
                               LastName = UM.LastName,
                               Password = UM.Password,
                               EmailID = UM.EmailID,
                               MobileNumber = UM.MobileNumber,
                           }).ToList();
                ObjProfile.AddRange(Obj);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_ProfileInformationOnUserID");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_ProfileInformationOnUserID");
            }
            return ObjProfile;
        }

        public static string RandomString(int size)
        {
            Random _rng = new Random();
            string _chars = "AB1CD2EF3GH4IJ5KL6MN7OP8QR9STU0VWXYZ";
            char[] buffer = new char[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }

        public string EncryptString(string Password)
        {
            byte[] iv = new byte[16];
            byte[] array;
            var key = "b14ca5898a4e4133bbce2ea2315a1916";

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(Password);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public string DecryptPassword(string Password)
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(Password);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static DateTime GetTimeZoneDate()
        {
            if (null != HttpContext.Current.Session["TIME_ZONE"])
            {
                if (Convert.ToString(HttpContext.Current.Session["TIME_ZONE"]) != "")
                {
                    DateTime datetimeFromBD = System.DateTime.Now;
                    TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById(Convert.ToString(HttpContext.Current.Session["TIME_ZONE"]));
                    return TimeZoneInfo.ConvertTime(datetimeFromBD, TimeZoneInfo.Local, timeInfo);
                }
                else
                {
                    return System.DateTime.Now;
                }
            }
            else
            {
                return System.DateTime.Now;
            }

        }

        public string GetFileSize(long Bytes)
        {
            if (Bytes >= 1073741824)
            {
                Decimal size = Decimal.Divide(Bytes, 1073741824);
                return String.Format("{0:##.##} GB", size);
            }
            else if (Bytes >= 1048576)
            {
                Decimal size = Decimal.Divide(Bytes, 1048576);
                return String.Format("{0:##.##} MB", size);
            }
            else if (Bytes >= 1024)
            {
                Decimal size = Decimal.Divide(Bytes, 1024);
                return String.Format("{0:##.##} KB", size);
            }
            else if (Bytes > 0 & Bytes < 1024)
            {
                Decimal size = Bytes;
                return String.Format("{0:##.##} Bytes", size);
            }
            else
            {
                return "0 Bytes";
            }
        }

        #endregion

        #region "Login Master"
        public List<SessionLoginDetail> GetLoginAccess(String LoginName, String Password)
        {

            List<SessionLoginDetail> ObjLoginSession = new List<SessionLoginDetail>();

            var EncrypPassword = EncryptString(Password);

            ObjLoginSession = (from User in db.TblUserMasters
                               join RoleMaster in db.TblRoleMasters on User.RoleID equals RoleMaster.RoleID
                               where User.LoginName == LoginName && User.Status == true
                               && RoleMaster.status == true
                               && User.Password == EncrypPassword && User.PasswordAttemptCount <= 3
                               select new SessionLoginDetail
                               {
                                   UserID = User.UserID,
                                   RoleID = User.RoleID,                                  
                                   UserName = User.FirstName + " " + User.LastName,
                                   RoleName = RoleMaster.RoleName,
                                   IsPOS = RoleMaster.IsPOS,
                                   ControllerName = (from m in db.TblHomePages where m.HomePageViewID == RoleMaster.HomePageViewID select m.ControllerName).FirstOrDefault(),
                                   ActionName = (from m in db.TblHomePages where m.HomePageViewID == RoleMaster.HomePageViewID select m.ActionName).FirstOrDefault(),

                               }).ToList();

            return ObjLoginSession;
        }

        public int GetPasswordAttemptCount(String LoginName)
        {
            int PasswordAttemptCount = 0;
            try
            {
                var ObjPasswordAttemptCount = (from User in db.TblUserMasters
                                               join RoleMaster in db.TblRoleMasters on User.RoleID equals RoleMaster.RoleID
                                               where User.LoginName == LoginName && User.Status == true
                                               && RoleMaster.status == true
                                               select User).ToList();

                if (ObjPasswordAttemptCount.Count > 0)
                {
                    PasswordAttemptCount = ObjPasswordAttemptCount[0].PasswordAttemptCount + 1;

                    ObjPasswordAttemptCount[0].PasswordAttemptCount = PasswordAttemptCount;
                    ObjPasswordAttemptCount[0].LastModifiedDate = CommonRepository.GetTimeZoneDate();
                    db.SaveChanges();
                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "GetPasswordAttemptCount");
            }
            catch (Exception e)
            {
                GlobalError(e, "GetPasswordAttemptCount");
            }
            return PasswordAttemptCount;
        }

        public SessionInvalidLogin IsRoleMapInvalidPassword(String LoginName, String Password)
        {

            SessionInvalidLogin ObjInvalidLogin = new SessionInvalidLogin();
            try
            {
                var UserID = (from User in db.TblUserMasters
                              where User.LoginName == LoginName && User.Status == true
                              select User.UserID
            ).FirstOrDefault();

                var EncrypPassword = EncryptString(Password);

                var InvalidPassword = (from User in db.TblUserMasters
                                       where User.UserID == UserID && User.Status == true
                                         && User.Password == EncrypPassword
                                       select User.UserID
                ).FirstOrDefault();

                if (InvalidPassword == 0)
                {
                    ObjInvalidLogin.UserID = UserID;
                    ObjInvalidLogin.IsRoleMapping = true;
                    ObjInvalidLogin.InValidPassword = true;
                }
                else
                {
                    ObjInvalidLogin.UserID = UserID;
                }

            }

            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "IsRoleMapInvalidPassword");
            }
            catch (Exception e)
            {
                GlobalError(e, "IsRoleMapInvalidPassword");
            }
            return ObjInvalidLogin;
        }
        #endregion        

        #region "Default Master"
        public List<DefaultTypeItems> Load_CommonDefaultDetails(string DefaultTypeHeaderID)
        {
            List<DefaultTypeItems> ObjList = new List<DefaultTypeItems>();

            var DropdownDefault = Add_DropdownDefault(DefaultTypeHeaderID);

            ObjList.Add(new DefaultTypeItems { DefaultID = "", DefaultName = DropdownDefault, DefaultOrder = 0 });

            try
            {
                var ObjDefault = (from DMD in db.TblDefaultMasterDetails
                                  where
                                  DMD.Status == true &&
                                  DMD.DefaultHeaderID == DefaultTypeHeaderID
                                  select new DefaultTypeItems
                                  {
                                      DefaultID = DMD.DefaultDetailID,
                                      DefaultName = DMD.DefaultTextField,
                                      DefaultValue = DMD.DefaultValueField,
                                      DefaultOrder = DMD.DefaultOrder
                                  }).Distinct().OrderBy(s => s.DefaultOrder).ToList();

                ObjList.AddRange(ObjDefault);


            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_DefaultType");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_DefaultType");
            }
            return ObjList;
        }

        public List<DefaultHeaderNameItems> Load_CommonDefaultHeaderDetails()
        {
            List<DefaultHeaderNameItems> ObjList = new List<DefaultHeaderNameItems>();


            ObjList.Add(new DefaultHeaderNameItems { DefaultID = "", DefaultName = PROMessage.Drp_DefaultHeaderMaster_DefaultType });

            try
            {
                var ObjDefault = (from DMD in db.TblDefaultMasterHeaders
                                  where DMD.SystemType == "Project" && DMD.IsImage != true && DMD.Status == true
                                  select new DefaultHeaderNameItems
                                  {
                                      DefaultID = DMD.DefaultHeaderID,
                                      DefaultName = DMD.DefaultHeaderName,
                                      DefaultOrder = DMD.DefaultOrder
                                  }).Distinct().OrderBy(s => s.DefaultOrder).ToList();

                ObjList.AddRange(ObjDefault);


            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_CommonDefaultHeaderDetails");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_CommonDefaultHeaderDetails");
            }
            return ObjList;
        }

        public string Add_DropdownDefault(string DefaultTypeHeaderID)
        {
            var DropdownDefault = "";

            return DropdownDefault;

        }

        public List<DefaultTypeItems> Load_DefaultType(string DefaultTypeHeaderID)
        {
            List<DefaultTypeItems> ObjList = new List<DefaultTypeItems>();

            var DropdownDefault = Add_DropdownDefault(DefaultTypeHeaderID);

            ObjList.Add(new DefaultTypeItems { DefaultID = "", DefaultName = DropdownDefault, DefaultOrder = 0 });

            try
            {
                var ObjDefault = (from DMD in db.TblDefaultMasterDetails
                                  where DMD.Status == true && DMD.DefaultHeaderID == DefaultTypeHeaderID
                                  select new DefaultTypeItems
                                  {
                                      DefaultID = DMD.DefaultDetailID,
                                      DefaultName = DMD.DefaultTextField,
                                      DefaultValue = DMD.DefaultValueField,
                                      DefaultOrder = DMD.DefaultOrder
                                  }).Distinct().OrderBy(s => s.DefaultName).ToList();

                ObjList.AddRange(ObjDefault);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_DefaultType");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_DefaultType");
            }
            return ObjList;
        }

        public List<DefaultTypeItems> Load_DefaultTypeSearch(string DefaultTypeHeaderID)
        {
            List<DefaultTypeItems> ObjList = new List<DefaultTypeItems>();

            var DropdownDefault = Add_DropdownDefault(DefaultTypeHeaderID);

            ObjList.Add(new DefaultTypeItems { DefaultID = "", DefaultName = DropdownDefault, DefaultOrder = 0 });

            try
            {
                var ObjDefault = (from DMD in db.TblDefaultMasterDetails
                                  where DMD.DefaultHeaderID == DefaultTypeHeaderID

                                  select new DefaultTypeItems
                                  {
                                      DefaultID = DMD.DefaultDetailID,
                                      DefaultName = DMD.DefaultTextField,
                                      DefaultOrder = DMD.DefaultOrder
                                  }).Distinct().OrderBy(s => s.DefaultName).ToList();

                ObjList.AddRange(ObjDefault);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_DefaultTypeSearch");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_DefaultTypeSearch");
            }
            return ObjList;
        }

        public List<DefaultTypeItems> Load_DefaultTypePage(string DefaultTypeHeaderID, bool IsPage)
        {
            List<DefaultTypeItems> ObjList = new List<DefaultTypeItems>();

            var DropdownDefault = Add_DropdownDefault(DefaultTypeHeaderID);

            ObjList.Add(new DefaultTypeItems { DefaultID = "", DefaultName = DropdownDefault, DefaultOrder = 0 });

            try
            {
                var ObjDefault = (from DMD in db.TblDefaultMasterDetails
                                  where DMD.DefaultHeaderID == DefaultTypeHeaderID
                                  && (DMD.IsPage == IsPage)

                                  select new DefaultTypeItems
                                  {
                                      DefaultID = DMD.DefaultDetailID,
                                      DefaultName = DMD.DefaultTextField,
                                      DefaultOrder = DMD.DefaultOrder
                                  }).Distinct().OrderBy(s => s.DefaultName).ToList();

                ObjList.AddRange(ObjDefault);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                LogPageError(e, "Load_DefaultTypePage");
            }
            catch (Exception e)
            {
                GlobalError(e, "Load_DefaultTypePage");
            }
            return ObjList;
        }

        #endregion

        #region "WhatsApp"        

        public string SendWhatsAppMessage(string ToNumber)
        {
            string ObjMessage = "";
            try
            {
                const string accountSid = "AC785d6aa23a057da8ba0bf3e0b8f50708";
                const string authToken = "1c28830ff79179f105a9b31639690ad2";
                TwilioClient.Init(accountSid, authToken);
                var messageOptions = new CreateMessageOptions(new PhoneNumber("+6585115494"));
                messageOptions.From = new PhoneNumber("+14155238886");
                messageOptions.Body = "Your appointment is coming up on July 21 at 3PM";
                
                var message = MessageResource.Create(messageOptions);

                ObjMessage = "Message Was Sent!";
            }

            catch (Exception e)
            {
                ObjMessage = WEBCONSTANTMESSAGE.UNAUTHINSERTUPDATE;
            }
            return ObjMessage;

        }

        public static string Base64Encode(string text)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        }
        #endregion

    }
}


