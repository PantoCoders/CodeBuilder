using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.Exceptions
{
    /// <summary>
    /// 数据校验异常信息
    /// </summary>
    public class ValidateException : MyException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyName">属性名称(方便前端定位控件)</param>
        /// <param name="message">提示信息</param
        public ValidateException(string propertyName, string message)
            : base(string.Format("{0}|{1}", propertyName, message))
        { }
    }
}
