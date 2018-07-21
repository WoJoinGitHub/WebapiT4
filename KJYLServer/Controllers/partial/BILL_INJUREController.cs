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
    public class BILL_INJUREController : ApiController
    {
        BILL_INJUREService bill = new BILL_INJUREService();       
        [HttpGet]
        [Route("api/BILL_INJURE/Compute")]
        public async System.Threading.Tasks.Task<IHttpActionResult> ComputeAsync(string id)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RESULT_OTHERService resultBill = new RESULT_OTHERService();
                //意外伤害没有汇率
                //获取此保险的利益表

                INTEREST_ORDEROTHERService liyiBill = new INTEREST_ORDEROTHERService();
                var billData = await bill.Select(p => p.RI_INJURE_ID == id);
                var SALEDETAILNO = billData.RI_INJURE.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.BILL_TYPE == "意外伤害保险责任" && p.RESULT_OTHER_STATE == "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                billData.BILL_INJURE_STATE = "计算完成";
                billData.RI_INJURE.RI_INJURE_STATE = "计算完成";
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME == "意外伤害保险责任");
                if (liyiModel == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                //计算
                decimal money =  liyiModel.INTEREST_ORDEROTHER_MONEY == null ? 0 : liyiModel.INTEREST_ORDEROTHER_MONEY.Value;
                if (money <= 0)
                {
                    re.msg = "此保险责任已赔付完";
                    return Json(re);
                }
                var level =
                    new Dictionary<string, int>
                    {
                        {"一级", 100},
                        {"二级", 90},
                        {"三级", 80},
                        {"四级", 70},
                        {"五级", 60},
                        {"六级", 50},
                        {"七级", 40},
                        {"八级", 30},
                        {"九级", 20},
                        {"十级", 10}
                    };
                var bili = level.FirstOrDefault(p => p.Key == billData.BILL_INJURE_AUTHLEVEL);
                decimal resultMoney = money * bili.Value / 100;
                string paySate = "正常赔付";
                if (resultMoney> liyiModel.INTEREST_ORDEROTHER_MONEY)
                {
                    resultMoney = liyiModel.INTEREST_ORDEROTHER_MONEY.Value;
                    paySate = "超出保险类型最高限额";
                }
                //billData.BILL_INJURE_AUTHLEVEL
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER
                {
                    BILL_TYPE = "意外伤害保险责任",
                    RESULT_OTHER_PAY = Math.Round(resultMoney, 2, MidpointRounding.AwayFromZero),                    
                    BILL_ID = billData.BILL_INJURE_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_PAYSTATE = paySate,
                    SALEDETAILNO = SALEDETAILNO
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
        // GET: api/BILL_INJURE/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id)
        {
            AjaxResultT<dynamic> re = new AjaxResultT<dynamic>()
            {
                code = 0
            };
            try
            {
                var model = await bill.Select(p => p.RI_INJURE_ID == id);
                dynamic resultModel = new
                {
                    model.BILL_INJURE_ID,
                    model.BILL_INJURE_AUTHDOC,
                    model.BILL_INJURE_AUTHTIME,
                    model.BILL_INJURE_ADDRESS,
                    model.BILL_INJURE_ADDRESSDETAIL,
                    model.BILL_INJURE_AUTHLEVEL,
                    model.BILL_INJURE_AUTHREASON,
                    model.BILL_INJURE_DETAILS,
                    model.BILL_INJURE_HTIME,
                    model.BILL_INJURE_PROCESS,
                    model.BILL_INJURE_TIME,
                    model.RI_INJURE_ID,
                    model.BILL_INJURE_STATE,
                    model.BILL_INJURE_WHY

                };
                re.code = 1;

                var list = new List<dynamic> { resultModel };

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_INJURE
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody] BILL_INJURE value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_INJUREService ribill = new RI_INJUREService();
                var rimodel = await ribill.SelectOne(value.RI_INJURE_ID);
                rimodel.RI_INJURE_STATE = "需理赔审核";
                value.RI_INJURE = rimodel;
                var model = await bill.Add(value);
                if (model.BILL_INJURE_ID.Length > 0)
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

        // PUT: api/BILL_INJURE/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put([FromBody]BILL_INJURE value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                var model = await bill.SelectOne(value.BILL_INJURE_ID);

                model.BILL_INJURE_AUTHDOC = value.BILL_INJURE_AUTHDOC;
                model.BILL_INJURE_AUTHTIME = value.BILL_INJURE_AUTHTIME;
                model.BILL_INJURE_ADDRESS = value.BILL_INJURE_ADDRESS;
                model.BILL_INJURE_ADDRESSDETAIL = value.BILL_INJURE_ADDRESSDETAIL;
                model.BILL_INJURE_AUTHLEVEL = value.BILL_INJURE_AUTHLEVEL;
                model.BILL_INJURE_AUTHREASON = value.BILL_INJURE_AUTHREASON;
                model.BILL_INJURE_DETAILS = value.BILL_INJURE_DETAILS;
                model.BILL_INJURE_HTIME = value.BILL_INJURE_HTIME;
                model.BILL_INJURE_PROCESS = value.BILL_INJURE_PROCESS;
                model.BILL_INJURE_TIME = value.BILL_INJURE_TIME;
                model.BILL_INJURE_STATE = value.BILL_INJURE_STATE;
                model.RI_INJURE.RI_INJURE_STATE = value.BILL_INJURE_STATE;
                model.BILL_INJURE_WHY = value.BILL_INJURE_WHY;
                
                bool f;
                if (value.BILL_INJURE_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO = model.RI_INJURE.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME == "意外伤害保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID && p.IPOTHER_NAME == "意外伤害保险责任");
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
                                SALEDETAILNO = model.RI_INJURE.REPORTINFORMATION.SALEDETAILNO,
                                INTEREST_ORDEROTHER_ID = CreateId.GetId()
                            };
                        //保险单号
                        List<IO_OTHER_ITEM> listItem = new List<IO_OTHER_ITEM>();
                        foreach (var item in reModel.IPOTHER_ITEM)
                        {
                            var liyiOreder = new IO_OTHER_ITEM
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
