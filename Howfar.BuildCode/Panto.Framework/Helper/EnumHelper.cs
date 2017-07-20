using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections;
namespace Panto.Framework
{
    /// <summary>
    /// 枚举助手
    /// </summary>
    public class EnumHelper
    {
        private static Hashtable cache = Hashtable.Synchronized(new Hashtable(1024));
        private static readonly object ObjLock = new object();

        /// <summary>
        /// 获取枚举成员列表
        /// </summary>
        /// <typeparam name="T">枚举</typeparam>
        /// <returns>枚举成员列表</returns>
        public static List<EnumEntity> GetEnumEntityList<T>()
        {
            return GetEnumEntityList(typeof(T));
        }

        /// <summary>
        /// 获取指定关键字枚举值
        /// </summary>
        /// <typeparam name="T">枚举</typeparam>
        /// <param name="Description">关键字</param>
        /// <param name="IsCase">是否区分大小写(默认不区分)</param>
        /// <returns></returns>
        public static T GetEnumValue<T>(string Description, bool IsCase = false)
        {
            if (string.IsNullOrEmpty(Description)) {
                throw new System.ArgumentNullException("Description");
            }
            List<EnumEntity> tmpResult = GetEnumEntityList(typeof(T));
            EnumEntity tmp;
            if (!IsCase) {
                tmp = tmpResult.FirstOrDefault(t => t.Description.ToLower().Contains(Description));
            }
            else {
                tmp = tmpResult.FirstOrDefault(t => t.Description.Contains(Description));
            }
            if (tmp == null) {
                throw new Panto.Framework.Exceptions.MyException("没有找到这个关键字" + Description + "枚举");
            }
            return (T)(tmp.Value as object);
        }

        /// <summary>
        /// 获取枚举成员列表
        /// </summary>
        /// <param name="type">枚举值的类型</param>
        /// <returns>枚举成员列表</returns>
        internal static List<EnumEntity> GetEnumEntityList(Type type)
        {
            if (!cache.ContainsKey(type)) {
                lock (ObjLock) {
                    if (cache.ContainsKey(type)) {
                        return cache[type] as List<EnumEntity>;
                    }
                    var result = new List<EnumEntity>();
                    foreach (var bt in Enum.GetValues(type)) {
                        EnumEntity field = new EnumEntity() { Key = bt.ToString(), Value = (int)bt };
                        object[] attr = type.GetField(field.Key).GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (attr != null && attr.Length > 0) {
                            field.Description = (attr.FirstOrDefault() as DescriptionAttribute).Description;
                        }
                        result.Add(field);
                    }
                    cache.Add(type, result);
                }
            }

            return cache[type] as List<EnumEntity>;
        }
    }
    /// <summary>
    /// 枚举成员信息实体
    /// </summary>
    public class EnumEntity
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
    }
}
