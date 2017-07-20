using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Panto.Map.Extensions.DAL
{
	/// <summary>
	/// 表示扩展数据列属性
	/// </summary>
	/// <example>
	///		<para>下面的代码演示了数据列属性的用法</para>
	///		<code>
	///		public class cbContract{
    ///			//ColumnType扩展列类型
    ///			//RelationColumn关联字段名
    ///			//Expression扩展列表达式
    ///			//   如果 ExtendColumnType = UserName，不会使用到此字段。
    ///         //   如果 ExtendColumnType = KeyParamOption_Guid，不会使用到此字段。
    ///         //   如果 ExtendColumnType = KeyParamOption_Code，则需要赋值 ParamGroup。
    ///         //   如果 ExtendColumnType = Common，则需要拼接LEFT JOIN语句，主表别名为实体名，请勿重复，字段名和别名尽量使用[]括起来，以免使用到关键字报错。
    ///         //   例如：LEFT JOIN dbo.RequirementType AS b ON b.TypeID = a.TypeID
    ///			[ExtendColumn(ColumnType=ExtendColumnTypeEnum.UserName, RelationColumn="CreatedBy")]
	///			public string CreatedByName { get; set; }
	///		}
	///		</code>
	/// </example>
	[AttributeUsageAttribute(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ExtendColumnAttribute : Attribute
	{
        /// <summary>
        /// 扩展列类型
        /// </summary>
        [DefaultValue(ExtendColumnTypeEnum.None)]
        public ExtendColumnTypeEnum ColumnType { get; set; }

        /// <summary>
        /// 主表的关联列名
        /// </summary>
        public string RelationColumn { get; set; }

        /// <summary>
        /// 扩展列表达式
        /// 如果 ExtendColumnType = UserName，不会使用到此字段。
        /// 如果 ExtendColumnType = KeyParamOption_Guid，不会使用到此字段。
        /// 如果 ExtendColumnType = KeyParamOption_Code，则需要赋值 ParamGroup。
        /// 如果 ExtendColumnType = Common，则需要拼接LEFT JOIN语句，主表别名为实体名，请勿重复，字段名和别名尽量使用[]括起来，以免使用到关键字报错。
        ///     例如：LEFT JOIN dbo.RequirementType AS b ON b.TypeID = a.TypeID
        /// </summary>
        public string Expression { get; set; }
	}

    /// <summary>
    /// 扩展列类型枚举值
    /// </summary>
    public enum ExtendColumnTypeEnum
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        /// <summary>
        /// 业务参数键值对_编码
        /// </summary>
        KeyParamOption_Code,
        /// <summary>
        /// 业务参数键值对_Guid
        /// </summary>
        KeyParamOption_Guid,
        /// <summary>
        /// 用户名_编码
        /// </summary>
        UserName_Code,
        /// <summary>
        /// 用户名_Guid
        /// </summary>
        UserName_Guid,
        /// <summary>
        /// 业务单元_编码
        /// </summary>
        BusinessUnit_Code,
        /// <summary>
        /// 业务单元_Guid
        /// </summary>
        BusinessUnit_Guid,
        /// <summary>
        /// 通用
        /// </summary>
        Common
    }
}
