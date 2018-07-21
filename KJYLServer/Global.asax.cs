using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace KJYLServer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
        
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var req = System.Web.HttpContext.Current.Request;
            if (req.HttpMethod == "OPTIONS" ) //过滤options请求，用于js跨域
            {
                Response.StatusCode = 200;
                Response.SubStatusCode = 200;
                Response.End();
            }
            if (Context.Request.FilePath == "/") Context.RewritePath("index.html");

        }
    }
}
