using Aspose.Words;
using DTO;
using Helper.Base;
using Helper.Out;
using Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using LinqKit;
using System.Globalization;
using BLL;

namespace KJYLServer.Controllers
{
    public partial class SALESDETAILController
    {
        /// <summary>
        /// 新增 包含 新用户的 方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("api/SALESDETAIL/AddDto")]
        public async Task<IHttpActionResult> AddDto(SealDetailInputDto model)
        {
            try
            {
                bool f = await bll.AddDto(model);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 1, Msg = "添加成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "失败" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 通过类型 获取列表
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/SALESDETAIL/GetByState")]
        public async Task<IHttpActionResult> GetByState(string state, int pageSize, int pageIndex)
        {
            try
            {
                var re = await bll.GetDto(state, pageSize, pageIndex,null);
                return this.JsonMy(new AjaxResultList<SealDetailInputDto> { Code = 1, Result = re.Item1.ToList(), ListCount = re.Item2, Msg = "获取成功" });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
        [HttpGet]
        [Route("api/SALESDETAIL/GetDtoOne")]
        public async Task<IHttpActionResult> GetDtoOne(string id)
        {
            try
            {
                var re = (await bll.GetDtoOne(id)).FirstOrDefault();
                return this.JsonMy(new AjaxResult<SealDetailInputDto> { Code = 1, Result = re,Msg = "获取成功" });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpPut]
        [Route("api/SALESDETAIL/UpdateDto")]
        public async Task<IHttpActionResult> UpdateDto(SealDetailInputDto model)
        {
            try
            {
                bool f = await bll.UpdateDto(model);
                if (f)
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 1, Msg = "添加成功" });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "失败" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }

        /// <summary>
        /// /// 购买完成 并且计算佣金和 公司服务费 佣金
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/SALESDETAIL/MoneyCustom")]
        public async Task<IHttpActionResult> PutMoneyCustom([FromBody] SALESDETAIL model)
        {
            try
            {
                //优化查询次数
                SALESDETAIL smodel = await bll.SelectOne(p => p.SALESDETAIL_ID == model.SALESDETAIL_ID);
                //获取 渠道信息 渠道返佣比例 渠道推荐人 推荐人返佣比例 渠道一级推荐人返佣比例
                CUSTOMService cBll =new CUSTOMService();
                ORGANIZATIONService oBll=new ORGANIZATIONService();
                CUSTOM cmodel = await cBll.SelectOne(p => p.CUSTOM_ID == smodel.CUSTOM_ID);
                if (cmodel.ORGANIZATION_ID != null)
                {
                    List<BROKERAGE> bList=new List<BROKERAGE>();
                    //获取 渠道信息 渠道返佣比例 渠道推荐人 推荐人返佣比例 渠道一级推荐人返佣比例
                    ORGANIZATION oModel = await oBll.SelectOne(p => p.ORGANIZATION_ID == cmodel.ORGANIZATION_ID);
                    if (oModel.ORGANIZATION_RATIO != null)
                    {
                        var money = (smodel.SALESDETAIL_ADDMONEY ?? 0 + smodel.SALESDETAIL_MONEY ?? 0) *
                                    oModel.ORGANIZATION_RATIO / 100;
                        BROKERAGE brokerageModel1 = new BROKERAGE
                        {
                            SALESDETAIL_ID = smodel.SALESDETAIL_ID,
                            BROKERAGE_ID = GetRandom.GetId(),
                            ORGANIZATION_ID = oModel.ORGANIZATION_ID,
                            BROKERAGE_FROM = "本渠道订单",
                            BROKERAGE_MONEY = money,
                            BROKERAGE_STATE = "需结算",
                            BROKERAGE_RATIO = oModel.ORGANIZATION_RATIO.ToString()
                        };
                        bList.Add(brokerageModel1);
                    }
                    // 返佣比例为空 记录日志（后期增加）
                    else
                    {
                    }
                    
                    if (oModel.ORGANIZATION_REFERRER != null)
                    {
                        ORGANIZATION oModel2 = await oBll.SelectOne(p => p.ORGANIZATION_ID == oModel.ORGANIZATION_REFERRER);
                        if (oModel2.ORGANIZATION_RATIO != null)
                        {
                            var money = (smodel.SALESDETAIL_ADDMONEY ?? 0 + smodel.SALESDETAIL_MONEY ?? 0) *
                                        oModel.ORGANIZATION_REFERRERRATIO / 100;
                            BROKERAGE brokerageModel2 = new BROKERAGE
                            {
                                SALESDETAIL_ID = smodel.SALESDETAIL_ID,
                                BROKERAGE_ID = GetRandom.GetId(),
                                ORGANIZATION_ID = oModel2.ORGANIZATION_ID,
                                BROKERAGE_FROM = "一级订单",
                                BROKERAGE_MONEY = money,
                                BROKERAGE_STATE = "需结算",
                                BROKERAGE_RATIO = oModel.ORGANIZATION_REFERRERRATIO.ToString()
                            };
                            bList.Add(brokerageModel2);
                        }
                        // 返佣比例为空 记录日志（后期增加）
                        else
                        {
                        }
                       
                        //如果 推荐人的 也有推荐人 并且 有该推荐人的返佣比例
                        if (oModel2.ORGANIZATION_REFERRER != null && oModel.ORGANIZATION_UPREFERRERRATIO != null)
                        {
                            ORGANIZATION oModel3 = await oBll.SelectOne(p => p.ORGANIZATION_ID == oModel2.ORGANIZATION_REFERRER);
                            if (oModel3.ORGANIZATION_RATIO != null)
                            {
                                var money = (smodel.SALESDETAIL_ADDMONEY ?? 0 + smodel.SALESDETAIL_MONEY ?? 0) *
                                            oModel.ORGANIZATION_UPREFERRERRATIO / 100;
                                BROKERAGE brokerageModel3 = new BROKERAGE
                                {
                                    SALESDETAIL_ID = smodel.SALESDETAIL_ID,
                                    BROKERAGE_ID = GetRandom.GetId(),
                                    ORGANIZATION_ID = oModel3.ORGANIZATION_ID,
                                    BROKERAGE_FROM = "二级订单",
                                    BROKERAGE_MONEY = money,
                                    BROKERAGE_STATE = "需结算",
                                    BROKERAGE_RATIO = oModel.ORGANIZATION_UPREFERRERRATIO.ToString()
                                };
                                bList.Add(brokerageModel3);
                            }
                        }
                    }
                    if (bList.Count != 0)
                    {
                        bool fs = await bll.UpdateMoney(model, bList);
                        if (fs)
                        {
                            return this.JsonMy(new AjaxResult<SALESDETAIL> { Code = 1, Msg = "" });
                        }
                        else
                        {
                            return this.JsonMy(new AjaxResult<SALESDETAIL> { Code = 0, Msg = "请求失败" });
                        }
                    }
                   
                }
                bool f = await bll.UpdateComlate(model);
                    if (f)
                    {
                        return this.JsonMy(new AjaxResult<SALESDETAIL> { Code = 1, Msg = "" });
                    }
                    else
                    {
                        return this.JsonMy(new AjaxResult<SALESDETAIL> { Code = 0, Msg = "请求失败" });
                    }
                
                

            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
       /// <summary>
       /// 根据名称 和状态获取
       /// </summary>
       /// <param name="state"></param>
       /// <param name="pageSize"></param>
       /// <param name="pageIndex"></param>
       /// <param name="name"></param>
       /// <returns></returns>
        [HttpGet]
        [Route("api/SALESDETAIL/GetByName")]
        public async Task<IHttpActionResult> GetByName(string state, int pageSize, int pageIndex,string name)
        {
            try
            {
                var re = await bll.GetDto(state, pageSize, pageIndex, p=>p.CUSTOM.CUSTOM_NAME.Contains(name));
                return this.JsonMy(new AjaxResultList<SealDetailInputDto> { Code = 1, Result = re.Item1.ToList(), ListCount = re.Item2, Msg = "获取成功" });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }
        }
        ///// <summary>
        ///// 财务结算用 获取列表
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("api/SALESDETAIL/GetSearchDynimic")]
//        public async Task<IHttpActionResult> GetSearchDynimic([FromUri]SealDetailSearchDto model)
//        {
//            try
//            {
//                if (model.PageIndex != null && model.PageSize != null) { 
//                    //统一为购买完成的
                   
//                    Expression<Func<SealDetailInputDto, bool>> whExpression = p => true ;
//                    ////结算类型 购买完成 
//                    switch (model.Type)
//                    {
//                        case "导出":
//                            //whExpression = whExpression.And(p => p.SALESDETAIL.SALESDETAIL_PAYSTATE == null && p.SALESDETAIL.SALESDETAIL_COMMISSIONSTATE == null && p.SALESDETAIL.SALESDETAIL_SERVERMONEYSTATE == null);
//                            break;
//                        case "保费支付":
//                            //whExpression = whExpression.And(p => p.SALESDETAIL.SALESDETAIL_PAYNUMBER!=null);
//                            break;
//                        case "佣金结算":
//                            //whExpression = whExpression.And(p =>  p.SALESDETAIL.SALESDETAIL_COMMISSIONNUMBER != null);
//                            break;
//                        case "服务费":
//                            //whExpression = whExpression.And(p => p.SALESDETAIL.SALESDETAIL_SERVERMONEYNUMBER != null);
//                            break;
//                    }
//                    //销售时间 开始 结束
//                    if (model.StimeStart!=null&& model.StimeEnd!=null)
//                    {
//                        DateTime startTime =model.StimeStart.Value;
//                        DateTime endTime = model.StimeEnd.Value.ToUniversalTime();
//                        DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
//                        {
//                            ShortDatePattern = "yyyy/MM/dd"
//                        };
//                        DateTime stateSt = Convert.ToDateTime(startTime, dtFormat);
//                        DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).AddDays(1);
//                        whExpression= whExpression.And(p => p.SALESDETAIL.SALESDETAIL_SALESTIME <= stateEnd && p.SALESDETAIL.SALESDETAIL_SALESTIME >= stateSt);
//                    }
//                    //退保时间
//                    if (model.TtimeStart != null && model.TtimeEnd != null)
//                    {
//                        DateTime startTime = model.TtimeStart.Value;
//                        DateTime endTime = model.TtimeEnd.Value;
//                        DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
//                        {
//                            ShortDatePattern = "yyyy/MM/dd"
//                        };
//                        DateTime stateSt = Convert.ToDateTime(startTime, dtFormat);
//                        DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).AddDays(1);
//                        whExpression = whExpression.And(p => p.SALESDETAIL.SALESDETAIL_BACKTIME <= stateEnd && p.SALESDETAIL.SALESDETAIL_BACKTIME >= stateSt);
//                    }
//                    var re = await bll.GetDtoCompany( model.PageSize.Value, model.PageIndex.Value,whExpression, model.Type);
//                    return this.JsonMy(new AjaxResultList<SealDetailInputDto> { Code = 1, Result = re.Item1.ToList(), ListCount = re.Item2, Msg = "获取成功" });
//                }
//                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "" });
//            }
//            catch (Exception e)
//            {
//                var msg = "请求失败";
//#if DEBUG
//                msg = e.Message;
//#endif
//                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
//            }
          
//        }
        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
//        [HttpPut]
//        [Route("api/SALESDETAIL/Center")]
//        public async Task<IHttpActionResult> PutCenter([FromBody] List<CenterDto> list)
//        {
//            try
//            {
//                var idList = list.Select(p => p.Id).ToList();
//                var modelList = (await bll.SelectList(p => idList.Contains(p.SALESDETAIL_ID))).ToList();
//                foreach (var item in list)
//                {
//                    var itemModel = modelList.FirstOrDefault(p => p.SALESDETAIL_ID == item.Id);
//                    if (item.Style == "保费支付")
//                    {
//                        if (itemModel != null)
//                        {
//                            var centerString = itemModel.SALESDETAIL_PAYNUMBER.IndexOf(',');
//                            if (centerString>0)
//                            {
//                                itemModel.SALESDETAIL_PAYNUMBER = item.Type == "购买结算" ? "退保结算" : "购买结算";
//                            }
//                            else
//                            {
//                                itemModel.SALESDETAIL_PAYNUMBER = null;
//                            }
//                        };
//                    }
//                    if (item.Style == "佣金结算")
//                    {
//                        if (itemModel != null)
//                        {
//                            var centerString = itemModel.SALESDETAIL_COMMISSIONNUMBER.IndexOf(',');
//                            if (centerString > 0)
//                            {
//                                itemModel.SALESDETAIL_COMMISSIONNUMBER = item.Type == "购买结算" ? "退保结算" : "购买结算";
//                            }
//                            else
//                            {
//                                itemModel.SALESDETAIL_COMMISSIONNUMBER = null;
//                            }
//                            //购买结算 确认
//                            if (itemModel.SALESDETAIL_COMMISSIONSTATE == null)
//                            {
//                                if (itemModel.SALESDETAIL_STATE == "退保完成" )
//                                {
//                                    //如果两个一起 确认
//                                    if (list.Where(p => p.Id == itemModel.SALESDETAIL_ID).ToList().Count == 2)
//                                    {
//                                        itemModel.SALESDETAIL_COMMISSIONSTATE = "退保结算已确认";
//                                    }
//                                    else
//                                    {
//                                        if (item.Type == "购买结算")
//                                        {
//                                            itemModel.SALESDETAIL_COMMISSIONSTATE = "已结算退保";
//                                        }
//                                        else
//                                        {
//                                            itemModel.SALESDETAIL_COMMISSIONSTATE = "已退保结算未销售结算";
//                                        }
//                                    }
//                                }
//                                else
//                                {
//                                    itemModel.SALESDETAIL_COMMISSIONSTATE = "销售结算已确认";
//                                }
                               
//                            }
//                            else
//                            {
//                                //已经结算 过 又退保了
//                                if (itemModel.SALESDETAIL_COMMISSIONSTATE == "已结算退保")
//                                {
//                                    itemModel.SALESDETAIL_COMMISSIONSTATE = "退保结算已确认";
//                                }
//                                // 已退保结算 未销售结算
//                                if (itemModel.SALESDETAIL_COMMISSIONSTATE == "已退保结算未销售结算")
//                                {
//                                    itemModel.SALESDETAIL_COMMISSIONSTATE = "退保结算已确认";
//                                }
//                            }
//                        }
//                    }
//                    if (item.Style == "服务费")
//                    {
//                        if (itemModel != null)
//                        {
//                            var centerString = itemModel.SALESDETAIL_SERVERMONEYNUMBER.IndexOf(',');
//                            if (centerString > 0)
//                            {
//                                itemModel.SALESDETAIL_SERVERMONEYNUMBER = item.Type == "购买结算" ? "退保结算" : "购买结算";
//                            }
//                            else
//                            {
//                                itemModel.SALESDETAIL_SERVERMONEYNUMBER = null;
//                            }
//                        }
//                    }
//                }
//                bool f = await bll.UpdataAll(modelList);
//                if (f)
//                {
//                    return this.JsonMy(new AjaxResult<object> { Code = 1, Msg = "" });
//                }
//                else
//                {
//                    return this.JsonMy(new AjaxResult<object> { Code = 0, Msg = "" });
//                }
//            }
//            catch (Exception e)
//            {
//                var msg = "请求失败";
//#if DEBUG
//                msg = e.Message;
//#endif
//                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
//            }
//        }
         /// <summary>
        /// 获取 投保单文件 使用aspone.word 破解版
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ///
        [HttpGet]
        [Route("api/SALESDETAIL/GetDoc")]
        public async Task<IHttpActionResult> GetDoc(string id)
        {
            try
            {
                var data = (await bll.GetDtoOne(id)).FirstOrDefault();               
                string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                if (data!=null)
                {
                    var sealDetailModel = data.SALESDETAIL;
                    var customModel = data.CUSTOM;
                    Document doc;
                    string strPath = baseurl+ @"Upload/PrintInsurancePolicy/大地境外留学医疗保障投保单.docx";
                    string strPath1 = baseurl + @"Upload/PrintInsurancePolicy/大地境外留学医疗保障投保单无受益人.docx";
                    List<FAVOREE> tdBeneficiaryList = sealDetailModel.FAVOREEs.ToList();//获取受益人信息
                    if (tdBeneficiaryList.Count > 0)
                    {
                        doc = new Document(strPath);
                        strPath1 = string.Empty;
                    }
                    else
                    {
                        doc = new Document(strPath1);
                        strPath = string.Empty;
                    }
                    DocumentBuilder builder = new DocumentBuilder(doc);
                    /***************************保险关系人信息**************************************/
                    #region 保险关系人信息 
                    Bookmark bm_SaleName;//投保人姓名
                    if (doc.Range.Bookmarks["txt_titleuser"] != null)
                    {
                        bm_SaleName = doc.Range.Bookmarks["txt_titleuser"];
                        //投保人和被保险人不是同一人
                        if (sealDetailModel.SALESDETAIL_ISINSURED == "否")
                        {
                            var model = sealDetailModel.RELATIONPEOPLEs.FirstOrDefault(p => p.RELATIONPEOPLE_TYPE == "投保人");
                            if (model != null)
                            {
                                bm_SaleName.Text = model.RELATIONPEOPLE_NAME;
                            }

                        }
                        else
                        {
                            Bookmark bm_txt_v_SaleIsClient;//同被保险人资料
                            if (doc.Range.Bookmarks["txt_v_SaleIsClient"] != null)
                            {
                                bm_txt_v_SaleIsClient = doc.Range.Bookmarks["txt_v_SaleIsClient"];
                                bm_txt_v_SaleIsClient.Text = "■";
                            }
                            bm_SaleName.Text = customModel.CUSTOM_NAME;
                        }

                    }

                    //Bookmark bm_SaleNumber;//投保单号
                    //if (doc.Range.Bookmarks["txt_SaleNumber"] != null)
                    //{
                    //    bm_SaleNumber = doc.Range.Bookmarks["txt_SaleNumber"];
                    //    bm_SaleNumber.Text = ret_val.Rows[0]["SaleNumber"].ToString();
                    //}
                    Bookmark bm_CuName;//姓名
                    if (doc.Range.Bookmarks["txt_CuName"] != null)
                    {
                        bm_CuName = doc.Range.Bookmarks["txt_CuName"];
                        bm_CuName.Text = customModel.CUSTOM_NAME;
                    }
                    if (customModel.CUSTOM_SEX == "男")
                    {
                        Bookmark bm_v_CuGender_Male;
                        if (doc.Range.Bookmarks["v_CuGender_Male"] != null)
                        {
                            bm_v_CuGender_Male = doc.Range.Bookmarks["v_CuGender_Male"];
                            bm_v_CuGender_Male.Text = "■";
                        }
                    }
                    else
                    {
                        Bookmark bm_v_CuGender_Female;
                        if (doc.Range.Bookmarks["v_CuGender_Female"] != null)
                        {
                            bm_v_CuGender_Female = doc.Range.Bookmarks["v_CuGender_Female"];
                            bm_v_CuGender_Female.Text = "■";
                        }
                    }


                    Bookmark bm_v_CuBirthday_Year;
                    //出生日期改为 本地时间
                    customModel.CUSTOM_BIRTHDATE = customModel.CUSTOM_BIRTHDATE.Value.ToLocalTime();
                    if (doc.Range.Bookmarks["txt_v_CuBirthday_Year"] != null)
                    {
                        bm_v_CuBirthday_Year = doc.Range.Bookmarks["txt_v_CuBirthday_Year"];
                        bm_v_CuBirthday_Year.Text = customModel.CUSTOM_BIRTHDATE.Value.Year.ToString();
                    }
                    Bookmark bm_v_CuBirthday_Month;
                    if (doc.Range.Bookmarks["txt_v_CuBirthday_Month"] != null)
                    {
                        bm_v_CuBirthday_Month = doc.Range.Bookmarks["txt_v_CuBirthday_Month"];
                        bm_v_CuBirthday_Month.Text = customModel.CUSTOM_BIRTHDATE.Value.Month.ToString();
                    }
                    Bookmark bm_v_CuBirthday_Day;
                    if (doc.Range.Bookmarks["txt_v_CuBirthday_Day"] != null)
                    {
                        bm_v_CuBirthday_Day = doc.Range.Bookmarks["txt_v_CuBirthday_Day"];
                        bm_v_CuBirthday_Day.Text = customModel.CUSTOM_BIRTHDATE.Value.Day.ToString();
                    }
                    Bookmark bm_CuYears;//年龄
                    if (doc.Range.Bookmarks["txt_CuYears"] != null)
                    {
                        bm_CuYears = doc.Range.Bookmarks["txt_CuYears"];
                        bm_CuYears.Text = sealDetailModel.SALESDETAIL_OLD ?? "";
                    }

                    Bookmark bm_CuPassportNumber;//护照号码
                    if (doc.Range.Bookmarks["txt_CuPassportNumber"] != null)
                    {
                        bm_CuPassportNumber = doc.Range.Bookmarks["txt_CuPassportNumber"];
                        bm_CuPassportNumber.Text = customModel.CUSTOM_PASSPORT ?? "";
                    }
                    //CuNationality 国籍/户籍所在地
                    Bookmark bm_CuNationality;
                    if (doc.Range.Bookmarks["txt_CuNationality"] != null)
                    {
                        bm_CuNationality = doc.Range.Bookmarks["txt_CuNationality"];
                        bm_CuNationality.Text = customModel.CUSTOM_COUNTRY ?? "";
                    }
                    Bookmark bm_CuHomeaddr;//境内居住地址
                    if (doc.Range.Bookmarks["txt_CuHomeaddr"] != null)
                    {
                        bm_CuHomeaddr = doc.Range.Bookmarks["txt_CuHomeaddr"];
                        bm_CuHomeaddr.Text = customModel.CUSTOM_ADDRESS ?? "";
                    }
                    Bookmark bm_txt_CuHomezip;//境内邮编
                    if (doc.Range.Bookmarks["txt_CuHomezip"] != null)
                    {
                        bm_txt_CuHomezip = doc.Range.Bookmarks["txt_CuHomezip"];
                        bm_txt_CuHomezip.Text = customModel.CUSTOM_ZIP ?? "";
                    }

                    Bookmark bm_txt_OverseasAddress;//境外通讯地址
                    if (doc.Range.Bookmarks["txt_OverseasAddress"] != null)
                    {
                        bm_txt_OverseasAddress = doc.Range.Bookmarks["txt_OverseasAddress"];
                        bm_txt_OverseasAddress.Text = customModel.CUSTOM_OADDRESS ?? "";
                    }

                    Bookmark bm_txt_OverseasZip;//境外邮编
                    if (doc.Range.Bookmarks["txt_OverseasZip"] != null)
                    {
                        bm_txt_OverseasZip = doc.Range.Bookmarks["txt_OverseasZip"];
                        bm_txt_OverseasZip.Text = customModel.CUSTOM_OZIP ?? "";
                    }

                    Bookmark bm_txt_CuMobile;//手机
                    if (doc.Range.Bookmarks["txt_CuMobile"] != null)
                    {
                        bm_txt_CuMobile = doc.Range.Bookmarks["txt_CuMobile"];
                        bm_txt_CuMobile.Text = customModel.CUSTOM_TEL ?? "";
                    }
                    Bookmark bm_txt_CuemailAddr;//邮件地址 境外通讯地址***************
                    if (doc.Range.Bookmarks["txt_CuemailAddr"] != null)
                    {
                        bm_txt_CuemailAddr = doc.Range.Bookmarks["txt_CuemailAddr"];
                        bm_txt_CuemailAddr.Text = customModel.CUSTOM_EMAIL ?? "";
                    }

                    Bookmark bm_txt_OfferNumber;//入学通知书或教育机构的申请确认编码
                    if (doc.Range.Bookmarks["txt_OfferNumber"] != null)
                    {
                        bm_txt_OfferNumber = doc.Range.Bookmarks["txt_OfferNumber"];
                        bm_txt_OfferNumber.Text = customModel.CUSTOM_CODE ?? "";
                    }

                    Bookmark bm_txt_CuEducationName;//教育机构名称
                    if (doc.Range.Bookmarks["txt_CuEducationName"] != null)
                    {
                        bm_txt_CuEducationName = doc.Range.Bookmarks["txt_CuEducationName"];
                        bm_txt_CuEducationName.Text = customModel.CUSTOM_SCHOOL ?? "";
                    }
                    Bookmark bm_txt_CuSACountry;//留学国家
                    if (doc.Range.Bookmarks["txt_CuSACountry"] != null)
                    {
                        bm_txt_CuSACountry = doc.Range.Bookmarks["txt_CuSACountry"];
                        bm_txt_CuSACountry.Text = customModel.CUSTOM_SCOUNTRY ?? "";
                    }

                    #endregion

                    /******************************投保人*********************************************/


                    if (sealDetailModel.SALESDETAIL_ISINSURED == "是")//是同被保险人资料
                    {
                        #region 是同被保险人资料
                        Bookmark bm_txt_t2SaleName;//姓名
                        if (doc.Range.Bookmarks["txt_t2SaleName"] != null)
                        {
                            bm_txt_t2SaleName = doc.Range.Bookmarks["txt_t2SaleName"];
                            bm_txt_t2SaleName.Text = customModel.CUSTOM_NAME;
                        }
                        if (customModel.CUSTOM_SEX == "男")
                        {
                            Bookmark bm_txt_v_SaleGender_Male;//
                            if (doc.Range.Bookmarks["txt_v_SaleGender_Male"] != null)
                            {
                                bm_txt_v_SaleGender_Male = doc.Range.Bookmarks["txt_v_SaleGender_Male"];
                                bm_txt_v_SaleGender_Male.Text = "■";
                            }
                        }
                        else
                        {
                            Bookmark bm_txt_v_SaleGender_Female;//
                            if (doc.Range.Bookmarks["txt_v_SaleGender_Female"] != null)
                            {
                                bm_txt_v_SaleGender_Female = doc.Range.Bookmarks["txt_v_SaleGender_Female"];
                                bm_txt_v_SaleGender_Female.Text = "■";
                            }

                        }


                        //证件类型 护照                      
                        Bookmark bm_txt_v_SaleCerttype_Passport;//
                        if (doc.Range.Bookmarks["txt_v_SaleCerttype_Passport"] != null)
                        {
                            bm_txt_v_SaleCerttype_Passport = doc.Range.Bookmarks["txt_v_SaleCerttype_Passport"];
                            bm_txt_v_SaleCerttype_Passport.Text = "■";
                        }


                        //证件号码
                        Bookmark bm_txt_SaleCertno;//护照号码
                        if (doc.Range.Bookmarks["txt_SaleCertno"] != null)
                        {
                            bm_txt_SaleCertno = doc.Range.Bookmarks["txt_SaleCertno"];
                            bm_txt_SaleCertno.Text = customModel.CUSTOM_PASSPORT;
                        }

                        //国籍/户籍所在地salenationality
                        Bookmark bm_txt_Salenationality;//国籍/户籍所在地
                        if (doc.Range.Bookmarks["txt_Salenationality"] != null)
                        {
                            bm_txt_Salenationality = doc.Range.Bookmarks["txt_Salenationality"];
                            bm_txt_Salenationality.Text = customModel.CUSTOM_COUNTRY;
                        }
                        //国籍/户籍所在地
                        Bookmark bm_txt_SaleHomeaddr;//通讯地址
                        if (doc.Range.Bookmarks["txt_SaleHomeaddr"] != null)
                        {
                            bm_txt_SaleHomeaddr = doc.Range.Bookmarks["txt_SaleHomeaddr"];
                            bm_txt_SaleHomeaddr.Text = customModel.CUSTOM_ADDRESS;
                        }
                        //
                        Bookmark bm_txt_SaleHomezip;//境内邮编
                        if (doc.Range.Bookmarks["txt_SaleHomezip"] != null)
                        {
                            bm_txt_SaleHomezip = doc.Range.Bookmarks["txt_SaleHomezip"];
                            bm_txt_SaleHomezip.Text = customModel.CUSTOM_ZIP;
                        }

                        Bookmark bm_txt_SaleMobile;//手机
                        if (doc.Range.Bookmarks["txt_SaleMobile"] != null)
                        {
                            bm_txt_SaleMobile = doc.Range.Bookmarks["txt_SaleMobile"];
                            bm_txt_SaleMobile.Text = customModel.CUSTOM_TEL;
                        }

                        Bookmark bm_txt_SaleEmailaddr;//
                        if (doc.Range.Bookmarks["txt_SaleEmailaddr"] != null)
                        {
                            bm_txt_SaleEmailaddr = doc.Range.Bookmarks["txt_SaleEmailaddr"];
                            bm_txt_SaleEmailaddr.Text = customModel.CUSTOM_EMAIL;
                        }
                        #endregion
                    }
                    else
                    {
                        var relattionPeole = sealDetailModel.RELATIONPEOPLEs.FirstOrDefault(p => p.RELATIONPEOPLE_TYPE == "投保人");
                        #region 不是同被保险人资料
                        if (relattionPeole != null)
                        {


                            Bookmark bm_txt_t2SaleName;//姓名
                            if (doc.Range.Bookmarks["txt_t2SaleName"] != null)
                            {
                                bm_txt_t2SaleName = doc.Range.Bookmarks["txt_t2SaleName"];
                                bm_txt_t2SaleName.Text = relattionPeole.RELATIONPEOPLE_NAME;
                            }
                            if (relattionPeole.RELATIONPEOPLE_SEX == "男")
                            {
                                Bookmark bm_txt_v_SaleGender_Male;//
                                if (doc.Range.Bookmarks["txt_v_SaleGender_Male"] != null)
                                {
                                    bm_txt_v_SaleGender_Male = doc.Range.Bookmarks["txt_v_SaleGender_Male"];
                                    bm_txt_v_SaleGender_Male.Text = "■";
                                }
                            }
                            else
                            {
                                Bookmark bm_txt_v_SaleGender_Female;//
                                if (doc.Range.Bookmarks["txt_v_SaleGender_Female"] != null)
                                {
                                    bm_txt_v_SaleGender_Female = doc.Range.Bookmarks["txt_v_SaleGender_Female"];
                                    bm_txt_v_SaleGender_Female.Text = "■";
                                }
                            }
                            Bookmark bm_txt_v_StoCRelation_Parent;//Relation with the Insured 与被保险人关系
                            Bookmark bm_txt_v_StoCRelation_Child;//
                            Bookmark bm_txt_v_StoCRelation_Spouse;//配偶
                            Bookmark bm_txt_v_StoCRelation_Others;//
                            switch (relattionPeole.RELATIONPEOPLE_RELATION)
                            {
                                case "父母":
                                    if (doc.Range.Bookmarks["txt_v_StoCRelation_Parent"] != null)
                                    {

                                        bm_txt_v_StoCRelation_Parent = doc.Range.Bookmarks["txt_v_StoCRelation_Parent"];
                                        bm_txt_v_StoCRelation_Parent.Text = "■";
                                    }
                                    break;
                                case "子女":
                                    if (doc.Range.Bookmarks["txt_v_StoCRelation_Child"] != null)
                                    {
                                        bm_txt_v_StoCRelation_Child = doc.Range.Bookmarks["txt_v_StoCRelation_Child"];
                                        bm_txt_v_StoCRelation_Child.Text = "■";
                                    }
                                    break;
                                case "配偶":
                                    if (doc.Range.Bookmarks["txt_v_StoCRelation_Spouse"] != null)
                                    {
                                        bm_txt_v_StoCRelation_Spouse = doc.Range.Bookmarks["txt_v_StoCRelation_Spouse"];
                                        bm_txt_v_StoCRelation_Spouse.Text = "■";
                                    }
                                    break;
                                case "其他":
                                    if (doc.Range.Bookmarks["txt_v_StoCRelation_Others"] != null)
                                    {
                                        bm_txt_v_StoCRelation_Others = doc.Range.Bookmarks["txt_v_StoCRelation_Others"];
                                        bm_txt_v_StoCRelation_Others.Text = "■";
                                    }
                                    break;
                            }
                            //证件类型
                            Bookmark bm_txt_v_SaleCerttype_IDCard;//
                                                                  //证件类型
                            Bookmark bm_txt_v_SaleCerttype_Passport;//

                            //证件类型
                            Bookmark bm_txt_v_SaleCerttype_Others;//
                            switch (relattionPeole.RELATIONPEOPLE_STYLE)
                            {
                                case "身份证":
                                    if (doc.Range.Bookmarks["txt_v_SaleCerttype_IDCard"] != null)
                                    {
                                        bm_txt_v_SaleCerttype_IDCard = doc.Range.Bookmarks["txt_v_SaleCerttype_IDCard"];
                                        bm_txt_v_SaleCerttype_IDCard.Text = "■";
                                    }
                                    break;
                                case "护照":
                                    if (doc.Range.Bookmarks["txt_v_SaleCerttype_Passport"] != null)
                                    {
                                        bm_txt_v_SaleCerttype_Passport = doc.Range.Bookmarks["txt_v_SaleCerttype_Passport"];
                                        bm_txt_v_SaleCerttype_Passport.Text = "■";
                                    }
                                    break;
                                case "其他":
                                    if (doc.Range.Bookmarks["txt_v_SaleCerttype_Others"] != null)
                                    {
                                        bm_txt_v_SaleCerttype_Others = doc.Range.Bookmarks["txt_v_SaleCerttype_Others"];
                                        bm_txt_v_SaleCerttype_Others.Text = "■";
                                    }
                                    break;
                            }
                            //证件号码
                            Bookmark bm_txt_SaleCertno;//
                            if (doc.Range.Bookmarks["txt_SaleCertno"] != null)
                            {
                                bm_txt_SaleCertno = doc.Range.Bookmarks["txt_SaleCertno"];
                                bm_txt_SaleCertno.Text = relattionPeole.RELATIONPEOPLE_NUMBER;
                            }
                            //国籍/户籍所在地salenationality
                            Bookmark bm_txt_Salenationality;//国籍/户籍所在地
                            if (doc.Range.Bookmarks["txt_Salenationality"] != null)
                            {
                                bm_txt_Salenationality = doc.Range.Bookmarks["txt_Salenationality"];
                                bm_txt_Salenationality.Text = relattionPeole.RELATIONPEOPLE_COUNTRY;
                            }
                            //国籍/户籍所在地
                            Bookmark bm_txt_SaleHomeaddr;//国籍/户籍所在地
                            if (doc.Range.Bookmarks["txt_SaleHomeaddr"] != null)
                            {
                                bm_txt_SaleHomeaddr = doc.Range.Bookmarks["txt_SaleHomeaddr"];
                                bm_txt_SaleHomeaddr.Text = relattionPeole.RELATIONPEOPLE_ADDRESS;
                            }
                            //
                            Bookmark bm_txt_SaleHomezip;//
                            if (doc.Range.Bookmarks["txt_SaleHomezip"] != null)
                            {
                                bm_txt_SaleHomezip = doc.Range.Bookmarks["txt_SaleHomezip"];
                                bm_txt_SaleHomezip.Text = relattionPeole.RELATIONPEOPLE_ZIP;
                            }
                            //
                            Bookmark bm_txt_Salehometel;//
                            if (doc.Range.Bookmarks["txt_Salehometel"] != null)
                            {
                                bm_txt_Salehometel = doc.Range.Bookmarks["txt_Salehometel"];
                                bm_txt_Salehometel.Text = relattionPeole.RELATIONPEOPLE_TELPHONE;
                            }
                            //
                            Bookmark bm_txt_SaleMobile;//
                            if (doc.Range.Bookmarks["txt_SaleMobile"] != null)
                            {
                                bm_txt_SaleMobile = doc.Range.Bookmarks["txt_SaleMobile"];
                                bm_txt_SaleMobile.Text = relattionPeole.RELATIONPEOPLE_TEL;
                            }

                            Bookmark bm_txt_SaleEmailaddr;//
                            if (doc.Range.Bookmarks["txt_SaleEmailaddr"] != null)
                            {
                                bm_txt_SaleEmailaddr = doc.Range.Bookmarks["txt_SaleEmailaddr"];
                                bm_txt_SaleEmailaddr.Text = relattionPeole.RELATIONPEOPLE_EMAIL;
                            }
                        }
                        #endregion
                    }



                    //同投保人资料 关键联系人
                    Bookmark bm_txt_v_ContactsIsSale;//
                    if (sealDetailModel.SALESDETAIL_ISHOLDER == "是")
                    {
                        bm_txt_v_ContactsIsSale = doc.Range.Bookmarks["txt_v_ContactsIsSale"];
                        bm_txt_v_ContactsIsSale.Text = "■";
                    }
                    // /******************************关键联络人*********************************************/
                    var relattionPeole2 = sealDetailModel.RELATIONPEOPLEs.FirstOrDefault(p => p.RELATIONPEOPLE_TYPE == "投保人");
                    if (sealDetailModel.SALESDETAIL_ISHOLDER == "是" && relattionPeole2 != null)//关键联络人同投保人为同一人
                    {
                        #region 关键联络人同投保人为同一人
                        Bookmark bm_txt_ContactsName;//姓名
                        if (doc.Range.Bookmarks["txt_ContactsName"] != null)
                        {
                            bm_txt_ContactsName = doc.Range.Bookmarks["txt_ContactsName"];
                            bm_txt_ContactsName.Text = relattionPeole2.RELATIONPEOPLE_NAME;
                        }
                        if (relattionPeole2.RELATIONPEOPLE_SEX == "男")
                        {
                            Bookmark bm_txt_v_ContactsGender_Male;//性别
                            if (doc.Range.Bookmarks["txt_v_ContactsGender_Male"] != null)
                            {
                                bm_txt_v_ContactsGender_Male = doc.Range.Bookmarks["txt_v_ContactsGender_Male"];
                                bm_txt_v_ContactsGender_Male.Text = "■";
                            }
                        }
                        else
                        {
                            Bookmark bm_txt_v_ContactsGender_Female;//性别
                            if (doc.Range.Bookmarks["txt_v_ContactsGender_Female"] != null)
                            {
                                bm_txt_v_ContactsGender_Female = doc.Range.Bookmarks["txt_v_ContactsGender_Female"];
                                bm_txt_v_ContactsGender_Female.Text = "■";
                            }
                        }

                        //与被保险人关系
                        //
                        Bookmark bm_txt_v_ContactsRelation_Parent;//
                        Bookmark bm_txt_v_ContactsRela;//
                                                       //配偶
                        Bookmark bm_txt_v_ContactsRelation_Spouse;//
                        switch (relattionPeole2.RELATIONPEOPLE_RELATION)
                        {
                            case "父母":
                                if (doc.Range.Bookmarks["txt_v_ContactsRelation_Parent"] != null)
                                {
                                    bm_txt_v_ContactsRelation_Parent = doc.Range.Bookmarks["txt_v_ContactsRelation_Parent"];
                                    bm_txt_v_ContactsRelation_Parent.Text = "■";
                                }
                                break;
                            case "子女":
                                if (doc.Range.Bookmarks["txt_v_ContactsRela"] != null)
                                {
                                    bm_txt_v_ContactsRela = doc.Range.Bookmarks["txt_v_ContactsRela"];
                                    bm_txt_v_ContactsRela.Text = "■";
                                }
                                break;
                            case "配偶":
                                if (doc.Range.Bookmarks["txt_v_ContactsRelation_Spouse"] != null)
                                {
                                    bm_txt_v_ContactsRelation_Spouse = doc.Range.Bookmarks["txt_v_ContactsRelation_Spouse"];
                                    bm_txt_v_ContactsRelation_Spouse.Text = "■";
                                }
                                break;
                            case "其他":
                                Bookmark bm_txt_v_ContactsRelation_Others;//
                                if (doc.Range.Bookmarks["txt_v_ContactsRelation_Others"] != null)
                                {
                                    bm_txt_v_ContactsRelation_Others = doc.Range.Bookmarks["txt_v_ContactsRelation_Others"];
                                    bm_txt_v_ContactsRelation_Others.Text = "■";
                                }
                                break;
                        }
                        //                     

                        //                       
                        //证件类型
                        Bookmark bm_txt_v_ContactsType_IDCard;//

                        Bookmark bm_txt_v_ContactsType_Passport;//

                        Bookmark bm_txt_v_ContactsType_Others;//

                        switch (relattionPeole2.RELATIONPEOPLE_STYLE)
                        {
                            case "身份证":
                                if (doc.Range.Bookmarks["txt_v_ContactsType_IDCard"] != null)
                                {
                                    bm_txt_v_ContactsType_IDCard = doc.Range.Bookmarks["txt_v_ContactsType_IDCard"];
                                    bm_txt_v_ContactsType_IDCard.Text = "■";
                                }
                                break;
                            case "护照":
                                if (doc.Range.Bookmarks["txt_v_ContactsType_Passport"] != null)
                                {
                                    bm_txt_v_ContactsType_Passport = doc.Range.Bookmarks["txt_v_ContactsType_Passport"];
                                    bm_txt_v_ContactsType_Passport.Text = "■";
                                }
                                break;
                            case "其他":
                                if (doc.Range.Bookmarks["txt_v_ContactsType_Others"] != null)
                                {
                                    bm_txt_v_ContactsType_Others = doc.Range.Bookmarks["txt_v_ContactsType_Others"];
                                    bm_txt_v_ContactsType_Others.Text = "■";
                                }
                                break;
                        }
                        Bookmark bm_txt_ContactsCertno;//证件号码
                        if (doc.Range.Bookmarks["txt_ContactsCertno"] != null)
                        {
                            bm_txt_ContactsCertno = doc.Range.Bookmarks["txt_ContactsCertno"];
                            bm_txt_ContactsCertno.Text = relattionPeole2.RELATIONPEOPLE_NUMBER;
                        }
                        Bookmark bm_txt_ContactsHomeaddr;//通讯地址
                        if (doc.Range.Bookmarks["txt_ContactsHomeaddr"] != null)
                        {
                            bm_txt_ContactsHomeaddr = doc.Range.Bookmarks["txt_ContactsHomeaddr"];
                            bm_txt_ContactsHomeaddr.Text = relattionPeole2.RELATIONPEOPLE_ADDRESS;
                        }

                        Bookmark bm_txt_ContactsHomezip;//境内邮编 
                        if (doc.Range.Bookmarks["txt_ContactsHomezip"] != null)
                        {
                            bm_txt_ContactsHomezip = doc.Range.Bookmarks["txt_ContactsHomezip"];
                            bm_txt_ContactsHomezip.Text = relattionPeole2.RELATIONPEOPLE_ZIP;
                        }
                        Bookmark bm_txt_Contactshometel;//
                        if (doc.Range.Bookmarks["txt_Contactshometel"] != null)
                        {
                            bm_txt_Contactshometel = doc.Range.Bookmarks["txt_Contactshometel"];
                            bm_txt_Contactshometel.Text = relattionPeole2.RELATIONPEOPLE_TELPHONE;
                        }
                        Bookmark bm_txt_ContactsMobile;//
                        if (doc.Range.Bookmarks["txt_ContactsMobile"] != null)
                        {
                            bm_txt_ContactsMobile = doc.Range.Bookmarks["txt_ContactsMobile"];
                            bm_txt_ContactsMobile.Text = relattionPeole2.RELATIONPEOPLE_TEL;
                        }
                        Bookmark bm_txt_ContactsEmailaddr;//电子邮箱
                        if (doc.Range.Bookmarks["txt_ContactsEmailaddr"] != null)
                        {
                            bm_txt_ContactsEmailaddr = doc.Range.Bookmarks["txt_ContactsEmailaddr"];
                            bm_txt_ContactsEmailaddr.Text = relattionPeole2.RELATIONPEOPLE_EMAIL;
                        }

                        #endregion
                    }
                    else
                    {
                        relattionPeole2 = sealDetailModel.RELATIONPEOPLEs.FirstOrDefault(p => p.RELATIONPEOPLE_TYPE == "关键联络人");
                        #region 关键联络人不与投保人为同一人 ContactsIsSale
                        Bookmark bm_txt_ContactsName;//姓名
                        if (doc.Range.Bookmarks["txt_ContactsName"] != null)
                        {
                            bm_txt_ContactsName = doc.Range.Bookmarks["txt_ContactsName"];
                            bm_txt_ContactsName.Text = relattionPeole2.RELATIONPEOPLE_NAME;
                        }
                        if (relattionPeole2.RELATIONPEOPLE_SEX == "男")
                        {
                            Bookmark bm_txt_v_ContactsGender_Male;//性别
                            if (doc.Range.Bookmarks["txt_v_ContactsGender_Male"] != null)
                            {
                                bm_txt_v_ContactsGender_Male = doc.Range.Bookmarks["txt_v_ContactsGender_Male"];
                                bm_txt_v_ContactsGender_Male.Text = "■";
                            }
                        }
                        else
                        {
                            Bookmark bm_txt_v_ContactsGender_Female;//性别
                            if (doc.Range.Bookmarks["txt_v_ContactsGender_Female"] != null)
                            {
                                bm_txt_v_ContactsGender_Female = doc.Range.Bookmarks["txt_v_ContactsGender_Female"];
                                bm_txt_v_ContactsGender_Female.Text = "■";
                            }
                        }

                        //与被保险人关系
                        //
                        Bookmark bm_txt_v_ContactsRelation_Parent;//
                        Bookmark bm_txt_v_ContactsRela;//
                                                       //配偶
                        Bookmark bm_txt_v_ContactsRelation_Spouse;//
                        switch (relattionPeole2.RELATIONPEOPLE_RELATION)
                        {
                            case "父母":
                                if (doc.Range.Bookmarks["txt_v_ContactsRelation_Parent"] != null)
                                {
                                    bm_txt_v_ContactsRelation_Parent = doc.Range.Bookmarks["txt_v_ContactsRelation_Parent"];
                                    bm_txt_v_ContactsRelation_Parent.Text = "■";
                                }
                                break;
                            case "子女":
                                if (doc.Range.Bookmarks["txt_v_ContactsRela"] != null)
                                {
                                    bm_txt_v_ContactsRela = doc.Range.Bookmarks["txt_v_ContactsRela"];
                                    bm_txt_v_ContactsRela.Text = "■";
                                }
                                break;
                            case "配偶":
                                if (doc.Range.Bookmarks["txt_v_ContactsRelation_Spouse"] != null)
                                {
                                    bm_txt_v_ContactsRelation_Spouse = doc.Range.Bookmarks["txt_v_ContactsRelation_Spouse"];
                                    bm_txt_v_ContactsRelation_Spouse.Text = "■";
                                }
                                break;
                            case "其他":
                                Bookmark bm_txt_v_ContactsRelation_Others;//
                                if (doc.Range.Bookmarks["txt_v_ContactsRelation_Others"] != null)
                                {
                                    bm_txt_v_ContactsRelation_Others = doc.Range.Bookmarks["txt_v_ContactsRelation_Others"];
                                    bm_txt_v_ContactsRelation_Others.Text = "■";
                                }
                                break;
                        }
                        //                     

                        //                       
                        //证件类型
                        Bookmark bm_txt_v_ContactsType_IDCard;//

                        Bookmark bm_txt_v_ContactsType_Passport;//

                        Bookmark bm_txt_v_ContactsType_Others;//

                        switch (relattionPeole2.RELATIONPEOPLE_STYLE)
                        {
                            case "身份证":
                                if (doc.Range.Bookmarks["txt_v_ContactsType_IDCard"] != null)
                                {
                                    bm_txt_v_ContactsType_IDCard = doc.Range.Bookmarks["txt_v_ContactsType_IDCard"];
                                    bm_txt_v_ContactsType_IDCard.Text = "■";
                                }
                                break;
                            case "护照":
                                if (doc.Range.Bookmarks["txt_v_ContactsType_Passport"] != null)
                                {
                                    bm_txt_v_ContactsType_Passport = doc.Range.Bookmarks["txt_v_ContactsType_Passport"];
                                    bm_txt_v_ContactsType_Passport.Text = "■";
                                }
                                break;
                            case "其他":
                                if (doc.Range.Bookmarks["txt_v_ContactsType_Others"] != null)
                                {
                                    bm_txt_v_ContactsType_Others = doc.Range.Bookmarks["txt_v_ContactsType_Others"];
                                    bm_txt_v_ContactsType_Others.Text = "■";
                                }
                                break;
                        }
                        Bookmark bm_txt_ContactsCertno;//证件号码
                        if (doc.Range.Bookmarks["txt_ContactsCertno"] != null)
                        {
                            bm_txt_ContactsCertno = doc.Range.Bookmarks["txt_ContactsCertno"];
                            bm_txt_ContactsCertno.Text = relattionPeole2.RELATIONPEOPLE_NUMBER;
                        }
                        Bookmark bm_txt_ContactsHomeaddr;//通讯地址
                        if (doc.Range.Bookmarks["txt_ContactsHomeaddr"] != null)
                        {
                            bm_txt_ContactsHomeaddr = doc.Range.Bookmarks["txt_ContactsHomeaddr"];
                            bm_txt_ContactsHomeaddr.Text = relattionPeole2.RELATIONPEOPLE_ADDRESS;
                        }

                        Bookmark bm_txt_ContactsHomezip;//境内邮编 
                        if (doc.Range.Bookmarks["txt_ContactsHomezip"] != null)
                        {
                            bm_txt_ContactsHomezip = doc.Range.Bookmarks["txt_ContactsHomezip"];
                            bm_txt_ContactsHomezip.Text = relattionPeole2.RELATIONPEOPLE_ZIP ?? "";
                        }
                        Bookmark bm_txt_Contactshometel;//
                        if (doc.Range.Bookmarks["txt_Contactshometel"] != null)
                        {
                            bm_txt_Contactshometel = doc.Range.Bookmarks["txt_Contactshometel"];
                            bm_txt_Contactshometel.Text = relattionPeole2.RELATIONPEOPLE_TELPHONE ?? "";
                        }
                        Bookmark bm_txt_ContactsMobile;//
                        if (doc.Range.Bookmarks["txt_ContactsMobile"] != null)
                        {
                            bm_txt_ContactsMobile = doc.Range.Bookmarks["txt_ContactsMobile"];
                            bm_txt_ContactsMobile.Text = relattionPeole2.RELATIONPEOPLE_TEL ?? "";
                        }
                        Bookmark bm_txt_ContactsEmailaddr;//电子邮箱
                        if (doc.Range.Bookmarks["txt_ContactsEmailaddr"] != null)
                        {
                            bm_txt_ContactsEmailaddr = doc.Range.Bookmarks["txt_ContactsEmailaddr"];
                            bm_txt_ContactsEmailaddr.Text = relattionPeole2.RELATIONPEOPLE_EMAIL ?? "";
                        }

                        #endregion
                    }

                    //     /******************************受益人*********************************************/

                    //DataTable td_BeneficiaryList = GetBeneficiaryList();//获取受益人信息
                    var binefiteList = sealDetailModel.FAVOREEs.ToList();
                    if ((binefiteList != null) && (binefiteList.Count > 0))
                    {

                        #region 受益人 0

                        Bookmark bm_txt_BFName;//受益人姓名
                        if (doc.Range.Bookmarks["txt_BFName"] != null)
                        {
                            bm_txt_BFName = doc.Range.Bookmarks["txt_BFName"];
                            bm_txt_BFName.Text = binefiteList[0].FAVOREE_NAME;
                        }
                        if (binefiteList[0].FAVOREE_SEX == "男")
                        {
                            Bookmark bm_txt_v_BFGender_Male;//
                            if (doc.Range.Bookmarks["txt_v_BFGender_Male"] != null)
                            {
                                bm_txt_v_BFGender_Male = doc.Range.Bookmarks["txt_v_BFGender_Male"];
                                bm_txt_v_BFGender_Male.Text = "■";
                            }
                        }
                        else
                        {
                            Bookmark bm_txt_v_BFGender_Female;//
                            if (doc.Range.Bookmarks["txt_v_BFGender_Female"] != null)
                            {
                                bm_txt_v_BFGender_Female = doc.Range.Bookmarks["txt_v_BFGender_Female"];
                                bm_txt_v_BFGender_Female.Text = "■";
                            }
                        }

                        switch (binefiteList[0].FAVOREE_RELATION)
                        {
                            case "父母":
                                Bookmark bm_txt_v_BtoCRelation_Parent;//
                                if (doc.Range.Bookmarks["txt_v_BtoCRelation_Parent"] != null)
                                {
                                    bm_txt_v_BtoCRelation_Parent = doc.Range.Bookmarks["txt_v_BtoCRelation_Parent"];
                                    bm_txt_v_BtoCRelation_Parent.Text = "■";
                                }
                                break;
                            case "子女":
                                Bookmark bm_txt_v_BtoCRelation_Child;//
                                if (doc.Range.Bookmarks["txt_v_BtoCRelation_Child"] != null)
                                {
                                    bm_txt_v_BtoCRelation_Child = doc.Range.Bookmarks["txt_v_BtoCRelation_Child"];
                                    bm_txt_v_BtoCRelation_Child.Text = "■";
                                }
                                break;
                            case "配偶":
                                Bookmark bm_txt_v_BtoCRelation_Spouse;//
                                if (doc.Range.Bookmarks["txt_v_BtoCRelation_Spouse"] != null)
                                {
                                    bm_txt_v_BtoCRelation_Spouse = doc.Range.Bookmarks["txt_v_BtoCRelation_Spouse"];
                                    bm_txt_v_BtoCRelation_Spouse.Text = "■";
                                }
                                break;
                            case "其他":
                                Bookmark bm_txt_v_BtoCRelation_Others;//
                                if (doc.Range.Bookmarks["txt_v_BtoCRelation_Others"] != null)
                                {
                                    bm_txt_v_BtoCRelation_Others = doc.Range.Bookmarks["txt_v_BtoCRelation_Others"];
                                    bm_txt_v_BtoCRelation_Others.Text = "■";
                                }
                                break;
                        }

                        switch (binefiteList[0].FAVOREE_STYLE)
                        {
                            case "身份证":
                                Bookmark bm_txt_v_BFCerttype_IDCard;//
                                if (doc.Range.Bookmarks["txt_v_BFCerttype_IDCard"] != null)
                                {
                                    bm_txt_v_BFCerttype_IDCard = doc.Range.Bookmarks["txt_v_BFCerttype_IDCard"];
                                    bm_txt_v_BFCerttype_IDCard.Text = "■";
                                }
                                break;
                            case "护照":
                                Bookmark bm_txt_v_BFCerttype_Passport;//
                                if (doc.Range.Bookmarks["txt_v_BFCerttype_Passport"] != null)
                                {
                                    bm_txt_v_BFCerttype_Passport = doc.Range.Bookmarks["txt_v_BFCerttype_Passport"];
                                    bm_txt_v_BFCerttype_Passport.Text = "■";
                                }
                                break;
                            case "其他":
                                Bookmark bm_txt_v_BFCerttype_Others;//
                                if (doc.Range.Bookmarks["txt_v_BFCerttype_Others"] != null)
                                {
                                    bm_txt_v_BFCerttype_Others = doc.Range.Bookmarks["txt_v_BFCerttype_Others"];
                                    bm_txt_v_BFCerttype_Others.Text = "■";
                                }
                                break;
                        }



                        Bookmark bm_txt_BFCERTNO;//证件号码
                        if (doc.Range.Bookmarks["txt_BFCERTNO"] != null)
                        {
                            bm_txt_BFCERTNO = doc.Range.Bookmarks["txt_BFCERTNO"];
                            bm_txt_BFCERTNO.Text = binefiteList[0].FAVOREE_NUMBER;
                        }
                        Bookmark bm_txt_BFSHARE;//受益份额
                        if (doc.Range.Bookmarks["txt_BFSHARE"] != null)
                        {
                            bm_txt_BFSHARE = doc.Range.Bookmarks["txt_BFSHARE"];
                            bm_txt_BFSHARE.Text = binefiteList[0].FAVOREE_PORTION;
                        }
                        Bookmark bm_txt_BFHOMEADDR;//通讯地址
                        if (doc.Range.Bookmarks["txt_BFHOMEADDR"] != null)
                        {
                            bm_txt_BFHOMEADDR = doc.Range.Bookmarks["txt_BFHOMEADDR"];
                            bm_txt_BFHOMEADDR.Text = binefiteList[0].FAVOREE_ADDRESS;
                        }
                        Bookmark bm_txt_BFHOMEZIP;//邮编
                        if (doc.Range.Bookmarks["txt_BFHOMEZIP"] != null)
                        {
                            bm_txt_BFHOMEZIP = doc.Range.Bookmarks["txt_BFHOMEZIP"];
                            bm_txt_BFHOMEZIP.Text = binefiteList[0].FAVOREE_ZIP;
                        }
                        Bookmark bm_txt_BFHOMETEL;//电话
                        if (doc.Range.Bookmarks["txt_BFHOMETEL"] != null)
                        {
                            bm_txt_BFHOMETEL = doc.Range.Bookmarks["txt_BFHOMETEL"];
                            bm_txt_BFHOMETEL.Text = binefiteList[0].FAVOREE_TELPHONE;
                        }
                        Bookmark bm_txt_BFMOBILE;//手机
                        if (doc.Range.Bookmarks["txt_BFMOBILE"] != null)
                        {
                            bm_txt_BFMOBILE = doc.Range.Bookmarks["txt_BFMOBILE"];
                            bm_txt_BFMOBILE.Text = binefiteList[0].FAVOREE_TEL;
                        }
                        Bookmark bm_txt_BFEMAILADDR;//电子邮箱
                        if (doc.Range.Bookmarks["txt_BFEMAILADDR"] != null)
                        {
                            bm_txt_BFEMAILADDR = doc.Range.Bookmarks["txt_BFEMAILADDR"];
                            bm_txt_BFEMAILADDR.Text = binefiteList[0].FAVOREE_EMAIL;
                        }

                        #endregion
                        if ((binefiteList != null) && (binefiteList.Count > 1))
                        {
                            #region 受益人1

                            Bookmark bm_txt_BFName1;//受益人姓名
                            if (doc.Range.Bookmarks["txt_BFName1"] != null)
                            {
                                bm_txt_BFName1 = doc.Range.Bookmarks["txt_BFName1"];
                                bm_txt_BFName1.Text = binefiteList[1].FAVOREE_NAME;
                            }
                            if (binefiteList[1].FAVOREE_NAME == "男")
                            {
                                Bookmark bm_txt_v_BFGender_Male1;//
                                if (doc.Range.Bookmarks["txt_v_BFGender_Male1"] != null)
                                {
                                    bm_txt_v_BFGender_Male1 = doc.Range.Bookmarks["txt_v_BFGender_Male1"];
                                    bm_txt_v_BFGender_Male1.Text = "■";
                                }
                            }
                            else
                            {
                                Bookmark bm_txt_v_BFGender_Female1;//
                                if (doc.Range.Bookmarks["txt_v_BFGender_Female1"] != null)
                                {
                                    bm_txt_v_BFGender_Female1 = doc.Range.Bookmarks["txt_v_BFGender_Female1"];
                                    bm_txt_v_BFGender_Female1.Text = "■";
                                }
                            }
                            switch (binefiteList[1].FAVOREE_RELATION)
                            {
                                case "父母":
                                    Bookmark bm_txt_v_BtoCRelation_Parent1;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Parent1"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Parent1 = doc.Range.Bookmarks["txt_v_BtoCRelation_Parent1"];
                                        bm_txt_v_BtoCRelation_Parent1.Text = "■";
                                    }
                                    break;
                                case "子女":
                                    Bookmark bm_txt_v_BtoCRelation_Child1;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Child1"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Child1 = doc.Range.Bookmarks["txt_v_BtoCRelation_Child1"];
                                        bm_txt_v_BtoCRelation_Child1.Text = "■";
                                    }
                                    break;
                                case "配偶":
                                    Bookmark bm_txt_v_BtoCRelation_Spouse1;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Spouse1"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Spouse1 = doc.Range.Bookmarks["txt_v_BtoCRelation_Spouse1"];
                                        bm_txt_v_BtoCRelation_Spouse1.Text = "■";
                                    }
                                    break;
                                case "其他":
                                    Bookmark bm_txt_v_BtoCRelation_Others1;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Others1"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Others1 = doc.Range.Bookmarks["txt_v_BtoCRelation_Others1"];
                                        bm_txt_v_BtoCRelation_Others1.Text = "■";
                                    }
                                    break;
                            }

                            switch (binefiteList[1].FAVOREE_STYLE)
                            {
                                case "身份证":
                                    Bookmark bm_txt_v_BFCerttype_IDCard1;//
                                    if (doc.Range.Bookmarks["txt_v_BFCerttype_IDCard1"] != null)
                                    {
                                        bm_txt_v_BFCerttype_IDCard1 = doc.Range.Bookmarks["txt_v_BFCerttype_IDCard1"];
                                        bm_txt_v_BFCerttype_IDCard1.Text = "■";
                                    }
                                    break;
                                case "护照":
                                    Bookmark bm_txt_v_BFCerttype_Passport1;//
                                    if (doc.Range.Bookmarks["txt_v_BFCerttype_Passport1"] != null)
                                    {
                                        bm_txt_v_BFCerttype_Passport1 = doc.Range.Bookmarks["txt_v_BFCerttype_Passport1"];
                                        bm_txt_v_BFCerttype_Passport1.Text = "■";
                                    }
                                    break;
                                case "其他":
                                    Bookmark bm_txt_v_BFCerttype_Others1;//
                                    if (doc.Range.Bookmarks["txt_v_BFCerttype_Others1"] != null)
                                    {
                                        bm_txt_v_BFCerttype_Others1 = doc.Range.Bookmarks["txt_v_BFCerttype_Others1"];
                                        bm_txt_v_BFCerttype_Others1.Text = "■";
                                    }
                                    break;
                            }
                            Bookmark bm_txt_BFCERTNO1;//证件号码
                            if (doc.Range.Bookmarks["txt_BFCERTNO1"] != null)
                            {
                                bm_txt_BFCERTNO1 = doc.Range.Bookmarks["txt_BFCERTNO1"];
                                bm_txt_BFCERTNO1.Text = binefiteList[1].FAVOREE_NUMBER;
                            }
                            Bookmark bm_txt_BFSHARE1;//受益份额
                            if (doc.Range.Bookmarks["txt_BFSHARE1"] != null)
                            {
                                bm_txt_BFSHARE1 = doc.Range.Bookmarks["txt_BFSHARE1"];
                                bm_txt_BFSHARE1.Text = binefiteList[1].FAVOREE_PORTION;
                            }
                            Bookmark bm_txt_BFHOMEADDR1;//通讯地址
                            if (doc.Range.Bookmarks["txt_BFHOMEADDR1"] != null)
                            {
                                bm_txt_BFHOMEADDR1 = doc.Range.Bookmarks["txt_BFHOMEADDR1"];
                                bm_txt_BFHOMEADDR1.Text = binefiteList[1].FAVOREE_ADDRESS;
                            }
                            Bookmark bm_txt_BFHOMEZIP1;//邮编
                            if (doc.Range.Bookmarks["txt_BFHOMEZIP1"] != null)
                            {
                                bm_txt_BFHOMEZIP1 = doc.Range.Bookmarks["txt_BFHOMEZIP1"];
                                bm_txt_BFHOMEZIP1.Text = binefiteList[1].FAVOREE_ZIP;
                            }
                            Bookmark bm_txt_BFHOMETEL1;//电话
                            if (doc.Range.Bookmarks["txt_BFHOMETEL1"] != null)
                            {
                                bm_txt_BFHOMETEL1 = doc.Range.Bookmarks["txt_BFHOMETEL1"];
                                bm_txt_BFHOMETEL1.Text = binefiteList[1].FAVOREE_TELPHONE;
                            }
                            Bookmark bm_txt_BFMOBILE1;//手机
                            if (doc.Range.Bookmarks["txt_BFMOBILE1"] != null)
                            {
                                bm_txt_BFMOBILE1 = doc.Range.Bookmarks["txt_BFMOBILE1"];
                                bm_txt_BFMOBILE1.Text = binefiteList[1].FAVOREE_TEL;
                            }
                            Bookmark bm_txt_BFEMAILADDR1;//电子邮箱
                            if (doc.Range.Bookmarks["txt_BFEMAILADDR1"] != null)
                            {
                                bm_txt_BFEMAILADDR1 = doc.Range.Bookmarks["txt_BFEMAILADDR1"];
                                bm_txt_BFEMAILADDR1.Text = binefiteList[1].FAVOREE_EMAIL;
                            }
                            #endregion
                        }
                        if ((binefiteList != null) && (binefiteList.Count > 2))
                        {
                            #region 受益人3
                            Bookmark bm_txt_BFName2;//受益人姓名
                            if (doc.Range.Bookmarks["txt_BFName2"] != null)
                            {
                                bm_txt_BFName2 = doc.Range.Bookmarks["txt_BFName2"];
                                bm_txt_BFName2.Text = binefiteList[2].FAVOREE_NAME;
                            }


                            if (binefiteList[2].FAVOREE_NAME == "男")
                            {
                                Bookmark bm_txt_v_BFGender_Male2;//
                                if (doc.Range.Bookmarks["txt_v_BFGender_Male2"] != null)
                                {
                                    bm_txt_v_BFGender_Male2 = doc.Range.Bookmarks["txt_v_BFGender_Male2"];
                                    bm_txt_v_BFGender_Male2.Text = "■";
                                }
                            }
                            else
                            {
                                Bookmark bm_txt_v_BFGender_Female2;//
                                if (doc.Range.Bookmarks["txt_v_BFGender_Female2"] != null)
                                {
                                    bm_txt_v_BFGender_Female2 = doc.Range.Bookmarks["txt_v_BFGender_Female2"];
                                    bm_txt_v_BFGender_Female2.Text = "■";
                                }
                            }


                            switch (binefiteList[2].FAVOREE_RELATION)
                            {
                                case "父母":
                                    Bookmark bm_txt_v_BtoCRelation_Parent2;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Parent2"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Parent2 = doc.Range.Bookmarks["txt_v_BtoCRelation_Parent2"];
                                        bm_txt_v_BtoCRelation_Parent2.Text = "■";
                                    }
                                    break;
                                case "子女":
                                    Bookmark bm_txt_v_BtoCRelation_Child2;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Child2"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Child2 = doc.Range.Bookmarks["txt_v_BtoCRelation_Child2"];
                                        bm_txt_v_BtoCRelation_Child2.Text = "■";
                                    }

                                    break;
                                case "配偶":
                                    Bookmark bm_txt_v_BtoCRelation_Spouse2;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Spouse2"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Spouse2 = doc.Range.Bookmarks["txt_v_BtoCRelation_Spouse2"];
                                        bm_txt_v_BtoCRelation_Spouse2.Text = "■";
                                    }
                                    break;
                                case "其他":
                                    Bookmark bm_txt_v_BtoCRelation_Others2;//
                                    if (doc.Range.Bookmarks["txt_v_BtoCRelation_Others2"] != null)
                                    {
                                        bm_txt_v_BtoCRelation_Others2 = doc.Range.Bookmarks["txt_v_BtoCRelation_Others2"];
                                        bm_txt_v_BtoCRelation_Others2.Text = "■";
                                    }
                                    break;
                            }

                            switch (binefiteList[2].FAVOREE_STYLE)
                            {
                                case "身份证":
                                    Bookmark bm_txt_v_BFCerttype_IDCard2;//
                                    if (doc.Range.Bookmarks["txt_v_BFCerttype_IDCard2"] != null)
                                    {
                                        bm_txt_v_BFCerttype_IDCard2 = doc.Range.Bookmarks["txt_v_BFCerttype_IDCard2"];
                                        bm_txt_v_BFCerttype_IDCard2.Text = "■";
                                    }

                                    break;
                                case "护照":
                                    Bookmark bm_txt_v_BFCerttype_Passport2;//
                                    if (doc.Range.Bookmarks["txt_v_BFCerttype_Passport2"] != null)
                                    {
                                        bm_txt_v_BFCerttype_Passport2 = doc.Range.Bookmarks["txt_v_BFCerttype_Passport2"];
                                        bm_txt_v_BFCerttype_Passport2.Text = "■";
                                    }
                                    break;
                                case "其他":
                                    Bookmark bm_txt_v_BFCerttype_Others2;//
                                    if (doc.Range.Bookmarks["txt_v_BFCerttype_Others2"] != null)
                                    {
                                        bm_txt_v_BFCerttype_Others2 = doc.Range.Bookmarks["txt_v_BFCerttype_Others2"];
                                        bm_txt_v_BFCerttype_Others2.Text = "■";
                                    }
                                    break;
                            }



                            Bookmark bm_txt_BFCERTNO2;//证件号码
                            if (doc.Range.Bookmarks["txt_BFCERTNO2"] != null)
                            {
                                bm_txt_BFCERTNO2 = doc.Range.Bookmarks["txt_BFCERTNO2"];
                                bm_txt_BFCERTNO2.Text = binefiteList[2].FAVOREE_NUMBER;
                            }
                            Bookmark bm_txt_BFSHARE2;//受益份额
                            if (doc.Range.Bookmarks["txt_BFSHARE2"] != null)
                            {
                                bm_txt_BFSHARE2 = doc.Range.Bookmarks["txt_BFSHARE2"];
                                bm_txt_BFSHARE2.Text = binefiteList[2].FAVOREE_PORTION;
                            }
                            Bookmark bm_txt_BFHOMEADDR2;//通讯地址
                            if (doc.Range.Bookmarks["txt_BFHOMEADDR2"] != null)
                            {
                                bm_txt_BFHOMEADDR2 = doc.Range.Bookmarks["txt_BFHOMEADDR2"];
                                bm_txt_BFHOMEADDR2.Text = binefiteList[2].FAVOREE_ADDRESS;
                            }
                            Bookmark bm_txt_BFHOMEZIP2;//邮编
                            if (doc.Range.Bookmarks["txt_BFHOMEZIP2"] != null)
                            {
                                bm_txt_BFHOMEZIP2 = doc.Range.Bookmarks["txt_BFHOMEZIP2"];
                                bm_txt_BFHOMEZIP2.Text = binefiteList[2].FAVOREE_ZIP;
                            }
                            Bookmark bm_txt_BFHOMETEL2;//电话
                            if (doc.Range.Bookmarks["txt_BFHOMETEL2"] != null)
                            {
                                bm_txt_BFHOMETEL2 = doc.Range.Bookmarks["txt_BFHOMETEL2"];
                                bm_txt_BFHOMETEL2.Text = binefiteList[2].FAVOREE_TELPHONE;
                            }
                            Bookmark bm_txt_BFMOBILE2;//手机
                            if (doc.Range.Bookmarks["txt_BFMOBILE2"] != null)
                            {
                                bm_txt_BFMOBILE2 = doc.Range.Bookmarks["txt_BFMOBILE2"];
                                bm_txt_BFMOBILE2.Text = binefiteList[2].FAVOREE_TEL;
                            }
                            Bookmark bm_txt_BFEMAILADDR2;//电子邮箱
                            if (doc.Range.Bookmarks["txt_BFEMAILADDR2"] != null)
                            {
                                bm_txt_BFEMAILADDR2 = doc.Range.Bookmarks["txt_BFEMAILADDR2"];
                                bm_txt_BFEMAILADDR2.Text = binefiteList[2].FAVOREE_EMAIL;
                            }
                            #endregion
                        }


                        /******************************受益人*********************************************/
                    }
                    #region 第二部分  保险保障
                    /******************************************第二部分  保险保障   Insurance Cover**********************************************************/
                    Bookmark bm_txt_v_productname = doc.Range.Bookmarks["txt_QingTongAnXin"];//                   

                    //switch (sealDetailModel.PRODUCE_NAME)
                    //{
                    //    case"铂金安心计划":
                    //        vr

                    //}
                    string productname = sealDetailModel.PRODUCE_NAME;
                    if (productname == "青铜安心计划")
                    {
                        //青铜计划 - 安心   
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_QingTongAnXin"];
                    }
                    if (productname == "青铜畅游计划")
                    {
                        //青铜计划 - 周详  
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_QingTongZhouXiang"];
                    }
                    if (productname == "青铜周详计划")
                    {
                        //青铜计划 - 畅游 
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_QingTongChangYou"];
                    }
                    if (productname == "白银安心计划")
                    {
                        //白银计划 - 安心
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_BaiYinAnXin"];
                    }
                    if (productname == "白银周详计划")
                    {
                        //白银计划 - 周详  
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_BaiYinZhouXiang"];
                    }
                    if (productname == "白银畅游计划")
                    {
                        //白银计划 - 畅游  
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_BaiYinChangYou"];
                    }
                    if (productname == "黄金安心计划")
                    {
                        //黄金计划 - 安心 
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_HuangJinAnXin"];
                    }
                    if (productname == "黄金周详计划")
                    {
                        //黄金计划 - 周详 
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_HuangJinZhouXiang"];
                    }
                    if (productname == "黄金畅游计划")
                    {
                        //黄金计划 - 畅游 
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_HuangJinChangYou"];
                    }


                    //铂金计划 - 畅游 
                    if (productname == "铂金安心计划")
                    { //铂金计划 - 安心   
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_BoJinAnXin"];
                    }
                    if (productname == "铂金周详计划")
                    {
                        //铂金计划 - 周详  
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_BoJinZhouXiang"];
                    }
                    if (productname == "铂金畅游计划")
                    {
                        bm_txt_v_productname = doc.Range.Bookmarks["txt_BoJinChangYou"];
                    }
                    if (bm_txt_v_productname != null)
                    {
                        bm_txt_v_productname.Text = "■";
                    }
                  

                    #endregion
                    /******************************其他特别约定*********************************************/
                    Bookmark bm_txt_EndorseMent;//其他特别约定
                    if (doc.Range.Bookmarks["txt_EndorseMent"] != null)
                    {
                        bm_txt_EndorseMent = doc.Range.Bookmarks["txt_EndorseMent"];
                        string strEndorseMent = string.Empty;
                        if (!string.IsNullOrEmpty(sealDetailModel.SALESDETAIL_CONTENT))
                        {
                            strEndorseMent = sealDetailModel.SALESDETAIL_CONTENT + "。";
                        }
                        bm_txt_EndorseMent.Text = strEndorseMent + sealDetailModel.SALESDETAIL_ADDITION;
                    }
                    //     /******************************第三部分保险期间Insurance Period*********************************************/
                    sealDetailModel.SALESDETAIL_STARTTIME = sealDetailModel.SALESDETAIL_STARTTIME.Value.ToLocalTime();
                    sealDetailModel.SALESDETAIL_ENDTIME = sealDetailModel.SALESDETAIL_ENDTIME.Value.ToLocalTime();
                    Bookmark bm_txt_v_InsuranceStartTime_Year;//第三部分保险期间Insurance Period txt_v_InsuranceStartTime_Year
                    if (doc.Range.Bookmarks["txt_v_InsuranceStartTime_Year"] != null)
                    {
                        bm_txt_v_InsuranceStartTime_Year = doc.Range.Bookmarks["txt_v_InsuranceStartTime_Year"];
                        bm_txt_v_InsuranceStartTime_Year.Text = sealDetailModel.SALESDETAIL_STARTTIME.Value.Year.ToString();
                    }
                    Bookmark bm_txt_v_InsuranceStartTime_Month;//第三部分保险期间Insurance Period txt_v_InsuranceStartTime_Month
                    if (doc.Range.Bookmarks["txt_v_InsuranceStartTime_Month"] != null)
                    {
                        bm_txt_v_InsuranceStartTime_Month = doc.Range.Bookmarks["txt_v_InsuranceStartTime_Month"];
                        bm_txt_v_InsuranceStartTime_Month.Text = sealDetailModel.SALESDETAIL_STARTTIME.Value.Month.ToString();
                    }
                    Bookmark bm_txt_v_InsuranceStartTime_Day;//第三部分保险期间Insurance Period  txt_v_InsuranceStartTime_Day
                    if (doc.Range.Bookmarks["txt_v_InsuranceStartTime_Day"] != null)
                    {
                        bm_txt_v_InsuranceStartTime_Day = doc.Range.Bookmarks["txt_v_InsuranceStartTime_Day"];
                        bm_txt_v_InsuranceStartTime_Day.Text = sealDetailModel.SALESDETAIL_STARTTIME.Value.Day.ToString();
                    }
                    Bookmark bm_txt_v_InsuranceEndTime_Year;//第三部分保险期间Insurance Period
                    if (doc.Range.Bookmarks["txt_v_InsuranceEndTime_Year"] != null)
                    {
                        bm_txt_v_InsuranceEndTime_Year = doc.Range.Bookmarks["txt_v_InsuranceEndTime_Year"];
                        bm_txt_v_InsuranceEndTime_Year.Text = sealDetailModel.SALESDETAIL_ENDTIME.Value.Year.ToString();
                    }
                    Bookmark bm_txt_v_InsuranceEndTime_Month;//第三部分保险期间Insurance Period
                    if (doc.Range.Bookmarks["txt_v_InsuranceEndTime_Month"] != null)
                    {
                        bm_txt_v_InsuranceEndTime_Month = doc.Range.Bookmarks["txt_v_InsuranceEndTime_Month"];
                        bm_txt_v_InsuranceEndTime_Month.Text = sealDetailModel.SALESDETAIL_ENDTIME.Value.Month.ToString();
                    }
                    Bookmark bm_txt_v_InsuranceEndTime_Day;//第三部分保险期间Insurance Period
                    if (doc.Range.Bookmarks["txt_v_InsuranceEndTime_Day"] != null)
                    {
                        bm_txt_v_InsuranceEndTime_Day = doc.Range.Bookmarks["txt_v_InsuranceEndTime_Day"];
                        bm_txt_v_InsuranceEndTime_Day.Text = sealDetailModel.SALESDETAIL_ENDTIME.Value.Day.ToString();
                    }
                    //     /******************************第四部分 保险费Premium（人民币RMB）*********************************************/
                    Bookmark bm_txt_PremiumAmount;//*第四部分 保险费Premium（人民币RMB）
                    var money = sealDetailModel.SALESDETAIL_MONEY ?? 0;
                    var addmoney = sealDetailModel.SALESDETAIL_ADDMONEY ?? 0;
                    sealDetailModel.SALESDETAIL_ALLMONEY = money + addmoney;
                    if (doc.Range.Bookmarks["txt_PremiumAmount"] != null)
                    {
                        bm_txt_PremiumAmount = doc.Range.Bookmarks["txt_PremiumAmount"];
                        if (!string.IsNullOrEmpty(sealDetailModel.SALESDETAIL_ALLMONEY.ToString()))
                        {
                            bm_txt_PremiumAmount.Text = sealDetailModel.SALESDETAIL_ALLMONEY.ToString();
                        }
                    }
                    Bookmark bm_txt_CapitalPremiumAmount;//*第四部分 保险费Premium（人民币RMB）
                    if (doc.Range.Bookmarks["txt_CapitalPremiumAmount"] != null)
                    {

                        bm_txt_CapitalPremiumAmount = doc.Range.Bookmarks["txt_CapitalPremiumAmount"];
                        if (!string.IsNullOrEmpty(sealDetailModel.SALESDETAIL_ALLMONEY.ToString()))
                        {
                            bm_txt_CapitalPremiumAmount.Text = RMB.CmycurD(sealDetailModel.SALESDETAIL_ALLMONEY.ToString());
                        }
                    }
                    //         /******************************第五部分其他告知*********************************************/

                    Bookmark bm_txt_v_question1_Y;// 
                    var question = sealDetailModel.QUESTION;
                    if (question.QUESTION1 == "是")
                    {
                        if (doc.Range.Bookmarks["txt_v_question1_Y"] != null)
                        {
                            bm_txt_v_question1_Y = doc.Range.Bookmarks["txt_v_question1_Y"];
                            bm_txt_v_question1_Y.Text = "■";
                        }
                    }
                    else
                    {
                        Bookmark bm_txt_v_question1_N;// 
                        if (doc.Range.Bookmarks["txt_v_question1_N"] != null)
                        {
                            bm_txt_v_question1_N = doc.Range.Bookmarks["txt_v_question1_N"];
                            bm_txt_v_question1_N.Text = "■";
                        }
                    }
                    if (question.QUESTION2 == "是")
                    {
                        Bookmark bm_txt_v_question2_Y;// 
                        if (doc.Range.Bookmarks["txt_v_question2_Y"] != null)
                        {
                            bm_txt_v_question2_Y = doc.Range.Bookmarks["txt_v_question2_Y"];
                            bm_txt_v_question2_Y.Text = "■"; ;
                        }

                    }
                    else
                    {
                        Bookmark bm_txt_v_question2_N;// 
                        if (doc.Range.Bookmarks["txt_v_question2_N"] != null)
                        {
                            bm_txt_v_question2_N = doc.Range.Bookmarks["txt_v_question2_N"];
                            bm_txt_v_question2_N.Text = "■";
                        }
                    }



                   
                    if (question.QUESTION3 == "是")
                    {
                        Bookmark bm_txt_v_question3_Y;// 
                        if (doc.Range.Bookmarks["txt_v_question3_Y"] != null)
                        {
                            bm_txt_v_question3_Y = doc.Range.Bookmarks["txt_v_question3_Y"];
                            bm_txt_v_question3_Y.Text = "■";
                        }
                        Bookmark bm_txt_DrivingType;//驾照类型 
                        if (doc.Range.Bookmarks["txt_DrivingType"] != null)
                        {
                            bm_txt_DrivingType = doc.Range.Bookmarks["txt_DrivingType"];
                            bm_txt_DrivingType.Text = question.QUESTION3_TYPE ?? "";
                        }
                        Bookmark bm_txt_DrivingAccident;//交通事故描述 
                        if (doc.Range.Bookmarks["txt_DrivingAccident"] != null)
                        {
                            bm_txt_DrivingAccident = doc.Range.Bookmarks["txt_DrivingAccident"];
                            bm_txt_DrivingAccident.Text = question.QUESTION3_1_CONTENT ?? "";
                        }
                        if (question.QUESTION3_1 == "是")
                        {
                            Bookmark bm_txt_v_Question3_1_Y;// 被保险人是否因驾车而发生过意外交通事故
                            if (doc.Range.Bookmarks["txt_v_Question3_1_Y"] != null)
                            {
                                bm_txt_v_Question3_1_Y = doc.Range.Bookmarks["txt_v_Question3_1_Y"];
                                bm_txt_v_Question3_1_Y.Text = "■";
                            }

                        }
                        else
                        {
                            Bookmark bm_txt_v_Question3_1_N;// 被保险人是否因驾车而发生过意外交通事故
                            if (doc.Range.Bookmarks["txt_v_Question3_1_N"] != null)
                            {
                                bm_txt_v_Question3_1_N = doc.Range.Bookmarks["txt_v_Question3_1_N"];
                                bm_txt_v_Question3_1_N.Text = "■";
                            }
                        }

                    }
                    else
                    {
                        Bookmark bm_txt_v_question3_N;// 
                        if (doc.Range.Bookmarks["txt_v_question3_N"] != null)
                        {
                            bm_txt_v_question3_N = doc.Range.Bookmarks["txt_v_question3_N"];
                            bm_txt_v_question3_N.Text = "■";
                        }
                    }

                   




                    #region 第四项 中英文

                    if (question.QUESTION4 == "是")
                    {
                        Bookmark bm_txt_v_question4_Y; // 
                        if (doc.Range.Bookmarks["txt_v_question4_Y"] != null)
                        {
                            bm_txt_v_question4_Y = doc.Range.Bookmarks["txt_v_question4_Y"];
                            bm_txt_v_question4_Y.Text = "■";
                        }

                    }
                    else
                    {
                        Bookmark bm_txt_v_question4_N; // 
                        if (doc.Range.Bookmarks["txt_v_question4_N"] != null)
                        {
                            bm_txt_v_question4_N = doc.Range.Bookmarks["txt_v_question4_N"];
                            bm_txt_v_question4_N.Text = "■";
                        }
                        if (question.QUESTION4_1 == "小于 1 年")
                        {
                            Bookmark bm_txt_v_question4_1_Y; // 小于 1 年
                            if (doc.Range.Bookmarks["txt_v_question4_1_Y"] != null)
                            {
                                bm_txt_v_question4_1_Y = doc.Range.Bookmarks["txt_v_question4_1_Y"];
                                bm_txt_v_question4_1_Y.Text = "■";
                            }
                            /**************************************英文**************************************************/
                            Bookmark bm_txt_v_question4_1_Y_En; // 小于 1 年
                            if (doc.Range.Bookmarks["txt_v_question4_1_Y_En"] != null)
                            {
                                bm_txt_v_question4_1_Y_En = doc.Range.Bookmarks["txt_v_question4_1_Y_En"];
                                bm_txt_v_question4_1_Y_En.Text = "■";
                            }

                        }
                        else
                        {
                            Bookmark bm_txt_v_question4_1_N; // 大于等于 1 年
                            if (doc.Range.Bookmarks["txt_v_question4_1_N"] != null)
                            {
                                bm_txt_v_question4_1_N = doc.Range.Bookmarks["txt_v_question4_1_N"];
                                bm_txt_v_question4_1_N.Text = "■";
                            }
                            Bookmark bm_txt_v_question4_1_N_En; // 大于等于 1 年
                            if (doc.Range.Bookmarks["txt_v_question4_1_N_En"] != null)
                            {
                                bm_txt_v_question4_1_N_En = doc.Range.Bookmarks["txt_v_question4_1_N_En"];
                                bm_txt_v_question4_1_N_En.Text = "■";
                            }
                        }

                        if (question.QUESTION4_2 == "居住证")
                        {
                            Bookmark bm_txt_v_question4_2_Y; // 居住证
                            if (doc.Range.Bookmarks["txt_v_question4_2_Y"] != null)
                            {
                                bm_txt_v_question4_2_Y = doc.Range.Bookmarks["txt_v_question4_2_Y"];
                                bm_txt_v_question4_2_Y.Text = "■";
                            }
                            Bookmark bm_txt_v_question4_2_Y_En; // 居住证
                            if (doc.Range.Bookmarks["txt_v_question4_2_Y_En"] != null)
                            {
                                bm_txt_v_question4_2_Y_En = doc.Range.Bookmarks["txt_v_question4_2_Y_En"];
                                bm_txt_v_question4_2_Y_En.Text = "■";
                            }
                        }
                        else
                        {
                            Bookmark bm_txt_v_question4_2_N; // 暂住证
                            if (doc.Range.Bookmarks["txt_v_question4_2_N"] != null)
                            {
                                bm_txt_v_question4_2_N = doc.Range.Bookmarks["txt_v_question4_2_N"];
                                bm_txt_v_question4_2_N.Text = "■";
                            }
                            Bookmark bm_txt_v_question4_2_N_En; // 暂住证
                            if (doc.Range.Bookmarks["txt_v_question4_2_N_En"] != null)
                            {
                                bm_txt_v_question4_2_N_En = doc.Range.Bookmarks["txt_v_question4_2_N_En"];
                                bm_txt_v_question4_2_N_En.Text = "■";
                            }
                        }

                        switch (question.QUESTION4_3)
                        {
                            case "工作":
                                Bookmark bm_txt_v_question4_3_1; // 工作	
                                if (doc.Range.Bookmarks["txt_v_question4_3_1"] != null)
                                {
                                    bm_txt_v_question4_3_1 = doc.Range.Bookmarks["txt_v_question4_3_1"];
                                    bm_txt_v_question4_3_1.Text = "■";
                                }
                                Bookmark bm_txt_v_question4_3_1_En; // 工作	
                                if (doc.Range.Bookmarks["txt_v_question4_3_1_En"] != null)
                                {
                                    bm_txt_v_question4_3_1_En = doc.Range.Bookmarks["txt_v_question4_3_1_En"];
                                    bm_txt_v_question4_3_1_En.Text = "■";
                                }
                                break;
                            case "探亲":
                                Bookmark bm_txt_v_question4_3_2; // 探亲
                                if (doc.Range.Bookmarks["txt_v_question4_3_2"] != null)
                                {
                                    bm_txt_v_question4_3_2 = doc.Range.Bookmarks["txt_v_question4_3_2"];
                                    bm_txt_v_question4_3_2.Text = "■";
                                }
                                Bookmark bm_txt_v_question4_3_2_En; // 探亲
                                if (doc.Range.Bookmarks["txt_v_question4_3_2_En"] != null)
                                {
                                    bm_txt_v_question4_3_2_En = doc.Range.Bookmarks["txt_v_question4_3_2_En"];
                                    bm_txt_v_question4_3_2_En.Text = "■";
                                }
                                break;
                            case "旅游":
                                Bookmark bm_txt_v_question4_3_3; // 旅游
                                if (doc.Range.Bookmarks["txt_v_question4_3_3"] != null)
                                {
                                    bm_txt_v_question4_3_3 = doc.Range.Bookmarks["txt_v_question4_3_3"];
                                    bm_txt_v_question4_3_3.Text = "■";
                                }
                                Bookmark bm_txt_v_question4_3_3_En; // 旅游
                                if (doc.Range.Bookmarks["txt_v_question4_3_3_En"] != null)
                                {
                                    bm_txt_v_question4_3_3_En = doc.Range.Bookmarks["txt_v_question4_3_3_En"];
                                    bm_txt_v_question4_3_3_En.Text = "■";
                                }
                                break;
                            case "其他":
                                Bookmark bm_txt_v_question4_3_4; // 其他
                                if (doc.Range.Bookmarks["txt_v_question4_3_4"] != null)
                                {
                                    bm_txt_v_question4_3_4 = doc.Range.Bookmarks["txt_v_question4_3_4"];
                                    bm_txt_v_question4_3_4.Text = "■";
                                }
                                Bookmark bm_txt_v_question4_3_4_En; // 其他
                                if (doc.Range.Bookmarks["txt_v_question4_3_4_En"] != null)
                                {
                                    bm_txt_v_question4_3_4_En = doc.Range.Bookmarks["txt_v_question4_3_4_En"];
                                    bm_txt_v_question4_3_4_En.Text = "■";
                                }
                                break;
                        }


                        if (question.QUESTION4_4 == "自有房产")
                        {
                            Bookmark bm_txt_v_question4_4_Y; // 自有房产
                            if (doc.Range.Bookmarks["txt_v_question4_4_Y"] != null)
                            {
                                bm_txt_v_question4_4_Y = doc.Range.Bookmarks["txt_v_question4_4_Y"];
                                bm_txt_v_question4_4_Y.Text = "■";
                            }
                            Bookmark bm_txt_v_question4_4_Y_En; // 自有房产
                            if (doc.Range.Bookmarks["txt_v_question4_4_Y_En"] != null)
                            {
                                bm_txt_v_question4_4_Y_En = doc.Range.Bookmarks["txt_v_question4_4_Y_En"];
                                bm_txt_v_question4_4_Y_En.Text = "■";
                            }
                        }
                        else
                        {
                            Bookmark bm_txt_v_question4_4_N; // 其它
                            if (doc.Range.Bookmarks["txt_v_question4_4_N"] != null)
                            {
                                bm_txt_v_question4_4_N = doc.Range.Bookmarks["txt_v_question4_4_N"];
                                bm_txt_v_question4_4_N.Text = "■";
                            }
                            Bookmark bm_txt_v_question4_4_N_En; // 其它
                            if (doc.Range.Bookmarks["txt_v_question4_4_N_En"] != null)
                            {
                                bm_txt_v_question4_4_N_En = doc.Range.Bookmarks["txt_v_question4_4_N_En"];
                                bm_txt_v_question4_4_N_En.Text = "■";
                            }
                        }
                    }

                    #endregion

                    if (question.QUESTION5 == "是")
                    {
                        Bookmark bm_txt_v_question5_Y;// 
                        if (doc.Range.Bookmarks["txt_v_question5_Y"] != null)
                        {
                            bm_txt_v_question5_Y = doc.Range.Bookmarks["txt_v_question5_Y"];
                            bm_txt_v_question5_Y.Text = "■";
                        }
                        Bookmark bm_txt_question5tInfo;// 
                        if (doc.Range.Bookmarks["txt_question5tInfo"] != null)
                        {
                            bm_txt_question5tInfo = doc.Range.Bookmarks["txt_question5tInfo"];
                            bm_txt_question5tInfo.Text = question.QUESTION5_CONTENT ?? "";
                        }
                    }
                    else
                    {
                        Bookmark bm_txt_v_question5_N;// 
                        if (doc.Range.Bookmarks["txt_v_question5_N"] != null)
                        {
                            bm_txt_v_question5_N = doc.Range.Bookmarks["txt_v_question5_N"];
                            bm_txt_v_question5_N.Text = "■";
                        }
                    }



                   
                    if (question.QUESTION6 == "是")
                    {
                        Bookmark bm_txt_v_question6_Y;// 
                        if (doc.Range.Bookmarks["txt_v_question6_Y"] != null)
                        {
                            bm_txt_v_question6_Y = doc.Range.Bookmarks["txt_v_question6_Y"];
                            bm_txt_v_question6_Y.Text = "■";
                        }
                        Bookmark bm_txt_question6tInfo;// 
                        if (doc.Range.Bookmarks["txt_question6tInfo"] != null)
                        {
                            bm_txt_question6tInfo = doc.Range.Bookmarks["txt_question6tInfo"];
                            bm_txt_question6tInfo.Text = question.QUESTION6_CONTENT ?? "";
                        }
                    }
                    else
                    {
                        Bookmark bm_txt_v_question6_N;// 
                        if (doc.Range.Bookmarks["txt_v_question6_N"] != null)
                        {
                            bm_txt_v_question6_N = doc.Range.Bookmarks["txt_v_question6_N"];
                            bm_txt_v_question6_N.Text = "■";
                        }
                    }


                   
                    if (question.QUESTION7 == "是")
                    {
                        Bookmark bm_txt_v_question7_Y;// 
                        if (doc.Range.Bookmarks["txt_v_question7_Y"] != null)
                        {
                            bm_txt_v_question7_Y = doc.Range.Bookmarks["txt_v_question7_Y"];
                            bm_txt_v_question7_Y.Text = "■";
                        }
                        Bookmark bm_txt_question7tInfo;// 
                        if (doc.Range.Bookmarks["txt_question7tInfo"] != null)
                        {
                            bm_txt_question7tInfo = doc.Range.Bookmarks["txt_question7tInfo"];
                            bm_txt_question7tInfo.Text = question.QUESTION7_CONTENT ?? ""; ;
                        }
                    }
                    else
                    {
                        Bookmark bm_txt_v_question7_N;// 
                        if (doc.Range.Bookmarks["txt_v_question7_N"] != null)
                        {
                            bm_txt_v_question7_N = doc.Range.Bookmarks["txt_v_question7_N"];
                            bm_txt_v_question7_N.Text = "■";
                        }
                    }


                   
                    string strPathName = customModel.CUSTOM_NAME+ DateTime.Now.ToString("yyyyMMddHHmmssffff");

                    if (!string.IsNullOrEmpty(strPath)) //you
                    {
                        doc.Save(baseurl + @"Upload/PrintInsurancePolicy/大地境外留学医疗保障投保单" + strPathName + ".docx", SaveFormat.Docx);
                        doc.Save(baseurl + @"Upload/PrintInsurancePolicy/大地境外留学医疗保障投保单" + strPathName + ".pdf", SaveFormat.Pdf);
                        dynamic obj = new ExpandoObject();
                        obj.url = "PrintInsurancePolicy/大地境外留学医疗保障投保单" + strPathName + ".pdf";//生成Docx

                        return Json(new AjaxResult<object> { Code = 1, Result = obj });
                    }
                    else //wushouyyiren
                    {
                        doc.Save(baseurl + @"Upload/PrintInsurancePolicy/大地境外留学医疗保障投保单无受益人" + strPathName + ".docx", SaveFormat.Docx);
                        doc.Save(baseurl + @"Upload/PrintInsurancePolicy/大地境外留学医疗保障投保单无受益人" + strPathName + ".pdf", SaveFormat.Pdf);
                        dynamic obj = new ExpandoObject();
                        obj.url = "PrintInsurancePolicy/大地境外留学医疗保障投保单无受益人" + strPathName + ".pdf";//生成Docx
                        return Json(new AjaxResult<object> { Code = 1, Result = obj });
                    }

                }
                else
                {
                    return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = "" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<ADMIN> { Code = 0, Msg = msg });
            }

        }
    }
}
