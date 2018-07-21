using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BLL;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public class LZ_SAM_INSURPRODDEFController : ApiController
    {
        PRODUCEService bll = new PRODUCEService();
        // GET: api/LZ_SAM_INSURPRODDEF
        public async Task<IHttpActionResult> Get()
        {
            var list =(await  bll.GetTop()).ToList();

            return Json(list);
        }        
    }
}
