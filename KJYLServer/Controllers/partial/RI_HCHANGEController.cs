using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BLL;
using Helper;
using Model;
using Newtonsoft.Json;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public class RI_HCHANGEController : ApiController
    {
        RI_HCHANGEService bll = new RI_HCHANGEService();        
        // GET: api/RI_HCHANGE/5
        public async Task<IHttpActionResult> Get(string id)
        {
            AjaxResultT<RI_HCHANGE> re = new AjaxResultT<RI_HCHANGE>
            {
                code = 0
            };
            try
            {
                var model = await bll.SelectOne(id);
                var model2 = new RI_HCHANGE
                {
                    REPORTINFORMATION_ID = model.REPORTINFORMATION_ID,
                    RI_HCHANGE_ADDRESS = model.RI_HCHANGE_ADDRESS,
                    RI_HCHANGE_ADDRESSDETAIL = model.RI_HCHANGE_ADDRESSDETAIL,
                    RI_HCHANGE_CDETAIL = model.RI_HCHANGE_CDETAIL,
                    RI_HCHANGE_DETAIL = model.RI_HCHANGE_DETAIL,
                    RI_HCHANGE_DOC = model.RI_HCHANGE_DOC,
                    RI_HCHANGE_HADDRESS = model.RI_HCHANGE_HADDRESS,
                    RI_HCHANGE_HEMAIL = model.RI_HCHANGE_HEMAIL,
                    RI_HCHANGE_HNAME = model.RI_HCHANGE_HNAME,
                    RI_HCHANGE_NADDRESS = model.RI_HCHANGE_NADDRESS,
                    RI_HCHANGE_HTEL = model.RI_HCHANGE_HTEL,
                    RI_HCHANGE_NEMAIL = model.RI_HCHANGE_NEMAIL,
                    RI_HCHANGE_ID = model.RI_HCHANGE_ID,
                    RI_HCHANGE_TIME = model.RI_HCHANGE_TIME,
                    RI_HCHANGE_NNAME=model.RI_HCHANGE_NNAME,
                    RI_HCHANGE_NTEL=model.RI_HCHANGE_NTEL
                   
                };
                //REPORTINFORMATION recomm = new REPORTINFORMATION();
                //List<BILL_HCHANGE> bill = new List<BILL_HCHANGE>();
                //model.REPORTINFORMATION = recomm;
                //model.BILL_HCHANGE = bill;
                List<RI_HCHANGE> list = new List<RI_HCHANGE>
                {
                    model2
                };
               
                re.result = list;
                re.code = 1;
                re.msg = "获取成功";
            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }       
    }
}
