using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

public class SessionExpire : ActionFilterAttribute
    {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {


        if (HttpContext.Current.Session["USER_ID"] == null)
        {
            // FormsAuthentication.SignOut();
            filterContext.Result =
           new RedirectToRouteResult(new RouteValueDictionary
            {
             { "action", "Index" },
            { "controller", "Login" }
               //{ "returnUrl", filterContext.HttpContext.Request.RawUrl}
           });

            return;
        }
    }

    public static int GetUserID()
    {
        if (null != HttpContext.Current.Session["USER_ID"])
            return Convert.ToInt32(HttpContext.Current.Session["USER_ID"]);
        else
            return 0;
    }
    
    public static string GetApplicationMode()
    {
        //if (null != System.Configuration.ConfigurationManager.AppSettings["ApplicationMode"])
        //    return System.Configuration.ConfigurationManager.AppSettings["ApplicationMode"].ToString();
        //else
            return "";
    } 
     

    public static string GetHomePageViewID()
    {
        if (null != HttpContext.Current.Session["HOME_PAGEViewID"])
            return Convert.ToString(HttpContext.Current.Session["HOME_PAGEViewID"]);
        else
            return "";
    }
    public static string GetRoleID()
    {
        if (null != HttpContext.Current.Session["ROLE_ID"])
            return Convert.ToString(HttpContext.Current.Session["ROLE_ID"]);
        else
            return "";
    }
    public static string GetStoreID()
    {
        if (null != HttpContext.Current.Session["STORE_ID"])
            return Convert.ToString(HttpContext.Current.Session["STORE_ID"]);
        else
            return "";
    }

    public static string GetIsPOS()
    {
        if (null != HttpContext.Current.Session["IsPOS"])
            return Convert.ToString(HttpContext.Current.Session["IsPOS"]);
        else
            return "";
    }



    public static string GetUserName()
    {
        if (null != HttpContext.Current.Session["USER_NAME"])
            return Convert.ToString(HttpContext.Current.Session["USER_NAME"]);
        else
            return "";
    }

    public static int GetRequestedBy()
    {
        if (null != HttpContext.Current.Session["RequestedBy"])
            return Convert.ToInt32(HttpContext.Current.Session["RequestedBy"]);
        else
            return 0;
    }

    public static DateTime? GetfromDate()
    {
        if (null != HttpContext.Current.Session["fromDate"])
            return Convert.ToDateTime(HttpContext.Current.Session["fromDate"]);
        else
            return null;
    }

    public static DateTime? GettoDate()
    {
        if (null != HttpContext.Current.Session["toDate"])
            return Convert.ToDateTime(HttpContext.Current.Session["toDate"]);
        else
            return null;
    }

}

public class NoCacheAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext filterContext)
    {
        if (filterContext == null) throw new ArgumentNullException("filterContext");

        var cache = GetCache(filterContext);

        cache.SetExpires(DateTime.UtcNow.AddDays(-1));
        cache.SetValidUntilExpires(false);
        cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        cache.SetCacheability(HttpCacheability.NoCache);
        cache.SetNoStore();

        base.OnResultExecuting(filterContext);
    }

    /// <summary>
    /// Get the reponse cache
    /// </summary>
    /// <param name="filterContext"></param>
    /// <returns></returns>
    protected virtual HttpCachePolicyBase GetCache(ResultExecutingContext filterContext)
    {
        return filterContext.HttpContext.Response.Cache;
    }
}
