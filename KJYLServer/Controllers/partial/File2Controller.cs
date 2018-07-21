using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using BLL;
using Model;
using Helper;
using KJYLServer.Filter;

namespace KJYLServer.Controllers
{
    [HttpBasicAuthorize]
    /// <summary>
    /// excle 账单的生成
    /// </summary>
    public partial class FileController : ApiController
    {
        
        /// <summary>
        ///支付结果
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ///   [HttpGet]
        [Route("api/File/GetZhifu")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetResultAsync(string id)
        {

            AjaxResult resutget = new AjaxResult
            {
                code = 0,
                msg = "",
                result = null
            };
            string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            BillService bll = new BillService();
            SALESDETAILService salebll = new SALESDETAILService();

            BILL model = new BILL();
            try
            {
                string sourceFile = @"ResultFile\ResultTempleate.xlsx";
                XSSFWorkbook wk = null;  //2007+使用xssf             
                using (FileStream fs = File.Open(baseurl + sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    //把xls文件读入workbook变量里，之后就可以关闭了  
                    wk = new XSSFWorkbook(fs);
                    fs.Close();
                }
                //获取第一个sheet
                //ISheet sheet = wk.GetSheetAt(0);
                model = await bll.SelectOne(id);
                //var Hnamelist = model.BILL_DATA.Select(p => new
                //{
                //    p.BILL_DATA_HNAME
                //}).Distinct().ToList();
                //遍历医院
                //foreach (var Hname in Hnamelist)
                //{
                #region 头部信息添加
                ISheet sheet = wk.GetSheetAt(0);

                sheet = wk.CloneSheet(0);
                wk.SetSheetName(1, model.BILL_HISPOTAL);

                var  saleno = model.RI_MEDICAL.REPORTINFORMATION.SALEDETAILNO;
                var salemodel = salebll.GetSalDetailCustomDto(p=>p.Salesdetail.SALESDETAIL_ID==saleno);
                //修改保单号
                sheet.GetRow(2).GetCell(1).SetCellValue(salemodel.Salesdetail.SALESDETAIL_INSURANCENUMBER);
                ////被保险人姓名
                sheet.GetRow(2).GetCell(5).SetCellValue(salemodel.Custom.CUSTOM_NAME);
                ////性别
                sheet.GetRow(3).GetCell(1).SetCellValue(salemodel.Custom.CUSTOM_SEX);
                ////邮箱
                sheet.GetRow(3).GetCell(5).SetCellValue(model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_EMAIL);
                ////护照
                sheet.GetRow(4).GetCell(1).SetCellValue(salemodel.Custom.CUSTOM_PASSPORT);
                ////保险名称
                sheet.GetRow(4).GetCell(5).SetCellValue(salemodel.Salesdetail.PRODUCE_NAME);
                ////保险期间
                if (salemodel.Salesdetail.SALESDETAIL_STARTTIME != null && salemodel.Salesdetail.SALESDETAIL_ENDTIME != null)
                    sheet.GetRow(5).GetCell(1).SetCellValue(salemodel.Salesdetail.SALESDETAIL_STARTTIME.Value.ToString("yyyy/MM/dd") + "-" + salemodel.Salesdetail.SALESDETAIL_ENDTIME.Value.ToString("yyyy/MM/dd"));
                ////出险时间
                if (model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_TIME != null)
                    sheet.GetRow(5).GetCell(5).SetCellValue(model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_TIME.Value.ToString("yyyy/MM/dd"));
                //医疗机构名称
                sheet.GetRow(7).GetCell(1).SetCellValue(model.BILL_HISPOTAL);
                ////医疗机构地址
                //sheet.GetRow(7).GetCell(3).SetCellValue("");

                //---------------------------------------------------------------------------//
                #endregion
                var billdetail = model.BILL_DATA;
                var row = sheet.GetRow(10);
                int currentrowid = 11;

                int start = currentrowid;
                var list = billdetail;
                ////网络内外
                sheet.GetRow(8).GetCell(1).SetCellValue(list.ElementAt(0).BILL_DATA_TYPE);
                //日期数组
                var datalist = list.Select(p => new
                {
                    p.BILL_DATA_DATA.Value
                }).Distinct().ToList();
                ICellStyle style = wk.CreateCellStyle();
                IDataFormat format = wk.CreateDataFormat();
                style.DataFormat = format.GetFormat("$#,##0.00;-$#,##0.00");
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.Alignment = HorizontalAlignment.Center;
                style.WrapText = true;
                #region 账单详情
                foreach (var listitme in list)
                {
                    var listdetail = listitme.BILL_DATA_DETAIL;
                    foreach (var detailitem in listdetail)
                    {
                        IRow newrow2 = sheet.GetRow(currentrowid);
                        Addrow(wk, sheet, row, currentrowid, false, out currentrowid, out newrow2);
                        for (int i = 0; i < newrow2.Cells.Count(); i++)
                        {

                            switch (i)
                            {
                                case 0:
                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA.BILL_DATA_DATA.Value);
                                    break;
                                case 1:

                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_ZNAME + "\n" + detailitem.BILL_DATA_DETAIL_ENAME);
                                    break;
                                case 3:
                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_COUNT.Value.ToString());
                                    break;
                                case 4:
                                    newrow2.Cells[i].SetCellValue(double.Parse(detailitem.BILL_DATA_DETAIL_PMONEY.Value.ToString()));
                                    newrow2.Cells[i].CellStyle = style;
                                    break;
                                case 5:
                                    newrow2.Cells[i].SetCellValue(double.Parse(detailitem.BILL_DATA_DETAIL_CMONEY.Value.ToString()));
                                    newrow2.Cells[i].CellStyle = style;
                                    break;
                                case 6:
                                    newrow2.Cells[i].SetCellValue(detailitem.DUTYITEM1.DUTYITEM_NAME + "/" + detailitem.DUTYITEM.DUTYITEM_NAME + "\n" + detailitem.DUTYITEM1.DUTYITEM_ENGLISNAME + "/" + detailitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                    break;
                            }

                        }
                    }

                }

                #endregion
                #region 责任item
                var resultlist = model.RESULTs;
                List<RESULT_DUTY> dutylist = new List<RESULT_DUTY>();
                foreach (var result in resultlist)
                {
                    foreach (var resultitem in result.RESULT_DUTY)
                    {
                        dutylist.Add(resultitem);
                    }

                }
                List<RESULT_DUTY> dutylistAll = new List<RESULT_DUTY>();
                //当前医院的 所有日期的 结果
                foreach (var datalistitem in datalist)
                {
                    List<RESULT_DUTY> listselect = dutylist.Where(p => p.RESULT_DUTY_DATE == datalistitem.Value).ToList();
                    dutylistAll.AddRange(listselect);
                }
                List<RESULT_ITEM> resultitemlist = new List<RESULT_ITEM>();
                foreach (var dutylistAllitem in dutylistAll)
                {

                    foreach (var dutylistAllitemitem in dutylistAllitem.RESULT_ITEM)
                    {
                        resultitemlist.Add(dutylistAllitemitem);
                    }

                }
                #endregion
                IRow newrow3 = sheet.GetRow(currentrowid);
                Addtitle(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                foreach (var resultitemlistitem in resultitemlist)
                {

                    Addrow(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                    for (int i = 0; i < newrow3.Cells.Count(); i++)
                    {

                        switch (i)
                        {
                            case 0:
                                newrow3.Cells[i].SetCellValue(resultitemlistitem.DUTYITEM.DUTYITEM_NAME + "\n" + resultitemlistitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                break;
                            case 1:

                                newrow3.Cells[i].SetCellValue(resultitemlistitem.DUTYITEM1.DUTYITEM_NAME + "/" + resultitemlistitem.DUTYITEM.DUTYITEM_NAME + "\n" + resultitemlistitem.DUTYITEM1.DUTYITEM_ENGLISNAME + "/" + resultitemlistitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                break;
                            case 3:

                                newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.RESULT_ITEM_ALLMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                            //case 3:
                            //    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_COUNT.Value.ToString());
                            //    break;
                            case 4:
                                if (list.ElementAt(0).BILL_DATA_TYPE == "网络外" && resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ISPER == "是")
                                {
                                    newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ZFEW.ToString()));
                                    newrow3.Cells[i].CellStyle = style;
                                }
                                else
                                {
                                    newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ZFEN.ToString()));
                                    newrow3.Cells[i].CellStyle = style;
                                }

                                break;
                            case 5:
                                newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.RESULT_ITEM_USERMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                            case 6:
                                newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.RESULT_ITEM_PAYMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                        }

                    }

                }
                RESULT resultmodel = resultlist.ElementAt(0);
                ICellStyle style2 = wk.CreateCellStyle();
             
                style2.DataFormat = format.GetFormat("¥#,##0.00;-¥#,##0.00");
                style2.BorderBottom = BorderStyle.Thin;
                style2.BorderLeft = BorderStyle.Thin;
                style2.BorderRight = BorderStyle.Thin;
                style2.BorderTop = BorderStyle.Thin;
                style2.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style2.Alignment = HorizontalAlignment.Center;
              
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {

                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("免赔额\nDeductible Per Service");
                            break;
                        case 1:
                           
                            newrow3.Cells[i].SetCellValue(double.Parse(resultmodel.RESULT_MPMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;

                    }

                }
                //
                Addsum(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {

                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("保险公司赔付金额总计\nTotal Benefits");
                            break;
                        case 2:

                            newrow3.Cells[i].SetCellValue(double.Parse(resultmodel.RESULT_PAYMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;
                        case 4:

                            newrow3.Cells[i].SetCellValue("被保险人自付金额总计\nTotal Insured Payment");
                            break;
                        case 6:
                            newrow3.Cells[i].SetCellValue(double.Parse(resultmodel.RESULT_USERMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;

                    }

                }
                Addsum(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {

                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("付款日期\nPayment Date");
                            break;
                        case 2:
                            if (resultmodel.RESULT_PAYTIME != null)
                            {
                                newrow3.Cells[i].SetCellValue(resultmodel.RESULT_PAYTIME.Value.ToString("yyyy.MM.dd"));
                                newrow3.Cells[i].CellStyle = style;
                            }
                            break;
                        case 4:

                            newrow3.Cells[i].SetCellValue("汇率(美元/人民币\nExchange Rate");
                            break;
                        case 6:
                            newrow3.Cells[i].SetCellValue(double.Parse(resultmodel.RESULT_PAYMONEYPEOPLELILV.ToString()));
                            //newrow3.Cells[i].CellStyle = style;
                            break;

                    }

                }
                Addsum(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {

                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("保险公司赔付金额总计(人民币)\nTotal Benefits");
                            break;
                        case 2:

                            newrow3.Cells[i].SetCellValue(double.Parse(resultmodel.RESULT_PAYMONEYPEOPLE.ToString()));
                            newrow3.Cells[i].CellStyle = style2;
                            break;
                        

                    }

                }
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                newrow3.Cells[0].SetCellValue("缮制人员综合报告\nPrepared by");
                newrow3.Cells[1].SetCellValue("属于保险责任");
               
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                newrow3.Cells[0].SetCellValue("专职医官意见\nPrepared by");              
                
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                newrow3.Cells[0].SetCellValue("核赔经理意见\nClaim Dept. Manager Comment");
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                newrow3.Cells[0].SetCellValue("公司经理意见\nManager comment");
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                newrow3.Cells[0].SetCellValue("备注\nNote");
               
                string resultFIle = @"ResultFile\" + id + "Result.xlsx";
                using (FileStream fileStream = File.Open(baseurl + resultFIle, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                    resutget.msg = "ResultFile/" + id + "Result.xlsx";
                    resutget.code = 1;
                }

            }
            catch (Exception e)
            {

                resutget.msg = e.Message;
            }
            return Json(resutget);
        }
        /// <summary>
        /// 折扣结果
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/File/GetZheKou")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetZheKouAsync(string id)
        {
            AjaxResult resutget = new AjaxResult()
            {
                code = 0,
                msg = "",
                result = null
            };
            string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            BillService bll = new BillService();
            SALESDETAILService salebll = new SALESDETAILService();

            BILL model = new BILL();
            try
            {
                string sourceFile = @"ResultFile\zkTempleate.xlsx";
                XSSFWorkbook wk = null;  //2007+使用xssf             
                using (FileStream fs = File.Open(baseurl + sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    //把xls文件读入workbook变量里，之后就可以关闭了  
                    wk = new XSSFWorkbook(fs);
                    fs.Close();
                }
                //获取第一个sheet
                //ISheet sheet = wk.GetSheetAt(0);
                model = await bll.SelectOne(id);

                //遍历医院

                #region 头部信息添加
                ISheet sheet = wk.GetSheetAt(0);

                sheet = wk.CloneSheet(0);
                wk.SetSheetName(1, model.BILL_HISPOTAL);

                var  saleno = model.RI_MEDICAL.REPORTINFORMATION.SALEDETAILNO ;
                var salemodel = salebll.GetSalDetailCustomDto(p => p.Salesdetail.SALESDETAIL_ID == saleno);
                //修改保单号
                sheet.GetRow(2).GetCell(1).SetCellValue(salemodel.Salesdetail.SALESDETAIL_INSURANCENUMBER);
                ////被保险人姓名
                sheet.GetRow(2).GetCell(5).SetCellValue(salemodel.Custom.CUSTOM_NAME);
                ////性别
                sheet.GetRow(3).GetCell(1).SetCellValue(salemodel.Custom.CUSTOM_SEX);
                ////邮箱
                sheet.GetRow(3).GetCell(5).SetCellValue(model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_EMAIL);
                ////护照
                sheet.GetRow(4).GetCell(1).SetCellValue(salemodel.Custom.CUSTOM_PASSPORT);
                ////保险名称
                sheet.GetRow(4).GetCell(5).SetCellValue(salemodel.Salesdetail.PRODUCE_NAME);
                ////保险期间
                if (salemodel.Salesdetail.SALESDETAIL_STARTTIME != null && salemodel.Salesdetail.SALESDETAIL_ENDTIME != null)
                    sheet.GetRow(5).GetCell(1).SetCellValue(salemodel.Salesdetail.SALESDETAIL_STARTTIME.Value.ToString("yyyy/MM/dd") + "-" + salemodel.Salesdetail.SALESDETAIL_ENDTIME.Value.ToString("yyyy/MM/dd"));
                ////出险时间
                if (model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_TIME != null)
                    sheet.GetRow(5).GetCell(5).SetCellValue(model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_TIME.Value.ToString("yyyy/MM/dd"));
                //医疗机构名称
                sheet.GetRow(7).GetCell(1).SetCellValue(model.BILL_HISPOTAL);
                ////医疗机构地址
                //sheet.GetRow(7).GetCell(3).SetCellValue("");

                //---------------------------------------------------------------------------//
                #endregion
                var billdetail = model.BILL_DATA;
                var row = sheet.GetRow(10);
                int currentrowid = 11;

                int start = currentrowid;
                var list = billdetail;
                ////网络内外
                sheet.GetRow(8).GetCell(1).SetCellValue(list.ElementAt(0).BILL_DATA_TYPE);
                //日期数组
                var datalist = list.Select(p => new
                {
                    p.BILL_DATA_DATA.Value
                }).Distinct().ToList();
                ICellStyle style = wk.CreateCellStyle();
                IDataFormat format = wk.CreateDataFormat();
                style.DataFormat = format.GetFormat("$#,##0.00;-$#,##0.00");
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.Alignment = HorizontalAlignment.Center;
                #region 账单详情
                foreach (var listitme in list)
                {
                    var listdetail = listitme.BILL_DATA_DETAIL;
                    foreach (var detailitem in listdetail)
                    {
                        IRow newrow2 = sheet.GetRow(currentrowid);
                        Addrow(wk, sheet, row, currentrowid, false, out currentrowid, out newrow2);
                        for (int i = 0; i < newrow2.Cells.Count(); i++)
                        {

                            switch (i)
                            {
                                case 0:
                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA.BILL_DATA_DATA.Value);
                                    break;
                                case 1:

                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_ZNAME);
                                    break;
                                case 3:
                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_COUNT.Value.ToString());
                                    break;
                                case 4:
                                    newrow2.Cells[i].SetCellValue(double.Parse(detailitem.BILL_DATA_DETAIL_PMONEY.Value.ToString()));
                                    newrow2.Cells[i].CellStyle = style;
                                    break;
                                case 5:
                                    newrow2.Cells[i].SetCellValue(double.Parse(detailitem.BILL_DATA_DETAIL_CMONEY.Value.ToString()));
                                    newrow2.Cells[i].CellStyle = style;
                                    break;
                                case 6:
                                    newrow2.Cells[i].SetCellValue(detailitem.DUTYITEM1.DUTYITEM_NAME + "/" + detailitem.DUTYITEM.DUTYITEM_NAME);
                                    break;
                            }
                          

                        }
                    }

                }
                #endregion
                #region 责任item
                var resultlist = model.RESULTs.Where(p => p.RESULT_STATE == "折扣完成");
                List<RESULT_DUTY> dutylist = new List<RESULT_DUTY>();
                foreach (var result in resultlist)
                {
                    foreach (var resultitem in result.RESULT_DUTY)
                    {
                        dutylist.Add(resultitem);
                    }

                }
                List<RESULT_DUTY> dutylistAll = new List<RESULT_DUTY>();
                //当前医院的 所有日期的 结果
                foreach (var datalistitem in datalist)
                {
                    List<RESULT_DUTY> listselect = dutylist.Where(p => p.RESULT_DUTY_DATE == datalistitem.Value).ToList();
                    dutylistAll.AddRange(listselect);
                }
                List<RESULT_ITEM> resultitemlist = new List<RESULT_ITEM>();
                foreach (var dutylistAllitem in dutylistAll)
                {

                    foreach (var dutylistAllitemitem in dutylistAllitem.RESULT_ITEM)
                    {
                        resultitemlist.Add(dutylistAllitemitem);
                    }

                }
                #endregion
                IRow newrow3 = sheet.GetRow(currentrowid);
                Addzktitle(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                foreach (var resultitemlistitem in resultitemlist)
                {

                    Addrowzk(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                    for (int i = 0; i < newrow3.Cells.Count(); i++)
                    {

                        switch (i)
                        {
                            case 0:
                                newrow3.Cells[i].SetCellValue(resultitemlistitem.DUTYITEM.DUTYITEM_NAME+ "\n"+ resultitemlistitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                break;
                            case 1:

                                newrow3.Cells[i].SetCellValue(resultitemlistitem.DUTYITEM1.DUTYITEM_NAME + "/" + resultitemlistitem.DUTYITEM.DUTYITEM_NAME+"\n"+ resultitemlistitem.DUTYITEM1.DUTYITEM_ENGLISNAME + "/" + resultitemlistitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                break;
                            case 2:
                                newrow3.Cells[i].SetCellValue(double.Parse( resultitemlistitem.RESULT_ITEM_ALLMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                            case 3:
                                newrow3.Cells[i].SetCellValue(double.Parse( (resultitemlistitem.RESULT_ITEM_ALLMONEY * resultlist.ElementAt(0).RESULT_ZHEKOUUSER / 100).ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                            //case 3:
                            //    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_COUNT.Value.ToString());
                            //    break;
                            case 4:
                                if (list.ElementAt(0).BILL_DATA_TYPE == "网络外" && resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ISPER == "是")
                                {
                                    newrow3.Cells[i].SetCellValue(double.Parse( resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ZFEW.ToString()));
                                    newrow3.Cells[i].CellStyle = style;
                                }
                                else
                                {
                                    newrow3.Cells[i].SetCellValue(double.Parse( resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ZFEN.ToString()));
                                    newrow3.Cells[i].CellStyle = style;
                                }

                                break;
                            case 5:
                                newrow3.Cells[i].SetCellValue(double.Parse( resultitemlistitem.RESULT_ITEM_USERMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                            case 6:
                                newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.RESULT_ITEM_PAYMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                        }

                    }

                }
                RESULT resultmodel = resultlist.ElementAt(0);
                //免赔额
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {

                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("免赔额" + "\n" + "Deductible Per Service");
                            break;
                        case 1:
                           
                            newrow3.Cells[i].SetCellValue(double.Parse( resultmodel.RESULT_MPMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;

                    }

                }
                Addsum(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {
                   
                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("保险公司赔付金额总计" + "\n" + "Total Benefits");
                            break;
                        case 2:

                            newrow3.Cells[i].SetCellValue(double.Parse( resultmodel.RESULT_PAYMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;
                        case 4:

                            newrow3.Cells[i].SetCellValue("被保险人自付金额总计");
                            break;
                        case 6:
                            newrow3.Cells[i].SetCellValue(double.Parse( resultmodel.RESULT_USERMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;

                    }

                }



                string resultFIle = @"ResultFile\" + id + "ZK" + ".xlsx";
                using (FileStream fileStream = File.Open(baseurl + resultFIle, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                    resutget.msg = "ResultFile/" + id + "ZK" + ".xlsx";
                    resutget.code = 1;
                }

            }
            catch (Exception e)
            {

                resutget.msg = e.Message;
            }
            return Json(resutget);
        }
        /// <summary>
        /// 导出excle 计算结果
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/File/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id)
        {
            AjaxResult resutget = new AjaxResult()
            {
                code = 0,
                msg = "",
                result = null
            };
            string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            BillService bll = new BillService();
            SALESDETAILService salebll = new SALESDETAILService();
            BILL model = new BILL();
            try
            {
                string sourceFile = @"ResultFile\Templeate.xlsx";
                XSSFWorkbook wk = null;  //2007+使用xssf             
                using (FileStream fs = File.Open(baseurl + sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    //把xls文件读入workbook变量里，之后就可以关闭了  
                    wk = new XSSFWorkbook(fs);
                    fs.Close();
                }
                //获取第一个sheet
                //ISheet sheet = wk.GetSheetAt(0);
                model = await bll.SelectOne(id);
                //var Hnamelist = model.BILL_DATA.Select(p => new
                //{
                //    p.BILL_DATA_HNAME
                //}).Distinct().ToList();
                //遍历医院
                //foreach (var Hname in Hnamelist)
                //{
                #region 头部信息添加
                ISheet sheet = wk.GetSheetAt(0);

                sheet = wk.CloneSheet(0);
                wk.SetSheetName(1, model.BILL_HISPOTAL);

                var  saleno = model.RI_MEDICAL.REPORTINFORMATION.SALEDETAILNO;
                var salemodel = salebll.GetSalDetailCustomDto(p => p.Salesdetail.SALESDETAIL_ID == saleno);
                //修改保单号
                sheet.GetRow(2).GetCell(1).SetCellValue(salemodel.Salesdetail.SALESDETAIL_INSURANCENUMBER);
                ////被保险人姓名
                sheet.GetRow(2).GetCell(5).SetCellValue(salemodel.Custom.CUSTOM_NAME);
                ////性别
                sheet.GetRow(3).GetCell(1).SetCellValue(salemodel.Custom.CUSTOM_SEX);
                ////邮箱
                sheet.GetRow(3).GetCell(5).SetCellValue(model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_EMAIL);
                ////护照
                sheet.GetRow(4).GetCell(1).SetCellValue(salemodel.Custom.CUSTOM_PASSPORT);
                ////保险名称
                sheet.GetRow(4).GetCell(5).SetCellValue(salemodel.Salesdetail.PRODUCE_NAME);
                ////保险期间
                if (salemodel.Salesdetail.SALESDETAIL_STARTTIME != null && salemodel.Salesdetail.SALESDETAIL_ENDTIME != null)
                    sheet.GetRow(5).GetCell(1).SetCellValue(salemodel.Salesdetail.SALESDETAIL_STARTTIME.Value.ToString("yyyy/MM/dd") + "-" + salemodel.Salesdetail.SALESDETAIL_ENDTIME.Value.ToString("yyyy/MM/dd"));
                ////出险时间
                if (model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_TIME != null)
                    sheet.GetRow(5).GetCell(5).SetCellValue(model.RI_MEDICAL.REPORTINFORMATION.REPORTINFORMATION_TIME.Value.ToString("yyyy/MM/dd"));
                //医疗机构名称
                sheet.GetRow(7).GetCell(1).SetCellValue(model.BILL_HISPOTAL);
                ////医疗机构地址
                //sheet.GetRow(7).GetCell(3).SetCellValue("");

                //---------------------------------------------------------------------------//
                #endregion
                var billdetail = model.BILL_DATA;
                var row = sheet.GetRow(10);
                int currentrowid = 11;

                int start = currentrowid;
                var list = billdetail;
                ////网络内外
                sheet.GetRow(8).GetCell(1).SetCellValue(list.ElementAt(0).BILL_DATA_TYPE);
                //日期数组
                var datalist = list.Select(p => new
                {
                    p.BILL_DATA_DATA.Value
                }).Distinct().ToList();
                ICellStyle style = wk.CreateCellStyle();
                IDataFormat format = wk.CreateDataFormat();
                style.DataFormat = format.GetFormat("$#,##0.00;-$#,##0.00");
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.Alignment = HorizontalAlignment.Center;
                #region 账单详情
                foreach (var listitme in list)
                {
                    var listdetail = listitme.BILL_DATA_DETAIL;
                    foreach (var detailitem in listdetail)
                    {
                        IRow newrow2 = sheet.GetRow(currentrowid);
                        Addrow(wk, sheet, row, currentrowid, false, out currentrowid, out newrow2);
                        for (int i = 0; i < newrow2.Cells.Count(); i++)
                        {

                            switch (i)
                            {
                                case 0:
                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA.BILL_DATA_DATA.Value);
                                    break;
                                case 1:

                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_ZNAME + "\n" + detailitem.BILL_DATA_DETAIL_ENAME);
                                    break;
                                case 3:
                                    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_COUNT.Value.ToString());
                                    break;
                                case 4:
                                    newrow2.Cells[i].SetCellValue(double.Parse( detailitem.BILL_DATA_DETAIL_PMONEY.Value.ToString()));
                                    newrow2.Cells[i].CellStyle = style;
                                    break;
                                case 5:
                                    newrow2.Cells[i].SetCellValue(double.Parse( detailitem.BILL_DATA_DETAIL_CMONEY.Value.ToString()));
                                    newrow2.Cells[i].CellStyle = style;
                                    break;
                                case 6:                                  
                                    newrow2.Cells[i].SetCellValue(detailitem.DUTYITEM1.DUTYITEM_NAME + "/" + detailitem.DUTYITEM.DUTYITEM_NAME+"\n"+ detailitem.DUTYITEM1.DUTYITEM_ENGLISNAME+"/"+detailitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                    break;
                            }

                        }
                    }

                }
              
                #endregion
                #region 责任item
                var resultlist = model.RESULTs;
                List<RESULT_DUTY> dutylist = new List<RESULT_DUTY>();
                foreach (var result in resultlist)
                {
                    foreach (var resultitem in result.RESULT_DUTY)
                    {
                        dutylist.Add(resultitem);
                    }

                }
                List<RESULT_DUTY> dutylistAll = new List<RESULT_DUTY>();
                //当前医院的 所有日期的 结果
                foreach (var datalistitem in datalist)
                {
                    List<RESULT_DUTY> listselect = dutylist.Where(p => p.RESULT_DUTY_DATE == datalistitem.Value).ToList();
                    dutylistAll.AddRange(listselect);
                }
                List<RESULT_ITEM> resultitemlist = new List<RESULT_ITEM>();
                foreach (var dutylistAllitem in dutylistAll)
                {

                    foreach (var dutylistAllitemitem in dutylistAllitem.RESULT_ITEM)
                    {
                        resultitemlist.Add(dutylistAllitemitem);
                    }

                }
                #endregion
                IRow newrow3 = sheet.GetRow(currentrowid);
                Addtitle(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                foreach (var resultitemlistitem in resultitemlist)
                {

                    Addrow(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                    for (int i = 0; i < newrow3.Cells.Count(); i++)
                    {

                        switch (i)
                        {
                            case 0:
                                newrow3.Cells[i].SetCellValue(resultitemlistitem.DUTYITEM.DUTYITEM_NAME+"/"+ resultitemlistitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                break;
                            case 1:

                                newrow3.Cells[i].SetCellValue(resultitemlistitem.DUTYITEM1.DUTYITEM_NAME + "/" + resultitemlistitem.DUTYITEM.DUTYITEM_NAME + "\n" + resultitemlistitem.DUTYITEM1.DUTYITEM_ENGLISNAME + "/" + resultitemlistitem.DUTYITEM.DUTYITEM_ENGLISNAME);
                                break;
                            case 3:
                              
                                newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.RESULT_ITEM_ALLMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                            //case 3:
                            //    newrow2.Cells[i].SetCellValue(detailitem.BILL_DATA_DETAIL_COUNT.Value.ToString());
                            //    break;
                            case 4:
                                if (list.ElementAt(0).BILL_DATA_TYPE == "网络外" && resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ISPER == "是")
                                {
                                    newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ZFEW.ToString()));
                                    newrow3.Cells[i].CellStyle = style;
                                }
                                else
                                {
                                    newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.DUTYITEM.IO_ITEM.ElementAt(0).IO_ITEM_ZFEN.ToString()));
                                    newrow3.Cells[i].CellStyle = style;
                                }

                                break;
                            case 5:
                                newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.RESULT_ITEM_USERMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                            case 6:
                                newrow3.Cells[i].SetCellValue(double.Parse(resultitemlistitem.RESULT_ITEM_PAYMONEY.ToString()));
                                newrow3.Cells[i].CellStyle = style;
                                break;
                        }

                    }

                }
                RESULT resultmodel = resultlist.ElementAt(0);
                //免赔额
                //ICellStyle style3 = wk.CreateCellStyle();
              
                //style3.DataFormat = format.GetFormat("$#,##0.00;-$#,##0.00");
                //style3.BorderBottom = BorderStyle.Thin;
                //style3.BorderLeft = BorderStyle.Thin;
                //style3.BorderRight = BorderStyle.Thin;
                //style3.BorderTop = BorderStyle.Thin;
                //style3.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                //style3.Alignment = HorizontalAlignment.Center;
                Addsum(wk, sheet, row, currentrowid, false, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {

                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("免赔额" + "\n" + "Deductible Per Service");
                            break;
                        case 1:
                         
                            newrow3.Cells[i].SetCellValue(double.Parse( resultmodel.RESULT_MPMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;

                    }

                }
                Addsum(wk, sheet, row, currentrowid, true, out currentrowid, out newrow3);
                for (int i = 0; i < newrow3.Cells.Count(); i++)
                {
                 
                    switch (i)
                    {
                        case 0:

                            newrow3.Cells[i].SetCellValue("保险公司赔付金额总计" + "\n" + "Total Benefits");
                            break;
                        case 2:

                            newrow3.Cells[i].SetCellValue(double.Parse( resultmodel.RESULT_PAYMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;
                        case 4:

                            newrow3.Cells[i].SetCellValue("被保险人自付金额总计" + "\n" + "Total Insured Payment");
                            break;
                        case 6:
                            newrow3.Cells[i].SetCellValue(double.Parse( resultmodel.RESULT_USERMONEY.ToString()));
                            newrow3.Cells[i].CellStyle = style;
                            break;

                    }

                }


                //}
                string resultFIle = @"ResultFile\" + id + ".xlsx";
                using (FileStream fileStream = File.Open(baseurl + resultFIle, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    wk.Write(fileStream);
                    fileStream.Close();
                    resutget.msg = "ResultFile/" + id + ".xlsx";
                    resutget.code = 1;
                }

            }
            catch (Exception e)
            {

                resutget.msg = e.Message;
            }
            return Json(resutget);
            //cell.SetCellType(CellType.String);


        }

        /// <summary>
        ///  汇总添加 免赔额和 汇总汇总all==true
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="all"></param>
        /// <param name="rowid"></param>
        /// <param name="newrow2"></param>
             
        private static void Addsum(XSSFWorkbook wk, ISheet sheet, IRow row, int id, bool all, out int rowid, out IRow newrow2)
        {
            var newrow = sheet.CreateRow(id);
            if (all)
            {
                newrow.Height = 800;
            }
            else
            {
                newrow.Height = 800;
            }


            for (int i = 0; i < row.Cells.Count; i++)
            {
                var cell = newrow.CreateCell(i);

            }
            if (!all)
            {
                sheet.AddMergedRegion(new CellRangeAddress(id, id, 1, newrow.Cells.Count() - 1));

            }
            else
            {
                for (int i = 0; i < 3; i++)
                {

                    sheet.AddMergedRegion(new CellRangeAddress(id, id, 2 * i, 2 * i + 1));

                }
            }

            for (int i = 0; i < newrow.Cells.Count; i++)
            {
                ICellStyle style = wk.CreateCellStyle();
                style.WrapText = true;
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.Alignment = HorizontalAlignment.Center;
                newrow.Cells[i].CellStyle = style;
            }
            rowid = id + 1;
            newrow2 = newrow;
        }
        /// <summary>
        /// 费用明细 标题
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="rowid"></param>
        private static void Addtitle(XSSFWorkbook wk, ISheet sheet, IRow row, int id, bool title, out int rowid, out IRow newrow2)
        {
            var newrow = sheet.CreateRow(id);

            newrow.Height = 782;

            List<string> listtile = new List<string>
            {
                "项目名称\nItem",
                "所属责任类型\nInsurance Liability",
                "项目费用\nExpenditure",
                "单次自付额\nCopay ",
                "客户自付\nOut-of-Pocket",
                "单项给付\nCoinsurance"
            };
            for (int i = 0; i < row.Cells.Count; i++)
            {
                var cell = newrow.CreateCell(i);

            }
            if (title)
            {
                for (int i = 0; i < 7; i++)
                {
                    if (i < 2)
                    {
                        newrow.Cells[i].SetCellValue(listtile[i]);
                        if (i == 1)
                        {
                            sheet.AddMergedRegion(new CellRangeAddress(id, id, i, i + 1));
                        }
                        continue;
                    }



                    if (i > 2)
                    {
                        newrow.Cells[i].SetCellValue(listtile[i - 1]);

                    }

                }
            }


            for (int i = 0; i < newrow.Cells.Count; i++)
            {

                ICellStyle style = wk.CreateCellStyle();
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中

                style.Alignment = HorizontalAlignment.Left;
                IFont f = wk.CreateFont();
                f.Boldweight = (short)FontBoldWeight.Bold;
                f.FontHeightInPoints = 9;
                style.SetFont(f);
                style.WrapText = true;
                style.Alignment = HorizontalAlignment.Center;


                newrow.Cells[i].CellStyle = style;

            }

            rowid = id + 1;
            newrow2 = newrow;
        }
        /// <summary>
        /// 费用明细 标题  折扣
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="rowid"></param>
        private static void Addzktitle(XSSFWorkbook wk, ISheet sheet, IRow row, int id, bool title, out int rowid, out IRow newrow2)
        {
            var newrow = sheet.CreateRow(id);

            newrow.Height = 782;

            List<string> listtile = new List<string>();
            listtile.Add("项目名称\nItem");
            listtile.Add("所属责任类型\nInsurance Liability");
            listtile.Add("项目费用\nExpenditure");
            listtile.Add("项目费用(折后)\nDiscounted Expenditure");
            listtile.Add("单次自付额\nCopay ");
            listtile.Add("客户自付\nOut-of-Pocket");
            listtile.Add("单项给付\nCoinsurance");

            for (int i = 0; i < row.Cells.Count; i++)
            {
                var cell = newrow.CreateCell(i);
                if (title)
                {
                    cell.SetCellValue(listtile[i]);
                }


            }



            for (int i = 0; i < newrow.Cells.Count; i++)
            {

                ICellStyle style = wk.CreateCellStyle();
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中

                style.Alignment = HorizontalAlignment.Left;
                IFont f = wk.CreateFont();
                f.Boldweight = (short)FontBoldWeight.Bold;
                f.FontHeightInPoints = 9;
                style.SetFont(f);
                style.WrapText = true;
                style.Alignment = HorizontalAlignment.Center;


                newrow.Cells[i].CellStyle = style;

            }

            rowid = id + 1;
            newrow2 = newrow;
        }
        private static void Addrowzk(XSSFWorkbook wk, ISheet sheet, IRow row, int id, bool title, out int rowid, out IRow newrow2)
        {

            IRow newrow = sheet.CreateRow(id);


            newrow.Height = 1395;



            for (int i = 0; i < row.Cells.Count; i++)
            {
                var cell = newrow.CreateCell(i);
                ICellStyle style = wk.CreateCellStyle();
                //设置单元格格式

                IDataFormat format = wk.CreateDataFormat();

                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.WrapText = true;


                if (i == 0)
                {
                    style.DataFormat = format.GetFormat("yyyy/mm/dd");
                }
                if (i == 4 || i == 3)
                {

                    style.DataFormat = format.GetFormat("$#,##0.00;-$#,##0.00");

                }
                style.Alignment = HorizontalAlignment.Center;


                cell.CellStyle = style;
            }


            rowid = id + 1;



            newrow2 = newrow;
        }
        /// <summary>
        /// 标题和费用详情添加
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="rowid"></param>
        private static void Addrow(XSSFWorkbook wk, ISheet sheet, IRow row, int id, bool title, out int rowid, out IRow newrow2)
        {

            IRow newrow = sheet.CreateRow(id);
            if (title)
            {
                newrow.Height = 538;
            }
            else
            {
                newrow.Height = 1395;
            }


            for (int i = 0; i < row.Cells.Count; i++)
            {
                var cell = newrow.CreateCell(i);
                ICellStyle style = wk.CreateCellStyle();
                //设置单元格格式

                IDataFormat format = wk.CreateDataFormat();

                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.WrapText = true;

                if (title)
                {
                    style.Alignment = HorizontalAlignment.Left;
                    IFont f = wk.CreateFont();
                    f.Boldweight = (short)FontBoldWeight.Bold;
                    style.SetFont(f);
                }
                else
                {
                    if (i == 0)
                    {
                        style.DataFormat = format.GetFormat("yyyy/mm/dd");
                    }
                    if (i == 4)
                    {

                        style.DataFormat = format.GetFormat("$#,##0.00;-$#,##0.00");

                    }
                    style.Alignment = HorizontalAlignment.Center;
                }

                cell.CellStyle = style;
            }


            rowid = id + 1;
            if (title)
            {
                sheet.AddMergedRegion(new CellRangeAddress(id, id, 0, newrow.Cells.Count() - 1));

            }
            else
            {
                for (int i = 0; i < 7; i++)
                {

                    if (i == 1)
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(id, id, i, i + 1));
                        continue;
                    }
                    if (i == 2)
                    {
                        continue;
                    }
                    sheet.AddMergedRegion(new CellRangeAddress(id, id, i, i));
                }
            }


            newrow2 = newrow;
        }        
        // POST: api/File
       
    }
}
