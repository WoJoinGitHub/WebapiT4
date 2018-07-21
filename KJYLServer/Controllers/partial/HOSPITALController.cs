using Helper.Out;
using LinqKit;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace KJYLServer.Controllers
{
    public partial class HOSPITALController : ApiController
    {
        /// <summary>
        /// 多条件查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/HOSPITAL/GetMultiple")]
        public async Task<IHttpActionResult> GetMultiple(string name, string address, int pageIndex, int pageSize)
        {
            try
            {
                Expression<Func<HOSPITAL, bool>> where = p => p.HOSPITAL_ID != "0";
                if (!string.IsNullOrEmpty(name) && name != "null")
                {
                    where = where.And(p => p.HOSPITAL_NAME.Contains(name));
                }
                if (!string.IsNullOrEmpty(address) && address != "null")
                {
                    where = where.And(p => p.HOSPITAL_ADDRESS.Contains(address));
                }
                Tuple<IQueryable<HOSPITAL>, int> resut = await bll.SelectPageList<string>(where, p => p.HOSPITAL_ID, pageIndex, pageSize);
                return Json(new AjaxResultList<HOSPITAL> { Code = 1, Msg = "获取成功", ListCount = resut.Item2, Result = resut.Item1.ToList() });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<DOCTOR> { Code = 0, Msg = msg });
            }
        }
    }
}
