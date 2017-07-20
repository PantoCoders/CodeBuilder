using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Configuration;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Web.Caching;

namespace Panto.Map.Extensions
{
	internal sealed class StaticFileHandler : IHttpHandler
	{
		// 默认缓存时间：10分钟
		private static readonly int s_DefaultDuration = 1200;

		// 统一的缓存时间，以及没有指定扩展名的缓存时间
		private static readonly int s_CacheDuration;
		// 针对指定扩展名的过期时间
		private static readonly Hashtable s_durationTable;
		// 每种扩展名对应诉Mime类型对照表
		private static readonly Hashtable s_mineTable = Hashtable.Synchronized(new Hashtable(10, StringComparer.OrdinalIgnoreCase));
		/// 用于提取CSS中的引用图片的正则表达式
		private static readonly Regex s_CssBackgroundImageRegex = new Regex(@"\burl\((?<file>[^\)]+)\)", RegexOptions.Compiled);

		private class LazyFileInfo
		{
			private string _filePath;
			private DateTime? _lastWriteTime;
			private string _extension;

			public LazyFileInfo(string filePath)
			{
				_filePath = filePath;
			}

			public DateTime LastWriteTime
			{
				get
				{
					if( _lastWriteTime.HasValue == false )
						_lastWriteTime = File.GetLastWriteTime(_filePath);
					return _lastWriteTime.Value;
				}
			}

			public string Extension
			{
				get
				{
					if( _extension == null )
						_extension = Path.GetExtension(_filePath);
					return _extension;
				}
			}
		}

		static StaticFileHandler()
		{
			string configValue = ConfigurationManager.AppSettings["MapExt:StaticFileHandler-CacheDuration"];

			if( string.IsNullOrEmpty(configValue) == false ) {

				int duration = s_DefaultDuration;
				if( int.TryParse(configValue, out duration) )
					s_CacheDuration = duration;
				else {
					// 此时的格式应该是：js:100;css:100;png:10000;jpg:10000;*:200
					Hashtable table = ParseConfig(configValue, out duration);

					if( table != null && table.Count > 0 )
						s_durationTable = Hashtable.Synchronized(table);

					if( duration > 0 )
						s_CacheDuration = duration;
				}
			}

			// 确保有一个有效的缓存时间
			if( s_CacheDuration <= 0 )
				s_CacheDuration = s_DefaultDuration;
		}

		public void ProcessRequest(HttpContext context)
		{
			string filePath = context.Request.PhysicalPath;
			bool isCssFile = filePath.EndsWith(".css", StringComparison.OrdinalIgnoreCase);

			if (File.Exists(filePath) == false){
				throw new HttpException(404, string.Format("文件{0}不存在。", filePath));
			}

			LazyFileInfo fileinfo = new LazyFileInfo(filePath);

			// 设置输出缓存头
			context.Response.Cache.SetCacheability(HttpCacheability.Public);

			// 如果请求的URL包含查询字符串，就认为是包含了版本参数，此时设置缓存时间为【一年】
			if( isCssFile == false 
				&& context.Request.QueryString.Count > 0 ) {
				context.Response.AppendHeader("X-StaticFileHandler", "1year");
				context.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(24 * 60 * 60 * 365));
				context.Response.Cache.SetExpires(DateTime.Now.AddYears(1));
			}
			else {
				int duration = GetDuration(fileinfo);
				context.Response.AppendHeader("X-StaticFileHandler", duration.ToString());
				context.Response.Cache.SetExpires(DateTime.Now.AddSeconds(duration));
				//context.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(duration));
				// 上面的代码不起作用，只能用下面的方法来处理了。
				context.Response.Cache.AppendCacheExtension("max-age=" + duration.ToString());
			}

			// 设置响应内容标头
			string contentType = (string)s_mineTable[fileinfo.Extension];
			if( contentType == null ) {
				contentType = GetMimeType(fileinfo);
				s_mineTable[fileinfo.Extension] = contentType;
			}

			context.Response.ContentType = contentType;

			if( isCssFile )
				OutputCssFile(context);
			else
				// 输出文件内容
				context.Response.TransmitFile(filePath);
		}

		private string GetMimeType(LazyFileInfo file)
		{
			string mimeType = "application/octet-stream";
			if( string.IsNullOrEmpty(file.Extension) )
				return mimeType;

			using( RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(file.Extension.ToLower()) ) {
				if( regKey != null ) {
					object regValue = regKey.GetValue("Content Type");
					if( regValue != null )
						mimeType = regValue.ToString();
				}
			}
			return mimeType;
		}



