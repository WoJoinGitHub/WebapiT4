using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DTO;
using BLL;
using System.Threading.Tasks;
using Helper.Out;
using Model;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public partial class BenifitController : ApiController
    {

        BenifitService bll = new BenifitService();
        // GET: api/INTEREST_PRODUCE/5
        /// <summary>
        /// 获取某个产品的利益表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Benifit/GetAll")]
        public async  Task<IHttpActionResult> GetAll(string id)
        {
            try
            {
              var model=await  bll.GetDto(id);
              return this.JsonMy(new AjaxResult<BenifitDto> { Code = 1, Msg = "成功",Result=model});
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
        /// 新增某个产品的利益表
        /// </summary>
        /// <param name="value"></param>
        // POST: api/INTEREST_PRODUCE
        [HttpPost]
        [Route("api/Benifit/PostNew")]
        public async Task<IHttpActionResult> PostNew([FromBody]BenifitDto value)
        {
            try
            {
                bool f = await bll.AddDto(value);

                if (f)
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 1, Msg = "添加成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "添加失败" });
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

        // PUT: api/INTEREST_PRODUCE/5
        /// <summary>
        /// 修改某个产品利益表
        /// </summary>
        /// <param name="value"></param>
        [HttpPut]
        [Route("api/Benifit/PutChange")]
        public async Task<IHttpActionResult> PutChange([FromBody]BenifitDto value)
        {
            try
            {
                bool f = await bll.UpdataAsync(value);

                if (f)
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 1, Msg = "修改成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "修改失败" });
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
