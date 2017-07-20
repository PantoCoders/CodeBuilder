using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 验证属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValidAttribute : System.Attribute
    {
    }
}
