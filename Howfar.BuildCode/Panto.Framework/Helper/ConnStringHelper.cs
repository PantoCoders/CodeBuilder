using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using System.Web;
using Panto.Framework.Exceptions;
using System.Configuration;

namespace Panto.Framework
{
    /// <summary>
    /// 连接字符串对象
    /// </summary>
    public class ConnStringHelper
    {
        /// <summary>
        /// 连接字符串缓存表
        /// </summary>
        private static Hashtable CSHashtable = Hashtable.Synchronized(new Hashtable(4096, StringComparer.OrdinalIgnoreCase));

        /// <summary>
        /// 初始化连接字符串缓存表
        /// </summary>
        public static void Initializer()
        {
            // 获取根节点
            var root = Registry.LocalMachine.OpenSubKey("SOFTWARE\\mysoft");

            // 如果是asp.net环境
            if (HttpRuntime.AppDomainAppId != null)
            {
                if (WebConfigurationManager.ConnectionStrings.Count <= 0)
                {
                    throw new MyException("Web.Config的ConnectionString节点没有配置任何的连接信息");
                }

                LoadASPNETConnString(root);
            }
            else
            {
                LoadAllConnString(root);
            }
        }

        /// <summary>
        /// 重新初始化连接字符串
        /// </summary>
        public static void ReLoad()
        {
            CSHashtable.Clear();
            Initializer();
        }

        /// <summary>
        /// 获取所有连接字符串名称数组
        /// </summary>
        /// <returns>所有连接字符串名称数组</returns>
        public static object[] GetAllConnNames()
        {
            ArrayList list = new ArrayList(CSHashtable.Keys);
            return list.ToArray();
        }

        /// <summary>
        /// 缓存更新锁
        /// </summary>
        private static object _lock = new object();
        /// <summary>
        /// 获取数据库连接实体
        /// </summary>
        /// <param name="key">连接字符串的key</param>
        /// <returns>数据库连接实体</returns>
        public static ConnString GetConn(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!CSHashtable.ContainsKey(key))
            {
                lock (_lock)
                {
                    if (CSHashtable.ContainsKey(key)) return (CSHashtable[key] as ConnString);
                    //从缓存中加载不到，则重新读取一次改节点
                    // 获取根节点
                    var root = Registry.LocalMachine.OpenSubKey("SOFTWARE\\mysoft");
                    var conn = GetSubKeyConn(root, key);
                    if (conn == null) return null;
                    //如果从注册表中找到了配置的数据库信息，则读取并加入缓存
                    CSHashtable.Add(key, conn);
                }
            }
            return (CSHashtable[key] as ConnString);
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="key">连接字符串的key</param>
        /// <returns>连接字符串</returns>
        public static string GetConnString(string key)
        {
            return GetConn(key).ConnectionString;
        }

        //zhangjm 根据key获取链接串
        public static string GetConnectString(string key)
        {
            return ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }

        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <param name="key">连接字符串的key</param>
        /// <returns>数据库名称</returns>
        public static string GetDBName(string key)
        {
            return GetConn(key).DBName;
        }

        /// <summary>
        /// 添加连接字符串进缓存表
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void AddConnString(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            if (CSHashtable.ContainsKey(key))
            {
                throw new MyException(string.Format("{0}已经存在，无法添加", key));
            }

            CSHashtable.Add(key, value);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyEx")]
        static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int sam, out IntPtr phkResult);

        [Flags]
        private enum eRegWow64Options : int
        {
            None = 0x0000,
            KEY_WOW64_64KEY = 0x0100,
            KEY_WOW64_32KEY = 0x0200
        }

        [Flags]
        private enum eRegistryRights : int
        {
            ReadKey = 131097,
            WriteKey = 131078,
        }

