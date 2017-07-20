using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// 标准分页实体
    /// </summary>
    public class StandardPagingInfo : PagingInfo
    {
        /// <summary>
        /// 排序片段
        /// Exm:ID DESC,Name ASC
        /// </summary>
        public string OrderSeq { get; set; }
        /// <summary>
        /// 自动跳转到有效页
        /// </summary>
        public bool AutoValidPage { get; set; }
    }
}
