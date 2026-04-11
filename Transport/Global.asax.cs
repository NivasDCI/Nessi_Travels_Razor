using Transport.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Transport
{
    public class MvcApplication : System.Web.HttpApplication
    {
        CommonRepository CommonRepository = new CommonRepository();

        protected void Application_Start()
        {
           // AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes); 
        }
        void Application_Error(object sender, EventArgs e)
        {
            DateTime now = CommonRepository.GetTimeZoneDate();
            ICommonRepository ObjCommonError = new CommonRepository();
            Exception objErr = Server.GetLastError().GetBaseException();
            ObjCommonError.GlobalError(objErr, "Global Error");
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
       {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["Language"];

            if (cookie != null && cookie.Value != null)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cookie.Value);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cookie.Value);
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(WEBCONSTANTMESSAGE.MultiLanguage_English);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(WEBCONSTANTMESSAGE.MultiLanguage_English);               
            }
        }
    }
}
