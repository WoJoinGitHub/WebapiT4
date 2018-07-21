using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;

namespace webApiVersion
{
    /// <summary>
    /// 这个类是为AppBuilder添加一个名叫UseMyApp的扩展方法
    /// </summary>
    public static class MyExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseMyApp(this IAppBuilder builder)
        {
            return builder.Use<MyMiddleware>();
            //UseXXX可以带多个参数，对应中间件构造函数中的第2、3、....参数;
        }
    }
}