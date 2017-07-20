using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.Exceptions
{
    /// <summary>
    /// 没有权限操作的异常信息
    /// </summary>
    [Serializable]
    public class PermissionDeniedException : MyException
    {
        public PermissionDeniedException()
            : base("您没有执行这个操作的权限")
        { }
    }
}
