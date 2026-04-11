using Transport.Model;
using Transport.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Transport.Controllers
{
    [SessionExpire]
    [NoCacheAttribute]
    public class HomeController : CommonController
    {
        ICommonRepository ObjCommonRepository = new CommonRepository();
       
        //Added for Session Handling
        [HttpPost]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult
            {
                Data = "Beat Generated"
            };
        }

        // GET: Home
        public ActionResult AdminIndex()
        {           
            return View();
        }

        public ActionResult SalesIndex()
        {
            return View();
        }

       


    }
}