using Helper.Out;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Aspose.Words.Lists;
using DTO;
using LinqKit;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{  
    public partial class BROKERAGEController : ApiController
    {
        /// <summary>
        /// 列表 按照投保单展示
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        /// <returns></returns>
        // GET: api/BROKERAGE
        [HttpGet]
        [Route("api/BROKERAGE/list")]
        public async Task<IHttpActionResult> GetList(string type,DateTime? timeStart, DateTime? timeEnd)
        {
            try
            {
                Expression<Func<SealDetailInputDto, bool>> wherExpression = null;
                if (timeStart != null && timeEnd != null)
                {
                        DateTime startTime = timeStart.Value;
                        DateTime endTime = timeEnd.Value;
                        DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                        {
                            ShortDatePattern = "yyyy/MM/dd"
                        };
                        DateTime stateSt = Convert.ToDateTime(startTime, dtFormat);
                        DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).AddDays(1);
                      wherExpression=(p => p.SALESDETAIL.SALESDETAIL_SALESTIME <= stateEnd && p.SALESDETAIL.SALESDETAIL_SALESTIME >= stateSt);
                }
                var getResult = await bll.GetList(type, wherExpression);
                return this.JsonMy(new AjaxResultList<SealDetailInputDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<object> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 列表 按照机构名称展示
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/BROKERAGE/OrganzationList")]
        public async Task<IHttpActionResult> GetOrganzationList(string type, DateTime? timeStart, DateTime? timeEnd)
        {
            try
            {
                Expression<Func<BROKERAGEDto, bool>> wherExpression =p=>p.BROKERAGE_STATE== "需结算";
                if (timeStart != null && timeEnd != null)
                {
                    DateTime startTime = timeStart.Value;
                    DateTime endTime = timeEnd.Value;
                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                    {
                        ShortDatePattern = "yyyy/MM/dd"
                    };
                    DateTime stateSt = Convert.ToDateTime(startTime, dtFormat);
                    DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).AddDays(1);
                    wherExpression = wherExpression.And(p => p.SALESDETAIL_SALESTIME <= stateEnd && p.SALESDETAIL_SALESTIME >= stateSt);
                }
                var getResult = await bll.GetExcelList(wherExpression);
                return this.JsonMy(new AjaxResultList<BROKERAGEDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<object> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 需要退保 处理的 渠道佣金列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/BROKERAGE/BackBrokerageList")]
        public async Task<IHttpActionResult> GetBackBrokerageList()
        {
            try
            {
                var getResult = await bll.GetGetBackBrokerageList();
                return this.JsonMy(new AjaxResultList<BROKERAGEDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<object> { Code = 0, Msg = msg });
            }

        }
        /// <summary>
        /// 批量确认
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/BROKERAGE/UpdateAll")]
        public async Task<IHttpActionResult> UpdateAll(List<string> idList)
        {
            try
            {
                var list = (await bll.SelectList(p => idList.Contains(p.BROKERAGE_ID))).ToList();
                foreach (var item in list)
                {
                    item.BROKERAGE_STATE = "已结算";
                }
                bool f = await  bll.UpdataAll(list);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<object> {Code = 1, Msg = "更新成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<object> { Code = 0, Msg = "更新失败" });
                }
               
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<object> { Code = 0, Msg = msg });
            }
        }
    }
}
