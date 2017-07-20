using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 字符串长度验证特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StringLengthAttribute : Attribute, IValidation
    {
        /// <summary>
        /// 最大长度设置
        /// </summary>
        /// <param name="maxLength"></param>
        public StringLengthAttribute(int maxLength) { this.MaxLength = maxLength; this.Enabled = true; }
        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; }
        /// <summary>
        /// 最小长度
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        ///错误提示信息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 验证方法
        /// </summary>
        /// <param name="value">需要验证的值</param>
        /// <returns>返回验证结果</returns>
        public ResultValidation Validation(object value)
        {
            //判断是否设置了异常提示信息，如果没有，设置默认值
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ErrorMessage = "字段长度不能小于{0}或大于{1}";
            }

            ResultValidation ev = new ResultValidation() { IsSuccess = true };
            if (value != null)
            {
                int newLength = value.ToString().Length;
                if (newLength >= MinLength && newLength <= MaxLength)
                {
                    return ev;
                }
                else
                {
                    ev.ErrorMessage = string.Format(this.ErrorMessage,this.MinLength,this.MaxLength);
                    ev.IsSuccess = false;
                }
            }
            else
            {
                ev.ErrorMessage = this.ErrorMessage;
                ev.IsSuccess = false;
            }
            return ev; ;
        }
    }
}
