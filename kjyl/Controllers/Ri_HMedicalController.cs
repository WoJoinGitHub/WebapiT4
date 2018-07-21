using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Model;
using Helper;
using BLL;
namespace kjyl.Controllers
{
    public class Ri_HMedicalController : ApiController
    {
        // GET: api/Ri_HMedical
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Ri_HMedical/5
        public string Get(int id)
        {
           
            return "value";
        }

        // POST: api/Ri_HMedical
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Ri_HMedical/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Ri_HMedical/5
        public void Delete(int id)
        {
        }
    }
}
