using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Out
{
   public class AjaxResult<T>
    {
        public int Code { get; set; }
        public T Result { get; set; }
        public string Msg { get; set; }
    }
}
