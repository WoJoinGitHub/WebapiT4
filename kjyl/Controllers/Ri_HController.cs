using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BLL;
using Helper;
using Model;
namespace kjyl.Controllers
{
    /// <summary>
    /// 紧急救援 报案信息 总接口 用于理赔账单录入 要查看修改 报案信息
    /// </summary>
    public class Ri_HController : ApiController
    {
        
        [HttpGet]
        [Route("api/Ri_H")]
        // GET: api/Ri_H/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id, string type)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {


                RI_HACCOMPANYService haccompanBill = new RI_HACCOMPANYService();
                RI_HBACKService hbackBill = new RI_HBACKService();
                RI_HBURIALService hburialBill = new RI_HBURIALService();
                RI_HCHANGEService hchangBill = new RI_HCHANGEService();
                RI_HMEDIAService hmediaBill = new RI_HMEDIAService();
                RI_HVISITService hvisitBill = new RI_HVISITService();
                switch (type)
                {
                    case "紧急医疗":
                        RI_HMEDIA hmediaModel = new RI_HMEDIA();
                        var model = await hmediaBill.Select(p => p.RI_HMEDIA_ID == id);
                        hmediaModel.RI_HMEDIA_ID = model.RI_HMEDIA_ID;
                        hmediaModel.RI_HMEDIA_ADDRESS = model.RI_HMEDIA_ADDRESS;
                        hmediaModel.RI_HMEDIA_ADDRESSDETAIL = model.RI_HMEDIA_ADDRESSDETAIL;

                        hmediaModel.RI_HMEDIA_BILLDOC = model.RI_HMEDIA_BILLDOC;
                        hmediaModel.RI_HMEDIA_DETAIS = model.RI_HMEDIA_DETAIS;
                        hmediaModel.RI_HMEDIA_ISLZ = model.RI_HMEDIA_ISLZ;
                        hmediaModel.RI_HMEDIA_ISUS = model.RI_HMEDIA_ISUS;
                        hmediaModel.RI_HMEDIA_TIME = model.RI_HMEDIA_TIME;
                        hmediaModel.RI_HMEDIA_TYPE = model.RI_HMEDIA_TYPE;
                        hmediaModel.RI_HMEDIA_WHY = model.RI_HMEDIA_WHY;
                        re.result = new List<dynamic>
                   {
                       hmediaModel
                   };
                        break;
                    case "紧急转院":
                        RI_HCHANGE hchangModel = new RI_HCHANGE();
                        var model2 = await hchangBill.Select(p => p.RI_HCHANGE_ID == id);
                        hchangModel.RI_HCHANGE_ADDRESS = model2.RI_HCHANGE_ADDRESS;
                        hchangModel.RI_HCHANGE_ADDRESSDETAIL = model2.RI_HCHANGE_ADDRESSDETAIL;
                        hchangModel.RI_HCHANGE_BILLDOC = model2.RI_HCHANGE_BILLDOC;
                        hchangModel.RI_HCHANGE_CDETAIL = model2.RI_HCHANGE_CDETAIL;
                        hchangModel.RI_HCHANGE_DETAIL = model2.RI_HCHANGE_DETAIL;
                        hchangModel.RI_HCHANGE_DOC = model2.RI_HCHANGE_DOC;
                        hchangModel.RI_HCHANGE_HADDRESS = model2.RI_HCHANGE_HADDRESS;
                        hchangModel.RI_HCHANGE_HEMAIL = model2.RI_HCHANGE_HEMAIL;
                        hchangModel.RI_HCHANGE_HNAME = model2.RI_HCHANGE_HNAME;
                        hchangModel.RI_HCHANGE_HTEL = model2.RI_HCHANGE_HTEL;
                        hchangModel.RI_HCHANGE_ID = model2.RI_HCHANGE_ID;
                        hchangModel.RI_HCHANGE_TIME = model2.RI_HCHANGE_TIME;
                        hchangModel.RI_HCHANGE_ISUS = model2.RI_HCHANGE_ISUS;
                        hchangModel.RI_HCHANGE_WHY = model2.RI_HCHANGE_WHY;
                        re.result = new List<dynamic>
                   {
                       hchangModel
                   };
                        break;
                    case "紧急运返":
                        RI_HBACK hbackModel = new RI_HBACK();
                        var model3 = await hbackBill.Select(p => p.RI_HBACK_ID == id);
                        hbackModel.RI_HBACK_ADDRESS = model3.RI_HBACK_ADDRESS;
                        hbackModel.RI_HBACK_ADDRESSDETAIL = model3.RI_HBACK_ADDRESSDETAIL;
                        hbackModel.RI_HBACK_BILLDOC = model3.RI_HBACK_BILLDOC;
                        hbackModel.RI_HBACK_DETAILS = model3.RI_HBACK_DETAILS;
                        hbackModel.RI_HBACK_DOC = model3.RI_HBACK_DOC;
                        hbackModel.RI_HBACK_HADDRESS = model3.RI_HBACK_HADDRESS;
                        hbackModel.RI_HBACK_HEMAIL = model3.RI_HBACK_HEMAIL;
                        hbackModel.RI_HBACK_HNAME = model3.RI_HBACK_HNAME;
                        hbackModel.RI_HBACK_HTEL = model3.RI_HBACK_HTEL;
                        hbackModel.RI_HBACK_ISUS = model3.RI_HBACK_ISUS;
                        hbackModel.RI_HBACK_TIME = model3.RI_HBACK_TIME;
                        hbackModel.RI_HBACK_YDETAILS = model3.RI_HBACK_YDETAILS;
                        hbackModel.RI_HBACK_WHY = model3.RI_HBACK_WHY;
                        re.result = new List<dynamic>
                   {
                       hbackModel
                   };
                        break;
                    case "陪同费":
                        RI_HACCOMPANY haccompanyModel = new RI_HACCOMPANY();
                        var model4 = await haccompanBill.Select(P => P.RI_HACCOMPANY_ID == id);
                        haccompanyModel.RI_HACCOMPANY_ID = model4.RI_HACCOMPANY_ID;
                        haccompanyModel.RI_HACCOMPANY_ADDRESS = model4.RI_HACCOMPANY_ADDRESS;
                        haccompanyModel.RI_HACCOMPANY_ADDRESSDETAIL = model4.RI_HACCOMPANY_ADDRESSDETAIL;
                        haccompanyModel.RI_HACCOMPANY_BILLDOC = model4.RI_HACCOMPANY_BILLDOC;
                        haccompanyModel.RI_HACCOMPANY_DETAILS = model4.RI_HACCOMPANY_DETAILS;
                        haccompanyModel.RI_HACCOMPANY_ISUS = model4.RI_HACCOMPANY_ISUS;
                        haccompanyModel.RI_HACCOMPANY_TIME = model4.RI_HACCOMPANY_TIME;
                        haccompanyModel.RI_HACCOMPANY_WHY = model4.RI_HACCOMPANY_WHY;
                        re.result = new List<dynamic>
                   {
                       haccompanyModel
                   };
                        break;
                    case "遗体运返或安葬":
                        RI_HBURIAL hburialModel = new RI_HBURIAL();
                        var model5 = await hburialBill.Select(p => p.RI_HBURIAL_ID == id);
                        hburialModel.RI_HBURIAL_ADDRESS = model5.RI_HBURIAL_ADDRESS;
                        hburialModel.RI_HBURIAL_ADDRESSDETAIL = model5.RI_HBURIAL_ADDRESSDETAIL;
                        hburialModel.RI_HBURIAL_BILLDOC = model5.RI_HBURIAL_BILLDOC;
                        hburialModel.RI_HBURIAL_DETAILS = model5.RI_HBURIAL_DETAILS;
                        hburialModel.RI_HBURIAL_ISLZ = model5.RI_HBURIAL_ISLZ;
                        hburialModel.RI_HBURIAL_ISUS = model5.RI_HBURIAL_ISUS;
                        hburialModel.RI_HBURIAL_TIME = model5.RI_HBURIAL_TIME;
                        hburialModel.RI_HBURIAL_TYPE = model5.RI_HBURIAL_TYPE;
                        hburialModel.RI_HBURIAL_ID = model5.RI_HBURIAL_ID;
                        hburialModel.RI_HBURIAL_WHY = model5.RI_HBURIAL_WHY;
                        re.result = new List<dynamic>
                   {
                       hburialModel
                   };
                        break;
                    case "探访费":
                        RI_HVISIT hvistModel = new RI_HVISIT();
                        var model6 = await hvisitBill.Select(p => p.RI_HVISIT_ID == id);
                        hvistModel.RI_HVISIT_ADDRESS = model6.RI_HVISIT_ADDRESS;
                        hvistModel.RI_HVISIT_ADDRESSDETAIL = model6.RI_HVISIT_ADDRESSDETAIL;
                        hvistModel.RI_HVISIT_BILLDOC = model6.RI_HVISIT_BILLDOC;
                        hvistModel.RI_HVISIT_DETAILS = model6.RI_HVISIT_DETAILS;
                        hvistModel.RI_HVISIT_ID = model6.RI_HVISIT_ID;
                        hvistModel.RI_HVISIT_ISUS = model6.RI_HVISIT_ISUS;
                        hvistModel.RI_HVISIT_TIME = model6.RI_HVISIT_TIME;
                        hvistModel.RI_HVISIT_WHY = model6.RI_HVISIT_WHY;
                        re.result = new List<dynamic>
                   {
                       hvistModel
                   };
                        break;

                };
                re.code = 1;
            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }

            return Json(re);
        }

        // POST: api/Ri_H
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Ri_H/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Ri_H/5
        public void Delete(int id)
        {
        }
    }
}
