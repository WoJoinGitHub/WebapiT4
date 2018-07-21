using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using Model;
using BLL;
namespace KJYLServer.Filter
{
   
    /// <summary>
    /// HttpBasic验证连接器
    /// </summary>
    public class HttpBasicAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {

       
    public override async void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            //是否有 AllowAnonymousAttribute 有则不进行验证
            bool flag = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0 ||
                        actionContext.ActionDescriptor.ControllerDescriptor
                            .GetCustomAttributes<AllowAnonymousAttribute>().Count > 0;
            if (flag)
            {
                return;
            }
            //try
            //{
            if (actionContext.Request.Headers.Authorization != null)
                {
                    string userInfo = Encoding.UTF8.GetString(Convert.FromBase64String(actionContext.Request.Headers.Authorization.Parameter));
                    
                    //获取用户名 秘密
                    string[] user = userInfo.Split(':');
                    ADMINService admin=new ADMINService();
                    var name = user[0];
                    var model=await  admin.SelectOne(p => p.ADMIN_NAME == name);
                    if (model.ADMIN_PASSWORD == user[1])
                    {
                        IsAuthorized(actionContext);
                    }
                    else
                    {
                        HandleUnauthorizedRequest(actionContext);
                    }
                }
                else
                {
                    HandleUnauthorizedRequest(actionContext);
                }
            //}
            //catch (Exception e)
            //{
            //    HandleUnauthorizedRequest(actionContext);
            //}
        
        }
        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            return false;
        }
        //protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
        //{
        //    //if (actionContext.Request.Headers.Referrer.Authority!=actionContext.Request.Headers.Host)
        //    //{
        //    //    base.HandleUnauthorizedRequest(actionContext);
        //    //}
        //    //else
        //    //{
        //    //    var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        //    //    challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
        //    //    throw new System.Web.Http.HttpResponseException(challengeMessage);
        //    //}
        //    var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        //    challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
        //    throw new System.Web.Http.HttpResponseException(challengeMessage);

        //}
    }
}