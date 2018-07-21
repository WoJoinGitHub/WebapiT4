using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Model;
using BLL;
using Helper;
using Newtonsoft.Json;

namespace kjyl.Controllers
{
    public class BILL_HBURIALController : ApiController
    {
        BILL_HBURIALService bill = new BILL_HBURIALService();
        // GET: api/BILL_HBURIAL
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        [HttpGet]
        [Route("api/BILL_HBURIAL/Compute")]
        public async Task<IHttpActionResult> Compute(string id, decimal lilv)
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
                BILL_HBURIAL billData =  bill.SelectRelation(id);
                var SALEDETAILNO = billData.RI_HBURIAL.REPORTINFORMATION.SALEDETAILNO;
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
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "遗体运返或安葬费");
                if (liyiItem == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                decimal? lingjiuMoney = 0;
                var lingjiuCount = billData.BILL_HBURIAL_MONEY;
                if(lingjiuCount!=null && lingjiuCount != 0)
                {

              
                var liyiItemLj = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME ==billData.BILL_HBURIAL_TYPE);
                //灵柩费
               
                if (liyiItemLj != null)
                {
                     lingjiuMoney = (lingjiuCount - liyiItemLj.IO_OTHER_ITEM_ZFMONEY * lilv) * liyiItemLj.IO_OTHER_ITEM_ZFBL / 100;

                    lingjiuMoney = lingjiuMoney > liyiItemLj.IO_OTHER_ITEM_MONEY * lilv ? liyiItemLj.IO_OTHER_ITEM_MONEY * lilv : lingjiuMoney;
                }
                }
                decimal? otherCount = 0;                
                if (billData.BILL_EMERGENCYOTHER != null && billData.BILL_EMERGENCYOTHER.Count > 0)
                {
                    otherCount = billData.BILL_EMERGENCYOTHER.Sum(p => p.BILL_EMERGENCYOTHER_MONEY);
                }             
                var allCount = otherCount;
                decimal payMoney = (allCount.Value - liyiItem.IO_OTHER_ITEM_ZFMONEY.Value * lilv) * liyiItem.IO_OTHER_ITEM_ZFBL.Value / 100+ lingjiuMoney.Value;
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

