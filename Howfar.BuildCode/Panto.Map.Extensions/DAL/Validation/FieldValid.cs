using System.Collections.Generic;
using System.Reflection;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 字段验证集
    /// </summary>
    public class FieldValid
    {
        /// <summary>
        /// 字段信息
        /// </summary>
        public PropertyInfo Property { get; set; }
        /// <summary>
        /// 字段所有验证
        /// </summary>
        public List<IValid> List { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="property">字段信息</param>
        /// <param name="list">字段所有验证</param>
        public FieldValid(PropertyInfo property, List<IValid> list)
        {
            Property = property;
            List = list;
        }
    }
}
