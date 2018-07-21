using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Model;
using BLL;
using Helper;
using Newtonsoft.Json;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    /// <summary>
    /// 医疗报销产品利益表
    /// </summary>
       [HttpBasicAuthorize]
    public class Interest_ProduceController : ApiController
    {
        INTEREST_PRODUCEService bll = new INTEREST_PRODUCEService();
        // GET: api/Interest_Produce
        public IHttpActionResult Get(int page, int pagesize)
        {
            PageResult<dynamic> result = new PageResult<dynamic>()
            {
                code = 0,
                msg = "获取失败",
                pagecount = 0,
                result = { }

            };
            int total = 0;
            try
            {
                var list = bll.SelectDetail(page,pagesize,out total,true);
                result.pagecount = total;
                result.code = 1;
                result.msg = "成功";
                result.result = list.ToList();
            }
            catch (Exception e)
            {
                result.msg = e.Message;
            }
            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // GET: api/Interest_Produce/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = "获取失败",
                result = { }

            };
            try
            {

                var list = (await bll.SelectById(id)).ToList();
                result.result = list;
                result.code = 1;
                result.msg = "获取成功";
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
        /// <summary>
        /// 产品利益表添加
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST: api/Interest_Produce
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody]INTEREST_PRODUCE value)
        {

            AjaxResult result = new AjaxResult()
            {
                code = 0,
                msg = "提交失败",
                result = { }

            };


            //vmodel.IP_DUTY

            try
            {

                INTEREST_PRODUCE model = await bll.Add(value);
                if (model.INTEREST_PRODUCE_ID.Length > 0)
                {
                    result.code = 1;
                    result.msg = "提交成功";

                }
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result);
        }

        // PUT: api/Interest_Produce/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put([FromBody]INTEREST_PRODUCE value)
        {
            AjaxResult result = new AjaxResult()
            {
                code = 0,
                msg = "提交失败",
                result = { }

            };


            //vmodel.IP_DUTY

            try
            {
                
                INTEREST_PRODUCE model = await bll.SelectAsync(value.INTEREST_PRODUCE_ID);
                model.INTEREST_PRODUCE_ISSINGLE = value.INTEREST_PRODUCE_ISSINGLE;
                model.INTEREST_PRODUCE_MAXMONEY = value.INTEREST_PRODUCE_MAXMONEY;
                model.INTEREST_PRODUCE_MPN = value.INTEREST_PRODUCE_MPN;
                model.INTEREST_PRODUCE_MPW = value.INTEREST_PRODUCE_MPW;
                model.INTEREST_PRODUCE_NAME = value.INTEREST_PRODUCE_NAME;
                model.INTEREST_PRODUCE_ZFMAX = value.INTEREST_PRODUCE_ZFMAX;
                var id = value.INTEREST_PRODUCE_ID;
                List<IP_DUTY> IPDutyList = model.IP_DUTY.ToList();
                List<IP_DUTY> NIPDutyList = value.IP_DUTY.ToList();
                for (int i = 0; i < IPDutyList.Count; i++)
                {
                    var list = NIPDutyList.Where(p => p.IP_DUTY_ID == IPDutyList[i].IP_DUTY_ID).ToList();
                    if (list == null || list.Count == 0)
                    {
                        IPDutyList.RemoveAt(i);
                    }
                }

                for (int i = 0; i < NIPDutyList.Count; i++)
                {
                    var list = IPDutyList.FirstOrDefault(p => p.IP_DUTY_ID == NIPDutyList[i].IP_DUTY_ID);
                    if (list == null)
                    {
                        IPDutyList.Add(NIPDutyList[i]);
                    }
                    else
                    {
                        list.IP_DUTY_MAXMONEY = NIPDutyList[i].IP_DUTY_MAXMONEY;
                        List< IP_ITEM> ipitem = list.IP_ITEM.ToList();
                        List<IP_ITEM> nipitem = NIPDutyList[i].IP_ITEM.ToList();
                        for (int k = 0; k < ipitem.Count; k++)
                        {
                            var iplist = nipitem.Where(p => p.IP_ITEM_ID == ipitem[k].IP_ITEM_ID).ToList();
                            if (iplist == null || iplist.Count == 0)
                            {
                                ipitem.Remove(ipitem[k]);
                            }
                        }
                        for (int k = 0; k < nipitem.Count; k++)
                        {
                            var iplist = ipitem.FirstOrDefault(p => p.IP_ITEM_ID == nipitem[k].IP_ITEM_ID);
                            if (iplist == null)
                            {
                                ipitem.Add(nipitem[k]);
                            }
                            else
                            {
                                iplist.DUTYITEM_ID = nipitem[k].DUTYITEM_ID;                              
                                iplist.IP_ITEM_ISPER = nipitem[k].IP_ITEM_ISPER;
                                iplist.IP_ITEM_MAXDAY = nipitem[k].IP_ITEM_MAXDAY;
                                iplist.IP_ITEM_MAXM = nipitem[k].IP_ITEM_MAXM;
                                iplist.IP_ITEM_MAXMW = nipitem[k].IP_ITEM_MAXMW;
                                iplist.IP_ITEM_ZFBLN = nipitem[k].IP_ITEM_ZFBLN;
                                iplist.IP_ITEM_ZFBLW = nipitem[k].IP_ITEM_ZFBLW;
                                iplist.IP_ITEM_ZFEN = nipitem[k].IP_ITEM_ZFEN;
                                iplist.IP_ITEM_ZFEW = nipitem[k].IP_ITEM_ZFEW;
                              


                            }
                        }
                    }
                }


                bool f = await bll.Updata(model);
                if (f)
                {
                    result.code = 1;
                    result.msg = "提交成功";

                }
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result);
        }        
    }
}