                billData.BILL_HBURIAL_STATE = "计算完成";
                billData.RI_HBURIAL.RI_HBURIAL_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "全球紧急救援保险责任",
                    RESULT_OTHER_PAY =Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),
                    BILL_ID = billData.BILL_HBURIAL_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = allCount,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = billData.RI_HBURIAL.REPORTINFORMATION.SALEDETAILNO
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
        // GET: api/BILL_HBURIAL/5
        public IHttpActionResult Get(string id)
        {
            AjaxResultT<BILL_HBURIAL> re = new AjaxResultT<BILL_HBURIAL>
            {
                code = 0
            };
            try
            {
                RI_HBURIAL hburialModel = new RI_HBURIAL();
                var model = bill.SelectRelation(id);
               var model5 = model.RI_HBURIAL;
                hburialModel.RI_HBURIAL_ADDRESS = model5.RI_HBURIAL_ADDRESS;
                hburialModel.RI_HBURIAL_ADDRESSDETAIL = model5.RI_HBURIAL_ADDRESSDETAIL;
                hburialModel.RI_HBURIAL_BILLDOC = model5.RI_HBURIAL_BILLDOC;
                hburialModel.RI_HBURIAL_DETAILS = model5.RI_HBURIAL_DETAILS;
                hburialModel.RI_HBURIAL_ISLZ = model5.RI_HBURIAL_ISLZ;
                hburialModel.RI_HBURIAL_ISUS = model5.RI_HBURIAL_ISUS;
                hburialModel.RI_HBURIAL_TIME = model5.RI_HBURIAL_TIME;
                hburialModel.RI_HBURIAL_TYPE = model5.RI_HBURIAL_TYPE;
                hburialModel.RI_HBURIAL_ID = model5.RI_HBURIAL_ID;
                hburialModel.RI_HBURIAL_WHY = model5.RI_HBURIAL_WHY;
                re.code = 1;
                model.RI_HBURIAL = hburialModel;
                List<BILL_HBURIAL> list = new List<BILL_HBURIAL> {model};

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_HBURIAL
        public async Task<IHttpActionResult> Post([FromBody] BILL_HBURIAL value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_HBURIALService ribill = new RI_HBURIALService();
                var hburialModel = await ribill.SelectOne(value.RI_HBURIAL_ID);
                hburialModel.RI_HBURIAL_STATE = "需理赔审核";
                hburialModel.RI_HBURIAL_ADDRESS = value.RI_HBURIAL.RI_HBURIAL_ADDRESS;
                hburialModel.RI_HBURIAL_ADDRESSDETAIL = value.RI_HBURIAL.RI_HBURIAL_ADDRESSDETAIL;
                hburialModel.RI_HBURIAL_BILLDOC = value.RI_HBURIAL.RI_HBURIAL_BILLDOC;
                hburialModel.RI_HBURIAL_DETAILS = value.RI_HBURIAL.RI_HBURIAL_DETAILS;
                hburialModel.RI_HBURIAL_ISLZ = value.RI_HBURIAL.RI_HBURIAL_ISLZ;
                hburialModel.RI_HBURIAL_ISUS = value.RI_HBURIAL.RI_HBURIAL_ISUS;
                hburialModel.RI_HBURIAL_TIME = value.RI_HBURIAL.RI_HBURIAL_TIME;
                hburialModel.RI_HBURIAL_TYPE = value.RI_HBURIAL.RI_HBURIAL_TYPE;
                hburialModel.RI_HBURIAL_ID = value.RI_HBURIAL.RI_HBURIAL_ID;
                value.RI_HBURIAL = hburialModel;
                foreach (var item in value.BILL_EMERGENCYOTHER)
                {
                    item.RI_ID = value.BILL_HBURIAL_ID;
                }
                var model = bill.AddRelation(value);
                if (model)
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

        // PUT: api/BILL_HBURIAL/5
        public async Task<IHttpActionResult> Put([FromBody]BILL_HBURIAL value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RI_HBURIALService ribill = new RI_HBURIALService();
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();

                var hburialModel = await ribill.SelectOne(value.RI_HBURIAL_ID);
                hburialModel.RI_HBURIAL_STATE = value.BILL_HBURIAL_STATE;
               if(value.BILL_HBURIAL_STATE=="需理赔审核"&& value.RI_HBURIAL != null)
                {
                    hburialModel.RI_HBURIAL_ADDRESS = value.RI_HBURIAL.RI_HBURIAL_ADDRESS;
                    hburialModel.RI_HBURIAL_ADDRESSDETAIL = value.RI_HBURIAL.RI_HBURIAL_ADDRESSDETAIL;
                    hburialModel.RI_HBURIAL_BILLDOC = value.RI_HBURIAL.RI_HBURIAL_BILLDOC;
                    hburialModel.RI_HBURIAL_DETAILS = value.RI_HBURIAL.RI_HBURIAL_DETAILS;
                    hburialModel.RI_HBURIAL_ISLZ = value.RI_HBURIAL.RI_HBURIAL_ISLZ;
                    hburialModel.RI_HBURIAL_ISUS = value.RI_HBURIAL.RI_HBURIAL_ISUS;
                    hburialModel.RI_HBURIAL_TIME = value.RI_HBURIAL.RI_HBURIAL_TIME;
                    hburialModel.RI_HBURIAL_TYPE = value.RI_HBURIAL.RI_HBURIAL_TYPE;
                    hburialModel.RI_HBURIAL_ID = value.RI_HBURIAL.RI_HBURIAL_ID;
                }
                var id = value.BILL_HBURIAL_ID;
                var model = await  bill.SelectOne(id);
                model.BILL_HBURIAL_STATE = value.BILL_HBURIAL_STATE;
                model.BILL_HBURIAL_WHY = value.BILL_HBURIAL_WHY;

                model.BILL_EMERGENCYOTHER = value.BILL_EMERGENCYOTHER;
                model.RI_HBURIAL = hburialModel;
                bool f;
                if (value.BILL_HBURIAL_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO= model.RI_HBURIAL.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await  pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID && p.IPOTHER_NAME == "全球紧急救援保险责任");
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
                                SALEDETAILNO = model.RI_HBURIAL.REPORTINFORMATION.SALEDETAILNO,
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

        // DELETE: api/BILL_HBURIAL/5
        public void Delete(int id)
        {
        }
    }
}

 