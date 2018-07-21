using DTO;
using Helper.Base;
using Helper.Out;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using BLL;
using KJYLServer.Npoi;
using LinqKit;

namespace KJYLServer.Controllers
{
    /// <summary>
    /// 文件上传 相关接口
    /// </summary>
    public partial class FileController : ApiController
    {

        // POST: api/File
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <returns></returns>
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var httpSever = HttpContext.Current.Server;
                FileDto fileClass = new FileDto();
                await Task.Run(() =>
                  {

                      if (httpRequest.Files.Count > 0)
                      {
                          var docfiles = new List<string>();
                          fileClass.FilesList = docfiles;
                          foreach (string file in httpRequest.Files)
                          {
                              var postedFile = httpRequest.Files[file];
                              if (postedFile != null)
                              {
                                  string filename = postedFile.FileName;
                                  string[] arry = filename.Split('.');
                                  string name = GetRandom.GetId();
                                  if (arry.Length > 1)
                                  {
                                      name += "." + arry[arry.Length - 1];
                                  }
                                  else
                                  {
                                      name = filename;
                                  }

                                  var filePath = httpSever.MapPath("~/Upload/" + name);
                                  postedFile.SaveAs(filePath);

                                  docfiles.Add(name);
                              }
                          }
                      }
                  });
                return Json(new AjaxResult<FileDto> { Code = 1, Msg = "上传成功", Result = fileClass });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = msg });
            }
        }
