using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using BLL;
using Model;
using Helper;

namespace kjyl.Controllers
{
    public class LipeiController : ApiController
    {
        /// <summary>
        /// 理赔录入列表获取
        /// </summary>
        ReportInformationService bll = new ReportInformationService();
        // GET: api/Lipei
        public IHttpActionResult Get(int page, int pageSize, string type, string style)
        {
            PageResult<ReportResult> re = new PageResult<ReportResult>()
            {
                code = 0,
                msg = "",
                pagecount = 0
            };
            try
            {
                int all;
                var list = bll.SelectPageListNew3<REPORTINFORMATION>(p => p.REPORTINFORMATION_ID, page, pageSize, out  all, true, type, style);
                re.pagecount = all;
                re.result = list.ToList();
                re.code = 1;
            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re);
        }

        // GET: api/Lipei/5
        /// <summary>
        /// 根据类型获和id取对应的报案信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Lipei/GetByType")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetByTypeAsync(string id, string type)
        {
            AjaxResult allre = new AjaxResult
            {
                code = 0
            };
            try
            {
                var model = await bll.SelectOne(id);
                switch (type)
                {
                    case "意外死亡":
                        AjaxResultT<dynamic> re = new AjaxResultT<dynamic>
                        {
                            code = 0,
                            msg = ""
                        };
                        if (model.RI_DIED.Count > 0)
                        {
                            re.code = 1;
                            re.msg = "成功";
                            var copayModel = model.RI_DIED.ElementAt(0);

                            dynamic resultModel = new
                            {
                                BILL_DIED_TIME = copayModel.RI_DIED_TIME,
                                BILL_DIED_HTIME = copayModel.RI_DIED_HTIME,
                                BILL_DIED_ADDRESS = copayModel.RI_DIED_ADDRESS,
                                BILL_DIED_ADDRESSDETAIL = copayModel.RI_DIED_ADDRESSDETAIL,
                                BILL_DIED_DETAIL = copayModel.RI_DIED_DETAIL
                            };
                            List<dynamic> dlist = new List<dynamic>
                            {
                                resultModel
                            };
                            re.result = dlist;
                            return Json(re);
                        }
                        break;
                    case "意外残疾":
                        AjaxResultT<dynamic> re2 = new AjaxResultT<dynamic>
                        {
                            code = 0,
                            msg = ""
                        };
                        if (model.RI_INJURE.Count > 0)
                        {
                            re2.code = 1;
                            re2.msg = "成功";
                            var copayModel = model.RI_INJURE.ElementAt(0);

                            dynamic resultModel = new
                            {
                                BILL_INJURE_TIME = copayModel.RI_INJURE_TIME,
                                BILL_INJURE_HTIME = copayModel.RI_INJURE_HTIME,
                                BILL_INJURE_ADDRESS = copayModel.RI_INJURE_ADDRESS,
                                BILL_INJURE_DETAILS = copayModel.RI_INJURE_DETAILS,
                                BILL_INJURE_ADDRESSDETAIL = copayModel.RI_INJURE_ADDRESSDETAIL

                            };
                            List<dynamic> dlist = new List<dynamic>
                            {
                                resultModel
                            };
                            re2.result = dlist;
                            return Json(re2);
                        }
                        break;
                    case "旅行延误":
                        AjaxResultT<dynamic> re3 = new AjaxResultT<dynamic>
                        {
                            code = 0,
                            msg = ""
                        };
                        if (model.RI_TRAVELDELAY.Count > 0)
                        {
                            re3.code = 1;
                            re3.msg = "成功";
                            var copayModel = model.RI_TRAVELDELAY.ElementAt(0);
                            dynamic resultModel = new
                            {
                                BILL_TRAVELDELAY_WAY = copayModel.RI_TRAVELDELAY_WAY,
                                BILL_TRAVELDELAY_NUMBER = copayModel.RI_TRAVELDELAY_NUMBER,
                                BILL_TRAVELDELAY_FROM = copayModel.RI_TRAVELDELAY_FROM,
                                BILL_TRAVELDELAY_TO = copayModel.RI_TRAVELDELAY_TO,
                                BILL_TRAVELDELAY_PLANOUT = copayModel.RI_TRAVELDELAY_PLANOUT,
                                BILL_TRAVELDELAY_PLANARBILLVE = copayModel.RI_TRAVELDELAY_PLANARRIVE,
                                BILL_TRAVELDELAY_FACTOUT = copayModel.RI_TRAVELDELAY_FACTOUT,
                                BILL_TRAVELDELAY_FACTARBILLVE = copayModel.RI_TRAVELDELAY_FACTARRIVE,
                                BILL_TRAVELDELAY_DETAILS = copayModel.RI_TRAVELDELAY_DETAILS,

                            };
                            List<dynamic> dlist = new List<dynamic>
                            {
                                resultModel
                            };
                            re3.result = dlist;
                            return Json(re3);
                        }
                        break;
                    case "个人第三者":
                        AjaxResultT<dynamic> re4 = new AjaxResultT<dynamic>
                        {
                            code = 0,
                            msg = ""
                        };
                        if (model.RI_OTHER.Count > 0)
                        {
                            re4.code = 1;
                            re4.msg = "成功";
                            var copayModel = model.RI_OTHER.ElementAt(0);

                            dynamic resultModel = new
                            {
                                BILL_OTHER_TYPE = copayModel.RI_OTHER_TYPE,
                                BILL_OTHER_TIME = copayModel.RI_OTHER_TIME,
                                BILL_OTHER_ADDRESS = copayModel.RI_OTHER_ADDRESS,
                                BILL_OTHER_REASON = copayModel.RI_OTHER_REASON,
                                BILL_OTHER_DESCBILLBE = copayModel.RI_OTHER_DESCRIBE,
                                BILL_OTHER_ASK = copayModel.RI_OTHER_ASK,
                                BILL_OTHER_ISLAW = copayModel.RI_OTHER_ISLAW,
                                BILL_OTHER_DETAILS = copayModel.RI_OTHER_DETAILS,
                                BILL_OTHER_DOC = copayModel.RI_OTHER_DOC,

                                BILL_OTHER_ADDRESSDETAILS = copayModel.RI_OTHER_ADDRESSDETAILS


                            };
                            List<dynamic> dlist = new List<dynamic>
                            {
                                resultModel
                            };
                            re4.result = dlist;
                            return Json(re4);
                        }
                        break;
                }

            }
            catch (Exception e)
            {
                allre.msg = e.Message;
            }

            return Json(allre);
        }

        // POST: api/Lipei
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Lipei/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Lipei/5
        public void Delete(int id)
        {
        }
    }
}
