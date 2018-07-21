using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BLL;
namespace kjyl.Controllers
{
    public class LZ_SAM_INSURPRODDEFController : ApiController
    {
        PRODUCEService bll = new PRODUCEService();
        // GET: api/LZ_SAM_INSURPRODDEF
        public async Task<IHttpActionResult> Get()
        {
            var list =(await  bll.GetTop()).ToList();

            return Json(list);
        }

        // GET: api/LZ_SAM_INSURPRODDEF/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/LZ_SAM_INSURPRODDEF
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/LZ_SAM_INSURPRODDEF/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/LZ_SAM_INSURPRODDEF/5
        public void Delete(int id)
        {
        }
    }
}
