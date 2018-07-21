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
    public class BILL_TRAVELDELAYController : ApiController
    {
        BILL_TRAVELDELAYService bill = new BILL_TRAVELDELAYService();        
        [HttpGet]
        [Route("api/BILL_TRAVELDELAY/Compute")]
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
                BILL_TRAVELDELAY billData = await bill.Select(p => p.RI_TRAVELDELAY_ID == id);
                var saledetaino = billData.RI_TRAVELDELAY.REPORTINFORMATION.SALEDETAILNO;
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
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "旅行延误");
                if (liyiItem == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
              
                var charArray = billData.BILL_TRAVELDELAY_TALLTIME.Split('天');
                var day = int.Parse(charArray[0]);               
                var hours = int.Parse(charArray[1].Split('小')[0]) + day * 24;
                decimal payMoney = 0;
                if (hours > 8 && hours <= 16)
                {
                    payMoney = 660 * lilv;
                }
                if (hours > 16)
                {
                    payMoney = 1320 * lilv;
                }
                string paySate = "正常赔付";
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

                billData.BILL_TRAVELDELAY_STATE = "计算完成";
                billData.RI_TRAVELDELAY.RI_TRAVELDELAY_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER
                {
                    BILL_TYPE = "旅行不便保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),
                    BILL_ID = billData.BILL_TRAVELDELAY_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    SALEDETAILNO = billData.RI_TRAVELDELAY.REPORTINFORMATION.SALEDETAILNO,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString()
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
        // GET: api/BILL_TRAVELDELAY/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id)
        {
            AjaxResultT<BILL_TRAVELDELAY> re = new AjaxResultT<BILL_TRAVELDELAY>
            {
                code = 0
            };
            try
            {
                RI_TRAVELDELAY riModel = new RI_TRAVELDELAY();
                var model = await bill.SelectOneByRiAsync(id);

                re.code = 1;
                model.RI_TRAVELDELAY = riModel;
                List<BILL_TRAVELDELAY> list = new List<BILL_TRAVELDELAY> { model };

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_TRAVELDELAY
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody] BILL_TRAVELDELAY value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RI_TRAVELDELAYService ribill = new RI_TRAVELDELAYService();
                var rimodel = await ribill.SelectOne(value.RI_TRAVELDELAY_ID);
                rimodel.RI_TRAVELDELAY_STATE = "需理赔审核";
                value.RI_TRAVELDELAY = rimodel;
                value.BILL_TRAVELDELAY_FACTARBILLVE = value.BILL_TRAVELDELAY_FACTARBILLVE;
                value.BILL_TRAVELDELAY_FACTOUT = value.BILL_TRAVELDELAY_FACTOUT;
                value.BILL_TRAVELDELAY_PLANARBILLVE = value.BILL_TRAVELDELAY_PLANARBILLVE;
                value.BILL_TRAVELDELAY_PLANOUT = value.BILL_TRAVELDELAY_PLANOUT;
                var model = await bill.Add(value);
                if (model.BILL_TRAVELDELAY_ID.Length > 0)
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

        // PUT: api/BILL_TRAVELDELAY/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Put([FromBody]BILL_TRAVELDELAY value)
        {

            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                var model = await bill.SelectOne(value.BILL_TRAVELDELAY_ID);
                model.BILL_TRAVELDELAY_DETAILS = value.BILL_TRAVELDELAY_DETAILS;
                model.BILL_TRAVELDELAY_DOC = value.BILL_TRAVELDELAY_DOC;
                
                model.BILL_TRAVELDELAY_FACTARBILLVE = value.BILL_TRAVELDELAY_FACTARBILLVE;
                model.BILL_TRAVELDELAY_FACTOUT = value.BILL_TRAVELDELAY_FACTOUT;
                model.BILL_TRAVELDELAY_FROM = value.BILL_TRAVELDELAY_FROM;
                model.BILL_TRAVELDELAY_NUMBER = value.BILL_TRAVELDELAY_NUMBER;
                model.BILL_TRAVELDELAY_PLANARBILLVE = value.BILL_TRAVELDELAY_PLANARBILLVE;
                model.BILL_TRAVELDELAY_PLANOUT = value.BILL_TRAVELDELAY_PLANOUT;
                model.BILL_TRAVELDELAY_STATE = value.BILL_TRAVELDELAY_STATE;
                model.BILL_TRAVELDELAY_TALLTIME = value.BILL_TRAVELDELAY_TALLTIME;
                model.BILL_TRAVELDELAY_TO = value.BILL_TRAVELDELAY_TO;
                model.BILL_TRAVELDELAY_WAY = value.BILL_TRAVELDELAY_WAY;
                model.BILL_TRAVELDELAY_WHY = value.BILL_TRAVELDELAY_WHY;
                model.RI_TRAVELDELAY.RI_TRAVELDELAY_STATE = value.BILL_TRAVELDELAY_STATE;
                bool f;
                if (value.BILL_TRAVELDELAY_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO = model.RI_TRAVELDELAY.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME == "旅行不便保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID && p.IPOTHER_NAME == "旅行不便保险责任");
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
                                SALEDETAILNO = model.RI_TRAVELDELAY.REPORTINFORMATION.SALEDETAILNO,
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

