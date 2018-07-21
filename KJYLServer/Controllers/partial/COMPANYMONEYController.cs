using Helper.Out;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using DTO;
using LinqKit;

namespace KJYLServer.Controllers
{
    public partial class COMPANYMONEYController : ApiController
    {
        [HttpGet]
        [Route("api/COMPANYMONEY/List")]
        public async Task<IHttpActionResult> GetList(int pageIndex, int pageSize,string type,DateTime? stateTime,DateTime? endTime,string company)
        {
            try
            {
                Expression<Func<COMPANYMONEYDto, bool>> whereExpression = p => true;
                switch (type)
                {
                    case "公司销售服务费":
                        whereExpression =
                            whereExpression.And(p => p.Companymoney.COMPANYMONEY_SEVERSTATE == null||
                                                     p.Companymoney.COMPANYMONEY_SEVERSTATE ==""
                                                     );
                        break;
                    case "公司销售有效保单":
                        whereExpression =
                            whereExpression.And(p => 
                                                     p.Companymoney.COMPANYMONEY_COMAPNYSTATE == null ||
                                                     p.Companymoney.COMPANYMONEY_COMAPNYSTATE == ""
                            );
                        break;
                    case "公司销售佣金":
                        whereExpression =
                            whereExpression.And(p => p.Companymoney.COMPANYMONEY_COMMIISSIONSTATE == null ||
                                                     p.Companymoney.COMPANYMONEY_COMMIISSIONSTATE == ""
                            );
                        break;
                    case "公司保费支付确认":
                        whereExpression = whereExpression.And(p =>
                                                 p.Companymoney.COMPANYMONEY_COMAPNYSTATE == "已导出"
                                                );
                        break;
                    case "公司佣金确认":
                        whereExpression = whereExpression.And(p =>
                            p.Companymoney.COMPANYMONEY_COMMIISSIONSTATE == "已导出"
                        );
                        break;
                    case "公司服务费确认":
                        whereExpression = whereExpression.And(p =>
                            p.Companymoney.COMPANYMONEY_SEVERSTATE == "已导出"
                        );
                        break;
                }
                if (stateTime != null && endTime != null)
                {
                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                    {
                        ShortDatePattern = "yyyy/MM/dd"
                    };
                    DateTime stateSt = Convert.ToDateTime(stateTime, dtFormat).ToUniversalTime();
                    DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).ToUniversalTime().AddDays(1);
                    whereExpression = whereExpression.And(p => p.Companymoney.COMPANYMONEY_TIME <= stateEnd && p.Companymoney.COMPANYMONEY_TIME >= stateSt);
                }
                if (!string.IsNullOrEmpty(company))
                {
                    whereExpression = whereExpression.And(p => p.Salesdetail.SALESDETAIL_COMPANY == company);
                }
                Tuple<IQueryable<COMPANYMONEYDto>, int> getResult = await bll.GetDto(pageSize,pageIndex, whereExpression);
                return this.JsonMy(new AjaxResultList<COMPANYMONEYDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<COMPANYMONEY> { Code = 0, Msg = msg });
            }
        }
        [HttpPut]
        [Route("api/COMPANYMONEY/Center")]
        public async Task<IHttpActionResult> PutCenter([FromBody]List<CenterDto> idList)
        {
            try
            {
                var idall = idList.Select(p => p.Id).ToList();
                Tuple<IQueryable<COMPANYMONEYDto>, int> getResult = await bll.GetDto(10, -1, p=> idall.Contains(p.Companymoney.COMPANYMONEY_ID));
                var list = getResult.Item1.Select(p=>p.Companymoney).ToList();
                var type = idList[0].Type;
                foreach (var item in list)
                {
                    switch (type)
                    {
                       
                        case "公司保费支付确认":
                            item.COMPANYMONEY_COMAPNYSTATE = "已确认";
                          
                            break;
                        case "公司佣金确认":
                            item.COMPANYMONEY_COMMIISSIONSTATE = "已确认";
                            
                            break;
                        case "公司服务费确认":
                            item.COMPANYMONEY_SEVERSTATE = "已确认";
                            break;
                    }
                }
                bool f = await  bll.UpdataAll(list);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<object> {Code = 1, Msg = ""});
                }
                else
                {
                    return this.JsonMy(new AjaxResult<object> { Code = 0, Msg = "" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<COMPANYMONEY> { Code = 0, Msg = msg });
            }
        }
    }
}
