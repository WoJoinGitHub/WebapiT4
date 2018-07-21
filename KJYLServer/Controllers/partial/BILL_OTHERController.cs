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
    public class BILL_OTHERController : ApiController
    {
        BILL_OTHERService bill = new BILL_OTHERService();       
        [HttpGet]
        [Route("api/BILL_OTHER/Compute")]
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
                BILL_OTHER billData = await bill.Select(p=>p.RI_OTHER_ID==id);
                var saledetaino = billData.RI_OTHER.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == saledetaino && p.BILL_TYPE == "个人第三者责任保险责任" && p.RESULT_OTHER_STATE == "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == saledetaino && p.INTEREST_ORDEROTHER_NAME == "个人第三者保险责任");
                //billData.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO此单号是否有未结算的全球紧急救援

                //resultBill.Select(p=>p.s)
                if (liyiModel == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                string paySate = "正常赔付";
                var payMoney = billData.BILL_OTHER_ALLMONEY.Value;     
                if(payMoney > liyiModel.INTEREST_ORDEROTHER_MONEY * lilv)
                {
                    payMoney = liyiModel.INTEREST_ORDEROTHER_MONEY.Value * lilv;
                    paySate = "超出保险类型最高限额";
                }
             

                billData.BILL_OTHER_STATE = "计算完成";
                billData.RI_OTHER.RI_OTHER_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "个人第三者责任保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),
                    BILL_ID = billData.BILL_OTHER_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = billData.BILL_OTHER_ALLMONEY,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = billData.RI_OTHER.REPORTINFORMATION.SALEDETAILNO
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
        // GET: api/BILL_OTHER/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id)
        {
            AjaxResultT<BILL_OTHER> re = new AjaxResultT<BILL_OTHER>
            {
                code = 0
            };
            try
            {
                RI_OTHER riModel = new RI_OTHER();
                var model = await bill.SelectOneByRiAsync(id);

                re.code = 1;
                model.RI_OTHER = riModel;
                List<BILL_OTHER> list = new List<BILL_OTHER> {model};

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }


        // POST: api/BILL_OTHER
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody] BILL_OTHER value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_OTHERService ribill = new RI_OTHERService();
                var rimodel = await ribill.SelectOne(value.RI_OTHER_ID);
                rimodel.RI_OTHER_STATE = "需理赔审核";
                value.RI_OTHER = rimodel;
                var model = await bill.Add(value);
                if (model.BILL_OTHER_ID.Length > 0)
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

        // PUT: api/BILL_OTHER/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put([FromBody]BILL_OTHER value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                var id = value.BILL_OTHER_ID;
                var model = await bill.SelectOne(id);
                model.BILL_OTHER_STATE = value.BILL_OTHER_STATE;
                model.RI_OTHER.RI_OTHER_STATE = value.BILL_OTHER_STATE;
                model.BILL_OTHER_ADDRESS = value.BILL_OTHER_ADDRESS;
                model.BILL_OTHER_ADDRESSDETAILS = value.BILL_OTHER_ADDRESSDETAILS;
                model.BILL_OTHER_ALLMONEY = value.BILL_OTHER_ALLMONEY;
                model.BILL_OTHER_ASK = value.BILL_OTHER_ASK;
                model.BILL_OTHER_DESCBILLBE = value.BILL_OTHER_DESCBILLBE;
                model.BILL_OTHER_DETAILS = value.BILL_OTHER_DETAILS;
                model.BILL_OTHER_DOC = value.BILL_OTHER_DOC;
                model.BILL_OTHER_ISLAW = value.BILL_OTHER_ISLAW;
                model.BILL_OTHER_REASON = value.BILL_OTHER_REASON;
                model.BILL_OTHER_TIME = value.BILL_OTHER_TIME;                
                model.BILL_OTHER_WHY = value.BILL_OTHER_WHY;
               
                bool f = false;
                if (value.BILL_OTHER_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO = model.RI_OTHER.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME== "个人第三者保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID &&p.IPOTHER_NAME== "个人第三者保险责任");
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
                                PRODUCTNO = reModel.PRODUCTNO,
                                SALEDETAILNO = model.RI_OTHER.REPORTINFORMATION.SALEDETAILNO,
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
                        f = await bill.Updata(model);
                    }
                }
                else
                {
                    f = await bill.Updata(model);
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

 