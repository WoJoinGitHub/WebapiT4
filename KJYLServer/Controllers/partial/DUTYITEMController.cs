using Helper.Out;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DTO;
using BLL;
using Newtonsoft.Json;
using Helper;

namespace KJYLServer.Controllers
{
    public partial class DUTYITEMController: ApiController
    {
        DUTYITEMService bll1 = new DUTYITEMService();
        BLL.DutyItemService bll = new DutyItemService();
        /// <summary>
        /// 根据所有二级菜单 所有
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("api/dutyitem/GetAll")]
       public async Task<IHttpActionResult> GetAll()
        {
            try
            {
                var  list= await bll1.GetDto();
                return this.JsonMy(new AjaxResultList<DutyItemDto> { Code = 1, Msg = "获取成功" ,Result=list});
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<DUTYITEM> { Code = 0, Msg = msg });
            }
        }
        // GET: api/DutyItem
        //类型获取
        public async System.Threading.Tasks.Task<IHttpActionResult> Get()
        {
            var list = await bll.selectlistAsync();
            return Json(list);
        }
        // [HttpGet]
        // [Route("api/DutyItem/GetAll")]
        ////获取所有信息
        // public async System.Threading.Tasks.Task<IHttpActionResult> GetAllAsync()
        // {
        //     var list =await  bll.selectlistAsync();
        //     return Json(list);
        // }
        // GET: api/DutyItem/5
        public async Task<IHttpActionResult> Get(int id,string fid)
        {

            var list =await  bll.SelectListWhAsync(id,fid);
            return Json(list, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/DutyItem
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody]DUTYITEM value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0,
                msg = "",

            };
            try
            {
                DUTYITEM model = await  bll.Add(value);
              
                if (model.DUTYITEM_ID.Length > 0)
                {
                    re.code = 1;
                    re.msg = "";
                }
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re);
        }
    }
}
