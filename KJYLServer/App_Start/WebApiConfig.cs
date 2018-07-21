
using KJYLServer.Filter;
using Swagger.Net.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace KJYLServer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
#if DEBUG
            config
     .EnableSwagger(c =>
     {
         c.SingleApiVersion("v1", "跨境医疗项目")
        .Description("A sample API for testing and prototyping")
                .TermsOfService("Some terms")
                .Contact(cc => cc
                    .Name("Some contact")
                    .Url("http://tempuri.org/contact")
                    .Email("some.contact@tempuri.org"))
                .License(lc => lc
                    .Name("Some License")
                    .Url("http://tempuri.org/license"));
         //var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
         //var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
         //var commentsFile = Path.Combine(baseDirectory, commentsFileName);
         c.BasicAuth("basic")
      .Description("基础授权");
         //c.OperationFilter<HttpBasicAuthorizeAttribute>();
         //c.SingleApiVersion("v1", "跨境医疗项目");
         //c.IncludeXmlComments(commentsFile);
         c.IncludeXmlComments(GetXmlCommentsPath());

     }).EnableSwaggerUi(c =>
     {
         c.DocumentTitle("跨境医疗项目");
         //c.EnableOAuth2Support("test-client-id", "test-realm", "Swagger UI");
     });
#endif

            // Web API 配置和服务

            // Web API 路由
            config.MapHttpAttributeRoutes();
            //自定义路由无法使用
            //config.Routes.MapHttpRoute(
            //       name: "DefaultVersion",
            //       routeTemplate: "api/{version}/{controller}/{id}",
            //       defaults: new { id = RouteParameter.Optional }
            //   );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //config.Services.Replace(typeof(IHttpControllerSelector), new VersionHttpControllerSelector((config)));
            //config.EnableSwagger(c => c.SingleApiVersion("v2", "A title for your API"))
            //   .EnableSwaggerUi();
        }
        static string GetXmlCommentsPath()
        {
            return $"{System.AppDomain.CurrentDomain.BaseDirectory}/bin/KJYLServer.xml";
        }
    }
}
