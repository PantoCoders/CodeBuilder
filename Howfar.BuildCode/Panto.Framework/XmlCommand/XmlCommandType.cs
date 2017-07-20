using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// XmlCommand类型
    /// </summary>
    public enum XmlCommandType
    {
        /// <summary>
        /// 获取单个实体（参数名为oid,类型为Guid）
        /// </summary>
        GetSingle,
        /// <summary>
        /// 获取列表
        /// </summary>
        GetList,
        /// <summary>
        /// 通过Id字符串获取列表（参数名为oids，类型为字符串，以","分隔）
        /// </summary>
        GetListByIds,
        /// <summary>
        /// 获取数量
        /// </summary>
        GetCount,
        /// <summary>
        /// 获取标准的分页信息
        /// </summary>
        GetPageList,
    }
}
