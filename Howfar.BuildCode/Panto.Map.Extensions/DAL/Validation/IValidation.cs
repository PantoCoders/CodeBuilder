using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 基础验证接口
    /// </summary>
    public interface IValidation
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// 错误提示信息
        /// </summary>
        string ErrorMessage { get; set; }
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ResultValidation Validation(object value);
    }
}
