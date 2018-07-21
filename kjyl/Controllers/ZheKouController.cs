using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BLL;
using Model;
namespace kjyl.Controllers
{
    public class ZheKouController : ApiController
    {
        RESULTService bll = new RESULTService();
        // GET: api/ZheKou
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ZheKou/5
        //public IHttpActionResult Get(int paybl,int userbl,string id)
        //{
        //    //RESULT model = bll.SelectOne(id);
        //    //model.RESULT_USERMONEY = model.RESULT_USERMONEY * userbl;
        //    //model.RESULT_PAYMONEY
        //}

        // POST: api/ZheKou
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ZheKou/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ZheKou/5
        public void Delete(int id)
        {
        }
    }
}
