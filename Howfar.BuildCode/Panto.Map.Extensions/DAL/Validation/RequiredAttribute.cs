using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 必填验证特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredAttribute : Attribute, IValidation
    {
        /// <summary>
        /// 必填校验构造函数
        /// </summary>
        public RequiredAttribute() {
            this.Enabled = true;
        }
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
                this.ErrorMessage = @"字段值不能为空";
            }

            ResultValidation ev = new ResultValidation(){ IsSuccess = true};
            if (value == null || (value != null && string.IsNullOrEmpty(value.ToString())))
            {
                ev.IsSuccess = false;
                ev.ErrorMessage = this.ErrorMessage;
            }
            return ev;
        }
    }
}
