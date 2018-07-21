using AutoMapper;
using BLL;
using DTO;
using Helper.Out;
using LinqKit;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace KJYLServer.Controllers
{
   
    /// <summary>
    /// 管理员表
    /// </summary>
    public partial class ADMINController : ApiController
    {
        /// <summary>
        /// 登录 暂用
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// [HttpBasicAuthorize]
        [AllowAnonymous]
        [HttpGet]
        [Route("api/admin/login")]
        public async Task<IHttpActionResult> Login(string name, string password)
        {
            try
            {               
               var model= await bll.SelectOne(p => p.ADMIN_NAME == name && p.ADMIN_PASSWORD == password);
                if (model != null)
                {
                    return this.Json(new AjaxResult<ADMIN> { Code = 1, Msg = "登录成功", Result = model });
                }
                return this.JsonMy(new AjaxResultList<ADMIN> { Code = 0, Msg = "登录失败" });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                 msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 获取所有销售员、主管
        /// </summary>
        /// <returns></returns>
    [HttpGet]
    [Route("api/admin/GetSealUserList")]
        public async Task<IHttpActionResult> GetSealUserList()
        {
            try
            {
                IQueryable<ADMIN> model = await  bll.SelectList(p => (p.ADMIN_ROLE == "销售员" || p.ADMIN_ROLE == "销售主管") && p.ADMIN_NAME!="admin");
                Mapper.Initialize(cfg => {
                    cfg.CreateMap<ADMIN, AdminDto>();                 
                });
                List<AdminDto> listDest = Mapper.Map<List<ADMIN>, List<AdminDto>>(model.ToList());               
                return this.JsonMy(new AjaxResultList<AdminDto> { Code = 1, Msg = "获取成功",Result= listDest });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="name"></param>
       /// <param name="pageIndex"></param>
       /// <param name="pageSize"></param>
       /// <returns></returns>
        [HttpGet]
        [Route("api/ADMIN/GetByName")]
        public async Task<IHttpActionResult> GetByName(string name,int pageIndex,int pageSize)
        {
            try
            {
                Expression<Func<ADMIN, bool>> where = p => p.ADMIN_ID != "0";
                if (!string.IsNullOrEmpty(name) && name != "null")
                {
                    where = where.And(p => p.ADMIN_NAME.Contains(name));
                }
                Tuple<IQueryable<ADMIN>,int> result = await bll.SelectPageList<string>(where, p=>p.ADMIN_ID,pageIndex,pageSize);              
                return this.JsonMy(new AjaxResultList<ADMIN> { Code = 1, Msg = "获取成功", Result = result.Item1.ToList(),ListCount=result.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpPut]
        [Route("api/ADMIN/PassWordRest")]
        public async Task<IHttpActionResult> PassWordRest([FromBody] ADMIN value)
        {
            try
            {
                var model = await  bll.SelectOne(p => p.ADMIN_ID == value.ADMIN_ID);
                model.ADMIN_PASSWORD = "123456";
                bool f = await bll.Updata(model);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 1, Msg = "修改成功"});
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 1, Msg = "修改失败" });
                }
              
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpPut]
        [Route("api/ADMIN/PasswordChange")]
        public async Task<IHttpActionResult> PasswordChange([FromBody]AdminPassWordDto value)
        {
            try
            {
                var model = await  bll.SelectOne(p => p.ADMIN_ID == value.ADMIN_ID);
                if (model.ADMIN_PASSWORD == value.ADMIN_PASSWORD)
                {
                    model.ADMIN_PASSWORD = value.ADMIN_NEWPASSWORD;
                    bool f = await  bll.Updata(model);
                    if (f)
                    {
                        return this.JsonMy(new AjaxResult<ADMIN> { Code = 1, Msg = "修改成功" });
                    }
                    else
                    {
                        return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "修改失败" });
                    }
                   
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "原密码错误" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
    }
    
}