        private static RegistryKey OpenSubKey(RegistryKey pParentKey, string pSubKeyName, bool pWriteable, eRegWow64Options pOptions)
        {
            if (pParentKey == null || GetRegistryKeyHandle(pParentKey).Equals(IntPtr.Zero))
            {
                throw new Exception("OpenSubKey: Parent key is not open");
            }
            eRegistryRights Rights = eRegistryRights.ReadKey;
            if (pWriteable)
            {
                Rights = eRegistryRights.WriteKey;
            }
            IntPtr SubKeyHandle;
            Int32 Result = RegOpenKeyEx(GetRegistryKeyHandle(pParentKey), pSubKeyName, 0, (int)Rights | (int)pOptions, out SubKeyHandle);
            if (Result != 0)
            {
                System.ComponentModel.Win32Exception W32ex = new System.ComponentModel.Win32Exception();
                return null;
            }
            return PointerToRegistryKey(SubKeyHandle, pWriteable, false);
        }

        private static IntPtr GetRegistryKeyHandle(RegistryKey pRegisteryKey)
        {
            Type Type = Type.GetType("Microsoft.Win32.RegistryKey");
            FieldInfo Info = Type.GetField("hkey", BindingFlags.NonPublic | BindingFlags.Instance);
            SafeHandle Handle = (SafeHandle)Info.GetValue(pRegisteryKey);
            IntPtr RealHandle = Handle.DangerousGetHandle();
            return Handle.DangerousGetHandle();
        }

