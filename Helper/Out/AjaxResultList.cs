using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Out
{
  public  class AjaxResultList<T>
    {
        public int Code { get; set; }
        public List<T> Result { get; set; }
        public int ListCount { get; set; }
        public string Msg { get; set; }
    }
}
