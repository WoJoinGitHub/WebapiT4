using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BLL;
using Model;
using System.Dynamic;

namespace kjyl.Controllers
{
    public class LoginController : ApiController
    {
        //StaffService bll = new StaffService();
        //STAFF model = new STAFF();
        //// GET: api/Login
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/Login/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Login
        ///// <summary>
        ///// 登陆
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public IHttpActionResult Post([FromBody]LoginInput value)
        //{
           
        //    AjaxResult result = new AjaxResult
        //    {
        //        code = 0,
        //        msg = "登陆失败"
              

        //    };
        //    try
        //    {
        //        model = bll.SelectOne(value.username);
        //        if (model.PASSWORD == value.PassWord)
        //        {
        //            result.code = 1;
        //            result.msg = "登陆成功";
        //            dynamic expando = new
        //            {
        //                //UserType = model.ROLEGROUP_ID
        //                UserType = ""
        //            };                    

        //            result.result = new List<dynamic>
        //            {
        //               expando
        //            };
        //        }                

        //    }
        //    catch (Exception e)
        //    {
        //        result.msg = e.Message;               
        //    }
        //    return Json(result);          
           
        //}

        // PUT: api/Login/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Login/5
        public void Delete(int id)
        {
        }
    }
}
