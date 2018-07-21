
  


//------------------------------------------------------------------------------
//     此代码由T4模板自动生成
//       生成时间 2018-07-21 11:23:23 by ShiJun Liu
//     对此文件的更改可能会导致不正确的行为，并且如果重新生成代码，这些更改将会丢失。
//     如需更改 请使用部分类
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Http;
using BLL;
using Model;
using System.Threading.Tasks;
using Helper.Out;
using Helper.SqlHeper;
using webApiVersion.Filter;
using Microsoft.Web.Http;
namespace webApiVersion.V2.Controllers
{
    [ApiVersion("2.0")]
    [RoutePrefix("api/v{api-version:apiVersion}/Admin")]
    public partial class AdminController : ApiController
    {
        AdminService bll = new AdminService();
        // GET: api/Admin
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> Get(int pageIndex, int pageSize)
        {
            try
            {
                Tuple<IQueryable<Admin>, int> getResult = await bll.SelectPageList<string>(p => true, p => p.Id.ToString(), pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<Admin> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = msg });
            }
        }
		 /// <summary>
        /// 获取单个
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Admin/5
        public async Task<IHttpActionResult> Get(int id)
        {
            try
            {
                Admin model = await bll.SelectOne(p => p.Id == id);
                return this.JsonMy(new AjaxResult<Admin> { Code = 1, Msg = "获取成功", Result = model });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = msg });

            }
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST: api/Admin
        public async Task<IHttpActionResult> Post([FromBody]Admin value)
        {
            try
            {
                Admin model = await bll.Add(value);
                if (model.Id > 0)
                {
                    return this.JsonMy(new AjaxResult<Admin> { Code = 1, Msg = "添加成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = "请求失败" });
                }

            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT: api/Admin
        public async Task<IHttpActionResult> Put([FromBody]Admin value)
        {
            try
            {
                SqlUpdate<Admin> sql = new SqlUpdate<Admin>();
                string sqlStr = sql.GetUpdateSql(value, value.Id.ToString());
                bool f = await bll.SqlGet(sqlStr);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<Admin> { Code = 1, Msg = "" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = "请求失败" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = msg });
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/Default/5
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {

                Admin model = await bll.SelectOne(p => p.Id == id);
                bool f = await bll.Delete(model);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<Admin> { Code = 1, Msg = "删除成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = "删除失败" });
                }

            }
            catch (Exception e)
            {
                var msg = "删除失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<Admin> { Code = 0, Msg = msg });
            }
        }
    }
	
}
 