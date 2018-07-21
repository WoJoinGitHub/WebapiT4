using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.SqlHeper
{
    /// <summary>
    /// ef 使用sql语句进行 更新  生成sql方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
  public  class SqlUpdate<T>
    {
        public string GetUpdateSql(T model,string id)
        {
            Type t = model.GetType();
            var  propInfos = t.GetProperties();
            StringBuilder str = new StringBuilder();
            str.Append("update "+ t.Name +" set "); 
            Dictionary<string, Dictionary<bool,string>> HaveList = new Dictionary<string, Dictionary<bool, string>>();
            foreach (var item in propInfos)
            {
               
               string name= item.Name;
               object value = item.GetValue(model,null);
                if(value==null || value.ToString() == "" || name== t.Name+ "_ID")
                {
                    continue;
                }
                bool f = false;
                var type= value.GetType().ToString();
                if(type.IndexOf("System.Collections.Generic") >= 0 || type.IndexOf("Model")>=0)
                {
                    continue;
                }
                if (type == "System.DateTime" )
                {
                    f = true;
                }
                Dictionary<bool, string> innerString = new Dictionary<bool, string>
            {
                { f, value.ToString() }
            };
                HaveList.Add(name, innerString);
              
               
            }
            int i = 0;
            foreach (var item in HaveList)
            {
                i++;
                Dictionary<bool, string> innerString = item.Value;
                if (innerString.ElementAt(0).Key)
                {
                    str.Append(item.Key + "=to_date( '"+ innerString.ElementAt(0).Value+"', 'yyyy/mm/dd hh24:mi:ss')");
                }
                else
                {
                    str.Append(item.Key + "= '" + innerString.ElementAt(0).Value + "'");
                }                
                if (i != HaveList.Count)
                {
                    str.Append(",");
                }
            }
            str.Append(" where  " + t.Name+ "_ID =" + id);
            return str.ToString();
            //string name = pi.Name;
            //object value = pi.GetValue(u1, null);
        }
    }
}
