using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Web.Compilation;
using System.Text.RegularExpressions;
using System.Web;
using OptimizeReflection;
using System.Xml;

namespace Panto.Framework.MVC
{
	// 说明：
	// 1. 对于处理页面请求的Controller类型，这里不缓存，只缓存所包含的Action
	//    用于页面请求的Action的描述中，已包含Controller的描述信息。
	// 2. 对于处理Service请求的Controller以及Action，在初始化时，只查找出Controller，Action采用延迟加载方式
	
	// 原因：
	// 1. 页面请求是在Action中指定的，因此，只能先找到所有能处理页面请求的Action
	// 2. Ajax调用时，可以从URL中解析出要调用的类名与方法名，因此可以在调用时再去查找。


	internal static class ReflectionHelper
	{
		public static readonly Type VoidType = typeof(void);
		
		// 保存PageAction的字典
		private static Dictionary<string, ActionDescription> s_PageActionDict;
		private static RegexActionDescription[] s_RegPageActionArray;

		// 为了快速定位Controller，这里创建了二个字典表，用于不同用途的查找。
		private static Dictionary<string, ControllerDescription> s_ServiceFullNameDict;
		private static Dictionary<string, ControllerDescription> s_ServiceShortNameDict;

		// 保存Service Action的字典
		private static Hashtable s_ServiceActionTable = Hashtable.Synchronized(
												new Hashtable(4096, StringComparer.OrdinalIgnoreCase));

		// 用于从类型查找Action的反射标记
		private static readonly BindingFlags ActionBindingFlags =
			BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

		static ReflectionHelper()
		{
			InitControllers();
		}


		private static BaseActionHandlerFactory[] GetConfigBaseActionHandlerFactory()
		{
			bool useIntegratedPipeline = (bool)typeof(HttpRuntime).InvokeMember("UseIntegratedPipeline",
				BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.NonPublic,
				null, null, null);

			string xpath = useIntegratedPipeline
				? "/configuration/system.webServer/handlers/add"
				: "/configuration/system.web/httpHandlers/add";

			XmlDocument doc = new XmlDocument();
			doc.Load(HttpContextHelper.AppRootPath + "web.config");
			XmlNodeList nodes = doc.SelectNodes(xpath);

			List<BaseActionHandlerFactory> result = new List<BaseActionHandlerFactory>();

			foreach( XmlNode node in nodes ) {
				if( useIntegratedPipeline ) {
					XmlAttribute preCondition = node.Attributes["preCondition"];
					if( preCondition == null || string.IsNullOrEmpty(preCondition.Value) || preCondition.Value.IndexOf("integratedMode") < 0 )
						continue;
				}

				XmlAttribute typeAttr = node.Attributes["type"];
				if( typeAttr == null )
					continue;

				string typeName = typeAttr.Value;
				if( string.IsNullOrEmpty(typeName) )
					continue;

				Type t = System.Web.Compilation.BuildManager.GetType(typeName, true, false);				
				if( t.IsSubclassOf(typeof(BaseActionHandlerFactory)) )
					result.Add((BaseActionHandlerFactory)Activator.CreateInstance(t));
			}

			return result.ToArray();
		}


		/// <summary>
		/// 加载所有的Controller
		/// </summary>
		private static void InitControllers()
		{
			BaseActionHandlerFactory[] baseActionHandlerFactoryList = GetConfigBaseActionHandlerFactory();

			List<ControllerDescription> serviceControllerList = new List<ControllerDescription>(1024);
			var pageControllerList = new List<ControllerDescription>(1024);

			ICollection assemblies = BuildManager.GetReferencedAssemblies();
			foreach( Assembly assembly in assemblies ) {
				// 过滤以【System】开头的程序集，加快速度
				if( assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase) )
					continue;

				foreach( Type t in assembly.GetExportedTypes() ) {
					if( t.IsClass == false )
						continue;

					if( t.Name.EndsWith("Controller") )
						pageControllerList.Add(new ControllerDescription(t));

					for( int i = 0; i < baseActionHandlerFactoryList.Length; i++ )
						if( baseActionHandlerFactoryList[i].TypeIsService(t) ) {
							serviceControllerList.Add(new ControllerDescription(t));
							break;
						}

				}
			}

			// 用于Ajax or Service 调用的Action信息则采用延迟加载的方式。
			
			s_ServiceFullNameDict = serviceControllerList.ToDictionary(x => x.ControllerType.FullName, StringComparer.OrdinalIgnoreCase);
			
			s_ServiceShortNameDict = new Dictionary<string, ControllerDescription>(s_ServiceFullNameDict.Count, StringComparer.OrdinalIgnoreCase);
			
