using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Model;
using Helper;
using System.Threading.Tasks;
using BLL;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public class RecommendHospitalController : ApiController
    {
        RecommendHospitalService bll = new RecommendHospitalService();
        // GET: api/RecommendHospital
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        /// <summary>
        /// 根据销售单号获取所有推荐医院
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/RecommendHospital/5
        public async Task<IHttpActionResult> Get(string id)
        {
            
            AjaxResultT<RECOMMENDHOSPITAL> re = new AjaxResultT< RECOMMENDHOSPITAL>()
            {
                code = 0,
                msg = ""
            };
            try
            {
                List<RECOMMENDHOSPITAL> model = new List<RECOMMENDHOSPITAL>();
                model = (await  bll.SelectList(id)).ToList();
                re.code = 1;
                re.msg = "获取成功";
                re.result = model;
            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re);
        }

        // POST: api/RecommendHospital
        public async Task<IHttpActionResult> Post([FromBody]List<RECOMMENDHOSPITAL> value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0,
                msg = ""

            };
            try
            {
                List<RECOMMENDHOSPITAL> model = new List<RECOMMENDHOSPITAL>();
                model = await  bll.AddAll(value);
                if (model.Count > 0)
                {
                    re.code = 1;
                    re.msg = "添加成功";
                }
              
                 
            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re);
        }

        // PUT: api/RecommendHospital/5
        public IHttpActionResult Put([FromBody] List<RECOMMENDHOSPITAL> value)
        { 

            AjaxResult re = new AjaxResult()
            {
                code = 0,
                msg = ""

            };
            try
            {
                List<RECOMMENDHOSPITAL> model = new List<RECOMMENDHOSPITAL>();
                bool f = bll.Changer(value);
                if (f)
                {
                    re.code = 1;
                    re.msg = "添加成功";
                }


            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re);

        }        
    }
}
