using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BLL;
using Helper;
using Model;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    /// <summary>
    /// 结算处理，支付处理
    /// </summary>
    public class BillController : ApiController
    {
        BillService bll = new BillService();
        /// <summary>
        /// 确认支付
        /// </summary>
        /// <param name="id"></param>
        /// <param name="huilv"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Bill/PayOk")]
        public async Task<IHttpActionResult> PayOk(string id, decimal huilv)
        {
            AjaxResult re = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {
                
                INTEREST_ORDERService iobll = new INTEREST_ORDERService();
                RESULTService resultbBill = new RESULTService();
                BILL model =await  bll.SelectOne(id);
                RESULT remodle = await resultbBill.Select(p => p.BILL.RI_MEDICAL_ID == id && p.RESULT_STATE == "折扣完成");
                //List<RESULT> relist = model.RESULTs.Where(p => p.RESULT_STATE == "折扣完成").ToList();
                var SALEDETAILNO = model.RI_MEDICAL.REPORTINFORMATION.SALEDETAILNO;
                INTEREST_ORDER iomodel = await iobll.SelectBySale(SALEDETAILNO);
                if (remodle != null)
                {
                    //保险责任金额
                    iomodel.INTEREST_ORDER_ZFMAX = iomodel.INTEREST_ORDER_ZFMAX - remodle.RESULT_ZFBLMONEY * huilv > 0 ? iomodel.INTEREST_ORDER_ZFMAX - remodle.RESULT_ZFBLMONEY * huilv : 0;
                    iomodel.INTEREST_ORDER_MAXMONEY = iomodel.INTEREST_ORDER_MAXMONEY - remodle.RESULT_PAYMONEY * huilv > 0 ? iomodel.INTEREST_ORDER_MAXMONEY - remodle.RESULT_PAYMONEY * huilv : 0;
                    remodle.RESULT_STATE = "支付完成";
                    remodle.RESULT_PAYTIME = System.DateTime.Now;
                    remodle.RESULT_PAYMONEYPEOPLE = Math.Round(remodle.RESULT_PAYMONEY.Value * huilv, 2, MidpointRounding.AwayFromZero);
                    remodle.RESULT_PAYMONEYPEOPLELILV = huilv;
                    List<IO_DUTY> iodutylist = iomodel.IO_DUTY.ToList();
                    foreach (var item in iodutylist)
                    {

                        List<IO_ITEM> ioItemList = item.IO_ITEM.ToList();
                        List<RESULT_DUTY> resultDutyList = remodle.RESULT_DUTY.ToList();
                        foreach (var dutyItem in resultDutyList)
                        {
                            dutyItem.BILL_DATA.BILL.BILL_STATE = "支付完成";
                            dutyItem.BILL_DATA.BILL.RI_MEDICAL.RI_MEDICAL_STATE = "支付完成";
                            List<RESULT_ITEM> resultItemList = dutyItem.RESULT_ITEM.ToList();
                            foreach (var reItemItem in resultItemList)
                            {
                                if (reItemItem.DUTYITEM_FID == item.DUTYITEM_ID)
                                {
                                    
                                 
                                    //更新单项给付限额
                                    item.IO_DUTY_MAXMONEY = item.IO_DUTY_MAXMONEY - reItemItem.RESULT_ITEM_PAYMONEY * huilv > 0 ? item.IO_DUTY_MAXMONEY - reItemItem.RESULT_ITEM_PAYMONEY * huilv : 0;
                                    //更新免赔额
                                    if (iomodel.INTEREST_ORDER_ISSINGLE == "累计")
                                    {
                                        if (dutyItem.BILL_DATA.BILL_DATA_TYPE == "网络内" && iomodel.INTEREST_ORDER_MPN != 0)
                                        {
                                            iomodel.INTEREST_ORDER_MPN = iomodel.INTEREST_ORDER_MPN - reItemItem.RESULT_ITEM_MPMONEY * huilv > 0 ? iomodel.INTEREST_ORDER_MPN - reItemItem.RESULT_ITEM_MPMONEY * huilv : 0;
                                        }
                                        if (dutyItem.BILL_DATA.BILL_DATA_TYPE == "网络外" && iomodel.INTEREST_ORDER_MPW != 0)
                                        {
                                            iomodel.INTEREST_ORDER_MPW = iomodel.INTEREST_ORDER_MPW - reItemItem.RESULT_ITEM_MPMONEY * huilv > 0 ? iomodel.INTEREST_ORDER_MPW - reItemItem.RESULT_ITEM_MPMONEY * huilv : 0;
                                        }
                                    }
                                   
                                }
                                //
                                List<IO_ITEM> ioItemListWh = ioItemList.Where(p => p.DUTYITEM_ID == reItemItem.DUTYITEM_ID).ToList();
                                if (ioItemListWh != null && ioItemListWh.Count > 0)
                                {
                                    IO_ITEM ioItemModel = ioItemListWh.ElementAt(0);
                                    //每天的
                                    if (ioItemModel.IO_ITEM_ISPER == "是")
                                    {
                                        if (ioItemModel.IO_ITEM_MAXDAY != null)
                                        {
                                            ioItemModel.IO_ITEM_MAXDAY = ioItemModel.IO_ITEM_MAXDAY - 1 > 0 ? ioItemModel.IO_ITEM_MAXDAY - 1 : 0;
                                        }
                                    }
                                    else
                                    {
                                        ioItemModel.IO_ITEM_MAXM = ioItemModel.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv > 0 ? ioItemModel.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv : 0;
                                    }
                                    // 如果是精神疾病和行为障碍医疗费
                                    if (ioItemModel.DUTYITEM.DUTYITEM_NAME== "精神疾病和行为障碍医疗费-住院治疗费" || ioItemModel.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费-门诊治疗费")
                                    {
                                        //
                                        IO_ITEM jingShen = item.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费").FirstOrDefault();
                                        jingShen.IO_ITEM_MAXM = jingShen.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv > 0 ? jingShen.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv : 0;
                                    }

                                    //如果是预防保健费
                                    if (ioItemModel.DUTYITEM.DUTYITEM_NAME== "预防保健费-免疫接种" || ioItemModel.DUTYITEM.DUTYITEM_NAME== "预防保健费-女性专属预防护理及疾病排查" || ioItemModel.DUTYITEM.DUTYITEM_NAME == "预防保健费-儿童/青少年预防护理及疾病排查")
                                    {
                                        //
                                        IO_ITEM jingShen = item.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "预防保健费").FirstOrDefault();
                                        jingShen.IO_ITEM_MAXM = jingShen.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv > 0 ? jingShen.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv : 0;
                                       
                                    }
                                    //如果是生育医疗费
                                    if (ioItemModel.DUTYITEM.DUTYITEM_NAME == "生育医疗费-生育保障及产后护理费" || ioItemModel.DUTYITEM.DUTYITEM_NAME == "生育医疗费-新生儿检查护理及预防保健费" || ioItemModel.DUTYITEM.DUTYITEM_NAME == "生育医疗费-妊娠综合并发症治疗费")
                                    {                                       
                                        //
                                        IO_ITEM jingShen = item.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "生育医疗费").FirstOrDefault();
                                        jingShen.IO_ITEM_MAXM = jingShen.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv > 0 ? jingShen.IO_ITEM_MAXM - reItemItem.RESULT_ITEM_PAYMONEY * huilv : 0;
                                      

                                    }

                                }

                            }
                        }

                    }
                }

                var f = await iobll.UpdateLiyi(iomodel, remodle);
                if (f)
                {
                    re.code = 1;
                    re.msg = "计算成功";
                }



            }
            catch (Exception e)
            {

                re.msg = e.Message;
            }
            return Json(re);
        }
        ///// <summary>
        ///// 获取应赔付 人民币
        ///// </summary>
        //[HttpGet]
        //[Route("api/Bill/GetPeople")]
        //public IHttpActionResult GetPeopleMoney(string id, decimal huilv)
        //{
        //    AjaxResult re = new AjaxResult
        //    {
        //        code = 0,
        //        msg = ""
        //    };
        //    try
        //    {
        //        INTEREST_ORDERService iobll = new INTEREST_ORDERService();
        //        RESULTService resultbBill = new RESULTService();
        //        BILL model = bll.SelectOne(id);
        //        RESULT remodle = resultbBill.Select(p => p.BILL.RI_MEDICAL_ID == id && p.RESULT_STATE == "折扣完成");
        //        re.msg = remodle.RESULT_PAYMONEY * huilv;
        //        re.code = 1;

        //    }
        //    catch (Exception e)
        //    {

        //        re.msg = e.Message;
        //    }
        //    return Json(re);
        //}
        /// <summary>
        /// 折扣计算
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="company"></param>
        /// <param name="huilv"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Bill/Zhekou")]
        public async Task<IHttpActionResult> ZhekouAsync(string id, int user, int company)
        {
            if (user == company)
            {
                AjaxResult re = await GetReult(user, id, 0, 1, 100, 100, false);
                return Json(re);
            }
            else
            {
                AjaxResult re = await  GetReult(100, id, 0, 2, user, company, false);
                return Json(re);
            }
        }
        /// <summary>
        /// 计算
        /// </summary>
        /// <param name="id"></param>
        /// <param name="huilv"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Bill/sum")]
        public async Task<IHttpActionResult> SumAsync(string id, decimal huilv)
        {
            AjaxResult re = await  GetReult(100, id, huilv, 0, 100, 100, true);
            return Json(re);
        }

        // GET: api/Bill
        /// <summary>
        /// 分页获取订单数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> Get(int page, int pagesize, string type)
        {
            var result = new PageResult<dynamic>
            {
                code = 0,
                msg = "获取失败",
                pagecount = 0
            };

            try
            {
                Expression<Func<BILL, bool>> wh;
                switch (type)
                {
                    case "待办":
                        wh = p => p.BILL_STATE == "需修改" || p.BILL_STATE == "审核不通过";
                        break;
                    case "审核通过":
                        wh = p => p.BILL_STATE == "审核通过" || p.BILL_STATE == "计算完成";
                        break;
                    case "计算完成":
                        wh = p => p.BILL_STATE == "计算完成" || p.BILL_STATE == "折扣完成";
                        break;
                    default:
                        wh = p => p.BILL_STATE == type;
                        break;
                }
                var list = await  bll.SelectPageListNew<string>(wh, p => p.BILL_ID, 1, 10, false);
                result.pagecount = list.Item2;
                result.code = 1;
                result.msg = "成功";
                result.result = list.Item1.ToList();
            }
            catch (Exception e)
            {
                result.msg = e.Message;

            }
            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        /// <summary>
        /// 获取单个账单信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Bill/5
        public async Task<IHttpActionResult> Get(string id)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {

                List<dynamic> list = (await  bll.SelectOnePart(id)).ToList();
                result.code = 1;
                result.msg = "获取成功";

                //List<dynamic> list = new List<dynamic>();
                //list.Add(dy);
                result.result = list;
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }

            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Bill/GetResult")]
        public async Task<IHttpActionResult> GetResultAsync(string id, string state)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {
                List<dynamic> list = (await bll.SelectResultPart(id, state)).ToList();
                result.code = 1;
                result.msg = "获取成功";
                //List<dynamic> list = new List<dynamic>();
                //list.Add(dy);
                result.result = list;
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }

            return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
        /// <summary>
        /// 账单添加
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST: api/Bill
        public async Task<IHttpActionResult> Post([FromBody] BILL value)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {
                value.BILL_CREATETIME = System.DateTime.Now;

                value.BILL_STATE = "需理赔审核";
                RI_MEDICALService medical = new RI_MEDICALService();
                RI_MEDICAL mdicalmodel = await medical.SelectOne(value.RI_MEDICAL_ID);
                mdicalmodel.RI_MEDICAL_STATE = "需理赔审核";
                value.REPORTINFORMATION_PRODUCE = mdicalmodel.REPORTINFORMATION.REPORTINFORMATION_PRODUCE;
                value.RI_MEDICAL = mdicalmodel;
                var model = await  bll.Add(value);
                if (model.BILL_ID.Length > 0)
                {
                    result.msg = "添加成功";
                    result.code = 1;
                }

            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result);

        }
        /// <summary>
        /// 医师审核 理赔审核
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT: api/Bill/5
        public async Task<IHttpActionResult> Put([FromBody] BILL value)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {
                string id = value.BILL_ID;
                BILL mode = await bll.Select(id);
                var list = mode.BILL_DATA.OrderBy(p => p.BILL_DATA_ID).ToList();
                int i = 0;
                var valuedt = value.BILL_DATA.OrderBy(p => p.BILL_DATA_ID).ToArray();
                mode.RI_MEDICAL.RI_MEDICAL_STATE = value.BILL_STATE;
                mode.BILL_STATE = value.BILL_STATE;
                mode.BILL_WHY = value.BILL_WHY;


                foreach (var item in list)
                {
                    item.BILL_DATA_TYPE = valuedt[i].BILL_DATA_TYPE;
                    item.BILL_DATA_URGENCY = valuedt[i].BILL_DATA_URGENCY;
                    var detaillist = item.BILL_DATA_DETAIL.OrderBy(p => p.BILL_DATA_DETAIL_ID);
                    var detailarry = valuedt[i].BILL_DATA_DETAIL.OrderBy(p => p.BILL_DATA_DETAIL_ID).ToArray();
                    var k = 0;
                    foreach (var detail in detaillist)
                    {
                        detail.DUTYITEM_FID = detailarry[k].DUTYITEM_FID;
                        detail.DUTYITEM_ID = detailarry[k].DUTYITEM_ID;
                        detail.BILL_DATA_DETAIL_STATE = detailarry[k].BILL_DATA_DETAIL_STATE;
                        detail.BILL_DATA_DETAIL_RELATION = detailarry[k].BILL_DATA_DETAIL_RELATION;
                        k++;
                    }
                    i++;
                }
                if (mode.BILL_STATE == "需计算")
                {
                    //1.汇总账单数据到账单日汇总表中
                    var billdata = mode.BILL_DATA;
                    foreach (var item in billdata)
                    {
                        List<BILL_DATA_COUNT> bcmodellist = new List<BILL_DATA_COUNT>();
                        var detaillist = item.BILL_DATA_DETAIL;
                        var grouplist = detaillist.Where(p => p.BILL_DATA_DETAIL_STATE != "删除" && p.BILL_DATA_DETAIL_ISSHOW!="删除" && p.BILL_DATA_DETAIL_RELATION == "是").GroupBy(p => p.DUTYITEM_ID, (p, v) => new
                        {
                            DUTYITEM_ID = i,
                            detaillist = v//这里的userlist是一个集合，它应该是延时的，不能使用ToList()将它在代码块中变为立即执行的，同理不能使用First(),FirstOrDefault等实时查询的方法
                        }).ToList();
                        foreach (var grouplistitem in grouplist)
                        {
                            BILL_DATA_COUNT bcmodel = new BILL_DATA_COUNT();
                            decimal cmoney = 0;
                            var sum = grouplistitem.detaillist?.ToList().Sum(p => p.BILL_DATA_DETAIL_CMONEY);
                            if (sum != null)
                                cmoney = (decimal)sum;
                            bcmodel.BILL_DATA_COUNT_MONEY = cmoney;
                            bcmodel.DUTYITEM_FID = grouplistitem.detaillist.ElementAt(0).DUTYITEM_FID;
                            bcmodel.DUTYITEM_ID = grouplistitem.detaillist.ElementAt(0).DUTYITEM_ID;
                            bcmodel.BILL_DATA_COUNT_ID = CreateId.GetId();
                            bcmodellist.Add(bcmodel);
                        }
                        item.BILL_DATA_COUNT = bcmodellist;
                    }
                    INTEREST_PRODUCEService pbll = new INTEREST_PRODUCEService();
                    INTEREST_ORDERService obll = new INTEREST_ORDERService();
                    // mode.REPORTINFORMATION_PRODUCE 产品名称
                    //1.通过产品id 查询利益表
                    INTEREST_PRODUCE pmodel = await pbll.SelectOne(mode.REPORTINFORMATION_PRODUCE);
                    if (pmodel == null)
                    {
                        result.msg = "无此产品的产品利益表";
                        return Json(result);
                    }
                    //是否有次订单的利益表
                    INTEREST_ORDER isomodel = await obll.SelectBySale(mode.RI_MEDICAL.REPORTINFORMATION.SALEDETAILNO.ToString());
                    if (isomodel == null)
                    {
                        //2.将产品利益表数据copy到订单利益表中
                        INTEREST_ORDER omodel = new INTEREST_ORDER
                        {
                            INTEREST_ORDER_ID = CreateId.GetId(),
                            INTEREST_ORDER_ISSINGLE = pmodel.INTEREST_PRODUCE_ISSINGLE,
                            INTEREST_ORDER_MAXMONEY = pmodel.INTEREST_PRODUCE_MAXMONEY,
                            INTEREST_ORDER_MPN = pmodel.INTEREST_PRODUCE_MPN,
                            INTEREST_ORDER_MPW = pmodel.INTEREST_PRODUCE_MPW,
                            INTEREST_ORDER_ZFMAX = pmodel.INTEREST_PRODUCE_ZFMAX,
                            SALEDETAILNO = mode.RI_MEDICAL.REPORTINFORMATION.SALEDETAILNO.ToString()
                        };
                        int it = 0;
                        List<IO_DUTY> iolist = new List<IO_DUTY>();
                        foreach (var item in pmodel.IP_DUTY)
                        {
                            //ioduty责任项
                            //omodel.IO_DUTY = pmodel.IP_DUTY;
                            IO_DUTY oitem = new IO_DUTY
                            {
                                IO_DUTY_ID = CreateId.GetId(),
                                DUTYITEM_ID = item.DUTYITEM_ID,
                                IO_DUTY_MAXMONEY = item.IP_DUTY_MAXMONEY
                            };

                            int k = 0;
                            List<IO_ITEM> ioitemlist = new List<IO_ITEM>();
                            //ioitem 责任小项
                            foreach (var ipitem in item.IP_ITEM)
                            {
                                IO_ITEM oioitem = new IO_ITEM
                                {
                                    IO_ITEM_ID = CreateId.GetId(),
                                    IO_ITEM_ISPER = ipitem.IP_ITEM_ISPER,
                                    IO_ITEM_MAXDAY = ipitem.IP_ITEM_MAXDAY,
                                    IO_ITEM_MAXM = ipitem.IP_ITEM_MAXM,
                                    IO_ITEM_MAXMW = ipitem.IP_ITEM_MAXMW,
                                    IO_ITEM_ZFBLN = ipitem.IP_ITEM_ZFBLN,
                                    IO_ITEM_ZFBLW = ipitem.IP_ITEM_ZFBLW,
                                    IO_ITEM_ZFEN = ipitem.IP_ITEM_ZFEN,
                                    IO_ITEM_ZFEW = ipitem.IP_ITEM_ZFEW,
                                    DUTYITEM_ID = ipitem.DUTYITEM_ID
                                };
                                k++;
                                ioitemlist.Add(oioitem);
                            }
                            oitem.IO_ITEM = ioitemlist;
                            iolist.Add(oitem);
                            it++;
                        }
                        omodel.IO_DUTY = iolist;
                        INTEREST_ORDER ordersuccessmodel =await  obll.Add(omodel);
                        if (ordersuccessmodel.INTEREST_ORDER_ID.Length > 0)
                        {
                            bool f = await  bll.Updata(mode);
                            if (f)
                            {
                                result.msg = "审核成功";  //如果是理赔审核通过，应该关联产品利益表
                                result.code = 1;
                            }
                            else
                            {
                               await  obll.Delete(ordersuccessmodel);
                            }
                        }
                    }
                    else
                    {
                        bool f = await  bll.Updata(mode);
                        if (f)
                        {
                            result.msg = "审核成功";  //如果是理赔审核通过，应该关联产品利益表
                            result.code = 1;
                        }

                    }


                }
                else
                {
                    bool u = await  bll.Updata(mode);
                    if (u)
                    {
                        result.msg = "审核成功";
                        result.code = 1;
                    }
                }


            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result);
        }
        /// <summary>
        /// 修改 新增或删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT: api/Bill/5
        [HttpPost]
        [Route("api/Bill/PutNew")]
        public async Task<IHttpActionResult> PutNewAsync([FromBody] BILL value)
        {
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = ""
            };
            try
            {
                string id = value.BILL_ID;
                BILL mode = await  bll.SelectOne(id);

                mode.BILL_STATE = "需理赔审核";
                mode.RI_MEDICAL.RI_MEDICAL_STATE = "需理赔审核";
                mode.BILL_WHY = value.BILL_WHY;
                mode.BILL_HISPOTAL = value.BILL_HISPOTAL;
                mode.BILL_DISEASE = value.BILL_DISEASE;
                mode.BILL_DISEASEDETAIL = value.BILL_DISEASEDETAIL;
                mode.BILL_FILE = value.BILL_FILE;
                //老数据按照 时间排序
                var list = mode.BILL_DATA.ToList().OrderBy(p => p.BILL_DATA_ID).ToList();
                int i = 0;
                //新数据按照时间排序
                var valuedt = value.BILL_DATA.OrderBy(p => p.BILL_DATA_ID).ToArray();

                foreach (var item in valuedt)
                {
                    if (i >= list.ToArray().Length)
                    {
                        list.Add(item);
                        if (i == valuedt.Length - 1)
                        {
                            mode.BILL_DATA = list;
                        }
                    }
                    else
                    {


                        var detaillist = list.ElementAt(i).BILL_DATA_DETAIL.OrderBy(p => p.BILL_DATA_DETAIL_ID).ToList();
                        var detailarry = item.BILL_DATA_DETAIL.OrderBy(p => p.BILL_DATA_DETAIL_ID).ToArray();
                        var k = 0;
                        foreach (var detail in detailarry)
                        {
                            if (k >= detaillist.ToArray().Length)
                            {

                                detaillist.Add(detail);
                                if (k == detailarry.Length - 1)
                                {
                                    mode.BILL_DATA.ElementAt(i).BILL_DATA_DETAIL = detaillist;
                                }
                            }
                            else
                            {
                                detaillist.ElementAt(k).DUTYITEM_FID = detail.DUTYITEM_FID;
                                detaillist.ElementAt(k).DUTYITEM_ID = detail.DUTYITEM_ID;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_STATE = detail.BILL_DATA_DETAIL_STATE;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_ENAME = detail.BILL_DATA_DETAIL_ENAME;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_MONEY = detail.BILL_DATA_DETAIL_MONEY;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_ZNAME = detail.BILL_DATA_DETAIL_ZNAME;

                                detaillist.ElementAt(k).BILL_DATA_DETAIL_EDETIAL = detail.BILL_DATA_DETAIL_EDETIAL;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_DETIAL = detail.BILL_DATA_DETAIL_DETIAL;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_CMONEY = detail.BILL_DATA_DETAIL_CMONEY;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_CODE = detail.BILL_DATA_DETAIL_CODE;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_COUNT = detail.BILL_DATA_DETAIL_COUNT;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_PMONEY = detail.BILL_DATA_DETAIL_PMONEY;
                                detaillist.ElementAt(k).BILL_DATA_DETAIL_ISSHOW = detail.BILL_DATA_DETAIL_ISSHOW;
                                

                            }
                            k++;

                        }
                    }
                    i++;
                }

                bool f = await  bll.Updata(mode);
                if (f)
                {
                    result.msg = "审核成功";
                    result.code = 1;
                }
            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return Json(result);
        }
        /// <summary>
        /// 计算
        /// </summary>
        /// <param name="bili">折扣比例</param>
        /// <param name="id">riMedical id</param>
        /// <param name="huilv">当前汇率</param>
        /// <param name="iszhekou">折扣类型 0 1 2</param>
        /// <param name="user">客户折扣</param>
        /// <param name="company">保险公司折扣</param>
        /// <param name="jisuan">是否是计算</param>
        /// <returns></returns>
        private static async Task<AjaxResult> GetReult(int bili, string id, decimal huilv, int iszhekou, int user, int company, bool jisuan)
        {
            BillService bll = new BillService();
            AjaxResult result = new AjaxResult
            {
                code = 0,
                msg = "计算失败"
            };
            try
            {

                RESULTService resultbll = new RESULTService();
                INTEREST_ORDERService interorder = new INTEREST_ORDERService();
                //获取订单数据
                BILL model = await  bll.SelectOne(id);
                //如果是折扣计算 将汇率从计算中找出
                if (!jisuan)
                {
                    huilv = model.RESULTs.FirstOrDefault(p => p.RESULT_ID != "").RESULT_LV.Value;
                }
                var  saledetailno = model.RI_MEDICAL.REPORTINFORMATION.SALEDETAILNO ;
                //获取产品利益表
                //是否有未结算的订单
                var isHave = await resultbll.Select(p => p.SALEDETAILNO == saledetailno && (p.RESULT_STATE == "折扣完成" || p.RESULT_STATE == "计算完成") && p.BILL.RI_MEDICAL.RI_MEDICAL_ID != id);
                if (isHave != null)
                {
                    result.msg = "此保险责任有未支付的报案";
                    return result;
                }
                #region 深拷贝 并进行汇率转换
                INTEREST_ORDER salmodel2 = await interorder.SelectBySale(saledetailno.ToString());
                INTEREST_ORDER salmodel =
                    new INTEREST_ORDER
                    {
                        INTEREST_ORDER_ISSINGLE = salmodel2.INTEREST_ORDER_ISSINGLE,
                        INTEREST_ORDER_MAXMONEY = salmodel2.INTEREST_ORDER_MAXMONEY!= null ? Math.Round((salmodel2.INTEREST_ORDER_MAXMONEY.Value * huilv), 2) : 0,
                        INTEREST_ORDER_MPN = salmodel2.INTEREST_ORDER_MPN != null ? Math.Round((salmodel2.INTEREST_ORDER_MPN.Value * huilv), 2) : 0,
                        INTEREST_ORDER_MPW = salmodel2.INTEREST_ORDER_MPW != null ? Math.Round((salmodel2.INTEREST_ORDER_MPW.Value * huilv), 2) : 0,
                        INTEREST_ORDER_ZFMAX = salmodel2.INTEREST_ORDER_ZFMAX != null ? Math.Round((salmodel2.INTEREST_ORDER_ZFMAX.Value * huilv), 2) : 0,
                        INTEREST_ORDER_ID = salmodel2.INTEREST_ORDER_ID
                    };
                List<IO_DUTY> dutymodle = new List<IO_DUTY>();
                var dutymodel2 = salmodel2.IO_DUTY;
                foreach (var item in dutymodel2)
                {
                    IO_DUTY du = new IO_DUTY
                    {
                        IO_DUTY_ID = item.IO_DUTY_ID,
                        IO_DUTY_MAXMONEY = item.IO_DUTY_MAXMONEY != null ? Math.Round((item.IO_DUTY_MAXMONEY.Value * huilv), 2) : 0,
                        DUTYITEM = item.DUTYITEM
                    };
                    var ioitem2 = item.IO_ITEM;
                    List<IO_ITEM> itemlist = new List<IO_ITEM>();
                    du.INTEREST_ORDER = salmodel;
                    du.INTEREST_ORDER_ID = salmodel.INTEREST_ORDER_ID;
                    foreach (var itmelistitem in ioitem2)
                    {
                        IO_ITEM iomodel = new IO_ITEM
                        {
                            IO_ITEM_ISPER = itmelistitem.IO_ITEM_ISPER,
                            IO_ITEM_MAXDAY = itmelistitem.IO_ITEM_MAXDAY,
                            IO_ITEM_MAXM = itmelistitem.IO_ITEM_MAXM != null ? Math.Round((itmelistitem.IO_ITEM_MAXM.Value * huilv), 2) : 0,
                            IO_ITEM_MAXMW = itmelistitem.IO_ITEM_MAXMW != null ? Math.Round((itmelistitem.IO_ITEM_MAXMW.Value * huilv), 2) : 0,
                            IO_ITEM_ZFBLN = itmelistitem.IO_ITEM_ZFBLN,
                            IO_ITEM_ZFBLW = itmelistitem.IO_ITEM_ZFBLW,
                            IO_ITEM_ZFEN = itmelistitem.IO_ITEM_ZFEN != null ? Math.Round((itmelistitem.IO_ITEM_ZFEN.Value * huilv), 2) : 0,
                            IO_ITEM_ZFEW = itmelistitem.IO_ITEM_ZFEW != null ? Math.Round((itmelistitem.IO_ITEM_ZFEW.Value * huilv), 2) : 0,
                            DUTYITEM = itmelistitem.DUTYITEM,
                            DUTYITEM_ID = itmelistitem.DUTYITEM_ID,
                            IO_DUTY = du,
                            IO_DUTY_ID = du.IO_DUTY_ID,
                            IO_ITEM_ID = itmelistitem.IO_ITEM_ID
                        };

                        itemlist.Add(iomodel);

                    }
                    du.IO_ITEM = itemlist;
                    dutymodle.Add(du);
                }
                salmodel.IO_DUTY = dutymodle;
                #endregion
                RESULT re = new RESULT();
                List<RESULT_DUTY> redutylist = new List<RESULT_DUTY>();
                re.RESULT_ID = CreateId.GetId();
                var list = model.BILL_DATA.OrderBy(p => p.BILL_DATA_DATA);
                //获取账单日信息
                bool compleated = false;//保险是否结束
                foreach (var item in list)
                {

                    RESULT_DUTY reduty = new RESULT_DUTY
                    {
                        RESULT_DUTY_DATE = item.BILL_DATA_DATA,
                        RESULT_DUTY_ID = CreateId.GetId()
                    };

                    List<RESULT_ITEM> reitemlist = new List<RESULT_ITEM>();


                    //天账单计算
                    #region 排序
                    //-----------------------------1.排序开始--------------------------------//
                    //产品利益表排序
                    List<IO_ITEM> orderResult = new List<IO_ITEM>();
                    //获取住院保险责任项和门急诊保险责任项
                    //将治疗费 医疗检验及相关医疗用品费 手术费 取各自的余额 算出最终的余额 赋值给各自
                    IO_DUTY zhuYuan = salmodel.IO_DUTY.Where(p => p.DUTYITEM.DUTYITEM_NAME == "住院保险责任").FirstOrDefault();
                    IO_ITEM zhiLiao = zhuYuan.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "治疗费").FirstOrDefault();
                    IO_ITEM YLJianCha = zhuYuan.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "医疗检验及其相关医疗用品费").FirstOrDefault();
                    IO_ITEM shouShu = zhuYuan.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "手术费").FirstOrDefault();
                    decimal? threeMoney = zhiLiao.IO_ITEM_MAXM + YLJianCha.IO_ITEM_MAXM + shouShu.IO_ITEM_MAXM - (2 * salmodel.INTEREST_ORDER_MAXMONEY);
                    //最终的余额
                    zhiLiao.IO_ITEM_MAXM = threeMoney;
                    YLJianCha.IO_ITEM_MAXM = threeMoney;
                    shouShu.IO_ITEM_MAXM = threeMoney;
                    List<IO_DUTY> ioduty1 = salmodel.IO_DUTY.Where(p => p.DUTYITEM.DUTYITEM_NAME == "住院保险责任" || p.DUTYITEM.DUTYITEM_NAME == "门（急）诊保险责任").ToList();
                    List<IO_DUTY> ioduty2 = salmodel.IO_DUTY.Where(p => p.DUTYITEM.DUTYITEM_NAME == "特殊医疗保险责任").ToList();

                    List<IO_ITEM> itemlist = new List<IO_ITEM>();
                    foreach (var ioduty1Item in ioduty1)
                    {
                        itemlist.AddRange(ioduty1Item.IO_ITEM);

                    }

                    //排序
                    if (item.BILL_DATA_TYPE == "网络内")
                    {
                        itemlist = itemlist.OrderBy(p => p.IO_ITEM_ZFBLN).ThenBy(p => p.IO_ITEM_MAXM).ThenBy(p=>p.DUTYITEM.DUTYITEM_ORDER).ToList();
                    }
                    else
                    {
                        itemlist = itemlist.OrderBy(p => p.IO_ITEM_ZFBLW).ThenBy(p => p.IO_ITEM_MAXM).ThenBy(p => p.DUTYITEM.DUTYITEM_ORDER).ToList();
                    }
                    List<IO_ITEM> itemlist2 = new List<IO_ITEM>();
                    foreach (var ioduty1Item in ioduty2)
                    {
                        itemlist2.AddRange(ioduty1Item.IO_ITEM);
                    }
                    //添加特殊医疗保险
                    if (item.BILL_DATA_TYPE == "网络内")
                    {
                        var teshuList=itemlist2.OrderBy(p => p.IO_ITEM_ZFBLN).ThenBy(p => p.IO_ITEM_MAXM).ThenBy(p => p.DUTYITEM.DUTYITEM_ORDER);
                        itemlist.AddRange(teshuList);
                    }
                    else
                    {
                        itemlist.AddRange(itemlist2.OrderBy(p => p.IO_ITEM_ZFBLW).ThenBy(p => p.IO_ITEM_MAXM).ThenBy(p => p.DUTYITEM.DUTYITEM_ORDER));
                    }



                    //-----------------------------1.排序完毕--------------------------------//
                    #endregion


                    //-----------------------------2.单项计算--------------------------------//
                    #region 单项计算开始
                    //责任项循环，先判断是否有次责任项账单

                    foreach (var itemlistitem in itemlist)
                    {
                        ////-----------------------汇率转换-------------------------//
                        //itemlistitem.IO_ITEM_MAXM = itemlistitem.IO_ITEM_MAXM != null ? Math.Round((itemlistitem.IO_ITEM_MAXM.Value * huilv), 2) : 0;
                        //itemlistitem.IO_ITEM_MAXMW = itemlistitem.IO_ITEM_MAXMW != null ? Math.Round((itemlistitem.IO_ITEM_MAXMW.Value * huilv), 2) : 0;
                        //itemlistitem.IO_ITEM_ZFEN = itemlistitem.IO_ITEM_ZFEN != null ? Math.Round((itemlistitem.IO_ITEM_ZFEN.Value * huilv), 2) : 0;
                        //itemlistitem.IO_ITEM_ZFEW = itemlistitem.IO_ITEM_ZFEN != null ? Math.Round((itemlistitem.IO_ITEM_ZFEW.Value * huilv), 2) : 0;
                        //int index = itemlist.IndexOf(itemlistitem);
                        ////只执行一次
                        //if (index == 0)
                        //{
                        //    itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY = itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY != null ? Math.Round((itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY.Value * huilv), 2) : 0;
                        //    itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY != null ? Math.Round((itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY.Value * huilv), 2) : 0;
                        //    itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN != null ? Math.Round((itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN.Value * huilv), 2) : 0;
                        //    itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW != null ? Math.Round((itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW.Value * huilv), 2) : 0;
                        //    itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX != null ? Math.Round((itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX.Value * huilv), 2) : 0;
                        //}
                        ////-----------------------汇率转换-------------------------//

                        var dutyid = itemlistitem.DUTYITEM_ID;
                        //获取到当前要计算的项
                        //查询
                        var countitemlist = item.BILL_DATA_COUNT.Where(p => p.DUTYITEM_ID == dutyid);
                        if (countitemlist == null || countitemlist.Count() == 0)
                        {
                            //没有要进行赔付的匹配项，返回
                            continue;
                        }
                        var countitem = countitemlist.ElementAt(0);
                        RESULT_ITEM reitem = new RESULT_ITEM()
                        {
                            //赔付结果单项 已确认数据
                            RESULT_ITEM_ALLMONEY = countitem.BILL_DATA_COUNT_MONEY,
                            RESULT_ITEM_ZHEKOU = countitem.BILL_DATA_COUNT_MONEY * bili / 100,
                            RESULT_ITEM_ID = CreateId.GetId(),
                            DUTYITEM_ID = countitem.DUTYITEM_ID,
                            DUTYITEM_FID = countitem.DUTYITEM_FID
                        };
                        if (compleated)
                        {
                            reitem.RESULT_ITEM_PAYMONEY = 0;
                            reitem.RESULT_ITEM_YHZFBLMONEY = 0;
                            if (bili == 100)
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * user / 100;
                            }
                            else
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                            }

                            reitem.RESULT_ITEM_DESCRIBE = "保险在本次结束";
                            reitemlist.Add(reitem);
                            continue;
                        }
                        //-----------------------------2.1 比例确定,免赔额，给付限额，单次自付额--------------------------------//
                        decimal bl = 0;//比例
                        decimal mp = 0;//免赔额
                        //decimal xmoney = 0;//给付限额
                        decimal zfmoney = 0;//自付额
                        decimal allmoneytrue;
                        if (item.BILL_DATA_TYPE == "网络内" && bili != 100)
                        {
                            allmoneytrue = countitem.BILL_DATA_COUNT_MONEY.Value * bili / 100;
                        }
                        else
                        {
                            //如果是网络外 且是进行折扣计算，则不打折
                            allmoneytrue = countitem.BILL_DATA_COUNT_MONEY.Value;
                        }
                        if (item.BILL_DATA_TYPE == "网络内" || item.BILL_DATA_URGENCY == "是")
                        {
                            bl = itemlistitem.IO_ITEM_ZFBLN ?? 0;
                            mp = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN ?? 0;
                            zfmoney = itemlistitem.IO_ITEM_ZFEN ?? 0;
                        }
                        else
                        {
                            bl = itemlistitem.IO_ITEM_ZFBLW == null ? 0 : itemlistitem.IO_ITEM_ZFBLW.Value;
                            mp = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW == null ? 0 : itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW.Value;
                            //按天计算的才有网络外给付限额
                            //if (itemlistitem.IO_ITEM_ISPER == "是")
                            //{
                            //}

                            zfmoney = itemlistitem.IO_ITEM_ZFEW ?? 0;
                        }
                        //超过自付比例上限 并且bl不等于0 即不是特殊医疗网络外的 下面判断是否是重症监护津贴
                        // 赔付比例为0，即为不赔付(特殊医疗网络外不赔付）
                        if (itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX == 0 && bl != 0)
                        {
                            bl = 100;
                        }
                        if (bl == 0 && itemlistitem.DUTYITEM.DUTYITEM_NAME != "重症监护津贴")
                        {
                            //返回赔付为0；
                            reitem.RESULT_ITEM_PAYMONEY = 0;
                            if (bili == 100)
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * user / 100;
                            }
                            else
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                            }
                            //if (item.BILL_DATA_TYPE == "网络内" || item.BILL_DATA_URGENCY == "是")
                            //{
                            //    reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN- itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN;
                            //}
                            //else
                            //{
                            //    reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW;
                            //}
                            reitem.RESULT_ITEM_YHZFBLMONEY = 0;
                           reitem.RESULT_ITEM_DESCRIBE = "赔付比例为0";
                            reitemlist.Add(reitem);
                            continue;
                        }
                        //分类保险额为0
                        if (itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY.Value == 0)
                        {
                            reitem.RESULT_ITEM_PAYMONEY = 0;
                            if (bili == 100)
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * user / 100;
                            }
                            else
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                            }
                            if (itemlistitem.IO_ITEM_ISPER != "是")
                            {

                            }
                            if (item.BILL_DATA_TYPE == "网络内" || item.BILL_DATA_URGENCY == "是")
                            {
                                reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN;
                            }
                            else
                            {
                                reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW;
                            }
                            reitem.RESULT_ITEM_DESCRIBE = "分类保险限额已用完";
                            reitemlist.Add(reitem);
                            continue;
                        }
                        //不是按天计算 并且此项单项给付限额为0
                        if (itemlistitem.IO_ITEM_ISPER != "是" && itemlistitem.IO_ITEM_MAXM.Value == 0)
                        {
                            reitem.RESULT_ITEM_PAYMONEY = 0;
                            if (bili == 100)
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * user / 100;
                            }
                            else
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                            }
                            if (item.BILL_DATA_TYPE == "网络内" || item.BILL_DATA_URGENCY == "是")
                            {
                                reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN;
                            }
                            else
                            {
                                reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW;
                            }
                            reitem.RESULT_ITEM_DESCRIBE = "单项给付限额已用完";
                            reitemlist.Add(reitem);
                            continue;
                        }
                        //如果是按天计算的 并且不是 床位费和膳食费 天数已经用完
                        if (itemlistitem.IO_ITEM_ISPER == "是" && itemlistitem.DUTYITEM.DUTYITEM_NAME != "床位费和膳食费" && itemlistitem.IO_ITEM_MAXDAY != null && itemlistitem.IO_ITEM_MAXDAY.Value == 0)
                        {
                            reitem.RESULT_ITEM_PAYMONEY = 0;
                            if (bili == 100)
                            {
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * user / 100;
                            }
                            else
                            {
                                //如果是网络外                               
                                reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                            }
                            if (item.BILL_DATA_TYPE == "网络内" || item.BILL_DATA_URGENCY == "是")
                            {
                                reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN;
                            }
                            else
                            {
                                reitem.RESULT_ITEM_YHZFBLMONEY = allmoneytrue - itemlistitem.IO_ITEM_ZFEN - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW;
                            }
                            reitem.RESULT_ITEM_DESCRIBE = "给付期限用完";
                            reitemlist.Add(reitem);
                            continue;
                        }
                        //当前账单信息

                        //-----------------------------变量声明--------------------------------//
                       

                        decimal ypay = 0;//保险公司应该赔付的
                        decimal zfbl = 0;//自付比例金额
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "重症监护津贴")
                        {
                            //重症监护津贴有网络内外
                            if(item.BILL_DATA_TYPE == "网络内")
                            {
                                ypay = itemlistitem.IO_ITEM_MAXM.Value;
                            }
                            else
                            {
                                ypay = itemlistitem.IO_ITEM_MAXMW.Value;
                            }
                           
                        }
                        else
                        {

                            //是否超过单次自付额
                            if (allmoneytrue < zfmoney)
                            {
                                //返回赔付为0；
                                reitem.RESULT_ITEM_MPMONEY = 0;
                                reitem.RESULT_ITEM_PAYMONEY = 0;
                                if (bili == 100)
                                {
                                    reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * user / 100;
                                }
                                else
                                {
                                    if (item.BILL_DATA_TYPE == "网络内")
                                    {
                                        reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                                    }
                                    else
                                    {
                                        reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY;
                                    }

                                }

                                reitem.RESULT_ITEM_YHZFBLMONEY = 0;
                                reitem.RESULT_ITEM_DESCRIBE = "未超过自付比例金额";
                                reitemlist.Add(reitem);
                                continue;
                            }
                            //是否大于免赔额
                            if (allmoneytrue - zfmoney < mp)
                            {
                                //不大于免赔额
                                reitem.RESULT_ITEM_MPMONEY = allmoneytrue - zfmoney;
                                if (item.BILL_DATA_TYPE == "网络内" || item.BILL_DATA_URGENCY == "是")
                                {
                                    itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN -= allmoneytrue - zfmoney;
                                    reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                                }
                                else
                                {
                                    itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW -= allmoneytrue - zfmoney;
                                    reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY;
                                   
                                }

                                //返回赔付为0；
                                reitem.RESULT_ITEM_PAYMONEY = 0;
                                if (bili == 100)
                                {
                                    reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * user / 100;
                                }
                                else
                                {
                                    if (item.BILL_DATA_TYPE == "网络内")
                                    {
                                        reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY * bili / 100;
                                    }
                                    else
                                    {
                                        reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY;
                                    }
                                }
                                reitem.RESULT_ITEM_YHZFBLMONEY = 0;
                                reitem.RESULT_ITEM_DESCRIBE = "未超过免赔额";
                                reitemlist.Add(reitem);
                                continue;
                            }
                            //计算应赔付的金额
                            ypay = (allmoneytrue - zfmoney - mp) * bl / 100;
                            //自付比例金额超
                            zfbl = (allmoneytrue - zfmoney - mp) * (100 - bl) / 100;
                            //自付比例金额超
                            if (zfbl > itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX)
                            {
                                ypay += zfbl - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX.Value;
                                zfbl = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX.Value;

                            }

                            //赔付金额大于单项限额
                            //按天计算的 要判断网络内外 并且不是重症监护津贴
                            if (itemlistitem.IO_ITEM_ISPER=="是"&& itemlistitem.DUTYITEM.DUTYITEM_NAME!= "重症监护津贴")
                            {
                                //是网络内或者 是紧急情况
                                if (item.BILL_DATA_TYPE == "网络内" ||  item.BILL_DATA_URGENCY == "是")
                                {
                                    if (ypay > itemlistitem.IO_ITEM_MAXM)
                                    {
                                        zfbl += ypay - itemlistitem.IO_ITEM_MAXM.Value;
                                        ypay = itemlistitem.IO_ITEM_MAXM.Value;
                                        reitem.RESULT_ITEM_DESCRIBE = "达到单项限额上限";

                                    }
                                }
                                else
                                {
                                    if (ypay > itemlistitem.IO_ITEM_MAXMW)
                                    {
                                        zfbl += ypay - itemlistitem.IO_ITEM_MAXMW.Value;
                                        ypay = itemlistitem.IO_ITEM_MAXMW.Value;
                                        reitem.RESULT_ITEM_DESCRIBE = "达到单项限额上限";

                                    }
                                }
                            }
                          else if (ypay > itemlistitem.IO_ITEM_MAXM)
                            {
                                zfbl += ypay - itemlistitem.IO_ITEM_MAXM.Value;
                                ypay = itemlistitem.IO_ITEM_MAXM.Value;
                                reitem.RESULT_ITEM_DESCRIBE = "达到单项限额上限";

                            }
                        }
                        IO_DUTY teShu = salmodel.IO_DUTY.Where(p => p.DUTYITEM.DUTYITEM_NAME == "特殊医疗保险责任").FirstOrDefault();
                        // 如果是精神疾病和行为障碍医疗费
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费-住院治疗费" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费-门诊治疗费")
                        {
                            //
                            IO_ITEM jingShen = teShu.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费").FirstOrDefault();
                            if (ypay > jingShen.IO_ITEM_MAXM)
                            {                                
                                zfbl += ypay - jingShen.IO_ITEM_MAXM.Value;
                                ypay = jingShen.IO_ITEM_MAXM.Value;
                                reitem.RESULT_ITEM_DESCRIBE = "达到精神疾病和行为障碍医疗费上限";
                            }
                        }

                        //如果是预防保健费
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "预防保健费-免疫接种" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "预防保健费-女性专属预防护理及疾病排查" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "预防保健费-儿童/青少年预防护理及疾病排查")
                        {
                            //
                            IO_ITEM jingShen = teShu.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "预防保健费").FirstOrDefault();
                            if (ypay > jingShen.IO_ITEM_MAXM)
                            {
                                zfbl += ypay - jingShen.IO_ITEM_MAXM.Value;
                                ypay = jingShen.IO_ITEM_MAXM.Value;
                                reitem.RESULT_ITEM_DESCRIBE = "达到预防保健费上限";
                            }
                        }
                        //如果是生育医疗费
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "生育医疗费-生育保障及产后护理费" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "生育医疗费-新生儿检查护理及预防保健费" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "生育医疗费-妊娠综合并发症治疗费")
                        {
                            //
                            IO_ITEM jingShen = teShu.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "生育医疗费").FirstOrDefault();
                            if (ypay > jingShen.IO_ITEM_MAXM)
                            {
                                zfbl += ypay - jingShen.IO_ITEM_MAXM.Value;
                                ypay = jingShen.IO_ITEM_MAXM.Value;
                                reitem.RESULT_ITEM_DESCRIBE = "达到生育医疗费上限";
                            }
                        }

                        //赔付金额大于分类保险额
                        if (ypay > itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY)
                        {
                            zfbl += ypay - itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY.Value;
                            ypay = itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY.Value;
                            reitem.RESULT_ITEM_DESCRIBE = "达到分类保险额上限";
                        }
                        //赔付金额大于保险金额

                        if (ypay > itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY)
                        {
                            zfbl += ypay - itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY.Value;
                            ypay = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY.Value;
                            reitem.RESULT_ITEM_DESCRIBE = "达到保险金额上限";
                            compleated = true;

                            //结束所有计算
                        }
                        //结束计算
                        //自付比例金额
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "重症监护津贴")
                        {
                            zfbl = 0;
                            //当前应减去的免赔额为0
                            reitem.RESULT_ITEM_MPMONEY = 0;
                            reitem.RESULT_ITEM_DESCRIBE = "重症监护津贴";
                            reitem.RESULT_ITEM_PAYMONEY = ypay;
                            reitem.RESULT_ITEM_USERMONEY = countitem.BILL_DATA_COUNT_MONEY.Value;
                            reitem.RESULT_ITEM_YHZFBLMONEY = zfbl;

                        }
                        else
                        {
                            if (reitem.RESULT_ITEM_DESCRIBE == null || reitem.RESULT_ITEM_DESCRIBE == "")
                            {
                                reitem.RESULT_ITEM_DESCRIBE = "正常赔付";
                            }
                            ypay = Math.Round(ypay, 2, MidpointRounding.AwayFromZero);
                            if (bili == 100)
                            {
                                reitem.RESULT_ITEM_PAYMONEY = ypay * company / 100;
                                reitem.RESULT_ITEM_USERMONEY = (allmoneytrue - ypay) * user / 100;

                            }
                            else
                            {
                                reitem.RESULT_ITEM_PAYMONEY = ypay;
                                reitem.RESULT_ITEM_USERMONEY = allmoneytrue - ypay;
                            }
                            //写入结果表  

                            reitem.RESULT_ITEM_YHZFBLMONEY = zfbl;
                            //免赔额 更新
                            if (item.BILL_DATA_TYPE == "网络内" || item.BILL_DATA_URGENCY == "是")
                            {
                                itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPN = 0;
                            }
                            else
                            {
                                itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MPW = 0;
                            }
                            zfbl = allmoneytrue - mp - zfmoney - ypay > 0 ? allmoneytrue - mp - zfmoney - ypay : 0;
                        }

                        //更新利益表

                        // 如果是精神疾病和行为障碍医疗费
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费-住院治疗费" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费-门诊治疗费")
                        {
                            //
                            IO_ITEM jingShen = teShu.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "精神疾病和行为障碍医疗费").FirstOrDefault();
                            jingShen.IO_ITEM_MAXM = jingShen.IO_ITEM_MAXM - ypay > 0 ? jingShen.IO_ITEM_MAXM - ypay : 0;
                        }

                        //如果是预防保健费
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "预防保健费-免疫接种" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "预防保健费-女性专属预防护理及疾病排查" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "预防保健费-儿童/青少年预防护理及疾病排查")
                        {
                            //
                            IO_ITEM jingShen = teShu.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "预防保健费").FirstOrDefault();
                            jingShen.IO_ITEM_MAXM = jingShen.IO_ITEM_MAXM - ypay > 0 ? jingShen.IO_ITEM_MAXM - ypay : 0;
                        }
                        //如果是生育医疗费
                        if (itemlistitem.DUTYITEM.DUTYITEM_NAME == "生育医疗费-生育保障及产后护理费" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "生育医疗费-新生儿检查护理及预防保健费" || itemlistitem.DUTYITEM.DUTYITEM_NAME == "生育医疗费-妊娠综合并发症治疗费")
                        {
                            //
                            IO_ITEM jingShen = teShu.IO_ITEM.Where(p => p.DUTYITEM.DUTYITEM_NAME == "生育医疗费").FirstOrDefault();
                            jingShen.IO_ITEM_MAXM = jingShen.IO_ITEM_MAXM - ypay > 0 ? jingShen.IO_ITEM_MAXM - ypay : 0;


                        }
                        if (itemlistitem.IO_ITEM_ISPER != "是")
                        {
                            //不是按天
                            //单项限额
                            itemlistitem.IO_ITEM_MAXM = itemlistitem.IO_ITEM_MAXM - ypay > 0 ? itemlistitem.IO_ITEM_MAXM - ypay : 0;
                            //分类限额

                        }
                        else
                        {
                            //更新天数
                            itemlistitem.IO_ITEM_MAXDAY = itemlistitem.IO_ITEM_MAXDAY - 1 > 0 ? itemlistitem.IO_ITEM_MAXDAY - 1 : 0;
                        }
                        itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY = itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY - ypay > 0 ? itemlistitem.IO_DUTY.IO_DUTY_MAXMONEY - ypay : 0;
                        //保险限额
                        itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY - ypay > 0 ? itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_MAXMONEY - ypay : 0;
                        //自付比例上限
                        itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX = itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX - zfbl > 0 ? itemlistitem.IO_DUTY.INTEREST_ORDER.INTEREST_ORDER_ZFMAX - zfbl : 0;

                        reitemlist.Add(reitem);

                    }
                    #endregion
                    //汇总最后结果 得出当日的赔付的金额,用户的实际支付的金额
                    decimal allpaymoney = 0;
                    decimal userpaymoney = 0;
                    decimal yhzfblmoney = 0;
                    decimal mpmoney = 0;
                    decimal allmoney = 0;
                    foreach (var reitemlistitem in reitemlist)
                    {
                        allpaymoney += reitemlistitem.RESULT_ITEM_PAYMONEY.Value;
                        userpaymoney += reitemlistitem.RESULT_ITEM_USERMONEY.Value;
                        yhzfblmoney += reitemlistitem.RESULT_ITEM_YHZFBLMONEY.Value;
                        if (reitemlistitem.RESULT_ITEM_MPMONEY != null)
                        {
                            mpmoney += reitemlistitem.RESULT_ITEM_MPMONEY.Value;
                        }
                        allmoney += reitemlistitem.RESULT_ITEM_ALLMONEY.Value;

                    }
                    reduty.RESULT_DUTY_PAYMONEY = allpaymoney;
                    reduty.RESULT_DUTY_USERMONEY = userpaymoney;
                    reduty.RESULT_DUTY_ZFBLMONEY = yhzfblmoney;
                    reduty.RESULT_DUTY_ALLMONEY = allmoney;
                    reduty.RESULT_DUTY_ZHEKOU = allmoney * bili / 100;
                    reduty.RESULT_DUTY_MPMONEY = mpmoney;
                    reduty.BILL_DATA_ID = item.BILL_DATA_ID;
                    reduty.RESULT_ITEM = reitemlist;
                    redutylist.Add(reduty);
                }
                //计算赔付的金额
                decimal allpaymoneyduty = 0;
                decimal userpaymoneyduty = 0;
                decimal yhzfblmoneyduty = 0;
                decimal mpmoneyduty = 0;
                decimal allmoneyduty = 0;
                foreach (var item in redutylist)
                {
                    allpaymoneyduty += item.RESULT_DUTY_PAYMONEY.Value;
                    userpaymoneyduty += item.RESULT_DUTY_USERMONEY.Value;
                    yhzfblmoneyduty += item.RESULT_DUTY_ZFBLMONEY.Value;
                    if (item.RESULT_DUTY_MPMONEY != null)
                    {
                        mpmoneyduty += item.RESULT_DUTY_MPMONEY.Value;
                    }
                    //allmoneyduty += item.RESULT_DUTY_ALLMONEY.Value;
                }
                //有医师审核 每天通过的项 计入总花费 和用户实际支出
                var billdateList = model.BILL_DATA;
                foreach (var billdateItem in billdateList)
                {
                    var billDetaDetailList = billdateItem.BILL_DATA_DETAIL.Where(p=>p.BILL_DATA_DETAIL_ISSHOW!="删除");                    
                    allmoneyduty += billdateItem.BILL_DATA_DETAIL.Sum(p => p.BILL_DATA_DETAIL_CMONEY).Value;                  
                }
              
                re.RESULT_PAYMONEY = allpaymoneyduty;
                //if()
                re.RESULT_USERMONEY = allmoneyduty*bili/100 - allpaymoneyduty;
                re.RESULT_ZFBLMONEY = yhzfblmoneyduty;
                re.RESULT_ALLMONEY = allmoneyduty;
                //最后更新利益表是 

                re.RESULT_MPMONEY = mpmoneyduty;
                if (iszhekou == 0)
                {
                    re.RESULT_STATE = "计算完成";
                }
                else
                {
                    re.RESULT_STATE = "折扣完成";
                }



                re.RESULT_DUTY = redutylist;

                //不能使用model 会更新保险利益表
                BILL model2 = await bll.SelectOne(id);
                if (iszhekou == 0)
                {
                    model2.BILL_STATE = "计算完成";
                    model2.RI_MEDICAL.RI_MEDICAL_STATE = "计算完成";
                }
                else
                {
                    model2.BILL_STATE = "折扣完成";
                    model2.RI_MEDICAL.RI_MEDICAL_STATE = "折扣完成";
                    if (user != 100)
                    {
                        re.RESULT_ZHEKOU = company;
                        re.RESULT_ZHEKOUUSER = user;
                    }
                    else
                    {
                        re.RESULT_ZHEKOU = bili;
                        re.RESULT_ZHEKOUUSER = bili;
                    }


                }
                re.SALEDETAILNO = saledetailno;
                re.RESULT_LV = huilv;
                re.BILL = model2;
                if (jisuan)
                {
                    RESULT resultsmodel = await  resultbll.Add(re);
                    if (resultsmodel.BILL_ID.Length > 0)
                    {
                        result.code = 1;
                        result.msg = "计算完毕";
                    }
                }
                else
                {
                    var haveResult = await resultbll.Select(p => p.SALEDETAILNO == saledetailno && p.BILL.RI_MEDICAL_ID == id);
                    haveResult.RESULT_STATE = re.RESULT_STATE;
                    haveResult.RESULT_LV = re.RESULT_LV;
                    haveResult.RESULT_MPMONEY = re.RESULT_MPMONEY;
                    haveResult.RESULT_PAYMONEY = re.RESULT_PAYMONEY;
                    haveResult.RESULT_USERMONEY = re.RESULT_USERMONEY;
                    haveResult.RESULT_ZFBLMONEY = re.RESULT_ZFBLMONEY;
                    haveResult.RESULT_ZHEKOU = re.RESULT_ZHEKOU;
                    haveResult.RESULT_ZHEKOUUSER = re.RESULT_ZHEKOUUSER;
                    haveResult.RESULT_ALLMONEY = re.RESULT_ALLMONEY;

                    int i = 0;
                    foreach (var item in haveResult.RESULT_DUTY)
                    {
                        var resultDuty = re.RESULT_DUTY.Where(p => p.RESULT_DUTY_DATE == item.RESULT_DUTY_DATE).ElementAtOrDefault(0);
                        item.RESULT_DUTY_PAYMONEY = resultDuty.RESULT_DUTY_PAYMONEY;
                        item.RESULT_DUTY_USERMONEY = resultDuty.RESULT_DUTY_USERMONEY;
                        item.RESULT_DUTY_ALLMONEY = resultDuty.RESULT_DUTY_ALLMONEY;
                        item.RESULT_DUTY_DATE = resultDuty.RESULT_DUTY_DATE;
                        item.RESULT_DUTY_ZFBLMONEY = resultDuty.RESULT_DUTY_ZFBLMONEY;
                        item.RESULT_DUTY_MPMONEY = resultDuty.RESULT_DUTY_MPMONEY;
                        item.RESULT_DUTY_ZHEKOU = resultDuty.RESULT_DUTY_MPMONEY;

                        int k = 0;

                        foreach (var item2 in item.RESULT_ITEM)
                        {
                            var itemDuty = resultDuty.RESULT_ITEM.Where(p => p.DUTYITEM_ID == item2.DUTYITEM_ID).ElementAtOrDefault(0);
                            item2.RESULT_ITEM_ALLMONEY = itemDuty.RESULT_ITEM_ALLMONEY;
                            item2.RESULT_ITEM_USERMONEY = itemDuty.RESULT_ITEM_USERMONEY;
                            item2.RESULT_ITEM_PAYMONEY = itemDuty.RESULT_ITEM_PAYMONEY;
                            item2.RESULT_ITEM_YHZFBLMONEY = itemDuty.RESULT_ITEM_YHZFBLMONEY;
                            item2.RESULT_ITEM_MPMONEY = itemDuty.RESULT_ITEM_MPMONEY;
                            item2.RESULT_ITEM_ZHEKOU = itemDuty.RESULT_ITEM_ZHEKOU;
                            k++;
                        }
                        i++;

                    }

                    bool f = await  resultbll.Updata(haveResult);
                    if (f)
                    {
                        result.code = 1;
                        result.msg = "折扣完成";
                    }
                }

            }
            catch (Exception e)
            {

                result.msg = e.Message;
            }
            return result;
        }

        ///// <summary>
        ///// 获取 未计算的账单  BILL_DATA_DETAIL_STATE=="删除"
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public IHttpActionResult GetIsDelect(string id) 
        //{
        //    AjaxResult result = new AjaxResult
        //    {
        //        code = 0,
        //        msg = ""
        //    };
        //    try
        //    {

        //        List<dynamic> list = bll.SelectOnePart(id).ToList();
        //        result.code = 1;
        //        result.msg = "获取成功";

        //        //List<dynamic> list = new List<dynamic>();
        //        //list.Add(dy);
        //        result.result = list;
        //    }
        //    catch (Exception e)
        //    {

        //        result.msg = e.Message;
        //    }

        //    return Json(result, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        //}
    }
   
}
