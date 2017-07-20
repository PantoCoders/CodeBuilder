using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// 实体Sql信息
    /// </summary>
    [Serializable]
    internal class EntitySqlInfo
    {
        internal EntitySqlInfo()
        {
            Properties = new List<string>();
            ExtendTables = new List<string>();
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// 属性列表
        /// </summary>
        public List<string> Properties { get; set; }

        /// <summary>
        /// 扩展连接的表名的SQL语句
        /// </summary>
        public List<string> ExtendTables { get; set; }
    }
}
