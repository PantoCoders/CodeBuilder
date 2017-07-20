using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 验证结果
    /// </summary>
    public class ResultValidation
    {
        /// <summary>
        /// 验证结果构造函数
        /// </summary>
        public ResultValidation()
        {
            this.IsSuccess = true;
            this.ErrorMessage = string.Empty;
        }
        /// <summary>
        /// 验证是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
