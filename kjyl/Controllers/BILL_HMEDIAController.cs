using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Model;
using BLL;
using Helper;
using Newtonsoft.Json;

namespace kjyl.Controllers
{
    public class BILL_HMEDIAController : ApiController
    {
        BILL_HMEDIAService bill = new BILL_HMEDIAService();
        // GET: api/BILL_HMEDIA
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("api/BILL_HMEDIA/Compute")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Compute(string id,decimal lilv)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                //检查是否有未完成支付的报案
                //获取此保险的利益表
                RESULT_OTHERService resultBill = new RESULT_OTHERService();
                INTEREST_ORDEROTHERService liyiBill = new INTEREST_ORDEROTHERService();
                BILL_HMEDIA billData = bill.SelectRelation(id);

                var saledetaino = billData.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == saledetaino && p.BILL_TYPE == "全球紧急救援保险责任" && p.RESULT_OTHER_STATE == "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == saledetaino && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                //billData.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO此单号是否有未结算的全球紧急救援
                if (liyiModel == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                //resultBill.Select(p=>p.s)
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急医疗救援费");
                if (liyiItem == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                decimal? otherCount=0, medicalCount = 0, medicalCount1=0;
                if (billData.BILL_EMERGENCYMEDICAL!=null && billData.BILL_EMERGENCYMEDICAL.Count > 0)
                {
                    medicalCount1 = billData.BILL_EMERGENCYMEDICAL.Sum(p => p.BILL_EMERGENCYMEDICAL_CMONEY);
                    medicalCount = billData.BILL_EMERGENCYMEDICAL.Where(p=>p.BILL_EMERGENCYMEDICAL_STATE== "属于保险责任").Sum(p => p.BILL_EMERGENCYMEDICAL_CMONEY);
                }
                if(billData.BILL_EMERGENCYOTHER!=null && billData.BILL_EMERGENCYOTHER.Count > 0)
                {
                     otherCount = billData.BILL_EMERGENCYOTHER.Sum(p => p.BILL_EMERGENCYOTHER_MONEY);
                }
               
                
                var allCount = medicalCount + otherCount;
              decimal payMoney=  (allCount.Value - liyiItem.IO_OTHER_ITEM_ZFMONEY.Value * lilv) * liyiItem.IO_OTHER_ITEM_ZFBL.Value / 100;
                string paySate = "正常赔付";
                if(payMoney > liyiItem.IO_OTHER_ITEM_MONEY * lilv)
                {
                    payMoney = liyiItem.IO_OTHER_ITEM_MONEY.Value * lilv;
                    paySate = "超出此项保险责任最高限额";
                }
                if(payMoney > liyiModel.INTEREST_ORDEROTHER_MONEY * lilv)
                {
                    payMoney = liyiModel.INTEREST_ORDEROTHER_MONEY.Value * lilv;
                    paySate = "超出保险类型最高限额";
                }
                billData.BILL_HMEDIA_STATE = "计算完成";
                billData.RI_HMEDIA.RI_HMEDIA_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "全球紧急救援保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),
                    BILL_ID = billData.BILL_HMEDIA_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = allCount- medicalCount+ medicalCount1,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = billData.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO
                };
                var addResult = resultBill.Acomput(model, billData);
                if (addResult)
                {
                    re.code = 1;    
                    re.msg = "计算完成";
                }
            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re);
        }
        // GET: api/BILL_HMEDIA/5
        public IHttpActionResult Get(string id)
        {
            AjaxResultT<BILL_HMEDIA> re = new AjaxResultT<BILL_HMEDIA>
            {
                code = 0
            };
            try
            {
                var model = bill.SelectRelation(id);
                //model.RI_HMEDIA = null;
                re.code = 1;
                RI_HMEDIA riModel = new RI_HMEDIA();
                riModel.RI_HMEDIA_ID = model.RI_HMEDIA.RI_HMEDIA_ID;
                riModel.RI_HMEDIA_ADDRESS = model.RI_HMEDIA.RI_HMEDIA_ADDRESS;
                riModel.RI_HMEDIA_ADDRESSDETAIL = model.RI_HMEDIA.RI_HMEDIA_ADDRESSDETAIL;

                riModel.RI_HMEDIA_BILLDOC = model.RI_HMEDIA.RI_HMEDIA_BILLDOC;
                riModel.RI_HMEDIA_DETAIS = model.RI_HMEDIA.RI_HMEDIA_DETAIS;
                riModel.RI_HMEDIA_ISLZ = model.RI_HMEDIA.RI_HMEDIA_ISLZ;
                riModel.RI_HMEDIA_ISUS = model.RI_HMEDIA.RI_HMEDIA_ISUS;
                riModel.RI_HMEDIA_TIME = model.RI_HMEDIA.RI_HMEDIA_TIME;
                riModel.RI_HMEDIA_TYPE = model.RI_HMEDIA.RI_HMEDIA_TYPE;
                riModel.RI_HMEDIA_WHY = model.RI_HMEDIA.RI_HMEDIA_WHY;
                BILL_HMEDIA reModel = new BILL_HMEDIA
                {
                    BILL_EMERGENCYMEDICAL = model.BILL_EMERGENCYMEDICAL,
                    BILL_EMERGENCYOTHER = model.BILL_EMERGENCYOTHER,
                    BILL_HMEDIA_ID = model.BILL_HMEDIA_ID,
                    BILL_HMEDIA_STATE = model.BILL_HMEDIA_STATE,
                    BILL_HMEDIA_WHY = model.BILL_HMEDIA_WHY,
                    RI_HMEDIA_ID = model.RI_HMEDIA_ID,
                    RI_HMEDIA= riModel
                };
                List<BILL_HMEDIA> list = new List<BILL_HMEDIA> {reModel};

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }
       
            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
           
        }

