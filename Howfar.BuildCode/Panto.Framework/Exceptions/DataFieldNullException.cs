using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.Exceptions
{
    /// <summary>
    /// 数据成员为空的异常信息
    /// </summary>
    [Serializable]
    public class DataFieldNullException : MyException
    {
        public DataFieldNullException(string fieldName)
            : base(string.Format("{0} 不允许为空", fieldName))
        { }
    }
}
