	//------------------------------------------------------------------------------
//     此代码由T4模板自动生成
//       生成时间 2018-01-09 09:10:43 by ShiJun Liu
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
using KJYLServer.Filter;
namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public partial class RELATIONPEOPLEController : ApiController
    {
        RELATIONPEOPLEService bll = new RELATIONPEOPLEService();
        // GET: api/RELATIONPEOPLE
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
                Tuple<IQueryable<RELATIONPEOPLE>, int> getResult = await bll.SelectPageList<string>(p => true, p =>p.RELATIONPEOPLE_ID, pageIndex, pageSize);             
                return this.JsonMy(new AjaxResultList<RELATIONPEOPLE> { Code = 1, Msg = "获取成功" ,Result=getResult.Item1.ToList(),ListCount=getResult.Item2});
            }
            catch (Exception e)
            {
		        var msg="请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 0, Msg = msg });
            }
        }

        // GET: api/RELATIONPEOPLE/5
        public async Task<IHttpActionResult> Get(string id)
        {
            try
            {
                RELATIONPEOPLE model = await bll.SelectOne(p => p.RELATIONPEOPLE_ID == id);
                return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 1, Msg = "获取成功", Result = model });
            }
            catch (Exception e)
            {
		        var msg="请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 0, Msg = msg });

            }
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST: api/RELATIONPEOPLE
        public async Task<IHttpActionResult> Post([FromBody]RELATIONPEOPLE value)
        {
            try
            {
                RELATIONPEOPLE model =  await bll.Add(value);
                if (model.RELATIONPEOPLE_ID.Length>0)
                {
                    return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 1, Msg = "添加成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 0, Msg = "请求失败" });
                }

            }
            catch (Exception e)
            {
		        var msg="请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT: api/RELATIONPEOPLE
        public async Task<IHttpActionResult> Put([FromBody]RELATIONPEOPLE value)
        {
            try
            {
                SqlUpdate<RELATIONPEOPLE> sql = new SqlUpdate<RELATIONPEOPLE>();
                string sqlStr = sql.GetUpdateSql(value, value.RELATIONPEOPLE_ID.ToString());               
                bool f= await bll.SqlGet(sqlStr);
				if(f){
                return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 1, Msg = "" });
				}
				else{
				 return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 0, Msg = "请求失败" });
				}
            }
            catch (Exception e)
            {
		        var msg="请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 0, Msg = msg });
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/Default/5
        public async Task<IHttpActionResult> Delete(string id)
        {
            try
            {

                RELATIONPEOPLE model = await bll.SelectOne(p=>p.RELATIONPEOPLE_ID==id);
                bool f = await bll.Delete(model);
                if (f)
                {
                    return this.JsonMy(new AjaxResult< RELATIONPEOPLE> { Code = 1, Msg = "删除成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult< RELATIONPEOPLE> { Code = 0, Msg = "删除失败" });
                }

            }
            catch (Exception e)
            {
                var msg = "删除失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RELATIONPEOPLE> { Code = 0, Msg = msg });
            }
        }
    }
	
} 