using DTO;
using Helper.Out;
using LinqKit;
using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Text;

namespace KJYLServer.Controllers
{
    public partial class CUSTOMController : ApiController
    {
       
        /// <summary>
        /// 获取某销售员的 客户
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/custom/GetUserList")]
        public async Task<IHttpActionResult> GetUserList(int pageIndex, int pageSize, string userId)
        {           
            try
            {

                Tuple<List<CustomDto>, int> getResult = await bll.GetDtoList(userId, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<CustomDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
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
        /// 获取 所有客户信息 dto
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/custom/GetAllUserList")]
        public async Task<IHttpActionResult> GetAllUserList(int pageIndex, int pageSize)
        {
            try
            {

                Tuple<List<CustomDto>, int> getResult = await bll.GetDtoListWhere(p=>p.ADMIN_ID!=null, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<CustomDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
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
        /// 根据 渠道id 获取列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/custom/GetByOrganizationId")]
        public async Task<IHttpActionResult> GetByOrganizationId(string id,int pageIndex,int pageSize)
        {
            try
            {
                Tuple<List<CustomDto>, int> getResult = await bll.GetDtoListWhere(p=>p.ORGANIZATION_ID==id, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<CustomDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
            }
            catch (Exception e)
            {

                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<CustomDto> { Code = 0, Msg = msg });
            }          
            
        }

        /// <summary>
        /// 获取客户信息 包含渠道名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/custom/GetDto")]
        public async Task<IHttpActionResult> GetDto(string id )
        {
            try
            {
                 CustomDto model = await bll.GetDtoOne(id);
                return this.JsonMy(new AjaxResult<CustomDto> { Code = 1, Msg = "获取成功", Result = model});
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<CustomDto> { Code = 0, Msg = msg });
            }

        }
        /// <summary>        
        /// 多条件搜索
        /// </summary>
        /// <param name="name"></param>
        /// <param name="school"></param>
        /// <param name="produce"></param>
        /// <param name="orgnization"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/custom/SelectUserList")]
        public async Task<IHttpActionResult> SelectUserList(string name, string school, string produce,string orgnization, string startTime, string endTime, int pageIndex, int pageSize, string userId)
        {            
            Expression<Func<CustomDto, bool>> where = p => p.CUSTOM_ID != "0";
            if (!string.IsNullOrEmpty(name) && name != "null")
            {
                where = where.And(p => p.CUSTOM_NAME.Contains(name));
            }
            if (!string.IsNullOrEmpty(school) && school != "null")
            {
                where = where.And(p => p.CUSTOM_SCHOOL.Contains(school));
            }
            if (!string.IsNullOrEmpty(produce) && produce != "null")
            {
                where = where.And(p => p.CUSTOM_PRODUCE.Contains(produce));
            }
            if (!string.IsNullOrEmpty(orgnization) && orgnization != "null")
            {
                where = where.And(p => p.ORGANIZATION_NAME.Contains(orgnization));
            }
            if (!string.IsNullOrEmpty(userId) && userId != "null")
            {
                where = where.And(p => p.ADMIN_ID == userId);
            }
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime) && startTime != "null" && endTime != "null" && startTime != "1970/1/1")
            {
                DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                {
                    ShortDatePattern = "yyyy/MM/dd"
                };
                DateTime stateSt = Convert.ToDateTime(startTime, dtFormat);
                DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).AddDays(1);
                where = where.And(p => p.CUSTOM_ADDTIME <= stateEnd && p.CUSTOM_ADDTIME >= stateSt);
                //where = where.And(p => EntityFunctions.DiffDays(p.CUSTOM_ADDTIME, stateEnd) <= 0 && EntityFunctions.DiffDays(p.CUSTOM_ADDTIME, stateSt) >= 0);
            }

            try
            {
                Tuple<List<CustomDto>, int> getResult = await bll.GetDtoListWhere(where,pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<CustomDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
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
        /// 获取待提醒的列表 三天未沟通客户(所有）
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
       [HttpGet]
       [Route("api/custom/GetRemainList")]
        public async Task<IHttpActionResult> GetRemainList(int pageIndex,int pageSize)
        {
            try
            {
                DateTime dt = DateTime.Now.AddDays(-3);
                Tuple<List<CustomDto>, int> getResult = await bll.GetDtoListWhere(p=>p.CUSTOM_ENDTIME< dt, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<CustomDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
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
        /// 获取待沟通提醒的 客户列表 指定用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/custom/GetRemainListByUser")]
        public async Task<IHttpActionResult> GetRemainListByUser(string  userId,int pageIndex, int pageSize)
        {
            try
            {
                DateTime dt = DateTime.Now.AddDays(-3);
                Tuple<List<CustomDto>, int> getResult = await bll.GetDtoListWhere(p => p.CUSTOM_ENDTIME < dt && p.ADMIN_ID== userId, pageIndex, pageSize);
                return this.JsonMy(new AjaxResultList<CustomDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
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
        /// 获取没有 机构的客户列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
     [HttpGet]
     [Route("api/custom/GetNoOrganzationList")]
        public async Task<IHttpActionResult> GetNoOrganzationList(int pageIndex,int PageSize)
        {
            try
            {               
                Tuple<List<CustomDto>, int> getResult = await bll.GetDtoAdmin(p => p.ORGANIZATION_ID==null|| p.ORGANIZATION_ID=="", pageIndex, PageSize);
                return this.JsonMy(new AjaxResultList<CustomDto> { Code = 1, Msg = "获取成功", Result = getResult.Item1.ToList(), ListCount = getResult.Item2 });
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
        /// 分配无渠道的客户
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/custom/ChangeCustomUser")]
        public async Task<IHttpActionResult> ChangeCustomUser([FromBody]AllocationDtoIn value)
        {
            try
            {
                List<CUSTOM> list = new List<CUSTOM>();
                string adminId = value.AdminId;
                for (int i = 0; i < value.List.Length; i++)
                {
                    string id = value.List[i];
                    CUSTOM model = await bll.SelectOne(p => p.CUSTOM_ID == id);
                    model.ADMIN_ID = adminId;
                    list.Add(model);
                }
                bool f = await bll.UpdataAll(list);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<CUSTOM> { Code = 1, Msg = "修改成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<CUSTOM> { Code = 0, Msg = "修改失败", });
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
        
    }
}
