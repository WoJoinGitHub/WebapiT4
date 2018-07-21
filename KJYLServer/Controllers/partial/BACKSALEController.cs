using Helper.Out;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using  BLL;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public partial class BACKSALEController: ApiController
    {
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("api/BACKSALE/GetOne")]
        public async Task<IHttpActionResult> GetOne(string id)
        {
            try
            {
                BACKSALE model = await bll.SelectOne(p => p.SALESDETAIL_ID == id);
                return this.JsonMy(new AjaxResult<BACKSALE> { Code = 1, Msg = "获取成功", Result = model });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<BACKSALE> { Code = 0, Msg = msg });

            }
        }

        /// <summary>
        /// 新增 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("api/BACKSALE/AddRelation")]
        public async Task<IHttpActionResult> AddRelation([FromBody]BACKSALE value)
        {
            try
            {
                bool f = await bll.AddRelation(value);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<BACKSALE> { Code = 1, Msg = "添加成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<BACKSALE> { Code = 0, Msg = "请求失败" });
                }

            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<BACKSALE> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
            [HttpPut]
            [Route("api/BACKSALE/UpdateRelation")]
        public async Task<IHttpActionResult> UpdateRelation([FromBody]BACKSALE value)
        {
            try
            {

                if (value.BACKSALE_CHECKSTATE == "需退保确认")
                {
                    //获取订单详情
                    SALESDETAILService Sbll = new SALESDETAILService();
                    var sealDetail = await Sbll.SelectOne(p => p.SALESDETAIL_ID == value.SALESDETAIL_ID);
                    var backModel = await bll.SelectOne(p => p.BACKSALE_ID == value.BACKSALE_ID);
                    //判断保期是否开始
                   var startTime= sealDetail.SALESDETAIL_STARTTIME;
                    var endTime= sealDetail.SALESDETAIL_ENDTIME;
                    var saleMoney = sealDetail.SALESDETAIL_MONEY ?? 0;
                    var addMoney = sealDetail.SALESDETAIL_ADDMONEY ?? 0;
                    var money = saleMoney + addMoney;
                    backModel.BACKSALE_TYPE = value.BACKSALE_TYPE;
                    backModel.BACKSALE_CHECKSTATE = value.BACKSALE_CHECKSTATE;
                    backModel.BACKSALE_CHECKMESSAGE = value.BACKSALE_CHECKMESSAGE;
                    backModel.BACKSALE_TIME = value.BACKSALE_TIME;
                    if (sealDetail.SALESDETAIL_MONEY != null)
                    {
                        if (startTime != null && endTime != null)
                        {
                           
                            value.BACKSALE_SEALMONEY = money;
                            //if(startTime)
                            //保期开始前退保：退还保费=保费*75%
                            if (backModel.BACKSALE_TYPE=="全款")
                            {
                                var payMoney = money ;
                                value.BACKSALE_MONEY = payMoney;
                            }
                            else
                            {
                                //存储时间统一为utc时间
                                if (backModel.BACKSALE_TIME != null)
                                {
                                    //startTime = startTime.Value.ToLocalTime();
                                    var timeSpan = startTime.Value.Subtract(backModel.BACKSALE_TIME.Value);
                                    //还未开始
                                    if (timeSpan.Days > 0)
                                    {
                                        var payMoney = money;
                                        value.BACKSALE_MONEY = payMoney*(decimal) 0.75;
                                    }
                                    else
                                    {
                                        //已过时间
                                        decimal goTime = (timeSpan.Days)*-1 + 1;
                                        //总时间
                                        decimal allTime = endTime.Value.Subtract(startTime.Value).Days + 1;
                                        var payMoney = money * (1 - goTime / allTime) * (decimal)0.65;
                                        value.BACKSALE_MONEY = Math.Round(payMoney, 2) ;
                                    }
                                }
                            }
                        }
                        else
                        {
                            value.BACKSALE_SEALMONEY = money;
                            value.BACKSALE_MONEY = 0;
                        }
                    }

                    //保期开始后：退还保费=保费*（1-(已经过保险天数/保险期间日数)）*65%
                }
                bool f = await bll.UpdateRelation(value);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<BACKSALE> { Code = 1, Msg = "" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<BACKSALE> { Code = 0, Msg = "请求失败" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<BACKSALE> { Code = 0, Msg = msg });
            }
        }
    }
}
