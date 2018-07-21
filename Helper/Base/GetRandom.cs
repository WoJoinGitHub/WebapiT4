using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Base
{
   public class GetRandom
    {
        private static Random ran = new Random();
        /// <summary>
        /// 获取 id
        /// </summary>
        /// <returns></returns>
        public static string GetId()
        {
            string time = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            string id = time + GetRex();
            return id;
        }
        public static string GetRex()
        {
            int RandKey = ran.Next(1000, 9999);
            return RandKey.ToString();
        }
        /// <summary>
        /// 获取随机字符串
        /// </summary>
        /// <param name="b">是否有复杂字符</param>
        /// <param name="n">生成的字符串长度</param>
        /// <returns></returns>
        public static string GetStr(bool b, int n)//b：是否有复杂字符，n：生成的字符串长度

        {

            string str = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (b)
            {
                str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";//复杂字符
            }
            StringBuilder SB = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < n; i++)
            {
                SB.Append(str.Substring(rd.Next(0, str.Length), 1));
            }
            return SB.ToString();

        }
    }
}
