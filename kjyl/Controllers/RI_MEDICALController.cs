using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BLL;
using Newtonsoft.Json;
using Helper;
using Model;

namespace kjyl.Controllers
{
    public class RI_MEDICALController : ApiController
    {
        RI_MEDICALService bll = new RI_MEDICALService();
        // GET: api/RI_MEDICAL
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(int page,int pagesize,string type)
        {
            PageResult<dynamic> result = new PageResult<dynamic>
            {
                code = 0,
                msg = "获取失败",
                pagecount = 0
            };
           
            try
            {
                //string wh = "审核通过";
                //if (type != "提交")
                //{
                //    wh = "已上传账单";
                //}
                int total;
                var list = await bll.SelectPageListNewAsync<string>(p => p.RI_MEDICAL_STATE == type, p => p.REPORTINFORMATION_ID, 1, 10,  false);               
                result.pagecount = list.Item2;
                result.code = 1;
                result.msg = "成功";
                result.result = list.Item1.ToList();
            }
            catch (Exception e)
            {
                result.msg = e.Message;
               
            }  
            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // GET: api/RI_MEDICAL/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/RI_MEDICAL
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/RI_MEDICAL/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RI_MEDICAL/5
        public void Delete(int id)
        {
        }
    }
}
