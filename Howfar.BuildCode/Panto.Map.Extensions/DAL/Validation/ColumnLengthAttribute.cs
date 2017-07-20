using System;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 字段长度
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnLengthAttribute : ValidAttribute, IValid
    {
        #region 属性
        /// <summary>
        /// 最小长度
        /// </summary>
        public Int64 MinLength
        { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public Int64 MaxLength
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
        /// <param name="minLength">最小长度</param>
        /// <param name="maxLength">最大长度</param>
        public ColumnLengthAttribute(String message, Int64 minLength, Int64 maxLength)
            : this(message, null)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="func">校验方法</param>
        public ColumnLengthAttribute(String message, Func<Object, Boolean> func)
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
            return value != null && value.ToString().Length >= MinLength && value.ToString().Length <= MaxLength;
        }

        /// <summary>
        /// 是否满足要求
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        public Boolean Valid(Object value)
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
