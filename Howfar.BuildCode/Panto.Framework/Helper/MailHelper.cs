using Panto.Map.Extensions.DAL;

namespace Panto.Framework
{
    /// <summary>
    /// 邮件助手
    /// </summary>
    public class MailHelper
    {
        /// <summary>
        /// 向邮件表中插入邮件
        /// </summary>
        /// <param name="msgGuid">业务主键GUID或者新建GUID</param>
        /// <param name="strTitle">邮件标题</param>
        /// <param name="strMailBody">邮件内容(HTML格式)</param>
        /// <param name="strReceiveMails">收件人列表(邮箱用;隔开)</param>
        /// <param name="strCcMails">抄送人列表(邮箱用;隔开--可传空或null)</param>
        /// <param name="strSendRemark">邮件发送说明(例如："项目管理系统发送")</param>
        /// <param name="strConn">连接字符串</param>
        /// <returns>返回true表示成功，返回false表示失败。</returns>
        public static bool SendMail(string msgGuid, string strTitle, string strMailBody, string strReceiveMails, string strCcMails, string strSendRemark, string strConn)
        {
            using (var scope = new ConnectionScope(TransactionMode.Inherits, strConn))
            {
                string sql = @"SELECT a.Sender,b.Mailserver,c.MailPsw FROM 
                                (SELECT ROW_NUMBER() OVER(ORDER BY ParamValue) AS sortno,ParamValue AS Sender FROM dbo.myBizParamOption 
                                WHERE ParamName='k_PubEmail' AND ScopeGUID='11B11DB4-E907-4F1F-8835-B9DAAB6E1F23') AS a INNER Join 
                                (SELECT ROW_NUMBER() OVER(ORDER BY ParamValue) AS sortno,ParamValue AS Mailserver FROM dbo.myBizParamOption 
                                WHERE ParamName='k_MailServerName' AND ScopeGUID='11B11DB4-E907-4F1F-8835-B9DAAB6E1F23') AS b
                                ON a.sortno =b.sortno INNER Join  
                                (SELECT ROW_NUMBER() OVER(ORDER BY ParamValue) AS sortno,ParamValue AS MailPsw FROM dbo.myBizParamOption 
                                WHERE ParamName='k_MailPassWord' AND ScopeGUID='11B11DB4-E907-4F1F-8835-B9DAAB6E1F23') AS c ON b.sortno =c.sortno ";
                var dtMail = CPQuery.From(sql, null).FillDataTable();
                if (dtMail.Rows.Count == 0) return false;

                var strMailServer = dtMail.Rows[0]["Mailserver"].ToString();
                var strSenderMail = dtMail.Rows[0]["Sender"].ToString();
                var strMailPsw = dtMail.Rows[0]["MailPsw"].ToString();

                return SendMail(msgGuid, strTitle, strMailBody, strReceiveMails, strCcMails, strSendRemark, strConn,
                    strSenderMail, strMailPsw, strMailServer);
            }
        }

        /// <summary>
        /// 向邮件表中插入邮件
        /// </summary>
        /// <param name="msgGuid">业务主键GUID或者新建GUID</param>
        /// <param name="strTitle">邮件标题</param>
        /// <param name="strMailBody">邮件内容(HTML格式)</param>
        /// <param name="strReceiveMails">收件人列表(邮箱用;隔开)</param>
        /// <param name="strCcMails">抄送人列表(邮箱用;隔开--可传空或null)</param>
        /// <param name="strSendRemark">邮件发送说明(例如："项目管理系统发送")</param>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sender">发送者</param>
        /// <param name="mailpsw">密码</param>
        /// <param name="server">服务地址</param>
        /// <returns>返回true表示成功，返回false表示失败。</returns>
        public static bool SendMail(string msgGuid, string strTitle, string strMailBody,
            string strReceiveMails, string strCcMails, string strSendRemark, string strConn,
            string sender, string mailpsw, string server)
        {
            using (var scope = new ConnectionScope(TransactionMode.Inherits, strConn))
            {
                if (string.IsNullOrEmpty(strReceiveMails))
                {
                    strReceiveMails = string.Empty;
                }
                if (string.IsNullOrEmpty(strCcMails))
                {
                    strCcMails = string.Empty;
                }

                var parameters = new
                {
                    MsgID = msgGuid,
                    ReceiveMail = strReceiveMails,
                    CCMail = strCcMails,
                    Subject = strTitle,
                    MailBody = strMailBody,
                    SMTPServer = server,
                    Sender = sender,
                    PassWord = mailpsw,
                    SenderName = strSendRemark
                };

                const string sql = @" INSERT INTO ReceiveMail(MsgID, ReceiveMail,CCMail, Sender, Subject, MailBody, SendState, ReceiveTime, SMTPServer, PassWord, SenderName, RetryTimes)
                         VALUES(@MsgID,@ReceiveMail,@CCMail,@Sender,@Subject,@MailBody,'未发送',getdate(),@SMTPServer,@PassWord,@SenderName,3);";

                var result = CPQuery.From(sql, parameters).ExecuteNonQuery();
                return result > -1;
            }
        }
    }
}
