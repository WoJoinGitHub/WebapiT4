using BLL;
using DTO;
using Helper.Out;
using KJYLServer.Filter;
using LinqKit;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace KJYLServer.Controllers
{    
    /// <summary>
    /// 渠道
    /// </summary>
    public partial class ORGANIZATIONController : ApiController
    {
        ORGANIZATIONService bllP = new ORGANIZATIONService();
        /// <summary>
        /// 获取某销售员的 渠道
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ORGANIZATION/GetUserList")]
        public async Task<IHttpActionResult> GetUserList(int pageIndex, int pageSize, string userId)
        {
            try
            {

                Tuple<List<OrganizationDto>, int> getResult = await bll.GetAdminDtoListWhere(p => p.ADMIN_ID == userId, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<OrganizationDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ORGANIZATION> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 多条件搜索
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="people"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ORGANIZATION/SelectUserList")]
        public async Task<IHttpActionResult> SelectUserList(string name, string address, string people, string startTime, string endTime, int pageIndex, int pageSize, string userId)
        {          
            Expression<Func<OrganizationDto, bool>> where = p => p.ORGANIZATION_ID != "0";
            if (!string.IsNullOrEmpty(name) && name != "null")
            {
                where = where.And(p => p.ORGANIZATION_NAME.Contains(name));
            }
            if (!string.IsNullOrEmpty(address) && address != "null")
            {
                where = where.And(p => p.ORGANIZATION_ADDRESS.Contains(address));
            }
            if (!string.IsNullOrEmpty(people) && people != "null")
            {
                where = where.And(p => p.ORGANIZATION_CONTACTS.Contains(people));
            }
            if (!string.IsNullOrEmpty(userId) && userId != "null")
            {
                where = where.And(p => p.ADMIN_ID == userId);
            }
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime) && startTime != "null" && endTime != "null" && startTime!= "1970/1/1")
            {
                DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                {
                    ShortDatePattern = "yyyy/MM/dd"
                };
                DateTime stateSt = Convert.ToDateTime(startTime, dtFormat);
                DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).AddDays(1);
                where = where.And(p => p.ORGANIZATION_TIME <= stateEnd && p.ORGANIZATION_TIME >= stateSt);
            }

            try
            {
                Tuple<List<OrganizationDto>, int> getResult = await bll.GetAdminDtoListWhere(where, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<OrganizationDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ORGANIZATION> { Code = 0, Msg = msg });
            }
        }
       /// <summary>
       /// 包含 推荐人姓名的Model 接口
       /// </summary>
       /// <returns></returns>
       [HttpGet]
       [Route("api/ORGANIZATION/GetRelationModel")]
        public async Task<IHttpActionResult> GetRelationModel(string id)
        {
            try
            {
                ORGANIZATION model1 = await bll.SelectOne(p => p.ORGANIZATION_ID == id);
                if(!string.IsNullOrEmpty(model1.ORGANIZATION_REFERRER) && model1.ORGANIZATION_REFERRER != "0")
                {
                    OrganizationDto model = await bll.GetDtoOne(id);
                    return this.JsonMy(new AjaxResult<OrganizationDto> { Code = 1, Msg = "获取成功", Result = model });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ORGANIZATION> { Code = 1, Msg = "获取成功", Result = model1 });
                }
              
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<OrganizationDto> { Code = 0, Msg = msg });

            }            
        }
        /// <summary>
        /// 获取待提醒的列表 七天未沟通渠道(所有）
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ORGANIZATION/GetRemainList")]
        public async Task<IHttpActionResult> GetRemainList(int pageIndex, int pageSize)
        {
            try
            {
                DateTime dt = DateTime.Now.AddDays(-7);
                Tuple<List<OrganizationDto>, int> getResult = await bll.GetAdminDtoListWhere(p => p.ORGANIZATION_ENDTIME < dt, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<OrganizationDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<CUSTOM> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 获取待沟通提醒的 渠道列表 指定用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ORGANIZATION/GetRemainListByUser")]
        public async Task<IHttpActionResult> GetRemainListByUser(string userId, int pageIndex, int pageSize)
        {
            try
            {
                DateTime dt = DateTime.Now.AddDays(-7);
                Tuple<List<OrganizationDto>, int> getResult = await bll.GetAdminDtoListWhere(p => p.ORGANIZATION_ENDTIME < dt && p.ADMIN_ID==userId, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<OrganizationDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<CUSTOM> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 分配渠道
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/ORGANIZATION/ChangeOrganizationUser")]
        public async Task<IHttpActionResult> ChangeOrganizationUser([FromBody]AllocationDtoIn value)
        {
            try
            {
                List<ORGANIZATION> list = new List<ORGANIZATION>();
                string adminId = value.AdminId;
                for (int i = 0; i < value.List.Length; i++)
                {
                    string id = value.List[i];
                    ORGANIZATION model = await bll.SelectOne(p => p.ORGANIZATION_ID == id);
                    model.ADMIN_ID = adminId;
                    list.Add(model);
                }
                bool f= await bll.UpdataAll(list);
                if (f)
                {                  
                    return this.JsonMy(new AjaxResult<OrganizationDto> { Code = 1, Msg = "修改成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ORGANIZATION> { Code =0, Msg = "修改失败", });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<CUSTOM> { Code = 0, Msg = msg });
            }
        }

        /// <summary>
        /// 获取 包含销售员信息的列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ORGANIZATION/GetOrganizationHaveAdmin")]
        public async Task<IHttpActionResult> GetOrganizationHaveAdmin(int pageIndex, int pageSize)
        {
            try
            {
                Tuple<List< OrganizationDto>,int> list = await bll.GetAdminDtoList(pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<OrganizationDto> { Code = 1, Msg = "修改成功",ListCount=list.Item2,Result=list.Item1 });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<CUSTOM> { Code = 0, Msg = msg });
            }
        }
    }
}
