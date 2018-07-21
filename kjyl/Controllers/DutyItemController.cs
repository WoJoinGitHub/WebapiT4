using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BLL;
using Newtonsoft.Json;
using Model;
using Helper;

namespace kjyl.Controllers
{
    /// <summary>
    /// 获取医疗保险责任项
    /// </summary>
    public class DutyItemController : ApiController
    {
        BLL.DutyItemService bll = new DutyItemService();
        // GET: api/DutyItem
        //类型获取
        public async System.Threading.Tasks.Task<IHttpActionResult> Get()
        {
          var list= await  bll.selectlistAsync();
            return Json(list);
        }
        [HttpGet]
        [Route("api/DutyItem/GetAll")]
       //获取所有信息
        public async System.Threading.Tasks.Task<IHttpActionResult> GetAllAsync()
        {
            var list =await  bll.selectlistAsync();
            return Json(list);
        }
        // GET: api/DutyItem/5
        public async Task<IHttpActionResult> Get(int id,string fid)
        {

            var list =await  bll.SelectListWhAsync(id,fid);
            return Json(list, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/DutyItem
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody]DUTYITEM value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0,
                msg = "",

            };
            try
            {
                DUTYITEM model = await  bll.Add(value);
              
                if (model.DUTYITEM_ID.Length > 0)
                {
                    re.code = 1;
                    re.msg = "";
                }
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re);
        }

        // PUT: api/DutyItem/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/DutyItem/5
        public void Delete(int id)
        {
        }
    }
}
