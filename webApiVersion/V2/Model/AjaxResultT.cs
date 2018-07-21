using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webApiVersion.V2.Model
{
    /// <summary>
    /// 请求结果 单个 泛型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AjaxResultT<T>
    {
        /// <summary>
        ///  0 失败 1成功
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 泛型结果
        /// </summary>
        public T Result { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public string Msg { get; set; }
    }
}