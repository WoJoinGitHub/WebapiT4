using Helper;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BLL;
using Newtonsoft.Json;
using System.Web;

namespace kjyl.Controllers
{
    public class ReportInfomationController : ApiController
    {
        /// <summary>
        /// 报案api
        /// </summary>
        ReportInformationService bll = new ReportInformationService();
        // GET: api/ReportInfomation/5
        /// <summary>
        /// 分页获取
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesum"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IHttpActionResult Get(int page, int pagesum, string state)
        {
            PageResult<ReportResult> result = new PageResult<ReportResult>
            {
                code = 0,
                msg = "获取失败",
                pagecount = 0

            };
            int all;
            try
            {
                if (state == "审核")
                {
                    var list = bll.SelectPageListNew2<ReportResult>(p => p.REPORTINFORMATION_STATE == "需审核", p => p.REPORTINFORMATION_ID, page, pagesum, out all, false);
                    result.result = list.ToList();
                }
                else
                {
                    var list = bll.SelectPageListNew2<ReportResult>(p => p.REPORTINFORMATION_STATE == "需修改" || p.REPORTINFORMATION_STATE == "有未通过" || p.REPORTINFORMATION_STATE == "需查看", p => p.REPORTINFORMATION_ID, page, pagesum, out all, false);
                    result.result = list.ToList();
                }
                result.pagecount = all;
                result.msg = "获取成功";
                result.code = 1;
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        }
        /// <summary>
        /// 根据id获取 审核用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> Get(string id)
        {
            AjaxResultT<dynamic> result = new AjaxResultT<dynamic>
            {
                code = 0,
                msg = "获取失败"
               
            };
            try
            {
                var list = await  bll.SelectOnePartAsync(id);

                List<dynamic> arry = new List<dynamic> {list};
                result.result = arry;
                result.msg = "获取成功";
                result.code = 1;
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        }
        /// <summary>
        /// 新增报案
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST: api/ReportInfomation
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody]REPORTINFORMATION value)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = "提交失败"
            };
            REPORTINFORMATION model;

            value.REPORTINFORMATION_TIME = DateTime.Now;
            
            //model.REPORTINFORMATION_EMAIL = value.REPORTINFORMATION_EMAIL;         

            try
            {
                model =await  bll.Add(value);
                if (model.REPORTINFORMATION_ID.Length > 0)
                {
                    result.code = 1;
                    result.msg = "提交成功";
                }

            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }


