using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webApiVersion.V2.Model
{
    public class AjaxResultList<T>
    {

        public int Code { get; set; }
        /// <summary>
        /// 列表
        /// </summary>
        public List<T> Result { get; set; }
        /// <summary>
        /// 总数目
        /// </summary>
        public int ListCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Msg { get; set; }
    }
}