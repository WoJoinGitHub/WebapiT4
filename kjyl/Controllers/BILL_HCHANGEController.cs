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
    public class BILL_HCHANGEController : ApiController
    {
        BILL_HCHANGEService bill = new BILL_HCHANGEService();
        // GET: api/BILL_HCHANGE
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        [HttpGet]
        [Route("api/BILL_HCHANGE/Compute")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Compute(string id, decimal lilv)
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
                BILL_HCHANGE billData = bill.SelectRelation(id);
                var saledetaino = billData.RI_HCHANGE.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == saledetaino && p.BILL_TYPE == "全球紧急救援保险责任" && p.RESULT_OTHER_STATE== "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == saledetaino && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                //billData.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO此单号是否有未结算的全球紧急救援

                //resultBill.Select(p=>p.s)
                if (liyiModel==null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急转院费");
                if (liyiItem == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                string paySate = "正常赔付";
                decimal? otherCount = 0, medicalCount = 0, medicalCount1=0;
                if (billData.BILL_EMERGENCYMEDICAL != null && billData.BILL_EMERGENCYMEDICAL.Count > 0)
                {
                    medicalCount1 = billData.BILL_EMERGENCYMEDICAL.Sum(p => p.BILL_EMERGENCYMEDICAL_CMONEY);
                    medicalCount = billData.BILL_EMERGENCYMEDICAL.Where(p => p.BILL_EMERGENCYMEDICAL_STATE == "属于保险责任").Sum(p => p.BILL_EMERGENCYMEDICAL_CMONEY);
                }
                if (billData.BILL_EMERGENCYOTHER != null && billData.BILL_EMERGENCYOTHER.Count > 0)
                {
                    otherCount = billData.BILL_EMERGENCYOTHER.Sum(p => p.BILL_EMERGENCYOTHER_MONEY);
                }
               
                var allCount = medicalCount + otherCount;
                var payMoney = (allCount.Value - liyiItem.IO_OTHER_ITEM_ZFMONEY.Value * lilv) * liyiItem.IO_OTHER_ITEM_ZFBL.Value / 100;
                if (payMoney > liyiItem.IO_OTHER_ITEM_MONEY * lilv)
                {
                    payMoney = liyiItem.IO_OTHER_ITEM_MONEY.Value * lilv;
                    paySate = "超出此项保险责任最高限额";
                }
               if(payMoney > liyiModel.INTEREST_ORDEROTHER_MONEY * lilv)
                {
                    payMoney = liyiModel.INTEREST_ORDEROTHER_MONEY.Value * lilv;
                    paySate = "超出保险类型最高限额";
                }
            

                billData.BILL_HCHANGE_STATE = "计算完成";
                billData.RI_HCHANGE.RI_HCHANGE_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "全球紧急救援保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),
                    BILL_ID = billData.BILL_HCHANGE_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = allCount- medicalCount+ medicalCount1,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = saledetaino
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
        // GET: api/BILL_HCHANGE/5
        public IHttpActionResult Get(string  id)
        {
            AjaxResultT<BILL_HCHANGE> re = new AjaxResultT<BILL_HCHANGE>
            {
                code = 0
            };
            try
            {
              
                var model = bill.SelectRelation(id);
                RI_HCHANGE hchangModel = new RI_HCHANGE();
               var  model2 = model.RI_HCHANGE;
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
                model.RI_HCHANGE = hchangModel;
                re.code = 1;
              
                List<BILL_HCHANGE> list = new List<BILL_HCHANGE> {model};

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_HCHANGE
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody] BILL_HCHANGE value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RI_HCHANGEService ribill = new RI_HCHANGEService();
                var rimodel = await ribill.SelectOne(value.RI_HCHANGE_ID);
                rimodel.RI_HCHANGE_STATE = "需理赔审核";
                rimodel.RI_HCHANGE_ADDRESS = value.RI_HCHANGE.RI_HCHANGE_ADDRESS;
                rimodel.RI_HCHANGE_ADDRESSDETAIL = value.RI_HCHANGE.RI_HCHANGE_ADDRESSDETAIL;
                rimodel.RI_HCHANGE_CDETAIL = value.RI_HCHANGE.RI_HCHANGE_CDETAIL;
                rimodel.RI_HCHANGE_DETAIL = value.RI_HCHANGE.RI_HCHANGE_DETAIL;
                rimodel.RI_HCHANGE_DOC = value.RI_HCHANGE.RI_HCHANGE_DOC;
                rimodel.RI_HCHANGE_HADDRESS = value.RI_HCHANGE.RI_HCHANGE_HADDRESS;
                rimodel.RI_HCHANGE_HEMAIL = value.RI_HCHANGE.RI_HCHANGE_HEMAIL;
                rimodel.RI_HCHANGE_HNAME = value.RI_HCHANGE.RI_HCHANGE_HNAME;
                rimodel.RI_HCHANGE_NADDRESS = value.RI_HCHANGE.RI_HCHANGE_NADDRESS;
                rimodel.RI_HCHANGE_HTEL = value.RI_HCHANGE.RI_HCHANGE_HTEL;
                rimodel.RI_HCHANGE_NEMAIL = value.RI_HCHANGE.RI_HCHANGE_NEMAIL;
                rimodel.RI_HCHANGE_ID = value.RI_HCHANGE.RI_HCHANGE_ID;
                rimodel.RI_HCHANGE_TIME = value.RI_HCHANGE.RI_HCHANGE_TIME;
                rimodel.RI_HCHANGE_NNAME = value.RI_HCHANGE.RI_HCHANGE_NNAME;
                rimodel.RI_HCHANGE_NTEL = value.RI_HCHANGE.RI_HCHANGE_NTEL;

                value.RI_HCHANGE = rimodel;

                foreach (var item in value.BILL_EMERGENCYMEDICAL)
                {
                    item.RI_ID = value.BILL_HCHANGE_ID;
                }
                foreach (var item in value.BILL_EMERGENCYOTHER)
                {
                    item.RI_ID = value.BILL_HCHANGE_ID;
                }
                bool f = bill.AddRelation(value);
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

        // PUT: api/BILL_HCHANGE/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put([FromBody]BILL_HCHANGE value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
               
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();

              

                var id = value.BILL_HCHANGE_ID;
                var model = await bill.SelectOne(id);
                model.BILL_HCHANGE_STATE = value.BILL_HCHANGE_STATE;
                model.BILL_EMERGENCYMEDICAL = value.BILL_EMERGENCYMEDICAL;
                model.BILL_EMERGENCYOTHER = value.BILL_EMERGENCYOTHER;
                model.BILL_HCHANGE_WHY = value.BILL_HCHANGE_WHY;
                model.RI_HCHANGE.RI_HCHANGE_STATE= value.BILL_HCHANGE_STATE;
                if (value.BILL_HCHANGE_STATE=="需理赔审核" && value.RI_HCHANGE!=null)
                {
                    model.RI_HCHANGE.RI_HCHANGE_ADDRESS = value.RI_HCHANGE.RI_HCHANGE_ADDRESS;
                    model.RI_HCHANGE.RI_HCHANGE_ADDRESSDETAIL = value.RI_HCHANGE.RI_HCHANGE_ADDRESSDETAIL;
                    model.RI_HCHANGE.RI_HCHANGE_CDETAIL = value.RI_HCHANGE.RI_HCHANGE_CDETAIL;
                    model.RI_HCHANGE.RI_HCHANGE_DETAIL = value.RI_HCHANGE.RI_HCHANGE_DETAIL;
                    model.RI_HCHANGE.RI_HCHANGE_DOC = value.RI_HCHANGE.RI_HCHANGE_DOC;
                    model.RI_HCHANGE.RI_HCHANGE_HADDRESS = value.RI_HCHANGE.RI_HCHANGE_HADDRESS;
                    model.RI_HCHANGE.RI_HCHANGE_HEMAIL = value.RI_HCHANGE.RI_HCHANGE_HEMAIL;
                    model.RI_HCHANGE.RI_HCHANGE_HNAME = value.RI_HCHANGE.RI_HCHANGE_HNAME;
                    model.RI_HCHANGE.RI_HCHANGE_NADDRESS = value.RI_HCHANGE.RI_HCHANGE_NADDRESS;
                    model.RI_HCHANGE.RI_HCHANGE_HTEL = value.RI_HCHANGE.RI_HCHANGE_HTEL;
                    model.RI_HCHANGE.RI_HCHANGE_NEMAIL = value.RI_HCHANGE.RI_HCHANGE_NEMAIL;
                    model.RI_HCHANGE.RI_HCHANGE_ID = value.RI_HCHANGE.RI_HCHANGE_ID;
                    model.RI_HCHANGE.RI_HCHANGE_TIME = value.RI_HCHANGE.RI_HCHANGE_TIME;
                    model.RI_HCHANGE.RI_HCHANGE_NNAME = value.RI_HCHANGE.RI_HCHANGE_NNAME;
                    model.RI_HCHANGE.RI_HCHANGE_NTEL = value.RI_HCHANGE.RI_HCHANGE_NTEL;
                }
                bool f;
                if (value.BILL_HCHANGE_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO= model.RI_HCHANGE.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME== "全球紧急救援保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID && p.IPOTHER_NAME== "全球紧急救援保险责任");
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
                                SALEDETAILNO = model.RI_HCHANGE.REPORTINFORMATION.SALEDETAILNO,
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
        // DELETE: api/BILL_HCHANGE/5
        public void Delete(int id)
        {
        }
    }
}

 