using Panto.Map.Extensions.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    public class WebSiteHelper
    {
        /// <summary>
        /// 获取mysite里的域名
        /// </summary>
        /// <param name="siteName">站点名称</param>
        /// <returns></returns>
        public static string GetWebSite(string siteName)
        {
            using (ConnectionScope scope = new ConnectionScope(TransactionMode.Inherits, Panto.Framework.ConnStringHelper.GetConnString("EKP")))
            {
                var sql = @"SELECT  CASE WHEN SUBSTRING(REVERSE(SitePath), 1, 1) = '/' THEN SitePath ELSE SitePath + '/' END AS SitePath FROM dbo.mySite WHERE SiteName = @siteName ";
                return CPQuery.From(sql, new { siteName = siteName }).ExecuteScalar<string>();
            }
        }
    }
}