			foreach( ControllerDescription description in serviceControllerList ) {
				try {
					s_ServiceShortNameDict.Add(description.ControllerType.Name, description);
				}
				catch( ArgumentException ) {
					// 如果遇到已存在的KEY，把原先存放的项也设置为 null，便于查找时返回 null
					// 也就是说：短名不能重复，如果重复则不返回任何一个。
					s_ServiceShortNameDict[description.ControllerType.Name] = null;
				}
			}
			
			

			// 提前加载Page Controller中的所有Action方法
			s_PageActionDict = new Dictionary<string, ActionDescription>(4096, StringComparer.OrdinalIgnoreCase);

			List<RegexActionDescription> regexActions = new List<RegexActionDescription>();


			foreach( ControllerDescription controller in pageControllerList ) {
				foreach( MethodInfo m in controller.ControllerType.GetMethods(ActionBindingFlags) ) {

					ActionAttribute actionAttr = m.GetMyAttribute<ActionAttribute>();
					if( actionAttr != null ) {
						ActionDescription actionDescription =
								new ActionDescription(m, actionAttr) { PageController = controller };

						PageUrlAttribute[] pageUrlAttrs = m.GetMyAttributes<PageUrlAttribute>();

						foreach( PageUrlAttribute attr in pageUrlAttrs ) {
							if( string.IsNullOrEmpty(attr.Url) == false )
								s_PageActionDict.Add(attr.Url, actionDescription);
						}



						PageRegexUrlAttribute[] regexAttrs = m.GetMyAttributes<PageRegexUrlAttribute>();
						foreach( PageUrlAttribute attr2 in regexAttrs ) {
							if( string.IsNullOrEmpty(attr2.Url) == false ) {
								Regex regex = new Regex(attr2.Url, RegexOptions.Compiled);
								regexActions.Add(new RegexActionDescription{ Regex= regex, ActionDescription = actionDescription});
							}
						}

					}
				}
			}


			if( regexActions.Count > 0 )
				s_RegPageActionArray = regexActions.ToArray();
			
		}

				

		/// <summary>
		/// 根据要调用的controller名返回对应的Controller 
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>
		private static ControllerDescription GetServiceController(string controller)
		{
			if( string.IsNullOrEmpty(controller) )
				throw new ArgumentNullException("controller");

			ControllerDescription description = null;

			// 查找类型的方式：如果有点号，则按全名来查找(包含命名空间)，否则只看类型名称。
			// 如果有多个类型的名称相同，必须用完整的命名空间来调用，否则不能定位Controller

			if( controller.IndexOf('.') > 0 )
				s_ServiceFullNameDict.TryGetValue(controller, out description);
			else
				s_ServiceShortNameDict.TryGetValue(controller, out description);

			return description;
		}

		/// <summary>
		/// 根据要调用的方法名返回对应的 Action 
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="action"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		private static ActionDescription GetServiceAction(Type controller, string action, HttpRequest request)
		{
			if( controller == null )
				throw new ArgumentNullException("controller");
			if( string.IsNullOrEmpty(action) )
				throw new ArgumentNullException("action");

			// 首先尝试从缓存中读取
			string key = request.HttpMethod + "#" + controller.FullName + "@" + action;
			ActionDescription mi = (ActionDescription)s_ServiceActionTable[key];

			if( mi == null ) {
				bool saveToCache = true;
				
				MethodInfo method = FindAction(action, controller, request);

				if( method == null ) {
					// 如果Action的名字是submit并且是POST提交，则需要自动寻找Action
					// 例如：多个提交都采用一样的方式：POST /Ajax/Product/submit
					if( action.IsSame("submit") && request.HttpMethod.IsSame("POST") ) {
						// 自动寻找Action
						method = FindSubmitAction(controller, request);
						saveToCache = false;
					}
				}

				if( method == null )
					return null;

				var attr = method.GetMyAttribute<ActionAttribute>();
				mi = new ActionDescription(method, attr);

				if( saveToCache ) 
					s_ServiceActionTable[key] = mi;
			}

			return mi;
		}

		
		private static MethodInfo FindAction(string action, Type controller, HttpRequest request)
		{
			foreach( MethodInfo method in controller.GetMethods() ) {
				if( method.Name.IsSame(action) ) {
					if( MethodActionIsMatch(method, request) )
						return method;
				}
			}
			return null;
		}

