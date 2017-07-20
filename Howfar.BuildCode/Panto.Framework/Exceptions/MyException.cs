using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Panto.Framework.Exceptions
{
    /// <summary>
    /// 简单的异常，一般只是为了方便从嵌套比较深的地方快速跳出，并带有一个错误信息。
    /// </summary>
    [Serializable]
    public class MyException : Exception
    {
        public MyException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        public MyException(string message, Exception innerException)
            : base(message, innerException)
        { 
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="message">异常信息</param>
        public MyException(string message)
            : base(message)
        { 
        }
    }
}
