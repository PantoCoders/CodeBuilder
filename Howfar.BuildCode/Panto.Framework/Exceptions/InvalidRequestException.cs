using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.Exceptions
{
    /// <summary>
    /// 无效的HTTP请求的异常信息
    /// </summary>
    [Serializable]
    public class InvalidRequestException : MyException
    {
        public InvalidRequestException()
            : this("无效的HTTP请求")
        { }

        public InvalidRequestException(string message) 
            : base(message) 
        { }

        public InvalidRequestException(string message, Exception ex)
            : base(message, ex) 
        { }
    }
}
