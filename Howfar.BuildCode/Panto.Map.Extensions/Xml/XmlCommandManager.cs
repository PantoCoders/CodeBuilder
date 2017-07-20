using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;
using System.Web.Caching;
using System.Web;
using System.Data.Common;


namespace Panto.Map.Extensions.Xml
{
	/// <summary>
	/// 用于维护配置文件中数据库访问命令的管理类
	/// </summary>
	public static class XmlCommandManager
	{
		private static readonly string CacheKey = Guid.NewGuid().ToString();
		private static System.Exception s_ExceptionOnLoad = null;
		private static Dictionary<string, XmlCommandItem> s_dict = null;


		/// <summary>
		/// 当前运行环境是否为测试环境（非ASP.NET环境）
		/// </summary>
		private static readonly bool IsAspnetEnvironment = (HttpRuntime.AppDomainAppId != null);


		/// <summary>
		/// <para>从指定的目录中加载全部的用于数据访问命令。</para>
		/// <para>说明：1. 这个方法只需要在程序初始化调用一次就够了。</para>
		/// <para>       2. 如果是一个ASP.NET程序，CommandManager还会负责监视此目录，如果文件有更新，会自动重新加载。</para>
		/// </summary>
		/// <param name="directoryPath">包含数据访问命令的目录。不加载子目录，仅加载扩展名为 .config 的文件。</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void LoadCommnads(string directoryPath)
		{
			if( s_dict != null && IsAspnetEnvironment )		// 不要删除这个判断检查，因为后面会监视这个目录。
				throw new InvalidOperationException("不允许重复调用这个方法。");

			if( Directory.Exists(directoryPath) == false )
				throw new DirectoryNotFoundException(string.Format("目录 {0} 不存在。", directoryPath));

			System.Exception exception = null;
			s_dict = LoadCommandsInternal(directoryPath, out exception);

			if( exception != null )
				s_ExceptionOnLoad = exception;

			if( s_ExceptionOnLoad != null )
				throw s_ExceptionOnLoad;
		}

		internal static void AddValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			try {
				dict.Add(key, value);
			}
			catch( ArgumentException ex ) {
				throw new ArgumentException(string.Format("往集合中插入元素时发生了异常，当前Key={0}", key), ex);
			}
		}
		
		private static Dictionary<string, XmlCommandItem> LoadCommandsInternal(string directoryPath, out System.Exception exception)
		{
			exception = null;
			Dictionary<string, XmlCommandItem> dict = null;

			try {
				string[] files = Directory.GetFiles(directoryPath, "*.config", SearchOption.AllDirectories);
				if( files.Length > 0 ) {
					dict = new Dictionary<string, XmlCommandItem>(1024 * 2);

					foreach( string file in files ) {
						List<XmlCommandItem> list = XmlHelper.XmlDeserializeFromFile<List<XmlCommandItem>>(file, Encoding.UTF8);
						list.ForEach(x => dict.AddValue(x.CommandName, x));
					}
				}
			}
			catch( System.Exception ex ) {
				exception = ex;
				dict = null;
			}

			if( IsAspnetEnvironment ) {
				// 如果程序运行在ASP.NET环境中，
				// 注册缓存移除通知，以便在用户修改了配置文件后自动重新加载。

				// 参考：细说 ASP.NET Cache 及其高级用法
				//	      http://www.cnblogs.com/fish-li/archive/2011/12/27/2304063.html
				CacheDependency dep = new CacheDependency(directoryPath);
				HttpRuntime.Cache.Insert(CacheKey, directoryPath, dep,
					Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, CacheRemovedCallback);
			}
			return dict;
		}

		private static void CacheRemovedCallback(string key, object value, CacheItemRemovedReason reason)
		{
			System.Exception exception = null;
			string directoryPath = (string)value;

			for( int i = 0; i < 5; i++ ) {
				// 由于事件发生时，文件可能还没有完全关闭，所以只好让程序稍等。
				System.Threading.Thread.Sleep(3000);


				// 重新加载配置文件
				Dictionary<string, XmlCommandItem> dict = LoadCommandsInternal(directoryPath, out exception);

				if( exception == null ) {
					try { }
					finally {
						s_dict = dict;
						s_ExceptionOnLoad = null;
					}
					return;
				}
				//else: 有可能是文件还在更新，此时加载了不完整的文件内容，最终会导致反序列化失败。
			}

			if( exception != null )
				s_ExceptionOnLoad = exception;
		}


		
		/// <summary>
		/// 根据配置文件中的命令名获取对应的命令对象。
		/// </summary>
		/// <param name="name">命令名称，它应该能对应一个XmlCommand</param>
		/// <returns>如果找到符合名称的XmlCommand，则返回它，否则返回null</returns>
		public static XmlCommandItem GetCommand(string name)
		{
			if( s_ExceptionOnLoad != null )
				throw s_ExceptionOnLoad;

			if( s_dict == null )
				return null;

			XmlCommandItem command;
			if( s_dict.TryGetValue(name, out command) )
				return command;

			return null;
		}


	}
}
