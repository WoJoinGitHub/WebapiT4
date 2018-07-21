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
    public class BILL_HBACKController : ApiController
    {
        BILL_HBACKService bill = new BILL_HBACKService();
        // GET: api/BILL_HBACK
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        [HttpGet]
        [Route("api/BILL_HBACK/Compute")]
        public async Task<IHttpActionResult> ComputeAsync(string id, decimal lilv)
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
                BILL_HBACK billData = bill.SelectRelation(id);
                var SALEDETAILNO = billData.RI_HBACK.REPORTINFORMATION.SALEDETAILNO;
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
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急运返费");
                if (liyiItem == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                decimal? otherCount = 0, medicalCount = 0,medicalCount1=0;
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
               

                billData.BILL_HBACK_STATE = "计算完成";
                billData.RI_HBACK.RI_HBACK_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "全球紧急救援保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero) ,
                    BILL_ID = billData.BILL_HBACK_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = allCount- medicalCount+ medicalCount1,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = billData.RI_HBACK.REPORTINFORMATION.SALEDETAILNO
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
        // GET: api/BILL_HBACK/5
        public IHttpActionResult Get(string id)
        {
            AjaxResultT<BILL_HBACK> re = new AjaxResultT<BILL_HBACK>()
            {
                code = 0
            };
            try
            {
                var model = bill.SelectRelation(id);
                
                re.code = 1;
                RI_HBACK riModel = new RI_HBACK();
                var reModel = model;
                riModel.RI_HBACK_ADDRESS = reModel.RI_HBACK.RI_HBACK_ADDRESS;
                riModel.RI_HBACK_ADDRESSDETAIL = reModel.RI_HBACK.RI_HBACK_ADDRESSDETAIL;
                riModel.RI_HBACK_BILLDOC = reModel.RI_HBACK.RI_HBACK_BILLDOC;
                riModel.RI_HBACK_DETAILS = reModel.RI_HBACK.RI_HBACK_DETAILS;
                riModel.RI_HBACK_DOC = reModel.RI_HBACK.RI_HBACK_DOC;
                riModel.RI_HBACK_HADDRESS = reModel.RI_HBACK.RI_HBACK_HADDRESS;
                riModel.RI_HBACK_HEMAIL = reModel.RI_HBACK.RI_HBACK_HEMAIL;
                riModel.RI_HBACK_HNAME = reModel.RI_HBACK.RI_HBACK_HNAME;
                riModel.RI_HBACK_HTEL = reModel.RI_HBACK.RI_HBACK_HTEL;
                riModel.RI_HBACK_ISUS = reModel.RI_HBACK.RI_HBACK_ISUS;
                riModel.RI_HBACK_TIME = reModel.RI_HBACK.RI_HBACK_TIME;
                riModel.RI_HBACK_YDETAILS = reModel.RI_HBACK.RI_HBACK_YDETAILS;
                riModel.RI_HBACK_WHY = reModel.RI_HBACK.RI_HBACK_WHY;
                reModel.RI_HBACK = riModel;


                List<BILL_HBACK> list = new List<BILL_HBACK> {reModel};

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_HBACK
        public async Task<IHttpActionResult> Post([FromBody] BILL_HBACK value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_HBACKService ribill = new RI_HBACKService();
                var rimodel = await ribill.SelectOne(value.RI_HBACK_ID);
                rimodel.RI_HBACK_ADDRESS = value.RI_HBACK.RI_HBACK_ADDRESS;
                rimodel.RI_HBACK_ADDRESSDETAIL = value.RI_HBACK.RI_HBACK_ADDRESSDETAIL;
                rimodel.RI_HBACK_BILLDOC = value.RI_HBACK.RI_HBACK_BILLDOC;
                rimodel.RI_HBACK_DETAILS = value.RI_HBACK.RI_HBACK_DETAILS;
                rimodel.RI_HBACK_DOC = value.RI_HBACK.RI_HBACK_DOC;
                rimodel.RI_HBACK_HADDRESS = value.RI_HBACK.RI_HBACK_HADDRESS;
                rimodel.RI_HBACK_HEMAIL = value.RI_HBACK.RI_HBACK_HEMAIL;
                rimodel.RI_HBACK_HNAME = value.RI_HBACK.RI_HBACK_HNAME;
                rimodel.RI_HBACK_HTEL = value.RI_HBACK.RI_HBACK_HTEL;
                rimodel.RI_HBACK_ISUS = value.RI_HBACK.RI_HBACK_ISUS;
                rimodel.RI_HBACK_TIME = value.RI_HBACK.RI_HBACK_TIME;
                rimodel.RI_HBACK_YDETAILS = value.RI_HBACK.RI_HBACK_YDETAILS;
                rimodel.RI_HBACK_STATE = "需理赔审核";
                foreach (var item in value.BILL_EMERGENCYMEDICAL)
                {
                    item.RI_ID = value.BILL_HBACK_ID;
                }
                foreach (var item in value.BILL_EMERGENCYOTHER)
                {
                    item.RI_ID = value.BILL_HBACK_ID;
                }
                value.RI_HBACK = rimodel;
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

        // PUT: api/BILL_HBACK/5
        public async Task<IHttpActionResult> Put([FromBody]BILL_HBACK value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
             
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();

              
                var id = value.BILL_HBACK_ID;
                var model = await  bill.SelectOne(id);
                model.BILL_HBACK_STATE = value.BILL_HBACK_STATE;
                model.BILL_EMERGENCYMEDICAL = value.BILL_EMERGENCYMEDICAL;
                model.BILL_EMERGENCYOTHER = value.BILL_EMERGENCYOTHER;
                model.BILL_HBACK_WHY = value.BILL_HBACK_WHY;
                model.RI_HBACK.RI_HBACK_STATE= value.BILL_HBACK_STATE;
              
                if (value.BILL_HBACK_STATE=="需理赔审核" && value.RI_HBACK != null)
                {
                    model.RI_HBACK.RI_HBACK_ADDRESS = value.RI_HBACK.RI_HBACK_ADDRESS;
                    model.RI_HBACK.RI_HBACK_ADDRESSDETAIL = value.RI_HBACK.RI_HBACK_ADDRESSDETAIL;
                    model.RI_HBACK.RI_HBACK_BILLDOC = value.RI_HBACK.RI_HBACK_BILLDOC;
                    model.RI_HBACK.RI_HBACK_DETAILS = value.RI_HBACK.RI_HBACK_DETAILS;
                    model.RI_HBACK.RI_HBACK_DOC = value.RI_HBACK.RI_HBACK_DOC;
                    model.RI_HBACK.RI_HBACK_HADDRESS = value.RI_HBACK.RI_HBACK_HADDRESS;
                    model.RI_HBACK.RI_HBACK_HEMAIL = value.RI_HBACK.RI_HBACK_HEMAIL;
                    model.RI_HBACK.RI_HBACK_HNAME = value.RI_HBACK.RI_HBACK_HNAME;
                    model.RI_HBACK.RI_HBACK_HTEL = value.RI_HBACK.RI_HBACK_HTEL;
                    model.RI_HBACK.RI_HBACK_ISUS = value.RI_HBACK.RI_HBACK_ISUS;
                    model.RI_HBACK.RI_HBACK_TIME = value.RI_HBACK.RI_HBACK_TIME;
                    model.RI_HBACK.RI_HBACK_YDETAILS = value.RI_HBACK.RI_HBACK_YDETAILS;
                }
                
                bool f = false;
                if (value.BILL_HBACK_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();

                    var SALEDETAILNO = model.RI_HBACK.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await  produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await  orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME== "全球紧急救援保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await  pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID && p.IPOTHER_NAME== "全球紧急救援保险责任");
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
                                SALEDETAILNO = model.RI_HBACK.REPORTINFORMATION.SALEDETAILNO,
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


        // DELETE: api/BILL_HBACK/5
        public void Delete(int id)
        {
        }
    }
}

 