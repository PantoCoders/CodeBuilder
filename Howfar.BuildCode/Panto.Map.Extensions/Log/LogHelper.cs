using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using Panto.Map.Extensions.Xml;
using System.Configuration;

namespace Panto.Map.Extensions.Log
{
	/// <summary>
	/// 日志记录的工具类
	/// </summary>
	public static class LogHelper
	{
		private static readonly string s_logfileFolder;

		static LogHelper()
		{
			string folder = ConfigurationManager.AppSettings["MapExt:LogFileFolder"];
			if( string.IsNullOrEmpty(folder) ) {
				folder = "ExceptionLog";
			}

			folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);

			try {
				if( Directory.Exists(folder) == false )
					Directory.CreateDirectory(folder);
			}
			catch (System.Exception ex){
				throw new System.IO.DirectoryNotFoundException(string.Format("配置错误,路径{0}不存在，且无法创建。", folder), ex);
			}

			s_logfileFolder = folder;
		}

		/// <summary>
		/// 记录一个异常或者消息。
		/// 注意：这个方法可能会抛出异常，例如：没有写文件权限。
		/// </summary>
		/// <param name="ex">异常对象</param>
		/// <param name="message">额外的消息</param>
		public static void LogException(System.Exception ex, string message)
		{
			if( ex == null && string.IsNullOrEmpty(message) )
				return;

			if( ex is NotSupportedException )
				return;
			if( ex is NotImplementedException )
				return;

			if( ex is HttpException ) {
				HttpException ee = ex as HttpException;
				int httpCode = ee.GetHttpCode();
				if( httpCode == 404 /* || httpCode == 403 */ )
					return;
			}


			LogInfo info = new LogInfo();
			info.Time = DateTime.Now;

			if( ex != null ) {
				info.ExceptionType = ex.GetBaseException().GetType().ToString();
				info.Exception = ex.ToString();
			}
			info.Message = message;

			HttpContext current = HttpContext.Current;
			if( current != null ) {
				// web application

				info.Url = current.Request.RawUrl;
				info.RequestType = current.Request.RequestType;
				info.ContentEncoding = current.Request.ContentEncoding.ToString();

				if( current.Request.UrlReferrer != null )
					info.UrlReferrer = current.Request.UrlReferrer.ToString();

				if( current.Request.Browser != null )
					info.Browser = current.Request.Browser.Type;

				if( current.Request.IsAuthenticated )
					info.UserName = current.User.Identity.Name;

				if( current.Request.RequestType == "POST" ) {
					if( current.Request.Files.Count == 0 ) {
						current.Request.InputStream.Position = 0;
						StreamReader reader = new StreamReader(current.Request.InputStream, current.Request.ContentEncoding);
						info.PostData = reader.ReadToEnd();
						reader.Close();
						current.Request.InputStream.Position = 0;
					}
					else {
						StringBuilder sb = new StringBuilder();
						foreach( string name in current.Request.Form.AllKeys ) {
							string[] values = current.Request.Form.GetValues(name);
							if( values != null ) {
								foreach( string value in values )
									sb.AppendFormat("&{0}={1}", HttpUtility.UrlEncode(name), HttpUtility.UrlEncode(value));
							}
						}

						if( sb.Length > 0 ) {
							sb.Remove(0, 1);
							info.PostData = sb.ToString();
						}
					}
				}

				if( current.Request.Cookies.Count > 0 ) {
					foreach( string cookieName in current.Request.Cookies.AllKeys ) {
						HttpCookie cookie = current.Request.Cookies[cookieName];
						info.Cookie.Add(new NameValue { Name = cookie.Name, Value = cookie.Value });
					}
				}

				if( current.Session != null ) {
					foreach( string sessionKey in current.Session.Keys ) {
						object sessionValue = current.Session[sessionKey];
						info.Session.Add(new NameValue {
							Name = sessionKey,
							Value = sessionValue == null ? null : sessionValue.ToString()
						});
					}
				}
			}

			string filePath = string.Format("{0}\\{1}.log", s_logfileFolder, DateTime.Now.ToString("yyyy-MM-dd"));

			string xml = XmlHelper.XmlSerialize(info, Encoding.UTF8);
			FileLoger.SaveToFile(xml, filePath);
		}



		/// <summary>
		/// 以安全方式记录一个异常或者消息。
		/// </summary>
		/// <param name="ex">异常对象</param>
		/// <param name="message">额外的消息</param>
		public static void SafeLogException(System.Exception ex, string message)
		{
			try {
				LogException(ex, message);
			}
			catch { }
		}

		/// <summary>
		/// 以安全方式记录一个异常或者消息。
		/// </summary>
		/// <param name="ex">异常对象</param>
		public static void SafeLogException(System.Exception ex)
		{
			SafeLogException(ex, null);
		}

		/// <summary>
		/// 以安全方式记录一个异常或者消息。
		/// </summary>
		/// <param name="message">异常信息</param>
		public static void SafeLogMessage(string message)
		{
			SafeLogException(null, message);
		}



		/// <summary>
		/// 从指定的文件或者目录中，解析日志文件，得到一个LogInfo列表。
		/// </summary>
		/// <param name="fileOrDirectories">文件或者目录全路径</param>
		/// <returns>解析得到的LogInfo列表</returns>
		public static List<LogInfo> ParaeFile(params string[] fileOrDirectories)
		{
			List<string> lines = FileLoger.ParseLines(fileOrDirectories);
			if( lines == null )
				return null;


			List<LogInfo> list = new List<LogInfo>();
			foreach( string line in lines )
				list.Add(XmlHelper.XmlDeserialize<LogInfo>(line, Encoding.UTF8));

			return list;
		}





	}
}