		private static Hashtable ParseConfig(string text, out int defaultDuration)
		{
			// 此时的格式应该是：js:100;css:100;png:10000;jpg:10000;*:200

			defaultDuration = s_DefaultDuration;
			List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();

			string[] pairs = text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach( string pair in pairs ) {
				string pp = pair.Trim();

				string[] kv = pp.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if( kv.Length != 2 )
					throw new ConfigurationErrorsException("无效的配置：StaticFileHandler-CacheDuration");

				int duration = 0;
				int.TryParse(kv[1], out duration);
				if( duration <= 0 )
					throw new ConfigurationErrorsException("无效的配置：StaticFileHandler-CacheDuration");

				if( string.IsNullOrEmpty(kv[0]) || kv[0] == "." )
					throw new ConfigurationErrorsException("无效的配置：StaticFileHandler-CacheDuration");

				if( kv[0] == "*" ) {
					defaultDuration = duration;
				}
				else {
					string key = kv[0].StartsWith(".") ? kv[0] : "." + kv[0];

					if( list.FindIndex(x => string.Compare(key, x.Key, StringComparison.OrdinalIgnoreCase) == 0) >= 0 )
						throw new ConfigurationErrorsException("无效的配置：StaticFileHandler-CacheDuration");

					list.Add(new KeyValuePair<string, int>(key, duration));
				}
			}

			if( list.Count == 0 )
				return null;


			Hashtable table = new Hashtable(10, StringComparer.OrdinalIgnoreCase);

			foreach( KeyValuePair<string, int> kv in list )
				table[kv.Key] = kv.Value;

			return table;
		}


		private int GetDuration(LazyFileInfo file)
		{
			if( s_durationTable == null )
				return s_CacheDuration;

			object val = s_durationTable[file.Extension];
			if( val == null )
				return s_CacheDuration;

			return (int)val;
		}

		private void OutputCssFile(HttpContext context)
		{
			// 1. 先读出文件内容。注意这里使用UTF-8编码
			// 2. 用正则表达式搜索所有的引用文件
			// 3. 循环匹配结果，
			// 4. 对于匹配之外的内容，直接写入StringBuilder实例，
			// 5. 如果是文件，则计算版本号，再一起写入到StringBuilder实例
			// 6. 最后，StringBuilder实例包含的内容就是处理后的结果。


			string filePath = context.Request.PhysicalPath;

			string key = string.Format("MapExt_cssFile_{0}", filePath);

			object objFile = HttpRuntime.Cache.Get(key);

			if( objFile != null ) {
				context.Response.Write(objFile.ToString());
				return;
			}

			string text = null;

			using( FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read) ) {
				using( StreamReader sr = new StreamReader(fs, Encoding.Default) ) {
					text = sr.ReadToEnd();
				}
			}

			List<string> dependFiles = new List<string>();
			dependFiles.Add(filePath);

			MatchCollection matches = s_CssBackgroundImageRegex.Matches(text);
			if( matches != null && matches.Count > 0 ) {
				int lastIndex = 0;
				StringBuilder sb = new StringBuilder(text.Length * 2);

				foreach( Match m in matches ) {
					Group g = m.Groups["file"];
					if( g.Success ) {

						sb.Append(text.Substring(lastIndex, g.Index - lastIndex));

						string url = g.Value;

						url = url.TrimStart(new char[] { ' ', '\'', '\"' });
						url = url.TrimEnd(new char[] {' ', '\'', '\"' });

						sb.Append(url);

						lastIndex = g.Index + g.Length;

						string fileFullPath = HttpRuntime.AppDomainAppPath.TrimEnd('\\') + url.Replace("/", "\\");
						if( File.Exists(fileFullPath) ) {
							dependFiles.Add(fileFullPath);

							string version = File.GetLastWriteTimeUtc(fileFullPath).Ticks.ToString();
							sb.Append("?_t=").Append(version);
						}
					}
				}

				if( lastIndex > 0 && lastIndex < text.Length )
					sb.Append(text.Substring(lastIndex));

				text = sb.ToString();
			}

			HttpRuntime.Cache.Insert(key, text, new CacheDependency(dependFiles.ToArray()),
				Cache.NoAbsoluteExpiration,
				Cache.NoSlidingExpiration);

			context.Response.Write(text);
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}
}
