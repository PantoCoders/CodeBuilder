using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.ViewDatas
{
    /// <summary>
    /// 列表显示数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ListViewData<T> where T : new()
    {
        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> List { get; set; }
    }
}