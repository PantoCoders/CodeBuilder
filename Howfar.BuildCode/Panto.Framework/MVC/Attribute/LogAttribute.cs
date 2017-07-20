using Panto.Framework.MVC.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework.MVC
{
    /// <summary>
    /// 行为拦截日志特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LogAttribute : Attribute
    {

        public virtual void Log(HttpContext context)
        {

        }


        /// <summary>
        /// 获取当前执行的Action信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public BehaviorActionInfo GetActionInfo(HttpContext context)
        {
            bool isAjaxRequest = string.Compare(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) == 0;
            BehaviorActionInfo action = new BehaviorActionInfo();
            InvokeInfo vkInfo = null;
            if (isAjaxRequest) {
                string vPath = UrlHelper.GetRealVirtualPath(context, context.Request.Path);

                ControllerActionPair pair = new AjaxHandlerFactory().ParseUrl(context, vPath);
                if (pair == null)
                    ExceptionHelper.Throw404Exception(context);

                // 获取内部表示的调用信息
                vkInfo = ReflectionHelper.GetActionInvokeInfo(pair, context.Request);
            }
            else {
                string requestPath = context.Request.Path;
                string vPath = UrlHelper.GetRealVirtualPath(context, requestPath);
                // 尝试根据请求路径获取Action
                vkInfo = ReflectionHelper.GetActionInvokeInfo(vPath);
            }
            if (vkInfo != null) {
                action.ActionName = vkInfo.Action.MethodInfo.Name;
                action.ControllerName = vkInfo.Controller.ControllerType.Name;

                object[] Values = ActionParametersProviderFactory.CreateActionParametersProvider(context).GetParameters(context, vkInfo.Action);


                for (int i = 0; i < vkInfo.Action.Parameters.Length; i++) {
                    action.Parameters.Add(new { Name = vkInfo.Action.Parameters[i].Name, Value = Values[i] });
                }

            }
            return action;
        }
    }
}
