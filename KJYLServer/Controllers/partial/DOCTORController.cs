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
using Helper.Out;

namespace KJYLServer.Controllers
{
    public partial class DOCTORController : ApiController
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
        [Route("api/DOCTOR/GetMultiple")]
        public async Task<IHttpActionResult> GetMultiple(string name, string address, int pageIndex, int pageSize)
        {
            try
            {
                Expression<Func<DOCTOR, bool>> where = p => p.DOCTOR_ID != "0";
                if (!string.IsNullOrEmpty(name) && name != "null")
                {
                    where = where.And(p => p.DOCTOR_NAME.Contains(name));
                }
                if (!string.IsNullOrEmpty(address) && address != "null")
                {
                    where = where.And(p => p.DOCTOR_ADDRESS.Contains(address));
                }
                Tuple<IQueryable<DOCTOR>, int> resut = await bll.SelectPageList<string>(where, p => p.DOCTOR_ID, pageIndex, pageSize);
                return Json(new AjaxResultList<DOCTOR> { Code = 1, Msg = "获取成功", ListCount = resut.Item2, Result = resut.Item1.ToList() });
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
