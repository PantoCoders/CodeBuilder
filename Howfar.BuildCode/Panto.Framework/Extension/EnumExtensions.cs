using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace Panto.Framework
{
    /// <summary>
    /// 枚举扩展方法
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取该枚举值的说明
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            if (value == null) {
                return "";
            }
            List<EnumEntity> result = EnumHelper.GetEnumEntityList(value.GetType());
            return result.FirstOrDefault(t => t.Key == value.ToString()).Description;

        }
    }
}
