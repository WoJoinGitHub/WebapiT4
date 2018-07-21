using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Model;
using Helper;
using BLL;
using Newtonsoft.Json;

namespace kjyl.Controllers
{
    /// <summary>
    /// 其他保险责任保险利益表
    /// </summary>
    public class Interest_ProduceOtherController : ApiController
    {
        INTEREST_PRODUCEOTHERService bll = new INTEREST_PRODUCEOTHERService();
        // GET: api/Interest_ProduceOther
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(int pagesize, int page)
        {
            PageResult<INTEREST_PRODUCEOTHER> re = new PageResult<INTEREST_PRODUCEOTHER>()
            {
                code = 0,
                msg = "",
                pagecount = 0
            };
            try
            {
                var list = await  bll.SelectDetailAsync(page, pagesize,true);
                re.result = list.Item1.ToList();
                re.pagecount = list.Item2;
                re.code = 1;
                re.msg = "获取成功";
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // GET: api/Interest_ProduceOther/5
        public IHttpActionResult Get(string id)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0,
                msg = ""
            };
            try
            {
                var model = bll.SelectOne(id);
                List<dynamic> list = new List<dynamic>();
                list.Add(model);
                re.code = 1;
                re.msg = "获取成功";
                re.result = list;
                
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/Interest_ProduceOther
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody]INTEREST_PRODUCEOTHER value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0,
                msg = ""
            };
            try
            {
                var model = await  bll.Add(value);
                if (model.IPOTHER_ID.Length > 0)
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

        // PUT: api/Interest_ProduceOther/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put( [FromBody]INTEREST_PRODUCEOTHER value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0,
                msg = ""
            };
            try
            {
                var model = await bll.SelectOne(value.IPOTHER_ID);
                model.IPOTHER_MONEY = value.IPOTHER_MONEY;
                model.IPOTHER_NAME = value.IPOTHER_NAME;
                var list = model.IPOTHER_ITEM;
                var listVale = value.IPOTHER_ITEM;
                int i = 0;
                foreach (var item in list)
                {
                    item.IPOTHER_ITEM_MONEY = listVale.ElementAt(i).IPOTHER_ITEM_MONEY;
                    item.IPOTHER_ITEM_NAME = listVale.ElementAt(i).IPOTHER_ITEM_NAME;
                    item.IPOTHER_ITEM_ZFBL = listVale.ElementAt(i).IPOTHER_ITEM_ZFBL;
                    item.IPOTHER_ITEM_ZFMONEY = listVale.ElementAt(i).IPOTHER_ITEM_ZFMONEY;
                }
                bool f = await bll.Updata(model);
                if (f)
                {
                    re.msg = "成功";
                    re.code = 1;
                }


            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }
            return Json(re);
        }

        // DELETE: api/Interest_ProduceOther/5
        public void Delete(int id)
        {
        }
    }
}
