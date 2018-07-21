using BLL;
using Helper;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace kjyl.Controllers
{
    public class BILL_HVISITController : ApiController
    {
        BILL_HVISITService bill = new BILL_HVISITService();
        // GET: api/BILL_HVISIT
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        /// <summary>
        /// 计算
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lilv"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/BILL_HVISIT/Compute")]
        public async System.Threading.Tasks.Task<IHttpActionResult> ComputeAsync(string id, decimal lilv)
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
                BILL_HVISIT billData =  bill.SelectRelation(id);
                var SALEDETAILNO = billData.RI_HVISIT.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.BILL_TYPE == "全球紧急救援保险责任" && p.RESULT_OTHER_STATE == "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                //billData.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO此单号是否有未结算的全球紧急救援

                //resultBill.Select(p=>p.s)
                if (liyiModel == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "家属境外慰问探访费");
                if (liyiItem == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }              
                var otherCount = billData.BILL_EMERGENCYOTHER.Sum(p => p.BILL_EMERGENCYOTHER_MONEY);
                var allCount = otherCount;
                decimal payMoney = (allCount.Value - liyiItem.IO_OTHER_ITEM_ZFMONEY.Value * lilv) * liyiItem.IO_OTHER_ITEM_ZFBL.Value / 100;
                string paySate = "正常赔付";
                if (payMoney > liyiItem.IO_OTHER_ITEM_MONEY * lilv)
                {
                    payMoney = liyiItem.IO_OTHER_ITEM_MONEY.Value * lilv;
                    paySate = "超出此项保险责任最高限额";
                }
                if (payMoney > liyiModel.INTEREST_ORDEROTHER_MONEY * lilv)
                {
                    payMoney = liyiModel.INTEREST_ORDEROTHER_MONEY.Value * lilv;
                    paySate = "超出保险类型最高限额";
                }
                billData.BILL_HVISIT_STATE = "计算完成";
                billData.RI_HVISIT.RI_HVISIT_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "全球紧急救援保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),
                   
                    BILL_ID = billData.BILL_HVISIT_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = allCount,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
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
        // GET: api/BILL_HVISIT/5
        public IHttpActionResult Get(string id)
        {
            AjaxResultT<BILL_HVISIT> re = new AjaxResultT<BILL_HVISIT>()
            {
                code = 0
            };
            try
            {
                RI_HVISIT hvistModel = new RI_HVISIT();
                var model = bill.SelectRelation(id);
                hvistModel.RI_HVISIT_ADDRESS = model.RI_HVISIT.RI_HVISIT_ADDRESS;
                hvistModel.RI_HVISIT_ADDRESSDETAIL = model.RI_HVISIT.RI_HVISIT_ADDRESSDETAIL;
                hvistModel.RI_HVISIT_BILLDOC = model.RI_HVISIT.RI_HVISIT_BILLDOC;
                hvistModel.RI_HVISIT_DETAILS = model.RI_HVISIT.RI_HVISIT_DETAILS;
                hvistModel.RI_HVISIT_ID = model.RI_HVISIT.RI_HVISIT_ID;
                hvistModel.RI_HVISIT_ISUS = model.RI_HVISIT.RI_HVISIT_ISUS;
                hvistModel.RI_HVISIT_TIME = model.RI_HVISIT.RI_HVISIT_TIME;
                hvistModel.RI_HVISIT_WHY = model.RI_HVISIT.RI_HVISIT_WHY;
                re.code = 1;
                model.RI_HVISIT = hvistModel;
                List<BILL_HVISIT> list = new List<BILL_HVISIT>
                {
                    model
                };
                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }


        // POST: api/BILL_HVISIT
        public async System.Threading.Tasks.Task<IHttpActionResult> Post([FromBody] BILL_HVISIT value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_HVISITService ribill =  new RI_HVISITService();
                var hvistModel = await ribill.SelectOne(value.RI_HVISIT_ID);
                hvistModel.RI_HVISIT_STATE = "需理赔审核";
                hvistModel.RI_HVISIT_ADDRESS = value.RI_HVISIT.RI_HVISIT_ADDRESS;
                hvistModel.RI_HVISIT_ADDRESSDETAIL = value.RI_HVISIT.RI_HVISIT_ADDRESSDETAIL;
                hvistModel.RI_HVISIT_BILLDOC = value.RI_HVISIT.RI_HVISIT_BILLDOC;
                hvistModel.RI_HVISIT_DETAILS = value.RI_HVISIT.RI_HVISIT_DETAILS;
                hvistModel.RI_HVISIT_ID = value.RI_HVISIT.RI_HVISIT_ID;
                hvistModel.RI_HVISIT_ISUS = value.RI_HVISIT.RI_HVISIT_ISUS;
                hvistModel.RI_HVISIT_TIME = value.RI_HVISIT.RI_HVISIT_TIME;
                value.RI_HVISIT = hvistModel;
                foreach (var item in value.BILL_EMERGENCYOTHER)
                {
                    item.RI_ID = value.BILL_HVISIT_ID;
                }
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

        // PUT: api/BILL_HVISIT/5
        public async Task<IHttpActionResult> Put([FromBody]BILL_HVISIT value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_HVISITService ribill = new RI_HVISITService();
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();

                var hvistModel = await ribill.SelectOne(value.RI_HVISIT_ID);
                hvistModel.RI_HVISIT_STATE = value.BILL_HVISIT_STATE;

                var id = value.BILL_HVISIT_ID;
                var model = await bill.SelectOne(id);
                model.BILL_HVISIT_STATE =  value.BILL_HVISIT_STATE;  
                if (value.BILL_HVISIT_STATE=="需理赔审核" && value.RI_HVISIT != null)
                {
                    hvistModel.RI_HVISIT_ADDRESS = value.RI_HVISIT.RI_HVISIT_ADDRESS;
                    hvistModel.RI_HVISIT_ADDRESSDETAIL = value.RI_HVISIT.RI_HVISIT_ADDRESSDETAIL;
                    hvistModel.RI_HVISIT_BILLDOC = value.RI_HVISIT.RI_HVISIT_BILLDOC;
                    hvistModel.RI_HVISIT_DETAILS = value.RI_HVISIT.RI_HVISIT_DETAILS;
                    hvistModel.RI_HVISIT_ID = value.RI_HVISIT.RI_HVISIT_ID;
                    hvistModel.RI_HVISIT_ISUS = value.RI_HVISIT.RI_HVISIT_ISUS;
                    hvistModel.RI_HVISIT_TIME = value.RI_HVISIT.RI_HVISIT_TIME;
                }             
                model.BILL_EMERGENCYOTHER = value.BILL_EMERGENCYOTHER;
                model.RI_HVISIT = hvistModel;
                bool f = false;
                if (value.BILL_HVISIT_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO = model.RI_HVISIT.REPORTINFORMATION.SALEDETAILNO;
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
                                SALEDETAILNO = model.RI_HVISIT.REPORTINFORMATION.SALEDETAILNO,
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


        // DELETE: api/BILL_HVISIT/5
        public void Delete(int id)
        {
        }
    }
}
