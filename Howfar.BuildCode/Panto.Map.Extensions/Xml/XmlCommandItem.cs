using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Data;
using System.ComponentModel;
using System.Data.SqlClient;

// 此处代码来源于博客【在.net中读写config文件的各种方法】的示例代码
// http://www.cnblogs.com/fish-li/archive/2011/12/18/2292037.html


namespace Panto.Map.Extensions.Xml
{
	/// <summary>
	/// 表示*.config文件中的一个XmlCommand配置项。
	/// </summary>
	[XmlType("XmlCommand")] 
    public class XmlCommandItem
    {
		/// <summary>
		/// 命令的名字，这个名字将在XmlCommand.From时被使用。
		/// </summary>
		[XmlAttribute("Name")]
		public string CommandName;

		/// <summary>
		/// 命令所引用的所有参数集合
		/// </summary>
		[XmlArrayItem("Parameter")]
		public List<XmlCmdParameter> Parameters = new List<XmlCmdParameter>();

		/// <summary>
		/// 命令的文本。是一段可运行的SQL脚本或存储过程名称。
		/// </summary>
		[XmlElement]
		public MyCDATA CommandText;
		//public string CommandText;

		/// <summary>
		/// SQL命令类型
		/// </summary>
		[DefaultValueAttribute(CommandType.Text)]
		[XmlAttribute]
		public CommandType CommandType = CommandType.Text;

		/// <summary>
		/// 获取或设置在终止执行命令的尝试并生成错误之前的等待时间。 
		/// </summary>
		[DefaultValueAttribute(30)]
		[XmlAttribute]
		public int Timeout = 30;
    }
	
	/// <summary>
	/// XmlCommand的命令参数。
	/// </summary>
	public class XmlCmdParameter
	{
		/// <summary>
		/// 参数名称
		/// </summary>
		[XmlAttribute]
		public string Name;

		/// <summary>
		/// 参数的数据类型
		/// </summary>
		[XmlAttribute]
		public SqlDbType Type;

		/// <summary>
		/// 参数值的长度。
		/// </summary>
		[DefaultValueAttribute(0)]
		[XmlAttribute]
		public int Size;

		
		/// <summary>
		/// 参数的输入输出方向
		/// </summary>
		[DefaultValueAttribute(ParameterDirection.Input)]
		[XmlAttribute]
		public ParameterDirection Direction = ParameterDirection.Input;
	}
}
