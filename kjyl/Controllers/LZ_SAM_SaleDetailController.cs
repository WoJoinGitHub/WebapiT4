using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using Helper;
using Model;
using Newtonsoft.Json;
using LinqKit;

namespace kjyl.Controllers
{
    public class LZ_SAM_SaleDetailController : ApiController
    {
        // GET: api/LZ_SAM_SaleDetail
        /// <summary>
        /// 报案前查询保险信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/OrderList/getlist")]
        public IHttpActionResult getlist([FromBody] LZ_SAM_SaleDetailSelectInput value)
        {
            AjaxResultT<SaleSelect> re = new AjaxResultT<SaleSelect>
            {
                code = 0,
                msg = ""

            };
            try
            {
                SALESDETAILService bll = new SALESDETAILService();
                Expression<Func<SalDetailCustomDto, bool>> wherExpression = p => true;
                if (!string.IsNullOrEmpty(value.PassportNumber))
                {
                    wherExpression = wherExpression.And(p => p.Custom.CUSTOM_PASSPORT == value.PassportNumber);
                }
                if (!string.IsNullOrEmpty(value.PolicyNumber))
                {
                    wherExpression =
                        wherExpression.And(p => p.Salesdetail.SALESDETAIL_INSURANCENUMBER == value.PolicyNumber);
                }
                if (!string.IsNullOrEmpty(value.Name))
                {
                    wherExpression =
                        wherExpression.And(p => p.Custom.CUSTOM_NAME == value.Name);
                }
               
                List<SaleSelect> js = bll.GetSalDetailCustomDtoList(wherExpression).ToList().Select(p=>new SaleSelect
                {
                    SALEDETAILNO=p.Salesdetail.SALESDETAIL_ID,
                    CERTDATE=p.Salesdetail.SALESDETAIL_STARTTIME,
                    INSURANCEENDTIME=p.Salesdetail.SALESDETAIL_ENDTIME,
                    //CTOCRELATION=p.Salesdetail.SALESDETAIL_ID,
                    //PREMIUMAMOUNT
                    //POLICYNUMBER
                    PASSPORTNUMBER=p.Custom.CUSTOM_PASSPORT,
                    STATUS=p.Salesdetail.SALESDETAIL_CHECKSTATE,
                    //ORGCODE
                    ORGNAME =p.Salesdetail.SALESDETAIL_COMPANY,
                    PRODUCTNAME=p.Salesdetail.PRODUCE_NAME,
                    PRODUCTNO=p.Salesdetail.PRODUCE_ID,
                    CUSTNAME =p.Custom.CUSTOM_NAME,

                    BIRTHDAY=p.Custom.CUSTOM_BIRTHDATE,
                    MOBILE=p.Custom.CUSTOM_TEL,
                    Cname = p.Salesdetail.SALESDETAIL_ISHOLDER == "否" ? p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "关键联络人").RELATIONPEOPLE_NAME : (p.Salesdetail.SALESDETAIL_ISINSURED == "是" ? p.Custom.CUSTOM_NAME : p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "投保人").RELATIONPEOPLE_NAME),
                    Ctel = p.Salesdetail.SALESDETAIL_ISHOLDER == "否" ? p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "关键联络人").RELATIONPEOPLE_TEL : (p.Salesdetail.SALESDETAIL_ISINSURED == "是" ? p.Custom.CUSTOM_TEL : p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "投保人").RELATIONPEOPLE_TEL),
                    CTOCRELATION = p.Salesdetail.SALESDETAIL_ISHOLDER == "否" ? p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "关键联络人").RELATIONPEOPLE_RELATION : (p.Salesdetail.SALESDETAIL_ISINSURED == "是" ? "自己" : p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "投保人").RELATIONPEOPLE_RELATION),
                }).ToList();
              
                re.code = 1;
                re.result =js.ToList();
            }
            catch (Exception e)
            {
                re.msg = e.Message;
               
            }
            
            return Json(re);
        }
        [HttpPost]
        [Route("api/OrderList/gethistory")]
        public async Task<IHttpActionResult> GetHistory([FromBody] LZ_SAM_SaleDetailSelectInput value)
        {
            PageResult<RepostHistory> re = new PageResult<RepostHistory>
            {
                code = 0,
                pagecount=0,
                msg = ""
            };
            try
            {
                SALESDETAILService bll = new SALESDETAILService();
                Expression<Func<SalDetailCustomDto, bool>> wherExpression = p => true;
                if (!string.IsNullOrEmpty(value.PassportNumber))
                {
                    wherExpression = wherExpression.And(p => p.Custom.CUSTOM_PASSPORT == value.PassportNumber);
                }
                if (!string.IsNullOrEmpty(value.PolicyNumber))
                {
                    wherExpression =
                        wherExpression.And(p => p.Salesdetail.SALESDETAIL_INSURANCENUMBER == value.PolicyNumber);
                }
                if (!string.IsNullOrEmpty(value.Name))
                {
                    wherExpression =
                        wherExpression.And(p => p.Custom.CUSTOM_NAME == value.Name);
                }

                var  js = bll.GetSalDetailCustomDtoList(wherExpression).ToList();
                if (js.Count>0)
                {
                    var item = js.ElementAt(0);
                    var  salno =item.Salesdetail.SALESDETAIL_ID;
                    string  name = item.Salesdetail.PRODUCE_NAME;
                    ReportInformationService rebll = new ReportInformationService();
                    var relist = (await  rebll.SelectList(p => p.SALEDETAILNO == salno &&p.RI_DIED.Count>0 ));
                    if (relist.Count()>0)
                    {
                        re.msg = "已有意外死亡报案";
                    }
                    var list = await rebll.SelectPartEntityAsync<RepostHistory, string>(p => p.SALEDETAILNO == salno, p => p.REPORTINFORMATION_ID, p=>new RepostHistory
                    {
                        REPORTINFORMATION_ID= p.REPORTINFORMATION_ID,
                        REPORTINFORMATION_CAUSE=p.REPORTINFORMATION_CAUSE,
                        REPORTINFORMATION_IDENTITY= p.REPORTINFORMATION_IDENTITY,
                        SALEDETAILNO=  p.SALEDETAILNO,
                        PRODUCTNAME = name
                    },1, 100, false);
                    re.pagecount = list.Item2;
                    re.code = 1;
                    re.result = list.Item1.ToList();
                }
                            
            }
            catch (Exception e)
            {
                re.msg = e.Message;

            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
      
        [HttpGet]
        [Route("api/OrderList/GetListByPage")]
        public async Task<IHttpActionResult> GetListByPageAsync(int page, int pagesize,string type)
        {
            PageResult<SALESDETAIL> re = new PageResult<SALESDETAIL>
            {
                code = 0,
                msg = "",
                pagecount = 0,

            };
            try
            {
                SALESDETAILService bll = new SALESDETAILService();
                int pageall;
                if (type==null|| type == "列表")
                {
                  
                   var js = await bll.SelectListHave(page, pagesize);
                    re.code = 1;
                    re.pagecount =js.Item2;
                    re.result = js.Item1.ToList();
                }else
                {
                   var  js = await  bll.SelectListHave(page, pagesize);
                    re.code = 1;
                    re.pagecount = js.Item2;
                    re.result = js.Item1.ToList();
                }
              
            }
            catch (Exception e)
            {
                re.msg = e.Message;

            }
            
            return Json(re);
        }
        /// <summary>
        /// 城市列表获取
        /// </summary>
        /// <returns></returns>
        //public IHttpActionResult Get()
        //{
        //    LZ_SAM_SaleDetailService bll = new LZ_SAM_SaleDetailService();
        //    List<dynamic> js = bll.City();
        //    return Json(js);

        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/OrderList")]
        public IHttpActionResult getbyid(string id)
        {
            AjaxResultT<SaleSelect> re = new AjaxResultT<SaleSelect>
            {
                code = 0,
                msg = ""

            };
            try
            {
                SALESDETAILService bll = new SALESDETAILService();
                List<SaleSelect> js = bll.GetSalDetailCustomDtoList(p=>p.Salesdetail.SALESDETAIL_ID==id).ToList().Select(p => new SaleSelect
                {
                    SALEDETAILNO = p.Salesdetail.SALESDETAIL_ID,
                    CERTDATE = p.Salesdetail.SALESDETAIL_STARTTIME,
                    INSURANCEENDTIME = p.Salesdetail.SALESDETAIL_ENDTIME,
                   
                    //PREMIUMAMOUNT
                    //POLICYNUMBER
                    PASSPORTNUMBER = p.Custom.CUSTOM_PASSPORT,
                    STATUS = p.Salesdetail.SALESDETAIL_CHECKSTATE,
                    //ORGCODE
                    ORGNAME = p.Salesdetail.SALESDETAIL_COMPANY,
                    PRODUCTNAME = p.Salesdetail.PRODUCE_NAME,
                    PRODUCTNO = p.Salesdetail.PRODUCE_ID,
                    CUSTNAME = p.Custom.CUSTOM_NAME,
                    BIRTHDAY = p.Custom.CUSTOM_BIRTHDATE,
                    MOBILE=p.Custom.CUSTOM_TEL,
                    Cname =p.Salesdetail.SALESDETAIL_ISHOLDER=="否"?p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r=>r.RELATIONPEOPLE_TYPE== "关键联络人").RELATIONPEOPLE_NAME:(p.Salesdetail.SALESDETAIL_ISINSURED == "是"?p.Custom.CUSTOM_NAME: p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "投保人").RELATIONPEOPLE_NAME),
                    Ctel= p.Salesdetail.SALESDETAIL_ISHOLDER == "否" ? p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "关键联络人").RELATIONPEOPLE_TEL : (p.Salesdetail.SALESDETAIL_ISINSURED == "是" ? p.Custom.CUSTOM_TEL : p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "投保人").RELATIONPEOPLE_TEL),
                    CTOCRELATION = p.Salesdetail.SALESDETAIL_ISHOLDER == "否" ? p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "关键联络人").RELATIONPEOPLE_RELATION : (p.Salesdetail.SALESDETAIL_ISINSURED == "是" ? "自己" : p.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(r => r.RELATIONPEOPLE_TYPE == "投保人").RELATIONPEOPLE_RELATION),
                }).ToList();
                re.code = 1;
                re.result = js.ToList();
            }
            catch (Exception e)
            {
                re.msg = e.Message;

            }
            return Json(re);
        }
        // POST: api/LZ_SAM_SaleDetail
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/LZ_SAM_SaleDetail/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/LZ_SAM_SaleDetail/5
        public void Delete(int id)
        {
        }
    }
}
