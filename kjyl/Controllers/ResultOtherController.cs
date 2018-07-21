using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Helper;
using BLL;
using Model;

namespace kjyl.Controllers
{
    public class ResultOtherController : ApiController
    {
        RESULT_OTHERService bill = new RESULT_OTHERService();
        // GET: api/ResultOther
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ResultOther/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id,string type)
        {
            AjaxResultT<RESULT_OTHER> re = new AjaxResultT<RESULT_OTHER>()
            {
                code = 0
            };
            try
            {
                id = id.Trim();
                string billId="",billType="";
                switch (type)
                {
                    case "紧急医疗":
                        billType = "全球紧急救援保险责任";
                        BILL_HMEDIAService hMedical = new BILL_HMEDIAService();
                      var model= await hMedical.Select(p => p.RI_HMEDIA_ID == id);
                        billId = model.BILL_HMEDIA_ID;
                        break;
                    case "紧急转院":
                        billType = "全球紧急救援保险责任";
                        BILL_HCHANGEService hChanger = new BILL_HCHANGEService();
                        var changModel = await hChanger.Select(p => p.RI_HCHANGE_ID == id);
                        billId =  changModel.BILL_HCHANGE_ID;
                        break;
                    case "紧急运返":
                        billType = "全球紧急救援保险责任";
                        BILL_HBACKService hback = new BILL_HBACKService();
                        var hbackModel = await hback.Select(p => p.RI_HBACK_ID == id);
                        billId =  hbackModel.BILL_HBACK_ID;
                        break;
                    case "探访费":
                        billType = "全球紧急救援保险责任";
                        BILL_HVISITService hvisit = new BILL_HVISITService();
                        var hvisitModel = await hvisit.Select(p => p.RI_HVISIT_ID == id);
                        billId = hvisitModel.BILL_HVISIT_ID;
                        break;
                    case "陪同费":
                        billType = "全球紧急救援保险责任";
                        BILL_HACCOMPANYService haccompany = new BILL_HACCOMPANYService();
                        var haccompanyModel = await haccompany.Select(p => p.RI_HACCOMPANY_ID == id);
                        billId =  haccompanyModel.BILL_HACCOMPANY_ID;
                        break;
                    case "遗体运返或安葬":
                        billType = "全球紧急救援保险责任";
                        BILL_HBURIALService hbuiral = new BILL_HBURIALService();
                        var hbuiralModel = await hbuiral.Select(p => p.RI_HBURIAL_ID == id);
                        billId = hbuiralModel.BILL_HBURIAL_ID;
                        break;
                    case "意外死亡":
                        billType = "意外伤害保险责任";
                        Bill_DiedService died = new Bill_DiedService();
                        var diedModel = await died.SelectOneByRiAsync(id);
                        billId = diedModel.BILL_DIED_ID;
                        break;
                    case "意外残疾":
                        billType = "意外伤害保险责任";
                        BILL_INJUREService injures = new BILL_INJUREService();
                        var injuresModel = await injures.Select(p => p.RI_INJURE_ID == id);
                        billId = injuresModel.BILL_INJURE_ID;
                        break;
                    case "学业中断":
                        billType = "学业中断保险责任";
                        BILL_ACADEMICService academic = new BILL_ACADEMICService();
                        var academicModel = await academic.Select(p => p.RI_ACADEMIC_ID == id);
                        billId = academicModel.BILL_ACADEMIC_ID;
                        break;
                    case "旅行延误":
                        billType = "旅行不便保险责任";
                        BILL_TRAVELDELAYService traval = new BILL_TRAVELDELAYService();
                        var travalModel = await traval.Select(p => p.RI_TRAVELDELAY_ID == id);
                        billId = travalModel.BILL_TRAVELDELAY_ID;
                        break;
                    case "物品丢失":
                        billType = "旅行不便保险责任";
                        BILL_LOSEService lose = new BILL_LOSEService();
                        var loseModel = await lose.Select(p => p.RI_LOSE_ID == id);
                        billId = loseModel.BILL_LOSE_ID;
                        break;
                    case "个人第三者":
                        billType = "个人第三者责任保险责任";
                        BILL_OTHERService other = new BILL_OTHERService();
                        var otherModel = await other.Select(p => p.RI_OTHER_ID == id);
                        billId = otherModel.BILL_OTHER_ID;
                        break;
                }
               
                var modle = await bill.Select(p => p.BILL_ID == billId && p.BILL_TYPE == billType);
                List<RESULT_OTHER> list = new List<RESULT_OTHER>
                {
                    modle
                };
                re.result = list;
                re.code = 1;
            }
            catch (Exception e)
            {
                re.msg = e.Message;
                
            }
            return Json(re);

        }
        /// <summary>
        /// 获取人民币值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="huilv"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ResultOther/People")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PeopleAsync(string id, string type, decimal huilv)
        {
            AjaxResultT<RESULT_OTHER> re = new AjaxResultT<RESULT_OTHER>()
            {
                code = 0
            };
            try
            {
                id = id.Trim();                     
              
                RESULT_OTHER billmodle = new RESULT_OTHER();
                switch (type)
                {
                    case "紧急医疗":

                        BILL_HMEDIAService hMedical = new BILL_HMEDIAService();
                        var model = await hMedical.Select(p => p.RI_HMEDIA_ID == id);                       
                        billmodle = await bill.Select(p => p.BILL_ID == model.BILL_HMEDIA_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "紧急转院":
                        BILL_HCHANGEService hChanger = new BILL_HCHANGEService();
                        var changModel = await hChanger.Select(p => p.RI_HCHANGE_ID == id);
                      
                        billmodle = await bill.Select(p => p.BILL_ID == changModel.BILL_HCHANGE_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "紧急运返":
                        BILL_HBACKService hback = new BILL_HBACKService();
                        var hbackModel = await hback.Select(p => p.RI_HBACK_ID == id);                      
                        billmodle = await bill.Select(p => p.BILL_ID == hbackModel.BILL_HBACK_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "陪同费":
                        BILL_HACCOMPANYService hccompany = new BILL_HACCOMPANYService();
                        var hccompanyModel = await hccompany.Select(p => p.RI_HACCOMPANY_ID == id);
                        billmodle = await bill.Select(p => p.BILL_ID == hccompanyModel.BILL_HACCOMPANY_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "探访费":
                        BILL_HVISITService hvisit = new BILL_HVISITService();
                        var hvisitModel = await hvisit.Select(p => p.RI_HVISIT_ID == id);                      
                        billmodle = await bill.Select(p => p.BILL_ID == hvisitModel.BILL_HVISIT_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "遗体运返或安葬":
                        //灵柩费费没有更新，只更新对应的 大责任，
                        ;
                        BILL_HBURIALService hbuiral = new BILL_HBURIALService();
                        var hbuiralModel = await hbuiral.Select(p => p.RI_HBURIAL_ID == id);                      
                        billmodle = await bill.Select(p => p.BILL_ID == hbuiralModel.BILL_HBURIAL_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "意外死亡":
                        //意外死亡和意外伤残没有汇率转换
                        Bill_DiedService died = new Bill_DiedService();
                        var diedModel = await died.SelectOneByRiAsync(id);                    
                        billmodle = await bill.Select(p => p.BILL_ID == diedModel.BILL_DIED_ID && p.BILL_TYPE == "意外伤害保险责任");
                        re.msg = billmodle.RESULT_OTHER_PAY ;
                        break;
                    case "意外残疾":
                        //意外死亡和意外伤残没有汇率转换
                        BILL_INJUREService injures = new BILL_INJUREService();
                        var injuresModel = await injures.Select(p => p.RI_INJURE_ID == id);                     
                        billmodle = await bill.Select(p => p.BILL_ID == injuresModel.BILL_INJURE_ID && p.BILL_TYPE == "意外伤害保险责任");
                        re.msg = billmodle.RESULT_OTHER_PAY;
                        break;
                    case "学业中断":

                        BILL_ACADEMICService academic = new BILL_ACADEMICService();
                        var academicModel = await academic.Select(p => p.RI_ACADEMIC_ID == id);                      
                        billmodle = await bill.Select(p => p.BILL_ID == academicModel.BILL_ACADEMIC_ID && p.BILL_TYPE == "学业中断保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "旅行延误":

                        BILL_TRAVELDELAYService traval = new BILL_TRAVELDELAYService();
                        var travalModel = await traval.Select(p => p.RI_TRAVELDELAY_ID == id);
                      
                        billmodle = await bill.Select(p => p.BILL_ID == travalModel.BILL_TRAVELDELAY_ID && p.BILL_TYPE == "旅行不便保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "物品丢失":

                        BILL_LOSEService lose = new BILL_LOSEService();
                        var loseModel = await lose.Select(p => p.RI_LOSE_ID == id);                      
                        billmodle = await bill.Select(p => p.BILL_ID == loseModel.BILL_LOSE_ID && p.BILL_TYPE == "旅行不便保险责任");
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                    case "个人第三者":
                        BILL_OTHERService other = new BILL_OTHERService();
                        var otherModel = await other.Select(p => p.RI_OTHER_ID == id);                      
                        billmodle = await bill.Select(p => p.BILL_ID == otherModel.BILL_OTHER_ID );
                        re.msg = Math.Round(billmodle.RESULT_OTHER_PAY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                        break;
                }
               
                    re.code = 1;
               
            }
            catch (Exception e)
            {
                re.msg = e.Message;

            }
            return Json(re);
        }
        [HttpGet]
        [Route("api/ResultOther/Pay")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PayAsync(string id, string type,decimal huilv)
        {
            AjaxResultT<RESULT_OTHER> re = new AjaxResultT<RESULT_OTHER>()
            {
                code = 0
            };
            try
            {
                id = id.Trim();
                bool f = false;
              //当最后赔付的金额 达到该项责任的总赔付金额时，最后更新的利益表可能不正确，如：旅行不便中 当赔付旅行证件遗失费和随身财产丢失费的时候，
              //如果当前总责任余额 不够时，我们两项都更新了，此时 无法进行分配，只能两项都进行更新，总额置零，此情况在医疗保险责任中出现概率更大
                RESULT_OTHERService resultBill = new RESULT_OTHERService();
                RESULT_OTHER billmodle = new RESULT_OTHER();
                INTEREST_ORDEROTHERService liyi = new INTEREST_ORDEROTHERService();
                INTEREST_ORDEROTHER liyiModel = new INTEREST_ORDEROTHER();
                decimal paymoney = 0;
                IO_OTHER_ITEM itemmodel = new IO_OTHER_ITEM();
              
                switch (type)
                {
                    case "紧急医疗":
                        
                        BILL_HMEDIAService hMedical = new BILL_HMEDIAService();
                        var model = await hMedical.Select(p => p.RI_HMEDIA_ID == id);
                        model.BILL_HMEDIA_STATE = "支付完成";
                        model.RI_HMEDIA.RI_HMEDIA_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == model.BILL_HMEDIA_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        //获取利益表
                        var SALEDETAILNO =model.RI_HMEDIA.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == SALEDETAILNO && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        //
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv>0? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv:0;
                        itemmodel=  liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急医疗救援费");
                        //分项金额更新
                        itemmodel.IO_OTHER_ITEM_MONEY=itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv>0? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv:0;
                        f = resultBill.Pay<BILL_HMEDIA>(billmodle, model, liyiModel);  
                        break;
                    case "紧急转院":                       
                        BILL_HCHANGEService hChanger = new BILL_HCHANGEService();
                        var changModel = await hChanger.Select(p => p.RI_HCHANGE_ID == id);                      
                        changModel.BILL_HCHANGE_STATE = "支付完成";
                        changModel.RI_HCHANGE.RI_HCHANGE_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == changModel.BILL_HCHANGE_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var changModelNumber = changModel.RI_HCHANGE.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == changModelNumber && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        //获取利益表
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv> 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv:0;
                         itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急转院费");
                        //分项金额更新
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv > 0 ? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv : 0;
                        f = resultBill.Pay<BILL_HCHANGE>(billmodle, changModel, liyiModel);
                        break;
                    case "紧急运返":                       
                        BILL_HBACKService hback = new BILL_HBACKService();
                        var hbackModel = await hback.Select(p => p.RI_HBACK_ID == id);
                        hbackModel.BILL_HBACK_STATE = "支付完成";
                        hbackModel.RI_HBACK.RI_HBACK_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == hbackModel.BILL_HBACK_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var hbackModelNumber = hbackModel.RI_HBACK.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == hbackModelNumber && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        //获取利益表
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv > 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv : 0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急运返费");
                        //分项金额更新
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv > 0 ? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv : 0;
                        f = resultBill.Pay<BILL_HBACK>(billmodle, hbackModel,liyiModel);
                        break;
                    case "陪同费":
                        BILL_HACCOMPANYService hccompany = new BILL_HACCOMPANYService();
                        var hccompanyModel = await hccompany.Select(p => p.RI_HACCOMPANY_ID == id);
                       
                        hccompanyModel.BILL_HACCOMPANY_STATE = "支付完成";
                        hccompanyModel.RI_HACCOMPANY.RI_HACCOMPANY_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == hccompanyModel.BILL_HACCOMPANY_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var hccompanyModelNumber = hccompanyModel.RI_HACCOMPANY.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == hccompanyModelNumber && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        //获取利益表
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv > 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv : 0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "紧急转院或运返陪同费");
                        //分项金额更新
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv > 0 ? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv : 0;
                        f = resultBill.Pay<BILL_HACCOMPANY>(billmodle, hccompanyModel, liyiModel);
                        break;
                    case "探访费":
                      
                        BILL_HVISITService hvisit = new BILL_HVISITService();
                        var hvisitModel = await hvisit.Select(p => p.RI_HVISIT_ID == id);
                        hvisitModel.BILL_HVISIT_STATE = "支付完成";
                        hvisitModel.RI_HVISIT.RI_HVISIT_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == hvisitModel.BILL_HVISIT_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var hvisitModelNumber = hvisitModel.RI_HVISIT.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == hvisitModelNumber && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        //获取利益表
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv > 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv : 0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "家属境外慰问探访费");
                        //
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv > 0 ? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv : 0;
                        f = resultBill.Pay<BILL_HVISIT>(billmodle, hvisitModel,liyiModel);
                        break;
                    case "遗体运返或安葬":
                        //灵柩费费没有更新，只更新对应的 大责任，
                      
                        BILL_HBURIALService hbuiral = new BILL_HBURIALService();
                        var hbuiralModel = await hbuiral.Select(p => p.RI_HBURIAL_ID == id);
                        hbuiralModel.BILL_HBURIAL_STATE = "支付完成";
                        hbuiralModel.RI_HBURIAL.RI_HBURIAL_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == hbuiralModel.BILL_HBURIAL_ID && p.BILL_TYPE == "全球紧急救援保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var hbuiralModelNumber = hbuiralModel.RI_HBURIAL.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == hbuiralModelNumber && p.INTEREST_ORDEROTHER_NAME == "全球紧急救援保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv> 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv:0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "遗体运返或安葬费");
                        //
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv>0? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv:0;
                        f = resultBill.Pay<BILL_HBURIAL>(billmodle, hbuiralModel, liyiModel);
                        break;
                    case "意外死亡":
                      //意外死亡和意外伤残没有汇率转换
                        Bill_DiedService died = new Bill_DiedService();
                        var diedModel = await died.SelectOneByRiAsync(id);
                        diedModel.BILL_DIED_STATE = "支付完成";
                        diedModel.RI_DIED.RI_DIED_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == diedModel.BILL_DIED_ID && p.BILL_TYPE == "意外伤害保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY ;
                        var diedModelNumber = diedModel.RI_DIED.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == diedModelNumber && p.INTEREST_ORDEROTHER_NAME== "意外伤害保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney>0? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney:0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "意外伤残/身故");
                        //
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney>0? itemmodel.IO_OTHER_ITEM_MONEY - paymoney:0 ;
                        f = resultBill.Pay<BILL_DIED>(billmodle, diedModel, liyiModel);
                        break;
                    case "意外残疾":
                        //意外死亡和意外伤残没有汇率转换
                        BILL_INJUREService injures = new BILL_INJUREService();
                        var injuresModel = await injures.Select(p => p.RI_INJURE_ID == id);
                        injuresModel.BILL_INJURE_STATE = "支付完成";
                        injuresModel.RI_INJURE.RI_INJURE_STATE = "支付完成";
                         billmodle = await bill.Select(p => p.BILL_ID == injuresModel.BILL_INJURE_ID && p.BILL_TYPE == "意外伤害保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY ;
                        var RI_INJURENumber = injuresModel.RI_INJURE.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == RI_INJURENumber && p.INTEREST_ORDEROTHER_NAME == "意外伤害保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney >0? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney :0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "意外伤残/身故");
                        //
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney>0? itemmodel.IO_OTHER_ITEM_MONEY - paymoney:0;
                        f = resultBill.Pay<BILL_INJURE>(billmodle, injuresModel,liyiModel);
                        break;
                    case "学业中断":
                      
                        BILL_ACADEMICService academic = new BILL_ACADEMICService();
                        var academicModel = await academic.Select(p => p.RI_ACADEMIC_ID == id);
                        academicModel.BILL_ACADEMIC_STATE = "支付完成";
                        academicModel.RI_ACADEMIC.RI_ACADEMIC_STATE = "支付完成";
                        billmodle = await bill.Select(p => p.BILL_ID == academicModel.BILL_ACADEMIC_ID && p.BILL_TYPE == "学业中断保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var academicModelNumber = academicModel.RI_ACADEMIC.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == academicModelNumber && p.INTEREST_ORDEROTHER_NAME == "学业中断保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        //
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv > 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv : 0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "学业中断");
                        //
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv > 0 ? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv : 0;
                        f = resultBill.Pay<BILL_ACADEMIC>(billmodle, academicModel, liyiModel);
                        break;
                    case "旅行延误":
                       
                        BILL_TRAVELDELAYService traval = new BILL_TRAVELDELAYService();
                        var travalModel = await traval.Select(p => p.RI_TRAVELDELAY_ID == id);
                        travalModel.BILL_TRAVELDELAY_STATE = "支付完成";
                        travalModel.RI_TRAVELDELAY.RI_TRAVELDELAY_STATE = "支付完成";
                         billmodle = await bill.Select(p => p.BILL_ID == travalModel.BILL_TRAVELDELAY_ID && p.BILL_TYPE == "旅行不便保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var travalModelNumber = travalModel.RI_TRAVELDELAY.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == travalModelNumber && p.INTEREST_ORDEROTHER_NAME == "旅行不便保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv > 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv : 0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "旅行延误");
                        //
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv > 0 ? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv : 0;
                        f = resultBill.Pay<BILL_TRAVELDELAY>(billmodle, travalModel, liyiModel);
                        break;
                    case "物品丢失":
                      
                        BILL_LOSEService lose = new BILL_LOSEService();
                        var loseModel = await lose.Select(p => p.RI_LOSE_ID == id);
                        loseModel.BILL_LOSE_STATE = "支付完成";
                        loseModel.RI_LOSE.RI_LOSE_STATE = "支付完成";
                         billmodle = await bill.Select(p => p.BILL_ID == loseModel.BILL_LOSE_ID && p.BILL_TYPE == "旅行不便保险责任");
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var loseModelNumber = loseModel.RI_LOSE.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == loseModelNumber && p.INTEREST_ORDEROTHER_NAME == "旅行不便保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv > 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv : 0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "旅行证件遗失");
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - billmodle.RESULT_OTHER_ZHENGJIAN * huilv>0 ? itemmodel.IO_OTHER_ITEM_MONEY - billmodle.RESULT_OTHER_ZHENGJIAN * huilv:0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "随身财产损失或丢失");
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - billmodle.RESULT_OTHER_SUISHEN * huilv>0? itemmodel.IO_OTHER_ITEM_MONEY - billmodle.RESULT_OTHER_SUISHEN * huilv:0;
                        f = resultBill.Pay<BILL_LOSE>(billmodle, loseModel, liyiModel);
                        break;
                    case "个人第三者":                      
                        BILL_OTHERService other = new BILL_OTHERService();
                        var otherModel = await other.Select(p => p.RI_OTHER_ID == id);
                        otherModel.BILL_OTHER_STATE = "支付完成";
                        otherModel.RI_OTHER.RI_OTHER_STATE = "支付完成";
                          billmodle = await bill.Select(p => p.BILL_ID == otherModel.BILL_OTHER_ID );
                        billmodle.RESULT_OTHER_STATE = "支付完成";
                        billmodle.RESULT_OTHER_PAYPEOPLEHUILV = huilv;
                        billmodle.RESULT_OTHER_PAYPEOPLE = billmodle.RESULT_OTHER_PAY * huilv;
                        var otherModelNumber = otherModel.RI_OTHER.REPORTINFORMATION.SALEDETAILNO;
                        liyiModel = await liyi.Select(p => p.SALEDETAILNO == otherModelNumber && p.INTEREST_ORDEROTHER_NAME == "个人第三者保险责任");
                        paymoney = billmodle.RESULT_OTHER_PAY.Value;
                        liyiModel.INTEREST_ORDEROTHER_MONEY = liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv > 0 ? liyiModel.INTEREST_ORDEROTHER_MONEY - paymoney * huilv : 0;
                        itemmodel = liyiModel.IO_OTHER_ITEM.FirstOrDefault(p => p.IO_OTHER_ITEM_NAME == "个人第三者责任");
                        itemmodel.IO_OTHER_ITEM_MONEY = itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv>0 ? itemmodel.IO_OTHER_ITEM_MONEY - paymoney * huilv:0;
                        f = resultBill.Pay<BILL_OTHER>(billmodle, otherModel,liyiModel);
                        break;
                }
               
               
                if (f)
                {
                    re.code = 1;
                }
            }
            catch (Exception e)
            {
                re.msg = e.Message;

            }
            return Json(re);

        }
        // POST: api/ResultOther
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ResultOther/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ResultOther/5
        public void Delete(int id)
        {
        }
    }
}
