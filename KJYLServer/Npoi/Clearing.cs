using DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL;
using Helper.Base;
using Model;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace KJYLServer.Npoi
{
    public class Clearing
    {
        //更新数据库
     static COMPANYMONEYService sbll = new COMPANYMONEYService();
        #region 渠道佣金

        public static string GetOrganizationMoney(List<BROKERAGEDto> list)
        {
            string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string sourceFile = @"Upload\Clearing\渠道佣金.xlsx";
            XSSFWorkbook wk = null;  //2007+使用xssf             
            using (FileStream fs = File.Open(baseurl + sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //把xls文件读入workbook变量里，之后就可以关闭了  
                wk = new XSSFWorkbook(fs);
                fs.Close();
            }
            ISheet sheet = wk.GetSheetAt(0);
            var row = sheet.GetRow(2);
            foreach (var item in list)
            {
                int k = list.IndexOf(item);
                IRow newrow = sheet.CreateRow(k + 2);
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
                    style.Alignment = HorizontalAlignment.Center;
                    style.WrapText = true;
                    cell.CellStyle = style;
                    switch (i)
                    {
                        case 0:
                            cell.SetCellValue(k + 1);
                            break;
                        case 1:
                            cell.SetCellValue(item.CUSTOM_NAME);
                            break;
                        case 2:
                            cell.SetCellValue(item.CUSTOM_SEX);
                            break;
                        case 3:
                            cell.SetCellValue(item.PRODUCE_NAME);
                            break;
                        case 4:
                            cell.SetCellValue((item.SALESDETAIL_MONEY ?? 0 + item.SALESDETAIL_ADDMONEY ?? 0).ToString());
                            break;
                        case 5:
                            if (item.SALESDETAIL_MONEYTIME != null)
                                cell.SetCellValue(item.SALESDETAIL_MONEYTIME.Value.ToString("yyyymmdd"));
                            break;
                        case 6:
                            cell.SetCellValue(item.BROKERAGE_RATIO);
                            break;
                        case 7:
                            cell.SetCellValue(item.BROKERAGE_MONEY.ToString());
                            break;
                        case 8:
                            cell.SetCellValue(item.ORGANIZATION_NAME);
                            break;
                        case 9:
                            cell.SetCellValue(item.BROKERAGE_FROM);
                            break;
                    }
                }
            }
            //合并单元格
            //sheet.AddMergedRegion(new CellRangeAddress(list.Count + 3, list.Count + 3, 0, 7));
            IRow newrow2 = sheet.CreateRow(list.Count + 2);
            for (int i = 0; i < row.Cells.Count; i++)
            {
                var cell = newrow2.CreateCell(i);
                ICellStyle style = wk.CreateCellStyle();
                //设置单元格格式
                IDataFormat format = wk.CreateDataFormat();
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.Alignment = HorizontalAlignment.Center;
                style.WrapText = true;
                cell.CellStyle = style;
                if (i == 7)
                {
                    newrow2.Cells[i].SetCellValue(list.Sum(p => p.BROKERAGE_MONEY).ToString());
                }

            }
            var name = DateTime.Now.ToString("yyyymmddhhmmss") + "渠道佣金.xlsx";
            string resultFIle = @"Upload\Clearing\" + name;
            using (FileStream fileStream = File.Open(baseurl + resultFIle, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                wk.Write(fileStream);
                fileStream.Close();
            }
            return "Clearing/" + name;
        }
        #endregion
        #region 佣金

        public static async Task<Tuple<string,bool>> GetCommission(List<COMPANYMONEYDto> list)
        {
            string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string sourceFile = @"Upload\Clearing\销售明细及佣金支付明细统计汇总表.xlsx";
            XSSFWorkbook wk = null;  //2007+使用xssf             
            using (FileStream fs = File.Open(baseurl + sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //把xls文件读入workbook变量里，之后就可以关闭了  
                wk = new XSSFWorkbook(fs);
                fs.Close();
            }
            ISheet sheet = wk.GetSheetAt(0);
            var row = sheet.GetRow(2);
            dynamic trueMoney = 0;
            dynamic moneyL = 0;//剩余钱 总
            dynamic moneyUp = 0;// 上次剩余钱
            dynamic moneyC = 0;//当前剩余钱
            foreach (var item in list)
            {
                int k = list.IndexOf(item);
                IRow newrow = sheet.CreateRow(k + 2);
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
                    style.Alignment = HorizontalAlignment.Center;
                    style.WrapText = true;
                    cell.CellStyle = style;
                    switch (i)
                    {
                        case 0:
                            cell.SetCellValue(k + 1);
                            break;
                        case 1:
                            cell.SetCellValue(item.Salesdetail.SALESDETAIL_INSURANCENUMBER);

                            break;
                        case 2:
                            if (item.Salesdetail.SALESDETAIL_APPLYTIME != null)
                            {
                                var stime = item.Salesdetail.SALESDETAIL_APPLYTIME.Value.ToString("yyyyMMdd");
                                cell.SetCellValue(stime);
                            }
                            break;
                        case 3:
                            if (item.Salesdetail.SALESDETAIL_STARTTIME != null)
                            {
                                var stime = item.Salesdetail.SALESDETAIL_STARTTIME.Value.ToString("yyyyMMdd");
                                cell.SetCellValue(stime);
                            }
                            break;
                        case 4:
                            if (item.Salesdetail.SALESDETAIL_ISINSURED == "是")
                            {
                                cell.SetCellValue(item.CUSTOM_NAME);
                            }
                            else
                            {
                                var firstOrDefault = item.Salesdetail.RELATIONPEOPLEs
                                    .FirstOrDefault(p => p.RELATIONPEOPLE_TYPE == "投保人");
                                if (firstOrDefault != null)
                                    cell.SetCellValue(firstOrDefault
                                        .RELATIONPEOPLE_NAME);
                            }
                            break;
                        case 5:
                            cell.SetCellValue(item.CUSTOM_NAME);
                            break;
                        case 6:
                            cell.SetCellValue(item.Salesdetail.PRODUCE_NAME);
                            //
                            break;
                        case 7:
                            if (item.Salesdetail.SALESDETAIL_ADDMONEY != null &&
                                string.IsNullOrEmpty(item.Salesdetail.SALESDETAIL_ADDITION))
                            {
                                cell.SetCellValue(item.Salesdetail.SALESDETAIL_ADDITION + "增加" + item.Salesdetail.SALESDETAIL_ADDMONEY.ToString());
                            }
                            //if (k == 0) cell.SetCellValue("实收保费的20%");
                            break;
                        case 8:
                            cell.SetCellValue((item.Salesdetail.SALESDETAIL_ALLMONEY ?? 0).ToString());
                            break;
                        case 9:
                            cell.SetCellValue((item.Companymoney.COMPANYMONEY_COMMISSIONMONEY).ToString());
                            break;
                        case 10:
                            cell.SetCellValue(item.Companymoney.COMPANYMONEY_TYPE);
                            break;
                        case 11:
                            if (k == 0)
                            {
                                //查询剩余佣金 计算
                                COMPANYBROKERAGEService cbll=new COMPANYBROKERAGEService();
                               var prve= (await cbll.SelectList(p => true)).OrderByDescending(p => p.COMPANYBROKERAGE_TIME)
                                    .Take(1).ToList();
                                var allmoney = list.Sum(p => p.Companymoney.COMPANYMONEY_COMMISSIONMONEY)??0;
                                var allTruMoney = list.Sum(p => p.Companymoney.COMPANYMONEY_COMMISSIONMONEY);
                                //本次的实际金额
                                // 开始 求本次剩余钱 
                                var arry2 = allTruMoney.ToString().Split('.');
                                if (arry2.Length == 2)
                                {
                                    moneyC = allTruMoney - int.Parse(arry2[0]);
                                }
                                // 结束 求本次剩余钱 

                                if (prve != null && prve.Count>0)
                                {
                                    moneyUp = prve[0].COMPANYBROKERAGE_SURPLUSREAL.Value;
                                    allTruMoney += prve[0].COMPANYBROKERAGE_SURPLUSREAL.Value;                                  
                                }
                                //取整 写入
                                var arry= allTruMoney.ToString().Split('.');
                                if (arry.Length == 2)
                                {
                                    moneyL = allTruMoney- int.Parse(arry[0]);
                                }
                                trueMoney = allTruMoney-moneyL;
                                cell.SetCellValue(arry[0]);
                            }
                            break;
                        case 12:
                            if (k == 0)
                            {
                                cell.SetCellValue(moneyL.ToString());
                            }
                            break;
                        case 13:
                                cell.SetCellValue(item.Salesdetail.SALESDETAIL_COMPANY.ToString());
                            break;
                    }
                }
                //需确认次数
                /*
                 * 合并销售记录 无重复 ，记录导出次数 ，需确认次数加一， 查询需确认时， 次数不为0或空
                 * */
                item.Companymoney.COMPANYMONEY_COMMIISSIONSTATE = "已导出";
            }
            //合并单元格
            sheet.AddMergedRegion(new CellRangeAddress(2, list.Count + 1, 11, 11));
            sheet.AddMergedRegion(new CellRangeAddress(2, list.Count + 1, 12, 12));
            var name = DateTime.Now.ToString("yyyymmddhhmmss") + "佣金.xlsx";
            string resultFIle = @"Upload\Clearing\" + name;
            using (FileStream fileStream = File.Open(baseurl + resultFIle, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                wk.Write(fileStream);
                fileStream.Close();
            }
            COMPANYBROKERAGE model = new COMPANYBROKERAGE
            {
                COMPANYBROKERAGE_ID = GetRandom.GetId(),
                COMPANYBROKERAGE_TIME = DateTime.Now,
                COMPANYBROKERAGE_SURPLUS =moneyL, //剩余
                COMPANYBROKERAGE_MONEY = trueMoney,//结算金额
                COMPANYBROKERAGE_CSURPLUS = moneyC,
                COMPANYBROKERAGE_UPSURPLUS=moneyUp
                // COMPANYBROKERAGE_MONEYREAL=0, //实际结算佣金
                //COMPANYBROKERAGE_SURPLUSREAL = 0,//实际剩余佣金
            };
            List<COMPANYMONEY> clist = list.Select(p => p.Companymoney).ToList();
            bool f = await sbll.UpdateRelation(clist, model);
            return new Tuple<string, bool>("Clearing/" + name,true);
        }
        #endregion

        #region 有效保单
        public static async Task<Tuple<string, bool>> GetVliad(List<COMPANYMONEYDto> list)
        {
            string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string sourceFile = @"Upload\Clearing\留学人员医疗保险有效保单.xlsx";
            XSSFWorkbook wk = null;  //2007+使用xssf             
            using (FileStream fs = File.Open(baseurl + sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //把xls文件读入workbook变量里，之后就可以关闭了  
                wk = new XSSFWorkbook(fs);
                fs.Close();
            }
            ISheet sheet = wk.GetSheetAt(0);
            var row = sheet.GetRow(3);
            foreach (var item in list)
            {
                int k = list.IndexOf(item);
                IRow newrow = sheet.CreateRow(k + 3);
                for (int i = 0; i < row.Cells.Count + 1; i++)
                {
                    if (i == 0) continue;
                    var cell = newrow.CreateCell(i);
                    ICellStyle style = wk.CreateCellStyle();
                    //设置单元格格式
                    IDataFormat format = wk.CreateDataFormat();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;
                    style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                    style.Alignment = HorizontalAlignment.Center;
                    style.WrapText = true;
                    cell.CellStyle = style;
                    switch (i)
                    {
                        case 1:
                            cell.SetCellValue(item.Salesdetail.SALESDETAIL_COMPANY);
                            break;
                        case 2:
                            cell.SetCellValue(item.Salesdetail.SALESDETAIL_INSURANCENUMBER);
                            break;
                        case 3:
                            cell.SetCellValue(item.CUSTOM_SCOUNTRY);
                            break;
                        case 4:
                            if (item.Salesdetail.SALESDETAIL_STARTTIME != null)
                            {
                                var stime = item.Salesdetail.SALESDETAIL_STARTTIME.Value.ToString("yyyyMMdd");
                                cell.SetCellValue(stime);
                            }
                            break;
                        case 5:
                            if (item.Salesdetail.SALESDETAIL_ENDTIME != null)
                            {
                                var stime = item.Salesdetail.SALESDETAIL_ENDTIME.Value.ToString("yyyyMMdd");
                                cell.SetCellValue(stime);
                            }
                            break;
                        case 6:
                            cell.SetCellValue((item.Companymoney.COMPANYMONEY_COMAPNYMONEY ?? 0 ).ToString());
                            break;
                        case 7:
                            if (k == 0) cell.SetCellValue("实收保费的20%");
                            break;
                        case 8:
                            cell.SetCellValue(item.Companymoney.COMPANYMONEY_TYPE);
                            break;
                    }
                }
                //需确认次数
                /*
                 * 合并销售记录 无重复 ，记录导出次数 ，需确认次数加一， 查询需确认时， 次数不为0或空
                 * */
                item.Companymoney.COMPANYMONEY_COMAPNYSTATE = "已导出";
            }
            //合并单元格
            sheet.AddMergedRegion(new CellRangeAddress(3, list.Count + 2, 7, 7));
            var name = DateTime.Now.ToString("yyyymmddhhmmss") + "有效保单.xlsx";
            string resultFIle = @"Upload\Clearing\" + name;
            using (FileStream fileStream = File.Open(baseurl + resultFIle, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                wk.Write(fileStream);
                fileStream.Close();
            }
            List<COMPANYMONEY> clist = list.Select(p => p.Companymoney).ToList();
            bool f = await sbll.UpdateAllNew(clist);
            return new Tuple<string, bool>("Clearing/" + name, true);
        }
        #endregion

        #region 服务费
        public static async Task<Tuple<string, bool>> GetServerMoneyAsync(List<COMPANYMONEYDto> list, DateTime startTime, DateTime endTime)
        {
            string baseurl = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string sourceFile = @"Upload\Clearing\留学人员医疗保险服务费清单.xlsx";
            XSSFWorkbook wk = null;  //2007+使用xssf             
            using (FileStream fs = File.Open(baseurl + sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //把xls文件读入workbook变量里，之后就可以关闭了  
                wk = new XSSFWorkbook(fs);
                fs.Close();
            }
            ISheet sheet = wk.GetSheetAt(0);
            var rowTitle = sheet.GetRow(3);
            rowTitle.Cells[7].SetCellValue("清单日期：" + startTime.Year + "年 " + startTime.Month + " 月 " +
                                           startTime.Day + " 日—" + endTime.Year + "年" + endTime.Month +
                                           " 月 " + endTime.Day + " 日");
            var row = sheet.GetRow(4);
            foreach (var item in list)
            {
                int k = list.IndexOf(item);
                IRow newrow = sheet.CreateRow(k + 5);
                for (int i = 0; i < row.Cells.Count + 1; i++)
                {
                    if (i == 0) continue;
                    var cell = newrow.CreateCell(i);
                    ICellStyle style = wk.CreateCellStyle();
                    //设置单元格格式
                    IDataFormat format = wk.CreateDataFormat();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;
                    style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                    style.Alignment = HorizontalAlignment.Center;
                    style.WrapText = true;
                    cell.CellStyle = style;
                    switch (i)
                    {
                        case 1:
                            cell.SetCellValue(k+1);
                            break;
                        case 2:
                            cell.SetCellValue(item.Salesdetail.SALESDETAIL_INSURANCENUMBER);
                            break;
                        case 3:
                            if (item.Salesdetail.SALESDETAIL_ISINSURED == "是")
                            {
                                cell.SetCellValue(item.CUSTOM_NAME);
                            }
                            else
                            {
                                var relation = item.Salesdetail.RELATIONPEOPLEs.FirstOrDefault(p => p.RELATIONPEOPLE_TYPE == "投保人");
                                if (relation != null) cell.SetCellValue(relation.RELATIONPEOPLE_NAME);
                            }
                            break;
                        case 4:
                            if (item.Salesdetail.SALESDETAIL_STARTTIME != null)
                            {
                                var stime = item.Salesdetail.SALESDETAIL_STARTTIME.Value.ToString("yyyy-MM-dd");
                                cell.SetCellValue(stime);
                            }
                            break;
                        case 5:
                            if (item.Salesdetail.SALESDETAIL_ENDTIME != null)
                            {
                                var stime = item.Salesdetail.SALESDETAIL_ENDTIME.Value.ToString("yyyy-MM-dd");
                                cell.SetCellValue(stime);
                            }
                            break;
                        case 6:
                            cell.SetCellValue((item.Salesdetail.SALESDETAIL_ALLMONEY??0).ToString());
                            break;
                        case 7:
                            if (item.Salesdetail.SALESDETAIL_MONEYTIME != null)
                            {
                                var stime = item.Salesdetail.SALESDETAIL_MONEYTIME.Value.ToString("yyyy-MM-dd");
                                cell.SetCellValue(stime);
                            }
                            break;
                        case 8:
                            cell.SetCellValue("20%");
                            break;
                        case 9:
                           cell.SetCellValue((item.Companymoney.COMPANYMONEY_SEVERMONEY??0).ToString());
                            break;
                        case 10:
                            cell.SetCellValue(item.Salesdetail.SALESDETAIL_COMPANY);
                            break;
                        case 11:
                            cell.SetCellValue(item.Companymoney.COMPANYMONEY_TYPE);
                            break;
                    }

                }
                item.Companymoney.COMPANYMONEY_SEVERSTATE = "已导出";
            }
            //汇总
            IRow newrow2 = sheet.CreateRow(list.Count + 5);
            for (int i = 0; i < row.Cells.Count + 1; i++)
            {
                if (i == 0) continue;
                var cell = newrow2.CreateCell(i);
                ICellStyle style = wk.CreateCellStyle();
                //设置单元格格式
                IDataFormat format = wk.CreateDataFormat();
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center; //垂直居中
                style.Alignment = HorizontalAlignment.Center;
                style.WrapText = true;
                cell.CellStyle = style;
                //汇总
                if (i == 1)
                {
                    cell.SetCellValue("合计");
                }
                if (i == 9)
                {
                    var allmoney = list.Sum(p => p.Companymoney.COMPANYMONEY_SEVERMONEY);
                    cell.SetCellValue(allmoney.ToString());
                }
            }
            //汇总
            IRow newrow3 = sheet.CreateRow(list.Count + 6);
            newrow3.Height = 500;
            for (int i = 0; i < row.Cells.Count + 1; i++)
            {
                if (i == 0) continue;
                var cell = newrow3.CreateCell(i);
                ICellStyle style = wk.CreateCellStyle();
                //设置单元格格式
                IDataFormat format = wk.CreateDataFormat();
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.VerticalAlignment = VerticalAlignment.Center; //垂直居中
                //style.Alignment = HorizontalAlignment.Center;
                style.WrapText = true;
                IFont font = wk.CreateFont();
                font.FontName = "Arial";//字体  
                font.FontHeightInPoints = 9;//字号 
                //font.Color = HSSFColor.PaleBlue.Index;//颜色  
                //font.Underline = NPOI.SS.UserModel.FontUnderlineType.Double;//下划线  
                //font.IsStrikeout = true;//删除线  
                //font.IsItalic = true;//斜体  
                //font.IsBold = true;//加粗  

                style.SetFont(font);
                cell.CellStyle = style;
                if (i == 1)
                {
                    cell.SetCellValue("分公司总经理:                                      分管总:                                   财务负责人:                                  人身险/业管部负责人:                                     经办人:                                     制表日期:  ");
                }
            }
            //合并单元格
            sheet.AddMergedRegion(new CellRangeAddress(list.Count + 6, list.Count + 6, 1, 10));
            var name = DateTime.Now.ToString("yyyymmddhhmmss") + "服务费.xlsx";
            string resultFIle = @"Upload\Clearing\" + name;
            using (FileStream fileStream = File.Open(baseurl + resultFIle, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                wk.Write(fileStream);
                fileStream.Close();
            }
            List<COMPANYMONEY> clist = list.Select(p => p.Companymoney).ToList();
            bool f = await sbll.UpdateAllNew(clist);
            return new Tuple<string, bool>("Clearing/" + name,f);
        }

        #endregion
    }
}