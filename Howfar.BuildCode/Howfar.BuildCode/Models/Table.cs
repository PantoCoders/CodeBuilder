using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Howfar.BuildCode.Models
{
    public class Table
    {
        public ConfigInfo ConfigInfo { get; set; }
        public List<Table> EntityList { get; set; }
        public string TableID { get; set; }
        public string TableName { get; set; }
        public string CreateDate { get; set; }
        public string ModifyDate { get; set; }
        public string Comment { get; set; }
        /// <summary>
        /// 格式化之后的
        /// 用于页面展示 Title
        /// </summary>
        public string CommentSimple { get; set; }
        public string FieldCount { get; set; }
        public string ColumnName { get; set; }
        public int ColumsID { get; set; }
        public string DefaultValue { get; set; }

        /// <summary>
        /// 父级名称 扩展字段 主表字段的名称
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool? IsPK { get; set; }

        /// <summary>
        /// 数据库 字段类型
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Csharp 字段类型
        /// </summary>
        public string CsharpType { get; set; }
        /// <summary>
        /// 字段长度
        /// </summary>
        public int MaxLength { get; set; }
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool NotNUll { get; set; }
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsCheck { get; set; }
        /// <summary>
        /// 是否数据库字段
        /// </summary>
        public bool IsDataColumn { get; set; }

        #region 配置字段

        /// <summary>
        /// 是否 字典表 字段
        /// </summary>
        public bool IsCodeField { get; set; }
        /// <summary>
        /// 是否验证字段
        /// </summary>
        public bool IsValidate { get; set; }
        /// <summary>
        /// 【列表】是否展示
        /// </summary>
        public bool IsShow { get; set; }
        /// <summary>
        /// 【列表】是否条件
        /// </summary>
        public bool IsCondition { get; set; }
        #endregion


    }
}