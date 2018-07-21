using BLL;
using Helper.Out;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace KJYLServer.Controllers
{
    /// <summary>
    /// 沟通记录
    /// </summary>
    public partial class RECORDController : ApiController
    {
        RECORDService bllPart = new RECORDService();
        /// <summary>
        /// 获取 渠道 客户沟通记录
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/RECORD/RecordByTypeGet")]
        public async Task<IHttpActionResult> RecordByTypeGet(string type,string id,int pageIndex, int pageSize)
        {
            try
            {
                //倒序查找
                Tuple<IQueryable<RECORD>, int> getResult = await bll.SelectPageListNew<string>(p => p.RECORD_TYPE==type && p.ID==id, p => p.RECORD_ID, pageIndex, pageSize,true);
                return this.JsonMy(new AjaxResultList<RECORD> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/record/RecordAdd")]
        public async Task<IHttpActionResult> RecordAdd([FromBody]RECORD value)
        {
            try
            {
               bool f= await bll.AddRelation(value);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<RECORD> { Code = 1, Msg = "添加成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = "请求失败" });
                }

            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = msg });
            }
        }
    }
}
