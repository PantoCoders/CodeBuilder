using System;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 验证特性接口
    /// </summary>
    public interface IValid
    {
        /// <summary>
        /// 提示信息
        /// </summary>
        String Message { get; set; }

        /// <summary>
        /// 验证接口
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        Boolean Valid(Object value);
    }
}
