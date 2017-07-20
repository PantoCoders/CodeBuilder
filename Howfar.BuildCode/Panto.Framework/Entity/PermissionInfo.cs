using System;
using System.Collections.Generic;

namespace Panto.Framework.Entity
{
    /// <summary>
    /// 权限值
    /// </summary>
    [Serializable]
    public class PermissionInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 事件名称
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
    }

    public class PermissionInfoCompare : IEqualityComparer<PermissionInfo>
    {
        public bool Equals(PermissionInfo p1, PermissionInfo p2)
        {
            if (p1 != null && p2 != null)
            {
                return p1.Code == p2.Code;
            }
            return false;
        }

        public int GetHashCode(PermissionInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}