/// <summary>
/// 获取 渠道佣金excle
/// </summary>
/// <param name="param"></param>
/// <returns></returns>
[HttpPut]
[Route("api/file/OrganizationMoney")]
        public async Task<IHttpActionResult> PutOrganizationMoney([FromBody]DoloadParam param)
        {
            try
            {
                //获取 佣金列表 包含渠道名称
                BROKERAGEService bll=new BROKERAGEService();
                Expression<Func<BROKERAGEDto, bool>> wherExpression = p => p.BROKERAGE_STATE==param.Type;
                if (param.TimeStart != null && param.TimeEnd != null)
                {
                    DateTime startTime = param.TimeStart.Value;
                    DateTime endTime = param.TimeEnd.Value;
                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                    {
                        ShortDatePattern = "yyyy/MM/dd"
                    };
                    DateTime stateSt = Convert.ToDateTime(startTime, dtFormat);
                    DateTime stateEnd = Convert.ToDateTime(endTime, dtFormat).AddDays(1);
                    wherExpression.And(p => p.SALESDETAIL_SALESTIME <= stateEnd && p.SALESDETAIL_SALESTIME >= stateSt);
                }
                if (param.IdList!=null && param.IdList.Count > 0)
                {
                    wherExpression.And(p => param.IdList.Contains(p.SALESDETAIL_ID));
                }
                var list = await bll.GetExcelList(wherExpression);
                var url = Clearing.GetOrganizationMoney(list.Item1.ToList());
                return Json(new AjaxResult<FileDto> { Code = 1, Msg = "上传成功", Result = new FileDto { FilesList = new List<string> { url } } });
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 获取佣金
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("api/file/Commission")]
        public async Task<IHttpActionResult> PutCommission([FromBody]DoloadParam param)
        {
            try
            {
                COMPANYBROKERAGEService combll = new COMPANYBROKERAGEService();
                var prve = (await combll.SelectList(p => true)).OrderByDescending(p => p.COMPANYBROKERAGE_TIME)
                    .Take(1).ToList();
                if (prve != null && prve.Count==1)
                {
                    if (prve[0].COMPANYBROKERAGE_MONEYREAL==null)
                    {
                        return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = "请先结算完成上次导出的佣金" });
                    }
                }
                Expression<Func<COMPANYMONEYDto, bool>> whereExpression = p => p.Companymoney.COMPANYMONEY_COMMIISSIONSTATE == null || p.Companymoney.COMPANYMONEY_COMMIISSIONSTATE == ""; 
                if (param.TimeStart != null && param.TimeEnd != null)
                {
                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                    {
                        ShortDatePattern = "yyyy/MM/dd"
                    };
                    DateTime stateSt = Convert.ToDateTime(param.TimeStart, dtFormat).ToUniversalTime();
                    DateTime stateEnd = Convert.ToDateTime(param.TimeEnd, dtFormat).ToUniversalTime().AddDays(1);
                    whereExpression = whereExpression.And(p => p.Companymoney.COMPANYMONEY_TIME <= stateEnd &&
                                                               p.Companymoney.COMPANYMONEY_TIME >= stateSt);
                }
                if (!string.IsNullOrEmpty(param.Company))
                {
                    whereExpression = whereExpression.And(p => p.Salesdetail.SALESDETAIL_COMPANY == param.Company);
                }
                if (param.IdList != null && param.IdList.Count > 0)
                {
                    whereExpression = whereExpression.And(p => param.IdList.Contains(p.Companymoney.COMPANYMONEY_ID));
                }
                COMPANYMONEYService sbll = new COMPANYMONEYService();
                CUSTOMService cbll = new CUSTOMService();
                List<COMPANYMONEYDto> list = (await sbll.GetDto(10, -1, whereExpression)).Item1.ToList();
                var result=await Clearing.GetCommission(list);
                if (result.Item2)
                {
                    return Json(new AjaxResult<FileDto> { Code = 1, Msg = "上传成功", Result = new FileDto { FilesList = new List<string> { result.Item1 } } });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = "" });
                }
               
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 获取有效保单
        /// </summary>

        /// <returns></returns>
        [HttpPut]
        [Route("api/file/vliad")]
        public async Task<IHttpActionResult> PutVliad([FromBody]DoloadParam param)
        {
            try
            {
                Expression<Func<COMPANYMONEYDto, bool>> whereExpression = p => p.Companymoney.COMPANYMONEY_COMAPNYSTATE == null || p.Companymoney.COMPANYMONEY_COMAPNYSTATE == "";
                if (param.TimeStart != null && param.TimeEnd != null)
                {
                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                    {
                        ShortDatePattern = "yyyy/MM/dd"
                    };
                    DateTime stateSt = Convert.ToDateTime(param.TimeStart, dtFormat).ToUniversalTime();
                    DateTime stateEnd = Convert.ToDateTime(param.TimeEnd, dtFormat).ToUniversalTime().AddDays(1);
                    whereExpression = whereExpression.And(p => p.Companymoney.COMPANYMONEY_TIME <= stateEnd &&
                                                               p.Companymoney.COMPANYMONEY_TIME >= stateSt);
                }
                if (!string.IsNullOrEmpty(param.Company))
                {
                    whereExpression = whereExpression.And(p => p.Salesdetail.SALESDETAIL_COMPANY == param.Company);
                }
                if (param.IdList != null && param.IdList.Count > 0)
                {
                    whereExpression = whereExpression.And(p => param.IdList.Contains(p.Companymoney.COMPANYMONEY_ID));
                }
                COMPANYMONEYService sbll = new COMPANYMONEYService();
                CUSTOMService cbll = new CUSTOMService();
                List<COMPANYMONEYDto> list = (await sbll.GetDto(10, -1, whereExpression)).Item1.ToList();
                var result = await  Clearing.GetVliad(list);
                if (result.Item2)
                {
                    return Json(new AjaxResult<FileDto> { Code = 1, Msg = "上传成功", Result = new FileDto { FilesList = new List<string> { result.Item1 } } });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = "" });
                }
           
            }
            catch (Exception e)
            {
                var msg = "请求失败";
#if DEBUG
                msg = e.Message;
#endif
                return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = msg });
            }
        }
        /// <summary>
        /// 导出 服务费
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("api/file/ServerMoney")]
        public async Task<IHttpActionResult> PutServerMoney([FromBody]DoloadParam param)
        {
            try
            {
                Expression<Func<COMPANYMONEYDto, bool>> whereExpression = p => p.Companymoney.COMPANYMONEY_SEVERSTATE == null || p.Companymoney.COMPANYMONEY_SEVERSTATE == "";
                if (param.TimeStart != null && param.TimeEnd != null)
                {
                    DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
                    {
                        ShortDatePattern = "yyyy/MM/dd"
                    };
                    DateTime stateSt = Convert.ToDateTime(param.TimeStart, dtFormat).ToUniversalTime();
                    DateTime stateEnd = Convert.ToDateTime(param.TimeEnd, dtFormat).ToUniversalTime().AddDays(1);
                    whereExpression = whereExpression.And(p => p.Companymoney.COMPANYMONEY_TIME <= stateEnd &&
                                                               p.Companymoney.COMPANYMONEY_TIME >= stateSt);
                }
                if (!string.IsNullOrEmpty(param.Company))
                {
                    whereExpression = whereExpression.And(p => p.Salesdetail.SALESDETAIL_COMPANY == param.Company);
                }
                if (param.IdList != null && param.IdList.Count > 0)
                {
                    whereExpression = whereExpression.And(p => param.IdList.Contains(p.Companymoney.COMPANYMONEY_ID));
                }
                COMPANYMONEYService sbll=new COMPANYMONEYService();
                CUSTOMService cbll=new CUSTOMService();
                List<COMPANYMONEYDto> list=(await sbll.GetDto( 10, -1, whereExpression)).Item1.ToList();
                var result = await Clearing.GetServerMoneyAsync(list, param.TimeStart??DateTime.Now, param.TimeEnd?? DateTime.Now);
                if (result.Item2)
                {
                    return Json(new AjaxResult<FileDto> { Code = 1, Msg = "上传成功", Result = new FileDto { FilesList = new List<string> { result.Item1 } } });
                }
                else
                {
                    return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = "" });
                }
            }
            catch (Exception e)
            {
                var msg = "请求失败";
//#if DEBUG
                msg = e.Message;
//#endif
                return this.JsonMy(new AjaxResult<RECORD> { Code = 0, Msg = msg });
            }
        }

    }
}
