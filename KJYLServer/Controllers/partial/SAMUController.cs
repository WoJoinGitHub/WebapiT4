using System;
using System.Linq;
using System.Web.Http;
using BLL;
using Model;
using System.Threading.Tasks;
using Helper.Out;
using Helper.SqlHeper;
using System.Linq.Expressions;
using LinqKit;

namespace KJYLServer.Controllers
{
    public partial class SAMUController
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
        [Route("api/SAMU/GetMultiple")]
        public async Task<IHttpActionResult> GetMultiple(string name, string address, int pageIndex, int pageSize)
        {
            try
            {
                Expression<Func<SAMU, bool>> where = p => p.SAMU_ID != "0";
                if (!string.IsNullOrEmpty(name) && name != "null")
                {
                    where = where.And(p => p.SAMU_NAME.Contains(name));
                }
                if (!string.IsNullOrEmpty(address) && address != "null")
                {
                    where = where.And(p => p.SAMU_ADDRESS.Contains(address));
                }
                Tuple<IQueryable<SAMU>, int> resut = await bll.SelectPageList<string>(where, p => p.SAMU_ID, pageIndex, pageSize);
                return Json(new AjaxResultList<SAMU> { Code = 1, Msg = "获取成功", ListCount = resut.Item2, Result = resut.Item1.ToList() });
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
