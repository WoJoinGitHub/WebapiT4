using System;
using System.Linq;
using System.Web.Http;
using BLL;
using Model;
using System.Threading.Tasks;
using Helper.Out;
using Helper.SqlHeper;
using DTO;
using AutoMapper;
using Helper;
using System.Linq.Expressions;
using LinqKit;

namespace KJYLServer.Controllers
{
    public partial class SCHOOLController
    {
        [HttpGet]
        [Route("api/school/GetDto")]
       public async Task<IHttpActionResult> GetDto(string id)
        {
            try
            {

                var model=await bll.GetDto(id);
                return Json(new AjaxResult<SchoolDto> { Code = 1, Msg = "获取成功", Result = model });
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
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/school/PostDto")]
        public async Task<IHttpActionResult> PostDto([FromBody] SchoolDto model)
        {
            try
            {
                bool f = await bll.PostDto(model);
                if (f)
                {
                    return Json(new AjaxResult<SCHOOL> { Code = 1, Msg = "新增成功" });
                }
                else
                {
                    return Json(new AjaxResult<SCHOOL> { Code = 1, Msg = "新增成功" });
                }
               
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
        /// <summary>
        /// 多条件查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/school/GetMultiple")]
        public async Task<IHttpActionResult> GetMultiple(string name, string address, int pageIndex, int pageSize)
        {
            try
            {
                Expression<Func<SCHOOL, bool>> where = p => p.SCHOOL_ID != "0";
                if (!string.IsNullOrEmpty(name) && name != "null")
                {
                    where = where.And(p => p.SCHOOL_NAME.Contains(name));
                }
                if (!string.IsNullOrEmpty(address) && address != "null")
                {
                    where = where.And(p => p.SCHOOL_ADDRESS.Contains(address));
                }
                Tuple<IQueryable<SCHOOL>, int> resut = await bll.SelectPageList<string>(where, p => p.SCHOOL_ID, pageIndex, pageSize);
                return Json(new AjaxResultList<SCHOOL> { Code = 1, Msg = "获取成功", ListCount = resut.Item2, Result = resut.Item1.ToList() });
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
        /// <summary>
        /// 修改 dto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpPut]
        [Route("api/school/PutDto")]
        public async Task<IHttpActionResult> PutDto([FromBody] SchoolDto model)
        {
            try
            {
                bool f = await  bll.PutDto(model);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<DOCTOR> { Code = 1, Msg = "修改成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<DOCTOR> { Code = 0, Msg = "修改失败" });
                }
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
