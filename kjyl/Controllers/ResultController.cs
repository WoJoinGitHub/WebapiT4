using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace kjyl.Controllers
{
    public class ResultController : ApiController
    {
        // GET: api/Result
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Result/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Result
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Result/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Result/5
        public void Delete(int id)
        {
        }
    }
}
