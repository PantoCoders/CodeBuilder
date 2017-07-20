using Panto.Map.Extensions.DAL;
using System;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 必填字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnRequiredAttribute : ValidAttribute, IValid
    {

        #region 属性
        /// <summary>
        /// 默认值
        /// </summary>
        public Object OriginValue
        { get; set; }

        /// <summary>
        /// 校验方法
        /// </summary>
        private Func<Object, Boolean> valid;

        /// <summary>
        /// 提示信息
        /// </summary>
        public String Message { get; set; }

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="type">属性类型</param>
        public ColumnRequiredAttribute(String message, Type type)
        {
            OriginValue = type.IsValueType ? Activator.CreateInstance(type) : null;
            Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="func">校验方法</param>
        public ColumnRequiredAttribute(String message, Func<Object, Boolean> func)
        {
            Message = message;
            valid += func;
        }

        /// <summary>
        /// 默认校验方法
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        private Boolean DefaultValid(Object value)
        {
            return !(value == null || String.IsNullOrEmpty(value.ToString()) || (OriginValue != null && value.ToString() == OriginValue.ToString()));
        }

        /// <summary>
        /// 校验是否满足要求
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        Boolean IValid.Valid(Object value)
        {
            if (valid == null)
            {
                valid += DefaultValid;

            }
            return valid(value);
        }
        #endregion

    }
}
