using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// BLL工厂
    /// </summary>
    public class BLLFactory
    {
        /// <summary>
        /// 缓存
        /// </summary>
        private static Hashtable cache;
        /// <summary>
        /// 锁住
        /// </summary>
        private static object obj;

        /// <summary>
        /// 私有化构造函数
        /// </summary>
        private BLLFactory() { }

        /// <summary>
        /// 初始化
        /// </summary>
        static BLLFactory()
        {
            cache = Hashtable.Synchronized(new Hashtable());
            obj = new object();
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetInstanceOf<T>()
        {
            lock (obj)
            {
                if (cache.Contains(typeof(T)))
                {
                    return (T)cache[typeof(T)];
                }
                else
                {
                    var instance = Activator.CreateInstance<T>();
                    cache.Add(typeof(T), instance);
                    return instance;
                }
            }
        }
    }
}
