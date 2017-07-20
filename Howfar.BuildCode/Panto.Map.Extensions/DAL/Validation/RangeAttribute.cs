using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 数据范围验证特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RangeAttribute : Attribute, IValidation
    {
        /// <summary>
        /// 设置范围
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public RangeAttribute(decimal min, decimal max)
        {
            this.Min = min;
            this.Max = max;
            this.Enabled = true;
        }
        /// <summary>
        /// 最大值
        /// </summary>
        public decimal Max { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public decimal Min { get; set; }

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
                this.ErrorMessage = "字段不能小于{0}或大于{1}";
            }

            ResultValidation ev = new ResultValidation() { IsSuccess = true };
            decimal data = 0;
            if (value != null && decimal.TryParse(value.ToString(), out data))
            {
                if (data >= this.Min && data <= this.Max)
                {
                    return ev;
                }
                else
                {
                    ev.ErrorMessage = string.Format(this.ErrorMessage, this.Min, this.Max);
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
