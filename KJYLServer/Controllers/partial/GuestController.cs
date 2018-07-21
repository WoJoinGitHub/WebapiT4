using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web.Http;

namespace KJYLServer.Controllers
{
    //pc官网 访客信息
    public class GuestController : ApiController
    {
       

      /// <summary>
      /// 发送邮件
      /// </summary>
      /// <param name="name">客户名称</param>
      /// <param name="tel">客户电话</param>
      /// <returns></returns>
        public IHttpActionResult Get(string name,string tel)
        {
            const string sendMailAddress = "liu.shijun@lzassist.com";
            const string sendMailPassWord = "Lzhy5188";
            var toMail = new string []{ "bdh@lzassist.com" };
            const string mailTitle = "官网访客信息";
            var mailBody = "刘璐好：" +
                           "官网访客信息如下：姓名：" + name + " 电话：" + tel+"请及时回访。谢谢！";
            const string smtpAddressSmtpAddress = "mail.lzassist.com";
            string msg=  SendEmail(sendMailAddress, sendMailPassWord, toMail, mailTitle, mailBody, smtpAddressSmtpAddress, "");
            if (msg == "发送成功")
            {
                return Ok();
            }
            return BadRequest();
        }
        /// <summary>
        ///  .net Mail发送邮件方法
        /// </summary>
        /// <param name="SendMailAddress">发送邮箱地址</param>
        /// <param name="SendMailPassWord">发送邮箱的密码</param>
        /// <param name="ToMail">收件人邮箱地址</param>
        /// <param name="MailTitle">邮件标题</param>
        /// <param name="MailBody">邮件内容</param>
        /// <param name="SmtpAddress">邮箱协议(Smtp服务名:smtp.163.com)</param>
        /// <param name="files">附件</param>
        /// <returns></returns>
        public static string SendEmail(string SendMailAddress, string SendMailPassWord, string[] ToMail, string MailTitle, string MailBody, string SmtpAddress, string files)
        {
            try
            {
                var smtp = new SmtpClient
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Host = SmtpAddress,
                    Port = 25,
                    UseDefaultCredentials = true,
                    Credentials = new NetworkCredential(SendMailAddress, SendMailPassWord)
                }; //实例化一个 SmtpClient
                //将smtp的出站方式设为   Network
                //smtp.EnableSsl = Ssl;//smtp服务器是否启用SSL加密
                //指定 smtp 服务器地址
                //指定 smtp 服务器的端口，默认是25，如果采用默认端口，可省去
                //如果你的SMTP服务器不需要身份认证，则使用下面的方式，不过，目前基本没有不需要认证的了
                //如果需要认证，则用下面的方式
                var mm = new MailMessage
                {
                    Priority = MailPriority.High,
                    From = new MailAddress(SendMailAddress, "", Encoding.GetEncoding(936)),
                    Sender = new MailAddress(SendMailAddress, "", Encoding.GetEncoding(936))
                }; //实例化一个邮件类
                //邮件的优先级，分为 Low, Normal, High，通常用 Normal即可
                //收件方看到的邮件来源；
                //增加附件
                files.Replace('\\', '/');
                if (!string.IsNullOrEmpty(files))
                {
                    mm.Attachments.Add(new Attachment(files));
                    mm.Attachments[0].Name = files.Substring(files.LastIndexOf("/") + 1);
                }
                foreach (var t in ToMail)
                {
                    mm.To.Add(new MailAddress(t));
                }
                //mm.To.Add(ToMail);//邮件的接收者，支持群发，多个地址之间用 半角逗号 分开//当然也可以用全地址添加              
                mm.Subject = MailTitle; //邮件标题
                mm.SubjectEncoding = Encoding.GetEncoding(936);// 这里非常重要，如果你的邮件标题包含中文，这里一定要指定，否则对方收到的极有可能是乱码。
                //936是简体中文的pagecode，如果是英文标题，这句可以忽略不用
                mm.IsBodyHtml = true; //邮件正文是否是HTML格式
                mm.BodyEncoding = Encoding.GetEncoding(936);//邮件正文的编码， 设置不正确， 接收者会收到乱码
                mm.Body = MailBody;//邮件正文             
                smtp.Send(mm);
                return "发送成功";
            }
            catch (Exception ex)
            {
                return "发送失败，失败原因是:" + ex.Message;
            }
        }

    }
}
