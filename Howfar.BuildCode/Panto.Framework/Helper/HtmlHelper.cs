using System;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using Panto.Framework.MVC;
using Panto.Framework.Exceptions;
using System.ComponentModel;
using Panto.Map.Extensions.DAL;
using OptimizeReflection;
using System.Linq.Expressions;
using System.Collections;

namespace Panto.Framework
{
    /// <summary>
    /// html助手类
    /// </summary>
    public class HtmlHelper : BaseHtmlHelper
    {
        #region 变量初始化
        /// <summary>
        /// 助手实体
        /// </summary>
        private static HtmlHelper _helper = new HtmlHelper();
        /// <summary>
        /// 系统guid主键
        /// </summary>
        private static Guid _systemGuid = Guid.Empty;
        /// <summary>
        /// 初始化系统GUID
        /// </summary>
        /// <param name="systemGuid"></param>
        public static void InitSystemGuid(Guid systemGuid)
        {
            _systemGuid = systemGuid;
        }
        #endregion

        #region 标签生成
        /// <summary>
        /// 生成Input标签
        /// </summary>
        /// <param name="functionCode">功能点</param>
        /// <param name="actionCode">动作点</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string InputFor(string functionCode, string actionCode, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Input, param, type, _helper.ValidFuncPermission(_systemGuid, functionCode, actionCode));
        }
        /// <summary>
        /// 生成Input标签
        /// </summary>
        /// <param name="validFunc">权限验证委托</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string InputFor(Func<bool> validFunc, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Input, param, type, validFunc());
        }
        /// <summary>
        /// 生成Button标签
        /// </summary>
        /// <param name="functionCode">功能点</param>
        /// <param name="actionCode">动作点</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string ButtonFor(string functionCode, string actionCode, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Button, param, type, _helper.ValidFuncPermission(_systemGuid, functionCode, actionCode));
        }
        /// <summary>
        /// 生成Button标签
        /// </summary>
        /// <param name="validFunc">权限验证委托</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string ButtonFor(Func<bool> validFunc, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Button, param, type, validFunc());
        }
        /// <summary>
        /// 生成Hidden标签
        /// </summary>
        /// <param name="functionCode">功能点</param>
        /// <param name="actionCode">动作点</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string HiddenFor(string functionCode, string actionCode, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Hidden, param, type, _helper.ValidFuncPermission(_systemGuid, functionCode, actionCode));
        }
        /// <summary>
        /// 生成Hidden标签
        /// </summary>
        /// <param name="validFunc">权限验证委托</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string HiddenFor(Func<bool> validFunc, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Hidden, param, type, validFunc());
        }
        /// <summary>
        /// 生成Label标签
        /// </summary>
        /// <param name="functionCode">功能点</param>
        /// <param name="actionCode">动作点</param>
        /// <param name="text">文本内容</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string LabelFor(string functionCode, string actionCode, string text, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Label, param, type, _helper.ValidFuncPermission(_systemGuid, functionCode, actionCode), text);
        }
        /// <summary>
        /// 生成Label标签
        /// </summary>
        /// <param name="validFunc">权限验证委托</param>
        /// <param name="text">文本内容</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string LabelFor(Func<bool> validFunc, string text, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Label, param, type, validFunc(), text);
        }
        /// <summary>
        /// 生成 A(Link)标签
        /// </summary>
        /// <param name="functionCode">功能点</param>
        /// <param name="actionCode">动作点</param>
        /// <param name="text">文本内容</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string LinkFor(string functionCode, string actionCode, string text, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Link, param, type, _helper.ValidFuncPermission(_systemGuid, functionCode, actionCode), text);
        }
        /// <summary>
        /// 生成 A(Link)标签
        /// </summary>
        /// <param name="validFunc">权限验证委托</param>
        /// <param name="text">文本内容</param>
        /// <param name="param">属性对象</param>
        /// <param name="type">控制类型</param>
        /// <returns>返回标签</returns>
        public static string LinkFor(Func<bool> validFunc, string text, object param, ControllType type = ControllType.Default)
        {
            return _helper.GetHtmlTag(TagEnum.Link, param, type, validFunc(), text);
        }


        #endregion



    }

    /// <summary>
    /// html助手类 基类
    /// </summary>
    public class BaseHtmlHelper
    {
        #region 辅助方法
        /// <summary>
        /// 获取html标签内容
        /// </summary>
        /// <param name="tag">标签类型</param>
        /// <param name="param">属性</param>
        /// <param name="type">控制类型</param>
        /// <param name="hasPermission">是否有控制权限</param>
        /// <param name="text">文本[可选]：默认为空</param>
        /// <returns>返回html标签</returns>
        public string GetHtmlTag(TagEnum tag, object param, ControllType type = ControllType.Default, bool hasPermission = true, string text = "")
        {
            if (type.Equals(ControllType.Default) && !hasPermission) return string.Empty;

            var description = tag.GetDescription().Split(';');
            List<string> exceptFileds = null;
            if (description.Length > 1)
            {
                exceptFileds = description[1].Split(',').ToList();
            }
            else if (description.Length == 0)
            {
                return string.Empty;
            }

            if (description[0].Contains(@"{1}"))
            {
                return string.Format(description[0], GetObjectPropertyString(exceptFileds, param, type, hasPermission).Join(" "), text);
            }
            else
            {
                return string.Format(description[0], GetObjectPropertyString(exceptFileds, param, type, hasPermission).Join(" "));
            }
        }

        /// <summary>
        /// 缓存集合
        /// </summary>
        private static Hashtable cache = Hashtable.Synchronized(new Hashtable(1024));
        /// <summary>
        /// 对象锁
        /// </summary>
        private static readonly object ObjLock = new object();
        /// <summary>
        /// 获取属性列表
        /// </summary>
        /// <param name="exceptFileds">需排除，不用生成的属性</param>
        /// <param name="param">属性</param>
        /// <param name="type">控制类型</param>
        /// <param name="hasPermission">是否有权限</param>
        /// <returns>返回属性列表</returns>
        private List<string> GetObjectPropertyString(List<string> exceptFileds, object param,
            ControllType type, bool hasPermission)
        {
            var key = new { ExceptFileds = exceptFileds, Param = param, ControllorType = type, HasPermission = hasPermission };
            if (!cache.ContainsKey(key))
            {
                lock (ObjLock)
                {
                    if (cache.ContainsKey(key))
                    {
                        return cache[key] as List<string>;
                    }
                    var attrList = new List<string>();
                    var properties = param.GetType().GetProperties();

                    if (!hasPermission && type.Equals(ControllType.Disabled))
                    {
                        attrList.Add(string.Format(@"{0}='{1}'", @"disabled", @"disabled"));
                    }

                    foreach (var property in properties)
                    {
                        if (exceptFileds != null && exceptFileds.Select(item => item.ToLower()).Contains(property.Name.ToLower()))
                            continue;
                        var valid = property.Name.ToLower() == @"style" && type.Equals(ControllType.Display) && !hasPermission;
                        attrList.Add(string.Format(@"{0}='{1}'", property.Name, property.FastGetValue(param) + (valid ? @";display:none" : string.Empty)));
                    }
                    if (!properties.Select(property => property.Name.ToLower()).Contains("style"))
                    {
                        attrList.Add((type.Equals(ControllType.Display) && !hasPermission) ? @"style='display:none'" : string.Empty);
                    }
                    cache.Add(key, attrList);
                }
            }
            return cache[key] as List<string>;
        }

        /// <summary>
        /// 验证当前用户是否具有某功能权限
        /// </summary>
        /// <param name="systemGuid">系统GUID</param>
        /// <param name="functionCode">功能编码</param>
        /// <param name="actionCode">动作点编码</param>
        /// <returns>返回true,表示有权限;返回false,表示无权限</returns>
        public bool ValidFuncPermission(Guid systemGuid, string functionCode, string actionCode)
        {
            using (ConnectionScope scope = new ConnectionScope(TransactionMode.Inherits, ConnStringHelper.GetConnString("SystemCore")))
            {
                if (systemGuid.Equals(Guid.Empty))
                {
                    throw new MyException("系统GUID没有初始化");
                }
                var parameters = new
                {
                    //BUGUID = Panto.Framework.Login.UserManager.GetCurrentUser().UserGUID,
                    FuncMenuCode = functionCode,
                    SystemGUID = systemGuid,
                    ActionCode = actionCode
                };
                var count = CPQuery.From(@"SELECT COUNT(*)
                                    FROM dbo.PERM_RoleToUser AS a
                                    INNER JOIN dbo.ROLE_RoleInfo AS b ON a.RoleGUID=b.RoleGUID
                                    INNER JOIN dbo.PERM_FuncPermissionInfo AS c ON a.RoleGUID=c.RoleGUID
                                    INNER JOIN dbo.SYS_ActionInfo AS d ON c.FuncGUID = d.ActionGUID
                                    INNER JOIN dbo.SYS_FuncMenuInfo AS e ON d.FuncMenuGUID = e.FuncMenuGUID
                                    WHERE a.BUGUID = @BUGUID AND e.FuncMenuCode = @FuncMenuCode AND b.SystemGUID =@SystemGUID AND d.ActionCode=@ActionCode", parameters).ExecuteScalar<int>();
                if (count > 0)
                    return true;
                else
                    return false;
            }
        }
        #endregion
    }

    /// <summary>
    /// 标签枚举
    /// </summary>
    public enum TagEnum
    {
        /// <summary>
        /// 输入框
        /// </summary>
        [Description("<input type='input' {0}/>;type")]
        Input,
        /// <summary>
        /// 按钮
        /// </summary>
        [Description("<input type='button' {0}/>;type")]
        Button,
        /// <summary>
        /// 隐藏域
        /// </summary>
        [Description("<input type='hidden' {0}/>;type")]
        Hidden,
        /// <summary>
        /// 标签
        /// </summary>
        [Description("<label {0}>{1}</label>")]
        Label,
        /// <summary>
        /// 超链接
        /// </summary>
        [Description("<a {0}>{1}</a>")]
        Link
    }

    /// <summary>
    /// 控制类型
    /// </summary>
    public enum ControllType
    {
        /// <summary>
        /// 默认类型（无权限，则不生成控件）
        /// </summary>
        Default,
        /// <summary>
        /// display属性控制控件的显示与隐藏
        /// </summary>
        Display,
        /// <summary>
        /// disabled属性控制控件的读写
        /// </summary>
        Disabled
    }
}
