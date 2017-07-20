using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.ViewDatas
{
    /// <summary>
    /// 分页列表显示数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PageListViewData<T> : ListViewData<T> where T : new()
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public StandardPagingInfo PagingInfo { get; set; }
    }
}