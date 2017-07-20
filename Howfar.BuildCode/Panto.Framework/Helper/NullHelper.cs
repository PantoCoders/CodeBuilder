using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panto.Map.Extensions.DAL;
using OptimizeReflection;

namespace Panto.Framework
{
    public class NullHelper
    {
        /// <summary>
        /// 将对象中字符串属性为null的字段，全部填充 string.empty
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertNull<T>(T obj) where T : BaseEntity
        {
            //处理null值
            var properties = obj.GetType().GetProperties();
            foreach (var p in properties)
            {
                if (p.CanWrite && p.CanRead && p.FastGetValue(obj) == null && p.PropertyType == typeof(string))
                {
                    p.FastSetValue(obj, string.Empty);
                }
            }
            return obj;
        }

        /// <summary>
        /// 用CPQuery执行更新前，将实体中为NULL的字段，全部更新为默认值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体</param>
        /// <returns>返回值</returns>
        public static T ConvertNullProprety<T>(T entity) where T : BaseEntity
        {
            var type = entity.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                //如果不是值类型则跳过
                if (!propertyType.IsValueType) continue;

                var attrs = property.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    var tempAttr = attr as DataColumnAttribute;
                    if (tempAttr != null)
                    {
                        var value = property.FastGetValue(entity);
                        if (value == null)
                        {
                            entity.SetPropertyDefaultValue(property.Name);
                        }
                        break;
                    }
                }
            }
            return entity;
        }

        /// <summary>
        /// 处理null值数据
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string ConvertNull(string obj)
        {
            return string.IsNullOrEmpty(obj) ? string.Empty : obj;
        }
    }
}