		private static MethodInfo FindSubmitAction(Type controller, HttpRequest request)
		{
			string[] keys = request.Form.AllKeys;

			foreach( MethodInfo method in controller.GetMethods() ) {
				string key = keys.FirstOrDefault(x => method.Name.IsSame(x));
				if( key != null && MethodActionIsMatch(method, request) )
					return method;
			}

			return null;
		}

		private static bool MethodActionIsMatch(MethodInfo method, HttpRequest request)
		{
			var attr = method.GetMyAttribute<ActionAttribute>();
			if( attr != null ) {
				if( attr.AllowExecute(request.HttpMethod) )
					return true;
			}
			return false;
		}

		/// <summary>
		/// 根据一个Action的调用信息（类名与方法名），返回内部表示的调用信息。
		/// </summary>
		/// <param name="pair"></param>
		/// <returns></returns>
		public static InvokeInfo GetActionInvokeInfo(ControllerActionPair pair, HttpRequest request)
		{
			if( pair == null )
				throw new ArgumentNullException("pair");

			InvokeInfo vkInfo = new InvokeInfo();

			vkInfo.Controller = GetServiceController(pair.Controller);
			if( vkInfo.Controller == null )
				return null;

			vkInfo.Action = GetServiceAction(vkInfo.Controller.ControllerType, pair.Action, request);
			if( vkInfo.Action == null )
				return null;

			if( vkInfo.Action.MethodInfo.IsStatic == false )
				//vkInfo.Instance = Activator.CreateInstance(vkInfo.Controller.ControllerType);
				vkInfo.Instance = vkInfo.Controller.ControllerType.FastNew();
			
			return vkInfo;
		}

		/// <summary>
		/// 根据一个页面请求路径，返回内部表示的调用信息。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static InvokeInfo GetActionInvokeInfo(string url)
		{
			if( string.IsNullOrEmpty(url) )
				throw new ArgumentNullException("url");

			MatchActionDescription md = null;
			ActionDescription action = null;

			// 先直接用URL从字典中查找匹配项。
			if( s_PageActionDict.TryGetValue(url, out action) == false ) {

				// 如果不能直接匹配URL，就用正则表达式再逐个匹配。
				md = GetActionUsingRegex(url);
				if( md == null )
					return null;
				else
					action = md.ActionDescription;
			}

			InvokeInfo vkInfo = new InvokeInfo();
			vkInfo.Controller = action.PageController;
			vkInfo.Action = action;

			if( md != null )
				vkInfo.RegexMatch = md.Match;

			if( vkInfo.Action.MethodInfo.IsStatic == false )
				//vkInfo.Instance = Activator.CreateInstance(vkInfo.Controller.ControllerType);
				vkInfo.Instance = vkInfo.Controller.ControllerType.FastNew();

			return vkInfo;
		}


		private static MatchActionDescription GetActionUsingRegex(string url)
		{
			if( s_RegPageActionArray == null || s_RegPageActionArray.Length == 0 )
				return null;

			Match match = null;

			for( int i = 0; i < s_RegPageActionArray.Length; i++ ) {
				RegexActionDescription rd = s_RegPageActionArray[i];
				match = rd.Regex.Match(url);
				if( match.Success )
					return new MatchActionDescription { Match = match, ActionDescription = rd.ActionDescription };
			}

			return null;
		}


		private static Hashtable s_modelTable = Hashtable.Synchronized(
											new Hashtable(4096, StringComparer.OrdinalIgnoreCase));

		/// <summary>
		/// 返回一个实体类型的描述信息（全部属性及字段）。
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ModelDescription GetModelDescription(Type type)
		{
			if( type == null )
				throw new ArgumentNullException("type");
			
			string key = type.FullName;
			ModelDescription mm = (ModelDescription)s_modelTable[key];

			if( mm == null ) {
				List<DataMember> list = new List<DataMember>();

				(from p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				 select new PropertyMember(p)).ToList().ForEach(x=>list.Add(x));

				(from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
				 select new FieldMember(f)).ToList().ForEach(x => list.Add(x));

				mm = new ModelDescription { Fields = list.ToArray() };
				s_modelTable[key] = mm;
			}
			return mm;
		}

		/// <summary>
		/// 返回一个实体类型的指定名称的数据成员
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static DataMember GetMemberByName(Type type, string name, bool ifNotFoundThrowException)
		{
			ModelDescription description = GetModelDescription(type);
			foreach( DataMember member in description.Fields )
				if( member.Name == name )
					return member;

			if( ifNotFoundThrowException )
				throw new ArgumentOutOfRangeException(
						string.Format("指定的成员 {0} 在类型 {1} 中并不存在。", name, type.ToString()));

			return null;
		}


		

	}
}