        private static RegistryKey PointerToRegistryKey(IntPtr hKey, bool pWritable, bool pOwnsHandle)
        {
            // Create a SafeHandles.SafeRegistryHandle from this pointer - this is a private class            
            BindingFlags privateConstructors = BindingFlags.Instance | BindingFlags.NonPublic;
            Type safeRegistryHandleType = typeof(SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
            Type[] safeRegistryHandleConstructorTypes = new Type[] { typeof(IntPtr), typeof(Boolean) };
            ConstructorInfo safeRegistryHandleConstructor = safeRegistryHandleType.GetConstructor(privateConstructors, null, safeRegistryHandleConstructorTypes, null);
            Object safeHandle = safeRegistryHandleConstructor.Invoke(new Object[] { hKey, pOwnsHandle });
            // Create a new Registry key using the private constructor using the
            // safeHandle - this should then behave like
            // a .NET natively opened handle and disposed of correctly
            Type registryKeyType = typeof(Microsoft.Win32.RegistryKey);
            Type[] registryKeyConstructorTypes = new Type[] { safeRegistryHandleType, typeof(Boolean) };
            ConstructorInfo registryKeyConstructor = registryKeyType.GetConstructor(privateConstructors, null, registryKeyConstructorTypes, null);
            RegistryKey result = (RegistryKey)registryKeyConstructor.Invoke(new Object[] { safeHandle, pWritable });
            return result;
        }

        /// <summary>
        /// 获取指定注册表的key值
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static ConnString GetSubKeyConn(RegistryKey root, string name)
        {
            // 读取子节点信息
            var node = GetSubKey(root, name);

            // 如果读取不到，则跳过
            if (node == null)
            {
                return null;
            }

            // 如果为非连接字符串的配置项，则跳过
            if (!node.GetValueNames().Contains("ServerName") || !node.GetValueNames().Contains("DBName") ||
                    !node.GetValueNames().Contains("UserName") || !node.GetValueNames().Contains("SaPassword"))
                return null;

            var conn = new ConnString()
            {
                ServerName = node.GetValue("ServerName").ToString(),
                DBName = node.GetValue("DBName").ToString(),
                UserName = node.GetValue("UserName").ToString(),
                SaPassword = node.GetValue("SaPassword").ToString()
            };
            return conn;
        }

        /// <summary>
        /// 加载ASP.NET项目连接字符串
        /// </summary>
        /// <param name="root">根节点</param>
        private static void LoadASPNETConnString(RegistryKey root)
        {
            // 遍历web.config中配置过的连接字符串
            for (var i = 0; i < WebConfigurationManager.ConnectionStrings.Count; i++)
            {
                // 如果有提供者的名称，说明此配置是由数据库安装文件提供，不属于人为在web.config中配置的，所以直接跳过，避免出错。
                if (!string.IsNullOrEmpty(WebConfigurationManager.ConnectionStrings[i].ProviderName))
                {
                    continue;
                }

                // 如果连接字符串为空，则跳过。
                if (string.IsNullOrEmpty(WebConfigurationManager.ConnectionStrings[i].ConnectionString))
                {
                    continue;
                }

                // 如果存在相同的Key则直接抛异常
                if (CSHashtable.ContainsKey(WebConfigurationManager.ConnectionStrings[i].Name))
                {
                    throw new MyException("Web.Config中配置了相同Name的连接信息");
                }

                // 读取子节点信息
                var node = GetSubKey(root, WebConfigurationManager.ConnectionStrings[i].ConnectionString);

                // 如果读取不到，则跳过
                if (node == null)
                {
                    continue;
                }

                // 如果为非连接字符串的配置项，则跳过
                if (!node.GetValueNames().Contains("ServerName") || !node.GetValueNames().Contains("DBName") ||
                        !node.GetValueNames().Contains("UserName") || !node.GetValueNames().Contains("SaPassword"))
                    continue;

                var conn = new ConnString()
                {
                    ServerName = node.GetValue("ServerName").ToString(),
                    DBName = node.GetValue("DBName").ToString(),
                    UserName = node.GetValue("UserName").ToString(),
                    SaPassword = node.GetValue("SaPassword").ToString()
                };

                CSHashtable.Add(WebConfigurationManager.ConnectionStrings[i].Name, conn);
            }
        }

        /// <summary>
        /// 加载所有链接字符串
        /// </summary>
        /// <param name="root">根节点</param>
        private static void LoadAllConnString(RegistryKey root)
        {
            if (root == null)
            {
                throw new MyException("注册表LocalMachine\\SOFTWARE\\mysoft节点下没有配置任何的连接信息");
            }

            foreach (var name in root.GetSubKeyNames())
            {
                // 读取子节点信息
                var node = GetSubKey(root, name);

                // 排除非连接字符串的配置项
                if (!node.GetValueNames().Contains("ServerName") ||
                        !node.GetValueNames().Contains("DBName") ||
                        !node.GetValueNames().Contains("UserName") ||
                        !node.GetValueNames().Contains("SaPassword"))
                    continue;

                var conn = new ConnString()
                {
                    ServerName = node.GetValue("ServerName").ToString(),
                    DBName = node.GetValue("DBName").ToString(),
                    UserName = node.GetValue("UserName").ToString(),
                    SaPassword = node.GetValue("SaPassword").ToString()
                };

                CSHashtable.Add(name, conn);
            }
        }

        /// <summary>
        /// 获取子节点（先从32位节点读取，读取不到再从64位节点读取）
        /// </summary>
        /// <param name="root">根节点</param>
        /// <param name="name">子节点名称</param>
        /// <returns>子节点</returns>
        private static RegistryKey GetSubKey(RegistryKey root, string name)
        {
            // 如果根节点不为空，则在根节点下读取子节点
            if (root != null)
            {
                var node = root.OpenSubKey(name);
                // 如果获取到了子节点，则直接返回该节点
                if (node != null)
                {
                    return node;
                }
            }
            // 从64位节点读取子节点信息
            return OpenSubKey(Registry.LocalMachine, "SOFTWARE\\mysoft\\" + name, false, eRegWow64Options.KEY_WOW64_64KEY);
        }
    }

    /// <summary>
    /// 连接字符串实体
    /// </summary>
    [Serializable]
    public class ConnString
    {
        /// <summary>
        /// 连接字符串模板
        /// </summary>
        private const string _CString = @"Data Source={0};Initial Catalog={1};User ID={2};Password={3}";

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string SaPassword { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return string.Format(_CString, ServerName, DBName, UserName, SecurityHelper.DecryptPwd(SaPassword));
            }
        }
    }
}
