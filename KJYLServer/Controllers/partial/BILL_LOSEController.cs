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
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public class BILL_LOSEController : ApiController
    {
        BILL_LOSEService bill = new BILL_LOSEService();        
        [HttpGet]
        [Route("api/BILL_LOSE/Compute")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Compute(string id, decimal lilv)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                //检查是否有未完成支付的报案
                //获取此保险的利益表
                RESULT_OTHERService resultBill = new RESULT_OTHERService();
                INTEREST_ORDEROTHERService liyiBill = new INTEREST_ORDEROTHERService();
                BILL_LOSE billData = bill.SelectRelation(id);
                var saledetaino = billData.RI_LOSE.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == saledetaino && p.BILL_TYPE == "旅行不便保险责任" && p.RESULT_OTHER_STATE == "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == saledetaino && p.INTEREST_ORDEROTHER_NAME == "旅行不便保险责任");
                //billData.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO此单号是否有未结算的全球紧急救援

                //resultBill.Select(p=>p.s)
                if (liyiModel == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "旅行证件遗失");
                var liyiItem1 = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "随身财产损失或丢失");
                if (liyiItem == null || liyiItem1==null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
               
                var allCountLvX = billData.BILL_EMERGENCYOTHER.Where(p=>p.BILL_EMERGENCYOTHER_TYPE=="旅行证件").Sum(p => p.BILL_EMERGENCYOTHER_MONEY);
                var allCountSuiS = billData.BILL_EMERGENCYOTHER.Where(p => p.BILL_EMERGENCYOTHER_TYPE == "随身财物").Sum(p => p.BILL_EMERGENCYOTHER_MONEY);
                string paySate = "正常赔付";
                var allCount1 = allCountLvX;
                if (allCountLvX > liyiItem.IO_OTHER_ITEM_MONEY * lilv)
                {
                    allCount1 =  liyiItem.IO_OTHER_ITEM_MONEY * lilv;
                    paySate = "超过分项限额";
                }
               var  allCount2 = allCountSuiS;
                if (allCountSuiS > liyiItem1.IO_OTHER_ITEM_MONEY * lilv)
                {
                    allCount2 = liyiItem1.IO_OTHER_ITEM_MONEY * lilv;
                    paySate = "超过分项限额";
                }
              
                var payMoney = allCount1.Value + allCount2.Value;
                if(payMoney > liyiModel.INTEREST_ORDEROTHER_MONEY * lilv)
                {
                    payMoney = liyiModel.INTEREST_ORDEROTHER_MONEY.Value * lilv;
                    paySate = "超出保险类型最高限额";
                }
                billData.BILL_LOSE_STATE = "计算完成";
                billData.RI_LOSE.RI_LOSE_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "旅行不便保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),
                    BILL_ID = billData.BILL_LOSE_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_ZHENGJIAN = allCount1,
                    RESULT_OTHER_SUISHEN = allCount2,
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = allCountLvX + allCountSuiS,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = billData.RI_LOSE.REPORTINFORMATION.SALEDETAILNO
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
        // GET: api/BILL_LOSE/5
        public IHttpActionResult Get(string id)
        {
            AjaxResultT<BILL_LOSE> re = new AjaxResultT<BILL_LOSE>
            {
                code = 0
            };
            try
            {
                RI_LOSE riModel = new RI_LOSE();
                var model = bill.SelectRelation(id);

                re.code = 1;
                model.RI_LOSE = riModel;
                List<BILL_LOSE> list = new List<BILL_LOSE> {model};

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }
            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_LOSE
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody] BILL_LOSE value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_LOSEService ribill = new RI_LOSEService();
                var rimodel = await ribill.SelectOne(value.RI_LOSE_ID);
                rimodel.RI_LOSE_STATE = "需理赔审核";
                value.RI_LOSE = rimodel;
                var f = bill.AddRelation(value);
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

        // PUT: api/BILL_LOSE/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put([FromBody]BILL_LOSE value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_LOSEService ribill = new RI_LOSEService();
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();

                var rimodel = await ribill.SelectOne(value.RI_LOSE_ID);
                rimodel.RI_LOSE_STATE = value.BILL_LOSE_STATE;

                var id = value.BILL_LOSE_ID;
                var model = await bill.SelectOne(id);
                model.BILL_LOSE_STATE = value.BILL_LOSE_STATE;
                model.BILL_LOSE_DOC = value.BILL_LOSE_DOC;
                model.BILL_EMERGENCYOTHER = value.BILL_EMERGENCYOTHER;
                model.BILL_LOSE_WHY = value.BILL_LOSE_WHY;
                model.RI_LOSE = rimodel;
               
                bool f = false;
                if (value.BILL_LOSE_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO= model.RI_LOSE.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME== "旅行不便保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await pItemBill.Select(p => p.PRODUCTNO == saleDetail.SALESDETAIL_ID && p.IPOTHER_NAME== "旅行不便保险责任");
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
                                SALEDETAILNO = model.RI_LOSE.REPORTINFORMATION.SALEDETAILNO,
                                INTEREST_ORDEROTHER_ID = CreateId.GetId()
                            };
                        //保险单号
                        List<IO_OTHER_ITEM> listItem = new List<IO_OTHER_ITEM>();
                        foreach (var item in reModel.IPOTHER_ITEM)
                        {
                            IO_OTHER_ITEM liyiOreder = new IO_OTHER_ITEM
                            {
                                IO_OTHER_ITEM_ID = CreateId.GetId(),
                                IO_OTHER_ITEM_MONEY = item.IPOTHER_ITEM_MONEY,
                                IO_OTHER_ITEM_NAME = item.IPOTHER_ITEM_NAME,
                                IO_OTHER_ITEM_ZFBL = item.IPOTHER_ITEM_ZFBL,
                                IO_OTHER_ITEM_ZFMONEY = item.IPOTHER_ITEM_ZFMONEY
                            };
                            listItem.Add(liyiOreder);

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
    }
}

 