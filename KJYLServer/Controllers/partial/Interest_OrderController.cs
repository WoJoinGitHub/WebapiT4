 using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Helper;
using BLL;
using Model;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public class Interest_OrderController : ApiController
    {
             
        [HttpGet]
        [Route("api/Interest_Order/liyi")]
        // GET: api/Interest_Order/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id, string pno)
        {
            INTEREST_ORDERService bill = new INTEREST_ORDERService();
            AjaxResult re = new AjaxResult
            {
                code = 0,
                msg = "获取失败"

            };
            try
            {
              //  RI_MEDICALService rimole = new RI_MEDICALService();
              //var riModel=  rimole.SelectOne(id);
               var model= (await bill.SelectPart(id)).ToList();
                if (model.Count == 0)
                {
                    INTEREST_PRODUCEService probill = new INTEREST_PRODUCEService();
                    var model2 = (await probill.SelectByName(pno)).ToList();
                    re.result = model2;
                }
                else{
                    re.result = model;
                }               
                re.code = 1;
                re.msg = "获取成功";
                
            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re);
        }       
    }
}
