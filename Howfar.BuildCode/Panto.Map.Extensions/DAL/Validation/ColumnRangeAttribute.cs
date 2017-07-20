using System;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 字段范围
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnRangeAttribute : ValidAttribute, IValid
    {
        #region 属性
        /// <summary>
        /// 最小值
        /// </summary>
        public Object MinValue
        { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public Object MaxValue
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
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        public ColumnRangeAttribute(String message, Object minValue, Object maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="func">校验方法</param>
        public ColumnRangeAttribute(String message, Func<Object, Boolean> func)
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
            Boolean flag = true;
            flag = flag && value != null;
            if (MinValue != null)
            {
                flag = flag && String.Compare(value.ToString(), MinValue.ToString()) >= 0;
            }
            if (MaxValue != null)
            {
                flag = flag && String.Compare(value.ToString(), MaxValue.ToString()) <= 0;
            }
            return flag;
        }

        /// <summary>
        /// 判断是否满足要求
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
