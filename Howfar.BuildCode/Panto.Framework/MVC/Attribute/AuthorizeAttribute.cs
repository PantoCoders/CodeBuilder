using Panto.Framework.MVC.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework.MVC {
    /// <summary>
    /// 用于验证用户身份的修饰属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeAttribute : Attribute {
        private string _user;
        private string[] _users;
        private string _role;
        private string[] _roles;



        /// <summary>
        /// 允许访问的用户列表，用逗号分隔。
        /// </summary>
        public string Users {
            get { return _user; }
            set {
                _user = value;
                _users = value.SplitTrim(StringExtensions.CommaSeparatorArray);
            }
        }

        /// <summary>
        /// 允许访问的角色列表，用逗号分隔。
        /// </summary>
        public string Roles {
            get { return _role; }
            set {
                _role = value;
                _roles = value.SplitTrim(StringExtensions.CommaSeparatorArray);
            }
        }


        public virtual bool AuthenticateRequest(HttpContext context) {
            if (context.Request.IsAuthenticated == false)
                return false;

            if (_users != null &&
                _users.Contains(context.User.Identity.Name, StringComparer.OrdinalIgnoreCase) == false)
                return false;

            if (_roles != null && _roles.Any(context.User.IsInRole) == false)
                return false;

            return true;
        }

        /// <summary>
        /// 获取当前执行的Action信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public BehaviorActionInfo GetActionInfo(HttpContext context) {
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
            } else {
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
    /// <summary>
    /// 记录用户行为的Action实体
    /// </summary>
    public class BehaviorActionInfo {

        public BehaviorActionInfo()
        {
            this.ActionName = string.Empty;
            this.ControllerName = string.Empty;
            this.Parameters = new List<object>();
        }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public List<object> Parameters { get; set; }
    }
}
