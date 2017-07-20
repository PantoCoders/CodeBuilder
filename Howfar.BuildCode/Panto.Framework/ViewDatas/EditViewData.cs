using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.ViewDatas
{
    /// <summary>
    /// 编辑页面显示数据
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class EditViewData<T> where T : new()
    {
        /// <summary>
        /// 实体
        /// </summary>
        public T Entity { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; set; }
    }
}