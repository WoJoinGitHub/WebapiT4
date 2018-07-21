
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BLL;
using Helper;
using Model;
using Newtonsoft.Json;

namespace kjyl.Controllers
{/// <summary>
/// 
/// </summary>
    public class BILL_ACADEMICController : ApiController
    {
        BILL_ACADEMICService bill = new BILL_ACADEMICService();
        // GET: api/BILL_ACADEMIC
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lilv"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/BILL_ACADEMIC/Compute")]
        public async Task<IHttpActionResult> ComputeAsync(string id,decimal lilv)
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
                BILL_ACADEMIC billData = await bill.SelectOneByRiAsync(id);
                var SALEDETAILNO = billData.RI_ACADEMIC.REPORTINFORMATION.SALEDETAILNO;
                var isHave = await resultBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.BILL_TYPE == "学业中断保险责任" && p.RESULT_OTHER_STATE == "计算完成");
                if (isHave != null)
                {
                    re.msg = "此保险责任有未支付的报案";
                    return Json(re);
                }
                billData.BILL_ACADEMIC_STATE = "计算完成";
                billData.RI_ACADEMIC.RI_ACADEMIC_STATE = "计算完成";
                //int liyiBill = new BILL_EMERGENCYOTHERService();
                var liyiModel = await liyiBill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME == "学业中断保险责任");
                if (liyiModel == null)
                {
                    re.msg = "无此保险责任利益表";
                    return Json(re);
                }
                //应付保险金额=实际支付学费*(未到期天数/总天数) – 学校已退还学费
                //缴纳学费
               
                var time = CreateId.TimeDate(billData.BILL_ACADEMIC_ENDTIME.Value,billData.BILL_ACADEMIC_PLANTIME.Value);
                var alltime = CreateId.TimeDate(billData.BILL_ACADEMIC_STARTTIME.Value, billData.BILL_ACADEMIC_PLANTIME.Value)+1;
               
                decimal jiaonaMoney = billData.BILL_ACADEMIC_MONEY.Value * time/alltime - billData.BILL_ACADEMIC_BACKMONEY.Value;
                //计算
                string paySate = "正常赔付";
                if(jiaonaMoney > liyiModel.INTEREST_ORDEROTHER_MONEY.Value * lilv)
                {
                    jiaonaMoney = liyiModel.INTEREST_ORDEROTHER_MONEY.Value * lilv;
                }
             
               
                    jiaonaMoney= jiaonaMoney>0 ? jiaonaMoney :0;
                     //将次结果计入计算结果表
                     RESULT_OTHER model = new RESULT_OTHER
                {
                    BILL_TYPE = "学业中断保险责任",
                    RESULT_OTHER_PAY = Math.Round(jiaonaMoney, 2, MidpointRounding.AwayFromZero),
                    BILL_ID = billData.BILL_ACADEMIC_ID,
                    RESULT_OTHER_ID = CreateId.GetId(),
                    RESULT_OTHER_STATE = "计算完成",
                    RESULT_OTHER_PAYSTATE = paySate,
                    RESULT_OTHER_HUILV = lilv.ToString(),
                    SALEDETAILNO = billData.RI_ACADEMIC.REPORTINFORMATION.SALEDETAILNO
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
        // GET: api/BILL_ACADEMIC/5
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
                    model.BILL_ACADEMIC_ID,
                    model.BILL_ACADEMIC_STARTTIME,
                    model.BILL_ACADEMIC_ENDTIME,
                    model.BILL_ACADEMIC_PLANTIME,
                    model.BILL_ACADEMIC_REASON,
                    model.BILL_ACADEMIC_DETAILS,
                    model.BILL_ACADEMIC_MONEY,
                    model.BILL_ACADEMIC_BACKMONEY,
                    model.BILL_ACADEMIC_STATE,
                    model.BILL_ACADEMIC_WHY,
                    model.RI_ACADEMIC_ID,
                    model.BILL_ACADEMIC_DOC


                };
                re.code = 1;

                List<dynamic> list = new List<dynamic> {resultModel};

                re.result = list;
            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }

            return Json(re, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        // POST: api/BILL_ACADEMIC
        public async Task<IHttpActionResult> Post([FromBody] BILL_ACADEMIC value)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0
            };
            try
            {
                RI_ACADEMICService ribill = new RI_ACADEMICService();
                var rimodel = await ribill.SelectOne(value.RI_ACADEMIC_ID);
                rimodel.RI_ACADEMIC_STATE = "需理赔审核";
                value.BILL_ACADEMIC_PLANTIME = value.BILL_ACADEMIC_PLANTIME;
                value.BILL_ACADEMIC_STARTTIME = value.BILL_ACADEMIC_STARTTIME;
                value.BILL_ACADEMIC_ENDTIME = value.BILL_ACADEMIC_ENDTIME;
                value.RI_ACADEMIC = rimodel;
                var model = await bill.Add(value);
                if (model.BILL_ACADEMIC_ID.Length > 0)
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

        // PUT: api/BILL_ACADEMIC/5
        public async Task<IHttpActionResult> Put([FromBody]BILL_ACADEMIC value)
        {

            AjaxResult re = new AjaxResult()
            {
                code = 0
            };
            try
            {
                var model =await  bill.SelectOne(value.BILL_ACADEMIC_ID);              
                 model.BILL_ACADEMIC_STARTTIME = value.BILL_ACADEMIC_STARTTIME;
                model.BILL_ACADEMIC_ENDTIME = value.BILL_ACADEMIC_ENDTIME;
                model.BILL_ACADEMIC_PLANTIME = value.BILL_ACADEMIC_PLANTIME;
                model.BILL_ACADEMIC_REASON = value.BILL_ACADEMIC_REASON;
                model.BILL_ACADEMIC_DETAILS = value.BILL_ACADEMIC_DETAILS;
                model.BILL_ACADEMIC_MONEY = value.BILL_ACADEMIC_MONEY;
                model.BILL_ACADEMIC_BACKMONEY = value.BILL_ACADEMIC_BACKMONEY;
                model.BILL_ACADEMIC_STATE = value.BILL_ACADEMIC_STATE;
                model.BILL_ACADEMIC_WHY = value.BILL_ACADEMIC_WHY;              
                model.BILL_ACADEMIC_DOC = value.BILL_ACADEMIC_DOC;
                model.RI_ACADEMIC.RI_ACADEMIC_STATE = value.BILL_ACADEMIC_STATE;               
                bool f;
                if (value.BILL_ACADEMIC_STATE == "需计算")
                {
                    //利益表
                    INTEREST_ORDEROTHERService orderbill = new INTEREST_ORDEROTHERService();
                    SALESDETAILService produceBill = new SALESDETAILService();
                    var SALEDETAILNO = model.RI_ACADEMIC.REPORTINFORMATION.SALEDETAILNO;
                    var saleDetail =  await produceBill.SelectOne(p => p.SALESDETAIL_ID == SALEDETAILNO);
                    // 
                    var orderModel = await orderbill.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME== "学业中断保险责任");
                    //没有此订单的 利益表
                    if (orderModel == null)
                    {
                        //检索此产品的利益表
                        INTEREST_PRODUCEOTHERService pItemBill = new INTEREST_PRODUCEOTHERService();

                        var reModel = await  pItemBill.Select(p => p.PRODUCTNO == saleDetail.PRODUCE_ID && p.IPOTHER_NAME== "学业中断保险责任");
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
                                SALEDETAILNO = model.RI_ACADEMIC.REPORTINFORMATION.SALEDETAILNO,
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
                        f = await  bill.Updata(model);
                    }
                }
                else
                {
                    f = await  bill.Updata(model);
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

        // DELETE: api/BILL_ACADEMIC/5
        public void Delete(int id)
        {
        }
    }
}

