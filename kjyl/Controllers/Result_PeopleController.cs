using BLL;
using Helper;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace kjyl.Controllers
{
    public class Result_PeopleController : ApiController
    {
        // GET: api/Result_People
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Result_People/5
        [HttpGet]
        [Route("api/Result_People/People")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id, decimal huilv)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {
                BillService bll = new BillService();
                INTEREST_ORDERService iobll = new INTEREST_ORDERService();
                RESULTService resultbBill = new RESULTService();               
                RESULT remodle = await resultbBill.Select(p => p.BILL.RI_MEDICAL_ID == id && p.RESULT_STATE == "折扣完成");
                re.msg = Math.Round(remodle.RESULT_PAYMONEY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                re.code = 1;

            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }
            return Json(re);
        }

        // POST: api/Result_People
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Result_People/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Result_People/5
        public void Delete(int id)
        {
        }
    }
}
