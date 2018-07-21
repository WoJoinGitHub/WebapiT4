using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;

namespace webApiVersion
{
    public class MyMiddleware : OwinMiddleware
    {

        /// <summary>
        /// 构造函数，第一个参数必须为 OwinMiddleware对象 ;第一个参数是固定的，后边还可以添加自定义的其它参数
        /// </summary>
        /// <param name="next">下一个中间件</param>
        public MyMiddleware(OwinMiddleware next)
            : base(next)
        {

        }
        /// <summary>
        /// 处理用户请求的具体方法，该方法是必须的
        /// </summary>
        /// <param name="c">OwinContext对象</param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext c)
        {
            if (c.Request.Method == "OPTIONS")
            {
                c.Response.StatusCode = 200;
                return Task.FromResult<int>(0);
            }
            return Next.Invoke(c);
        }
    }
}