using Model;
using System;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text;
//using log4net;

namespace DAL
{
    public static class DBFactory
    {
    
        public static GitHubEntities GetCurrentDbContext()
        {
            //ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            GitHubEntities dbContext = CallContext.GetData("Entities") as GitHubEntities;
            if (dbContext == null)  
            {
                dbContext = new GitHubEntities();
                CallContext.SetData("Entities", dbContext);
            }
            dbContext.Database.Log = s =>
            {
                Debug.Print(s);
                //log.Info(s);
            };
            return dbContext;
        }
        
        //private static  void  NewMethod(Entities dbContext, string s)
        //{
        //    try
        //    {
        //        LogWriteLock.EnterWriteLock();
        //        string fileName = AppDomain.CurrentDomain.BaseDirectory + "/DataLog/" + DateTime.Now.ToString("yy-MM-dd") + ".txt";
        //        using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
        //        {
        //             writer.Write(s);

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
               
        //    }
        //    finally
        //    {
        //        //退出写入模式，释放资源占用
        //        //注意：一次请求对应一次释放
        //        //      若释放次数大于请求次数将会触发异常[写入锁定未经保持即被释放]
        //        //      若请求处理完成后未释放将会触发异常[此模式不下允许以递归方式获取写入锁定]
        //        LogWriteLock.ExitWriteLock();
        //    }



        //}

    }
}
