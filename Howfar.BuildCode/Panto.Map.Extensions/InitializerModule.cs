using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;

using Panto.Map.Extensions.DAL;
//using Panto.Map.Extensions.Json;


namespace Panto.Map.Extensions
{
	/// <summary>
	/// 表示初始化HttpModule,用于初始化连接字符串,预编译实体
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description>本类通过反射方式加载Panto.Map.Core.dll程序集中MyDB类中的链接字符串,并初始化</description></item>
	/// <item><description>本类只能使用在Map项目中.不支持在其他项目中调用本类,如果需要在其他项目中使用数据访问层,请使用<see cref="Initializer"/>类</description></item>
	/// </list>
	/// </remarks>
	/// <exception cref="System.IO.FileNotFoundException">Panto.Map.Core.dll文件不存在或Panto.Map.Data.MyDB类不存在</exception>
	/// <example>
	/// <para>下面的代码演示了如何在web.config文件中配置HttpModule</para>
	/// <code>
	///&lt;?xml version="1.0"?&gt;
	///&lt;configuration&gt;
	///	 &lt;system.web&gt;
	///    &lt;httpModules&gt;
	///      &lt;add name="MapExtends" type="Panto.Map.Extensions.InitializerModule,Panto.Map.Extensions" /&gt;
	///    &lt;/httpModules&gt;
	///  &lt;/system.web&gt;
	///  &lt;system.webServer&gt;
	///    &lt;modules&gt;
	///      &lt;remove name="ScriptModule" /&gt;
	///	  &lt;add name="MapExtends" preCondition="managedHandler" type="Panto.Map.Extensions.InitializerModule,Panto.Map.Extensions" /&gt;
	///    &lt;/modules&gt;
	///  &lt;/system.webServer&gt;
	///&lt;/configuration&gt;
	/// </code>
	/// </example>
	public class InitializerModule : IHttpModule
	{

		static InitializerModule()
		{
			InitMyDBAPI();
		}

		private ContentEncodingModule _contentEncodingModule = new ContentEncodingModule();

		private static void InitMyDBAPI()
		{
			string path = Panto.Map.Extensions.CodeDom.BuildManager.BinDirectory + "Panto.Map.Core.dll";
			if( !System.IO.File.Exists(path) )
				throw new System.IO.FileNotFoundException("Panto.Map.Core.dll文件不存在!");

			Assembly assembly = Assembly.LoadFile(path);

			Type typeMyDB = assembly.GetType("Panto.Map.Data.MyDB");
			if( typeMyDB == null ) {
				throw new InvalidProgramException("Panto.Map.Core.dll中未找到MyDB类型!");
			}


			string connectionString = (string)typeMyDB.InvokeMember("GetSqlConnectionString",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, null);

			Type typeUser = assembly.GetType("Panto.Map.Application.Security.User");
			if( typeUser != null ) {

				MethodInfo method = typeUser.GetMethod("CheckUserRight",
					BindingFlags.Static | BindingFlags.Public);

				if( method != null ) {

					Delegate func = Delegate.CreateDelegate(typeof(Func<string, string, string, bool>), method);

					DecouplingUtils.CheckUserRight = (Func<string, string, string, bool>)func;
				}
			}

			//初始化连接字符串
			Initializer.UnSafeInit(connectionString);
		}

		

		/// <summary>
		/// 此方法为Asp.net运行时调用.不支持在代码中直接调用.
		/// </summary>
		/// <param name="app">HttpApplication实例</param>
		public void Init(HttpApplication app)
		{
			app.BeginRequest += new EventHandler(app_BeginRequest);
			app.EndRequest += app_EndRequest;

			_contentEncodingModule.Init(app);
		}

		private static object s_lockObject = new object();
		private static bool s_inited = false;

		void app_BeginRequest(object sender, EventArgs e)
		{
			if( s_inited == false ) {
				lock( s_lockObject ) {
					if( s_inited == false ) {
						TableTrace.InitTrace();
						s_inited = true;
					}
				}
			}
		}

		void app_EndRequest(object sender, EventArgs e)
		{
			ConnectionScope.ForceClose();
		}


		/// <summary>
		/// 此方法为Asp.net运行时调用.不支持在代码中直接调用.
		/// </summary>
		public void Dispose()
		{
		}

	}
}
