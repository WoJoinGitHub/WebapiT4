using Helper.Out;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace KJYLServer.Controllers
{
    public partial class SCHOOL_MEDICALRESOURCEController 
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="SId"></param>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [Route("api/SCHOOL_MEDICALRESOURCE/DeleteOne")]
        public async Task<IHttpActionResult> DeleteOne(string  Id,string SId)
        {
            try
            {
                var model = await  bll.SelectOne(p => p.SCHOOL_MEDICALRESOURCE_SID == SId && p.SCHOOL_MEDICALRESOURCE_MID == Id);
                bool f = await  bll.Delete(model);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<SCHOOL_MEDICALRESOURCE> { Code = 1, Msg = "删除成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<SCHOOL_MEDICALRESOURCE> { Code = 0, Msg = "删除失败" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<SCHOOL_MEDICALRESOURCE> { Code = 0, Msg = msg });
            }
        }
    }
}
