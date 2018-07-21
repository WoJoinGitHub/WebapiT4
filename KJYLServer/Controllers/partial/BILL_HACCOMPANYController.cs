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
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    public class BILL_HACCOMPANYController : ApiController
    {
        BILL_HACCOMPANYService bill = new BILL_HACCOMPANYService();
        // GET: api/BILL_HACCOMPANY        
        [HttpGet]
        [Route("api/BILL_HACCOMPANY/Compute")]
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
                BILL_HACCOMPANY billData = bill.SelectRelation(id);
                var SALEDETAILNO = billData.RI_HACCOMPANY.REPORTINFORMATION.SALEDETAILNO;
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
                var liyiItem = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急转院或运返陪同费");
                if (liyiItem == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                decimal? otherCount = 0;
                if (billData.BILL_EMERGENCYOTHER != null && billData.BILL_EMERGENCYOTHER.Count > 0)
                {
                     otherCount = billData.BILL_EMERGENCYOTHER.Sum(p => p.BILL_EMERGENCYOTHER_MONEY);
                }
              
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

                billData.BILL_HACCOMPANY_STATE = "计算完成";
                billData.RI_HACCOMPANY.RI_HACCOMPANY_STATE = "计算完成";
                //将次结果计入计算结果表
                RESULT_OTHER model = new RESULT_OTHER()
                {
                    BILL_TYPE = "全球紧急救援保险责任",
                    RESULT_OTHER_PAY = Math.Round(payMoney, 2, MidpointRounding.AwayFromZero),                  
                    BILL_ID = billData.BILL_HACCOMPANY_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_COST = allCount,
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = billData.RI_HACCOMPANY.REPORTINFORMATION.SALEDETAILNO
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
        // GET: api/BILL_HACCOMPANY/5
        public IHttpActionResult Get(string id)
        {
            AjaxResultT<BILL_HACCOMPANY> re = new AjaxResultT<BILL_HACCOMPANY>()
            {
                code = 0
            };
            try
            {
                RI_HACCOMPANY haccompanyModel = new RI_HACCOMPANY();
                var model = bill.SelectRelation(id);
                haccompanyModel.RI_HACCOMPANY_ID = model.RI_HACCOMPANY.RI_HACCOMPANY_ID;
                haccompanyModel.RI_HACCOMPANY_ADDRESS = model.RI_HACCOMPANY.RI_HACCOMPANY_ADDRESS;
                haccompanyModel.RI_HACCOMPANY_ADDRESSDETAIL = model.RI_HACCOMPANY.RI_HACCOMPANY_ADDRESSDETAIL;
                haccompanyModel.RI_HACCOMPANY_BILLDOC = model.RI_HACCOMPANY.RI_HACCOMPANY_BILLDOC;
                haccompanyModel.RI_HACCOMPANY_DETAILS = model.RI_HACCOMPANY.RI_HACCOMPANY_DETAILS;
                haccompanyModel.RI_HACCOMPANY_ISUS = model.RI_HACCOMPANY.RI_HACCOMPANY_ISUS;
                haccompanyModel.RI_HACCOMPANY_TIME = model.RI_HACCOMPANY.RI_HACCOMPANY_TIME;
                haccompanyModel.RI_HACCOMPANY_WHY = model.RI_HACCOMPANY.RI_HACCOMPANY_WHY;
                re.code = 1;
                model.RI_HACCOMPANY = haccompanyModel;
                List<BILL_HACCOMPANY> list = new List<BILL_HACCOMPANY> {model};
                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_HACCOMPANY
        public async Task<IHttpActionResult> Post([FromBody] BILL_HACCOMPANY value)
        {
            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                RI_HACCOMPANYService ribill = new RI_HACCOMPANYService();
                var rimodel = await ribill.SelectOne(value.RI_HACCOMPANY_ID);
                rimodel.RI_HACCOMPANY_STATE = "需理赔审核";
                rimodel.RI_HACCOMPANY_ID = value.RI_HACCOMPANY_ID;
                rimodel.RI_HACCOMPANY_ADDRESS = value.RI_HACCOMPANY.RI_HACCOMPANY_ADDRESS;
                rimodel.RI_HACCOMPANY_ADDRESSDETAIL = value.RI_HACCOMPANY.RI_HACCOMPANY_ADDRESSDETAIL;
                rimodel.RI_HACCOMPANY_BILLDOC = value.RI_HACCOMPANY.RI_HACCOMPANY_BILLDOC;
                rimodel.RI_HACCOMPANY_DETAILS = value.RI_HACCOMPANY.RI_HACCOMPANY_DETAILS;
                rimodel.RI_HACCOMPANY_ISUS = value.RI_HACCOMPANY.RI_HACCOMPANY_ISUS;
                rimodel.RI_HACCOMPANY_TIME = value.RI_HACCOMPANY.RI_HACCOMPANY_TIME;
                value.RI_HACCOMPANY = rimodel;
                foreach (var item in value.BILL_EMERGENCYOTHER)
                {
                    item.RI_ID = value.BILL_HACCOMPANY_ID;
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

        // PUT: api/BILL_HACCOMPANY/5
        public async Task<IHttpActionResult> Put([FromBody]BILL_HACCOMPANY value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RI_HACCOMPANYService ribill = new RI_HACCOMPANYService();
                BILL_EMERGENCYMEDICALService embill = new BILL_EMERGENCYMEDICALService();

                var rimodel = await  ribill.SelectOne(value.RI_HACCOMPANY_ID);
                rimodel.RI_HACCOMPANY_STATE = value.BILL_HACCOMPANY_STATE;
                if(value.BILL_HACCOMPANY_STATE=="需理赔审核" && value.RI_HACCOMPANY != null)
                {
                    rimodel.RI_HACCOMPANY_ID = value.RI_HACCOMPANY_ID;
                    rimodel.RI_HACCOMPANY_ADDRESS = value.RI_HACCOMPANY.RI_HACCOMPANY_ADDRESS;
                    rimodel.RI_HACCOMPANY_ADDRESSDETAIL = value.RI_HACCOMPANY.RI_HACCOMPANY_ADDRESSDETAIL;
                    rimodel.RI_HACCOMPANY_BILLDOC = value.RI_HACCOMPANY.RI_HACCOMPANY_BILLDOC;
                    rimodel.RI_HACCOMPANY_DETAILS = value.RI_HACCOMPANY.RI_HACCOMPANY_DETAILS;
                    rimodel.RI_HACCOMPANY_ISUS = value.RI_HACCOMPANY.RI_HACCOMPANY_ISUS;
                    rimodel.RI_HACCOMPANY_TIME = value.RI_HACCOMPANY.RI_HACCOMPANY_TIME;
                }
                var id = value.BILL_HACCOMPANY_ID;
                var model = await  bill.SelectOne(id);
                model.BILL_HACCOMPANY_STATE = value.BILL_HACCOMPANY_STATE;
           
                model.BILL_EMERGENCYOTHER = value.BILL_EMERGENCYOTHER;
                model.RI_HACCOMPANY = rimodel;
                model.BILL_HACCOMPANY_WHY = value.BILL_HACCOMPANY_WHY;
                bool f;
                if (value.BILL_HACCOMPANY_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO= model.RI_HACCOMPANY.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail = await  produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);// 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME== "全球紧急救援保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await  pItemBill.Select(p => p.PRODUCTNO == saleDetail.SALESDETAIL_ID && p.IPOTHER_NAME== "全球紧急救援保险责任");
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
                                SALEDETAILNO = model.RI_HACCOMPANY.REPORTINFORMATION.SALEDETAILNO,
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

 