        // POST: api/BILL_HMEDIA
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody] BILL_HMEDIA value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
               
                RI_HMEDIAService ribill = new RI_HMEDIAService();
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();
                
                var rimodel = await ribill.SelectOne(value.RI_HMEDIA_ID);
                rimodel.RI_HMEDIA_STATE = "需理赔审核";
                rimodel.RI_HMEDIA_ADDRESS = value.RI_HMEDIA.RI_HMEDIA_ADDRESS;
                rimodel.RI_HMEDIA_ADDRESSDETAIL = value.RI_HMEDIA.RI_HMEDIA_ADDRESSDETAIL;

                rimodel.RI_HMEDIA_BILLDOC = value.RI_HMEDIA.RI_HMEDIA_BILLDOC;
                rimodel.RI_HMEDIA_DETAIS = value.RI_HMEDIA.RI_HMEDIA_DETAIS;
                rimodel.RI_HMEDIA_ISLZ = value.RI_HMEDIA.RI_HMEDIA_ISLZ;
                rimodel.RI_HMEDIA_ISUS = value.RI_HMEDIA.RI_HMEDIA_ISUS;
                rimodel.RI_HMEDIA_TIME = value.RI_HMEDIA.RI_HMEDIA_TIME;
                rimodel.RI_HMEDIA_TYPE = value.RI_HMEDIA.RI_HMEDIA_TYPE;
                foreach (var item in value.BILL_EMERGENCYMEDICAL)
                {
                    item.RI_ID = value.BILL_HMEDIA_ID;
                }
                foreach (var item in value.BILL_EMERGENCYOTHER)
                {
                    item.RI_ID = value.BILL_HMEDIA_ID;
                }
                value.RI_HMEDIA = rimodel;
                bool f= bill.AddRelation(value);
                if (f)
                {
                    re.code = 1;
                    re.msg = "添加成功";
                }
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }
            return Json(re);
        }

        // PUT: api/BILL_HMEDIA/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put( [FromBody]BILL_HMEDIA value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RI_HMEDIAService ribill = new RI_HMEDIAService();
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();

                var rimodel = await ribill.SelectOne(value.RI_HMEDIA_ID);
                rimodel.RI_HMEDIA_STATE = value.BILL_HMEDIA_STATE;
                if (value.BILL_HMEDIA_STATE == "需理赔审核" && value.RI_HMEDIA!=null)
                {
                  
                    rimodel.RI_HMEDIA_ADDRESS = value.RI_HMEDIA.RI_HMEDIA_ADDRESS;
                    rimodel.RI_HMEDIA_ADDRESSDETAIL = value.RI_HMEDIA.RI_HMEDIA_ADDRESSDETAIL;

                    rimodel.RI_HMEDIA_BILLDOC = value.RI_HMEDIA.RI_HMEDIA_BILLDOC;
                    rimodel.RI_HMEDIA_DETAIS = value.RI_HMEDIA.RI_HMEDIA_DETAIS;
                    rimodel.RI_HMEDIA_ISLZ = value.RI_HMEDIA.RI_HMEDIA_ISLZ;
                    rimodel.RI_HMEDIA_ISUS = value.RI_HMEDIA.RI_HMEDIA_ISUS;
                    rimodel.RI_HMEDIA_TIME = value.RI_HMEDIA.RI_HMEDIA_TIME;
                    rimodel.RI_HMEDIA_TYPE = value.RI_HMEDIA.RI_HMEDIA_TYPE;
                }
             

                var id = value.BILL_HMEDIA_ID;
                var model = await bill.SelectOne(id);
                model.BILL_HMEDIA_STATE = value.BILL_HMEDIA_STATE;             
                model.BILL_EMERGENCYMEDICAL = value.BILL_EMERGENCYMEDICAL;                
                model.BILL_EMERGENCYOTHER = value.BILL_EMERGENCYOTHER;
                model.BILL_HMEDIA_WHY = value.BILL_HMEDIA_WHY;
                model.RI_HMEDIA = rimodel;              
                bool f = false;
                if (value.BILL_HMEDIA_STATE == "需计算")
                {                 
                  //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO= model.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);                    
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME== "全球紧急救援保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {                       
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                         var reModel = await pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID &&p.IPOTHER_NAME== "全球紧急救援保险责任");
                        if (reModel == null)
                        {
                            re.msg = "此保险类型没有此保险责任";
                            return Json(re);
                        }
                        INTEREST_ORDEROTHER otherModel =
                            new INTEREST_ORDEROTHER
                            {
                                INTEREST_ORDEROTHER_NAME = reModel.IPOTHER_NAME,
                                INTEREST_ORDEROTHER_MONEY = reModel.IPOTHER_MONEY,
                                PRODUCTNO =reModel.PRODUCTNO,
                                SALEDETAILNO = model.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO,
                                INTEREST_ORDEROTHER_ID = CreateId.GetId()
                            };
                        //保险单号
                        List<IO_OTHER_ITEM> listItem = new List<IO_OTHER_ITEM>();
                        foreach (var item in reModel.IPOTHER_ITEM)
                        {
                            IO_OTHER_ITEM LiyiOreder = new IO_OTHER_ITEM
                            {
                                IO_OTHER_ITEM_ID = CreateId.GetId(),
                                IO_OTHER_ITEM_MONEY = item.IPOTHER_ITEM_MONEY,
                                IO_OTHER_ITEM_NAME = item.IPOTHER_ITEM_NAME,
                                IO_OTHER_ITEM_ZFBL = item.IPOTHER_ITEM_ZFBL,
                                IO_OTHER_ITEM_ZFMONEY = item.IPOTHER_ITEM_ZFMONEY
                            };
                            listItem.Add(LiyiOreder);
                           
                        }
                        otherModel.IO_OTHER_ITEM = listItem;
                        f = bill.UpdateLiyi(model, otherModel);
                    }
                    else
                    {
                        f = bill.UpdataRelation(model);
                    }
                }
                else
                {
                    f = bill.UpdataRelation(model);
                }
                if (f)
                {
                    re.code = 1;
                    re.msg = "修改成功";
                }
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }
            return Json(re);
        }

        // DELETE: api/BILL_HMEDIA/5
        public void Delete(int id)
        {
        }
    }
}

 