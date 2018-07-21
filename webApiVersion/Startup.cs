


[assembly: Microsoft.Owin.OwinStartup(typeof(webApiVersion.Startup))]

namespace webApiVersion
{
    using AutoMapper;    
    using Model;
    using global::Owin;
    using Microsoft.Web.Http.Routing;
    //using Swashbuckle.Application;
    using Swagger.Net.Application;
    using System.IO;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.Http.Routing;

    /// <summary>
    /// Represents the startup process for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures the application using the provided builder.
        /// </summary>
        /// <param name="builder">The current application builder.</param>
        public void Configuration(IAppBuilder builder)
        {
            //过滤 option请求 使用owin 中间件
            builder.UseMyApp();
            // we only need to change the default constraint resolver for services that want urls with versioning like: ~/v{version}/{controller}
            //添加版本号 默认 添加？api-version
            var constraintResolver = new DefaultInlineConstraintResolver() { ConstraintMap = { ["apiVersion"] = typeof(ApiVersionRouteConstraint) } };
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer(configuration);
            //configuration.SuppressDefaultHostAuthentication
          
            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning(o => o.ReportApiVersions = true);
            configuration.MapHttpAttributeRoutes(constraintResolver);
            //不添加此路由模板 无法识别 没有Rout 属性的方法
            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                
            );

            // add the versioned IApiExplorer and capture the strongly-typed implementation (e.g. VersionedApiExplorer vs IApiExplorer)
            // note: the specified format code will format the version as "'v'major[.minor][-status]"

            //只在调试环境下 启用swagger

            var apiExplorer = configuration.AddVersionedApiExplorer(o => o.GroupNameFormat = "'V'VVV");
            var thisAssembly = typeof(Startup).Assembly;
            configuration.EnableSwagger(
                            "{apiVersion}/swagger",
                            swagger =>
                            {
                                // build a swagger document and endpoint for each discovered API version
                                //swagger.SingleApiVersion("v1", "跨境医疗项目");
                                swagger.MultipleApiVersions(
                                    (apiDesc, targetApiVersion) => apiDesc.GetGroupName() == targetApiVersion,
                                    info =>
                                    {
                                        foreach (var group in apiExplorer.ApiDescriptions)
                                        {
                                            var description = "跨境医疗项目,API在线文档.";

                                            if (group.IsDeprecated)
                                            {
                                                description += " <br/><b>此 API 版本已弃用！</b>";
                                            }

                                            info.Version(group.Name, $"版本 {group.ApiVersion}")
                                                .Contact(c => c.Name("Shijun Liu"))
                                                .Description(description);
                                        }
                                    });
                                // add a custom operation filter which sets default values
                                //swagger.OperationFilter<SwaggerDefaultValues>();
                                swagger.BasicAuth("basic")
                                    .Description("基础授权");
#if DEBUG
                                //api注释
                                swagger.IncludeXmlComments(XmlCommentsFilePath);
#endif
                            })
                         .EnableSwaggerUi(swagger =>
                         {
                             swagger.DocumentTitle("跨境医疗项目");
                         });


            builder.UseWebApi(httpServer);
            //注册automap  只能注册一次
            //Mapper.Initialize(cfg => {
            //    cfg.CreateMap<ADMIN, AdminDto>();
            //    cfg.CreateMap<SCHOOL, SchoolDto>();
            //});
        }
        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = System.AppDomain.CurrentDomain.RelativeSearchPath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }
    }
}