            return Json(result);

        }
        /// <summary>
        ///  报案不通过后修改报案
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST: api/ReportInfomation
        [HttpPost]
        [Route("api/ReportInfomation/chang")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostchangAsync([FromBody]REPORTINFORMATION value)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = "提交失败"
            };
            REPORTINFORMATION model;

            //value.REPORTINFORMATION_TIME = System.DateTime.Now;
            //model.REPORTINFORMATION_EMAIL = value.REPORTINFORMATION_EMAIL;         

            try
            {
                model = await bll.SelectOne(value.REPORTINFORMATION_ID);
                model.REPORTINFORMATION_STATE = "需审核";
                value.SALEDETAILNO = model.SALEDETAILNO;
                var i = 0;
                foreach (var item in model.RI_MEDICAL)
                {
                    var list = value.RI_MEDICAL.ElementAt(i);
                    if (list.RI_MEDICAL_STATE == "需修改")
                    {
                        item.RI_MEDICAL_STATE = "需审核";
                        item.RI_MEDICAL_ADDRESS = list.RI_MEDICAL_ADDRESS;
                        item.RI_MEDICAL_DADDRESS = list.RI_MEDICAL_DADDRESS;
                        item.RI_MEDICAL_DETAIS = list.RI_MEDICAL_DETAIS;
                        item.RI_MEDICAL_HTYPE = list.RI_MEDICAL_HTYPE;
                        item.RI_MEDICAL_HLNAME = list.RI_MEDICAL_HLNAME;
                        item.RI_MEDICAL_TIME = list.RI_MEDICAL_TIME;
                    }
                    i++;
                }


                i = 0;
                foreach (var item in model.RI_ACADEMIC)
                {
                    var list = value.RI_ACADEMIC.ElementAt(i);
                    if (list.RI_ACADEMIC_STATE == "需修改")
                    {
                        item.RI_ACADEMIC_STATE = "需审核";
                        item.RI_ACADEMIC_DETAILS = list.RI_ACADEMIC_DETAILS;
                        item.RI_ACADEMIC_REASON = list.RI_ACADEMIC_REASON;
                        item.RI_ACADEMIC_TIME = list.RI_ACADEMIC_TIME;
                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_DIED)
                {
                    var list = value.RI_DIED.ElementAt(i);
                    if (list.RI_DIED_STATE == "需修改")
                    {
                        item.RI_DIED_STATE = "需审核";
                        item.RI_DIED_ADDRESS = list.RI_DIED_ADDRESS;
                        item.RI_DIED_ADDRESSDETAIL = list.RI_DIED_ADDRESSDETAIL;
                        item.RI_DIED_DETAIL = list.RI_DIED_DETAIL;

                        item.RI_DIED_HTIME = list.RI_DIED_HTIME;
                        item.RI_DIED_TIME = list.RI_DIED_TIME;
                        item.RI_DIED_HADDRESS = list.RI_DIED_HADDRESS;
                    }
                    i++;

                }
                i = 0;
                foreach (var item in model.RI_HBACK)
                {
                    var list = value.RI_HBACK.ElementAt(i);
                    if (list.RI_HBACK_STATE == "需修改")
                    {
                        item.RI_HBACK_STATE = "需审核";
                        item.RI_HBACK_ADDRESS = list.RI_HBACK_ADDRESS;
                        item.RI_HBACK_ADDRESSDETAIL = list.RI_HBACK_ADDRESSDETAIL;
                        item.RI_HBACK_DETAILS = list.RI_HBACK_DETAILS;

                        item.RI_HBACK_DOC = list.RI_HBACK_DOC;
                        item.RI_HBACK_HADDRESS = list.RI_HBACK_HADDRESS;
                        item.RI_HBACK_HEMAIL = list.RI_HBACK_HEMAIL;
                        item.RI_HBACK_HNAME = list.RI_HBACK_HNAME;

                        item.RI_HBACK_HTEL = list.RI_HBACK_HTEL;
                        item.RI_HBACK_TIME = list.RI_HBACK_TIME;
                        item.RI_HBACK_YDETAILS = list.RI_HBACK_YDETAILS;

                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_HBURIAL)
                {
                    var list = value.RI_HBURIAL.ElementAt(i);
                    if (list.RI_HBURIAL_STATE == "需修改")
                    {
                        item.RI_HBURIAL_STATE = "需审核";
                        item.RI_HBURIAL_ADDRESS = list.RI_HBURIAL_ADDRESS;
                        item.RI_HBURIAL_ADDRESSDETAIL = list.RI_HBURIAL_ADDRESSDETAIL;
                        item.RI_HBURIAL_DETAILS = list.RI_HBURIAL_DETAILS;

                        item.RI_HBURIAL_ISLZ = list.RI_HBURIAL_ISLZ;
                        item.RI_HBURIAL_TIME = list.RI_HBURIAL_TIME;
                        item.RI_HBURIAL_TYPE = list.RI_HBURIAL_TYPE;
                        item.RI_HBURIAL_FADDRESS = list.RI_HBURIAL_FADDRESS;

                    }
                    i++;

                }
                i = 0;
                foreach (var item in model.RI_HCHANGE)
                {
                    var list = value.RI_HCHANGE.ElementAt(i);
                    if (list.RI_HCHANGE_STATE == "需修改")
                    {
                        item.RI_HCHANGE_STATE = "需审核";
                        item.RI_HCHANGE_ADDRESS = list.RI_HCHANGE_ADDRESS;
                        item.RI_HCHANGE_ADDRESSDETAIL = list.RI_HCHANGE_ADDRESSDETAIL;
                        item.RI_HCHANGE_CDETAIL = list.RI_HCHANGE_CDETAIL;

                        item.RI_HCHANGE_DETAIL = list.RI_HCHANGE_DETAIL;
                        item.RI_HCHANGE_DOC = list.RI_HCHANGE_DOC;
                        item.RI_HCHANGE_HADDRESS = list.RI_HCHANGE_HADDRESS;

                        item.RI_HCHANGE_HEMAIL = list.RI_HCHANGE_HEMAIL;
                        item.RI_HCHANGE_HNAME = list.RI_HCHANGE_HNAME;
                        item.RI_HCHANGE_HTEL = list.RI_HCHANGE_HTEL;

                        item.RI_HCHANGE_TIME = list.RI_HCHANGE_TIME;


                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_HMEDIA)
                {
                    var list = value.RI_HMEDIA.ElementAt(i);
                    if (list.RI_HMEDIA_STATE == "需修改")
                    {
                        item.RI_HMEDIA_STATE = "需审核";
                        item.RI_HMEDIA_ADDRESS = list.RI_HMEDIA_ADDRESS;
                        item.RI_HMEDIA_ADDRESSDETAIL = list.RI_HMEDIA_ADDRESSDETAIL;
                        item.RI_HMEDIA_DETAIS = list.RI_HMEDIA_DETAIS;

                        item.RI_HMEDIA_ISLZ = list.RI_HMEDIA_ISLZ;
                        item.RI_HMEDIA_TIME = list.RI_HMEDIA_TIME;
                        item.RI_HMEDIA_TYPE = list.RI_HMEDIA_TYPE;

                    }
                    i++;

                }
                i = 0;
                foreach (var item in model.RI_HVISIT)
                {
                    var list = value.RI_HVISIT.ElementAt(i);
                    if (list.RI_HVISIT_STATE == "需修改")
                    {
                        item.RI_HVISIT_STATE = "需审核";
                        item.RI_HVISIT_ADDRESS = list.RI_HVISIT_ADDRESS;
                        item.RI_HVISIT_ADDRESSDETAIL = list.RI_HVISIT_ADDRESSDETAIL;
                        item.RI_HVISIT_DETAILS = list.RI_HVISIT_DETAILS;

                        item.RI_HVISIT_TIME = list.RI_HVISIT_TIME;


                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_HVISIT)
                {
                    var list = value.RI_HVISIT.ElementAt(i);
                    if (list.RI_HVISIT_STATE == "需修改")
                    {
                        item.RI_HVISIT_STATE = "需审核";
                        item.RI_HVISIT_ADDRESS = list.RI_HVISIT_ADDRESS;
                        item.RI_HVISIT_ADDRESSDETAIL = list.RI_HVISIT_ADDRESSDETAIL;
                        item.RI_HVISIT_DETAILS = list.RI_HVISIT_DETAILS;

                        item.RI_HVISIT_TIME = list.RI_HVISIT_TIME;

                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_INJURE)
                {
                    var list = value.RI_INJURE.ElementAt(i);
                    if (list.RI_INJURE_STATE == "需修改")
                    {
                        item.RI_INJURE_STATE = "需审核";
                        item.RI_INJURE_ADDRESS = list.RI_INJURE_ADDRESS;
                        item.RI_INJURE_ADDRESSDETAIL = list.RI_INJURE_ADDRESSDETAIL;
                        item.RI_INJURE_DETAILS = list.RI_INJURE_DETAILS;

                        item.RI_INJURE_HTIME = list.RI_INJURE_HTIME;
                        item.RI_INJURE_TIME = list.RI_INJURE_TIME;
                        item.RI_INJURE_FADDRESS = list.RI_INJURE_FADDRESS;

                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_LOSE)
                {
                    var list = value.RI_LOSE.ElementAt(i);
                    if (list.RI_LOSE_STATE == "需修改")
                    {
                        item.RI_LOSE_STATE = "需审核";
                        item.RI_LOSE_ADDRESS = list.RI_LOSE_ADDRESS;
                        item.RI_LOSE_DETAILS = list.RI_LOSE_DETAILS;
                        item.RI_LOSE_FROM = list.RI_LOSE_FROM;

                        item.RI_LOSE_TIME = list.RI_LOSE_TIME;
                        item.RI_LOSE_TO = list.RI_LOSE_TO;



                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_OTHER)
                {
                    var list = value.RI_OTHER.ElementAt(i);
                    if (list.RI_OTHER_STATE == "需修改")
                    {
                        item.RI_OTHER_STATE = "需审核";
                        item.RI_OTHER_ADDRESS = list.RI_OTHER_ADDRESS;
                        item.RI_OTHER_ADDRESSDETAILS = list.RI_OTHER_ADDRESSDETAILS;
                        item.RI_OTHER_ASK = list.RI_OTHER_ASK;

                        item.RI_OTHER_DESCRIBE = list.RI_OTHER_DESCRIBE;
                        item.RI_OTHER_DETAILS = list.RI_OTHER_DETAILS;


                        item.RI_OTHER_DOC = list.RI_OTHER_DOC;
                        item.RI_OTHER_ISLAW = list.RI_OTHER_ISLAW;
                        item.RI_OTHER_REASON = list.RI_OTHER_REASON;

                        item.RI_OTHER_TIME = list.RI_OTHER_TIME;
                        item.RI_OTHER_TYPE = list.RI_OTHER_TYPE;
                        
                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_TRAVELDELAY)
                {
                    var list = value.RI_TRAVELDELAY.ElementAt(i);
                    if (list.RI_TRAVELDELAY_STATE == "需修改")
                    {
                        item.RI_TRAVELDELAY_STATE = "需审核";
                        item.RI_TRAVELDELAY_DETAILS = list.RI_TRAVELDELAY_DETAILS;
                        item.RI_TRAVELDELAY_FACTARRIVE = list.RI_TRAVELDELAY_FACTARRIVE;
                        item.RI_TRAVELDELAY_FACTOUT = list.RI_TRAVELDELAY_FACTOUT;

                        item.RI_TRAVELDELAY_FROM = list.RI_TRAVELDELAY_FROM;
                        item.RI_TRAVELDELAY_NUMBER = list.RI_TRAVELDELAY_NUMBER;


                        item.RI_TRAVELDELAY_PLANARRIVE = list.RI_TRAVELDELAY_PLANARRIVE;
                        item.RI_TRAVELDELAY_PLANOUT = list.RI_TRAVELDELAY_PLANOUT;
                        item.RI_TRAVELDELAY_TO = list.RI_TRAVELDELAY_TO;

                        item.RI_TRAVELDELAY_WAY = list.RI_TRAVELDELAY_WAY;
                        item.RI_TRAVELDELAY_WHY = list.RI_TRAVELDELAY_WHY;
                        

                    }
                    i++;
                }
                i = 0;
                foreach (var item in model.RI_HACCOMPANY)
                {
                    var list = value.RI_HACCOMPANY.ElementAt(i);
                    if (list.RI_HACCOMPANY_STATE == "需修改")
                    {
                        item.RI_HACCOMPANY_STATE = "需审核";
                        item.RI_HACCOMPANY_ADDRESS = list.RI_HACCOMPANY_ADDRESS;
                        item.RI_HACCOMPANY_ADDRESSDETAIL = list.RI_HACCOMPANY_ADDRESSDETAIL;
                        item.RI_HACCOMPANY_DETAILS = list.RI_HACCOMPANY_DETAILS;

                        item.RI_HACCOMPANY_TIME = list.RI_HACCOMPANY_TIME;
                        item.RI_HACCOMPANY_WHY = list.RI_HACCOMPANY_WHY;


                    }
                    i++;
                }
                model.REPORTINFORMATION_EMAIL = value.REPORTINFORMATION_EMAIL;
                model.REPORTINFORMATION_IDENTITY = value.REPORTINFORMATION_IDENTITY;
                model.REPORTINFORMATION_RELATION = value.REPORTINFORMATION_RELATION;
                model.REPORTINFORMATION_TEL = value.REPORTINFORMATION_TEL;

                model.REPORTINFORMATION_USERNAME = value.REPORTINFORMATION_USERNAME;
                model.REPORTINFORMATION_WAY = value.REPORTINFORMATION_WAY;

                //model.REPORTINFORMATION_STATE = "提交审核";
                var up = await bll.Updata(model);
                if (up)
                {
                    result.code = 1;
                    result.msg = "修改成功";
                }

            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }


            return Json(result);

        }
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/ReportInfomation/file")]
        public IHttpActionResult FileUp()
        {

            var httpRequest = HttpContext.Current.Request;
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = "提交失败"
               

            };
            try
            {


                if (httpRequest.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        string filename = postedFile.FileName;
                        string[] arry = filename.Split('.');
                        string name = CreateId.GetId();
                        if (arry.Length > 1)
                        {
                            name += "." + arry[arry.Length - 1];
                        }
                        else
                        {
                            name = filename;
                        }

                        var filePath = HttpContext.Current.Server.MapPath("~/Upload/" + name);
                        postedFile.SaveAs(filePath);

                        docfiles.Add(name);
                    }
                    result.msg = docfiles;
                    result.code = 1;


                }
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }

            return Json(result);
        }
        /// <summary>
        /// 追加报案
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/ReportInfomation/Zadd")]
        public async System.Threading.Tasks.Task<IHttpActionResult> ZaddAsync([FromBody]REPORTINFORMATION value)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = "审核失败"
               

            };
            var id = value.REPORTINFORMATION_ID;
            var model = await bll.SelectOne(id);
            foreach (var item in value.RI_ACADEMIC)
            {
                model.RI_ACADEMIC.Add(item);
            }
            foreach (var item in value.RI_DIED)
            {
                model.RI_DIED.Add(item);
            }

            foreach (var item in value.RI_HBACK)
            {
                model.RI_HBACK.Add(item);
            }
            foreach (var item in value.RI_HBURIAL)
            {
                model.RI_HBURIAL.Add(item);
            }
            foreach (var item in value.RI_HCHANGE)
            {
                model.RI_HCHANGE.Add(item);
            }
            foreach (var item in value.RI_HMEDIA)
            {
                model.RI_HMEDIA.Add(item);
            }

            foreach (var item in value.RI_HVISIT)
            {
                model.RI_HVISIT.Add(item);
            }
            foreach (var item in value.RI_INJURE)
            {
                model.RI_INJURE.Add(item);
            }
            
            foreach (var item in value.RI_LOSE)
            {
                model.RI_LOSE.Add(item);
            }
            foreach (var item in value.RI_MEDICAL)
            {
                model.RI_MEDICAL.Add(item);
            }

            foreach (var item in value.RI_OTHER)
            {
                model.RI_OTHER.Add(item);
            }
            foreach (var item in value.RI_TRAVELDELAY)
            {
                model.RI_TRAVELDELAY.Add(item);
            }
            foreach (var item in value.RI_HACCOMPANY)
            {
                model.RI_HACCOMPANY.Add(item);
            }
            model.REPORTINFORMATION_STATE = "需审核";
            try
            {
                bool f = await bll.Updata(model);
                if (f)
                {
                    result.msg = "追加成功";
                    result.code = 1;
                }
            }
            catch (Exception e)
            {
                result.msg = e.Message;
            }
            return Json(result);
        }
        /// <summary>
        /// 提交报案审核结果
        /// </summary>
        /// <param name="value"></param>
        // PUT: api/ReportInfomation/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put([FromBody]REPORTINFORMATION value)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = "审核失败"
            };
            REPORTINFORMATION model = await bll.SelectOne(value.REPORTINFORMATION_ID);
            model.REPORTINFORMATION_CHECKPEOPLE = value.REPORTINFORMATION_CHECKPEOPLE;
            model.REPORTINFORMATION_CHECKTIME = DateTime.Now;
            var i = 0;
            foreach (var item in model.RI_MEDICAL)
            {
                var list = value.RI_MEDICAL.ElementAt(i);
                item.RI_MEDICAL_STATE = list.RI_MEDICAL_STATE;
                item.RI_MEDICAL_WHY = list.RI_MEDICAL_WHY;
                item.RI_MEDICAL_CHANGETIME = System.DateTime.Now;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_ACADEMIC)
            {
                var list = value.RI_ACADEMIC.ElementAt(i);
                item.RI_ACADEMIC_STATE = list.RI_ACADEMIC_STATE;
                item.RI_ACADEMIC_WHY = list.RI_ACADEMIC_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_DIED)
            {
                var list = value.RI_DIED.ElementAt(i);
                item.RI_DIED_STATE = list.RI_DIED_STATE;
                item.RI_DIED_WHY = list.RI_DIED_WHY;
                i++;

            }
            i = 0;
            foreach (var item in model.RI_HBACK)
            {
                var list = value.RI_HBACK.ElementAt(i);
                item.RI_HBACK_STATE = list.RI_HBACK_STATE;
                item.RI_HBACK_WHY = list.RI_HBACK_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_HBURIAL)
            {
                var list = value.RI_HBURIAL.ElementAt(i);
                item.RI_HBURIAL_STATE = list.RI_HBURIAL_STATE;
                item.RI_HBURIAL_WHY = list.RI_HBURIAL_WHY;
                i++;

            }
            i = 0;
            foreach (var item in model.RI_HCHANGE)
            {
                var list = value.RI_HCHANGE.ElementAt(i);
                item.RI_HCHANGE_STATE = list.RI_HCHANGE_STATE;
                item.RI_HCHANGE_WHY = list.RI_HCHANGE_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_HMEDIA)
            {
                var list = value.RI_HMEDIA.ElementAt(i);
                item.RI_HMEDIA_STATE = list.RI_HMEDIA_STATE;
                item.RI_HMEDIA_WHY = list.RI_HMEDIA_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_HVISIT)
            {
                var list = value.RI_HVISIT.ElementAt(i);
                item.RI_HVISIT_STATE = list.RI_HVISIT_STATE;
                item.RI_HVISIT_WHY = list.RI_HVISIT_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_HVISIT)
            {
                var list = value.RI_HVISIT.ElementAt(i);
                item.RI_HVISIT_STATE = list.RI_HVISIT_STATE;
                item.RI_HVISIT_WHY = list.RI_HVISIT_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_INJURE)
            {
                var list = value.RI_INJURE.ElementAt(i);
                item.RI_INJURE_STATE = list.RI_INJURE_STATE;
                item.RI_INJURE_WHY = list.RI_INJURE_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_LOSE)
            {
                var list = value.RI_LOSE.ElementAt(i);
                item.RI_LOSE_STATE = list.RI_LOSE_STATE;
                item.RI_LOSE_WHY = list.RI_LOSE_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_OTHER)
            {
                var list = value.RI_OTHER.ElementAt(i);
                item.RI_OTHER_STATE = list.RI_OTHER_STATE;
                item.RI_OTHER_WHY = list.RI_OTHER_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_TRAVELDELAY)
            {
                var list = value.RI_TRAVELDELAY.ElementAt(i);
                item.RI_TRAVELDELAY_STATE = list.RI_TRAVELDELAY_STATE;
                item.RI_TRAVELDELAY_WHY = list.RI_TRAVELDELAY_WHY;
                i++;
            }
            i = 0;
            foreach (var item in model.RI_HACCOMPANY)
            {
                var list = value.RI_HACCOMPANY.ElementAt(i);
                item.RI_HACCOMPANY_STATE = list.RI_HACCOMPANY_STATE;
                item.RI_HACCOMPANY_WHY = list.RI_HACCOMPANY_WHY;
                i++;
            }
            model.REPORTINFORMATION_STATE = value.REPORTINFORMATION_STATE;

            //model.REPORTINFORMATION_EMAIL = value.REPORTINFORMATION_EMAIL;


            try
            {
                bool f = await bll.Updata(model);
                if (f)
                {
                    result.code = 1;
                    result.msg = "审核成功";
                }

            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }


            return Json(result);
        }

        // DELETE: api/ReportInfomation/5
        public void Delete(int id)
        {
        }
    }
}
