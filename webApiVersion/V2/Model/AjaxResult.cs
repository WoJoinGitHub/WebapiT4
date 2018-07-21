using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webApiVersion.V2.Model
{
    /// <summary>
    /// ajax 结果 非泛型类
    /// </summary>
    public class AjaxResult
    {

        /// <summary>
        ///  0失败 1 成功
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Msg { get; set; }
    }
}