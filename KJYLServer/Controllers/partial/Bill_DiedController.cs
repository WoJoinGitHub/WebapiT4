using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

using Helper;
using BLL;
using Newtonsoft.Json;
using Model;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public class Bill_DiedController : ApiController
    {
        Bill_DiedService bill = new Bill_DiedService();
        // GET: api/Bill_Died
        public IEnumerable<string> Get()
        {          
            return new string[] { "value1", "value2" };
        }
        [HttpGet]
        [Route("api/Bill_Died/Compute")]
        public async Task<IHttpActionResult> ComputeAsync(string  id)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                //意外伤害没有汇率
                //获取此保险的利益表
                RESULT_OTHERService resultBill = new RESULT_OTHERService();
                INTEREST_ORDEROTHERService liyiBill = new INTEREST_ORDEROTHERService();
                BILL_DIED billData =await  bill.SelectOneByRiAsync(id);
                var SALEDETAILNO = billData.RI_DIED.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.BILL_TYPE == "意外伤害保险责任" && p.RESULT_OTHER_STATE == "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                billData.BILL_DIED_STATE = "计算完成";
                billData.RI_DIED.RI_DIED_STATE = "计算完成";
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var  liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME=="意外伤害保险责任");
                if (liyiModel == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                //是否是未成年人
                //计算
                decimal money = liyiModel.INTEREST_ORDEROTHER_MONEY == null ? 0 : liyiModel.INTEREST_ORDEROTHER_MONEY.Value;
                if (money <= 0)
                {
                    re.msg = "此保险责任已赔付完";
                    return Json(re);
                }
                else
                {
                    //是否是未成年人
                    //获取出生日期
                    SALESDETAILService salebill = new SALESDETAILService();
                    CUSTOMService custombill = new CUSTOMService();
                    var saleId = billData.RI_DIED.REPORTINFORMATION.SALEDETAILNO;
                   var saleModel = await salebill.SelectOne(p=>p.SALESDETAIL_ID== saleId);
                    var customModel = (await  custombill.SelectOne(p => p.CUSTOM_ID == saleModel.CUSTOM_ID)).CUSTOM_BIRTHDATE;
                    string paySate = "正常赔付";
                    bool f = CreateId.TimeYang(customModel.Value, billData.BILL_DIED_HTIME.Value);
                    if (!f)
                    {
                        paySate = "当事人未成年";
                        money = money > 100000 ? 100000 : money;
                    }
                    //将次结果计入计算结果表
                    RESULT_OTHER model = new RESULT_OTHER
                    {
                        BILL_TYPE = "意外伤害保险责任",
                        RESULT_OTHER_PAY = Math.Round(money, 2, MidpointRounding.AwayFromZero),
                        BILL_ID = billData.BILL_DIED_ID,
                        RESULT_OTHER_ID = CreateId.GetId(),
                        RESULT_OTHER_STATE = "计算完成",
                        RESULT_OTHER_PAYSTATE = paySate,                        
                        SALEDETAILNO = saleId
                    };
                    var addResult = resultBill.Acomput(model,billData);
                    if (addResult)
                    {
                        re.code = 1;
                        re.msg = "计算完成";
                    }
                }
              
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }          
            return Json(re);
        }
       
        // GET: api/Bill_Died/5
        public async Task<IHttpActionResult> Get(string id)
        {
            AjaxResultT<dynamic> re = new AjaxResultT<dynamic>
            {
                code = 0
            };
            try
            {
                var model = await bill.SelectOneByRiAsync(id);               
                dynamic resultModel = new
                {
                    model.BILL_DIED_ID,
                    model.BILL_DIED_DOC,
                    model.BILL_DIED_TIME,
                    model.BILL_DIED_HTIME,
                    model.BILL_DIED_ADDRESS,
                    model.BILL_DIED_ADDRESSDETAIL,
                    model.BILL_DIED_STATE,
                    model.BILL_DIED_WHY,
                    model.BILL_DIED_DETAIL

                };
                re.code = 1;
                var list = new List<dynamic> {resultModel};
                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/Bill_Died
        public async Task<IHttpActionResult> Post([FromBody] BILL_DIED value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RI_DIEDService ribill = new RI_DIEDService();
                var rimodel = await ribill.SelectOne(value.RI_DIED_ID);
                rimodel.RI_DIED_STATE = "需理赔审核";
                value.RI_DIED = rimodel;
                var model = await bill.Add(value);
                if (model.BILL_DIED_ID.Length > 0)
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

        // PUT: api/Bill_Died/5
        public async Task<IHttpActionResult> Put([FromBody]BILL_DIED value)
        {

            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                SALESDETAILService produceBill = new SALESDETAILService();
                var model = await bill.SelectOne(value.BILL_DIED_ID);
                model.BILL_DIED_ADDRESS = value.BILL_DIED_ADDRESS;
                model.BILL_DIED_ADDRESSDETAIL = value.BILL_DIED_ADDRESSDETAIL;
                model.BILL_DIED_DETAIL = value.BILL_DIED_DETAIL;
                model.BILL_DIED_DOC = value.BILL_DIED_DOC;
                model.BILL_DIED_HTIME = value.BILL_DIED_HTIME;
                model.BILL_DIED_STATE = value.BILL_DIED_STATE;
                model.BILL_DIED_TIME = value.BILL_DIED_TIME;
                model.BILL_DIED_WHY = value.BILL_DIED_WHY;
                model.RI_DIED.RI_DIED_STATE = value.BILL_DIED_STATE;
                bool f;
                //pBill.SelectOne();
                if (value.BILL_DIED_STATE == "需计算")
                {
                    //利益表

                    var id = model.RI_DIED.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == id);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == id && p.INTEREST_ORDEROTHER_NAME== "意外伤害保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();
                        var reModel = await pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID && p.IPOTHER_NAME== "意外伤害保险责任"); 
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
                                SALEDETAILNO = model.RI_DIED.REPORTINFORMATION.SALEDETAILNO,
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
                        f = await bill.UpdateLiyi(model, otherModel);
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
