using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Panto.Map.Extensions.Xml;

namespace Panto.Map.Extensions.DAL
{
	/// <summary>
	/// 表示对*.config配置的XmlCommand命令封装。
	/// </summary>
	public sealed class XmlCommand : Panto.Map.Extensions.DAL.IDbExecute
	{
		private Panto.Map.Extensions.DAL.CPQuery _query;

		/// <summary>
		/// 创建新的XmlCommand对象。
		/// </summary>
		/// <param name="name">命令名字</param>
		public XmlCommand(string name) : this(name, null)
		{
		}

		/// <summary>
		/// 创建新的XmlCommand对象。
		/// </summary>
		/// <param name="name">命令名字</param>
		/// <param name="argsObject">匿名对象表示的参数</param>
		public XmlCommand(string name, object argsObject) : this(name, argsObject, null)
		{			
		}

		/// <summary>
		/// 创建新的XmlCommand对象。
		/// </summary>
		/// <param name="name">命令名字</param>
		/// <param name="argsObject">匿名对象表示的参数</param>
		/// <param name="replaces">替换的关键字字典</param>
		public XmlCommand(string name, object argsObject, Dictionary<string, string> replaces)
		{
			if( string.IsNullOrEmpty(name) )
				throw new ArgumentNullException("name");

			XmlCommandItem command = XmlCommandManager.GetCommand(name);
			if( command == null )
				throw new ArgumentOutOfRangeException("name", string.Format("指定的XmlCommand名称 {0} 不存在。", name));

			// 根据XML的定义以及传入参数，生成SqlParameter数组
			SqlParameter[] parameters = GetParameters(command, argsObject);

			// 创建CPQuery实例
			StringBuilder commandText = new StringBuilder(command.CommandText);
			if( replaces != null )
				foreach( KeyValuePair<string, string> kvp in replaces )
					commandText.Replace(kvp.Key, kvp.Value);


			_query = CPQuery.From(commandText.ToString(), parameters);
			_query.Command.CommandTimeout = command.Timeout;
			_query.Command.CommandType = command.CommandType;
		}

		/// <summary>
		/// 返回新的XmlCommand对象实例。
		/// </summary>
		/// <param name="name">命令名字</param>
		/// <returns>新的XmlCommand对象实例</returns>
		public static XmlCommand From(string name)
		{
			return new XmlCommand(name);
		}

		/// <summary>
		/// 返回新的XmlCommand对象实例。
		/// </summary>
		/// <param name="name">命令名字</param>
		/// <param name="argsObject">匿名对象表示的参数</param>
		/// <returns>新的XmlCommand对象实例</returns>
		public static XmlCommand From(string name, object argsObject)
		{
			return new XmlCommand(name, argsObject);
		}

		/// <summary>
		/// 返回新的XmlCommand对象实例。
		/// </summary>
		/// <param name="name">命令名字</param>
		/// <param name="argsObject">匿名对象表示的参数</param>
		/// <param name="replaces">替换的关键字字典</param>
		/// <returns></returns>
		public static XmlCommand From(string name, object argsObject, Dictionary<string, string> replaces)
		{
			return new XmlCommand(name, argsObject, replaces);
		}

		/// <summary>
		/// 根据XmlCommandItem对象，返回SqlParameter对象数组
		/// </summary>
		/// <param name="command">XmlCommandItem对象实例</param>
		/// <param name="argsObject">匿名对表示的数据库参数</param>
		/// <returns>SqlParameter对象数组</returns>
		private SqlParameter[] GetParameters(Panto.Map.Extensions.Xml.XmlCommandItem command, object argsObject)
		{
			if( command == null ) {
				throw new ArgumentNullException("command");
			}

			if( argsObject == null || command.Parameters.Count == 0 )
				return new SqlParameter[0];

			// 将XML定义的参数，转成SqlParameter数组。
			SqlParameter[] parameters = (from p in command.Parameters
										 let p2 = new SqlParameter {
											 ParameterName = p.Name,
											 SqlDbType = p.Type,
											 Direction = p.Direction,
											 Size = p.Size
										 }
										 select p2).ToArray();

			PropertyInfo[] properties = argsObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			// 为每个SqlParameter赋值。
			foreach( PropertyInfo pInfo in properties ) {
				string name = "@" + pInfo.Name;

				// 如果传入了在XML中没有定义的参数项，则会抛出异常。
				SqlParameter p = parameters.FirstOrDefault(x => string.Compare(x.ParameterName, name, StringComparison.OrdinalIgnoreCase) == 0);
				if( p == null )
					throw new ArgumentException(string.Format("传入的参数对象中，属性 {0} 没有在MXL定义对应的参数名。", pInfo.Name));
				
				p.Value = pInfo.FastGetValue(argsObject) ?? DBNull.Value;
			}

			return parameters;
		}

		private Regex _pagingRegex = new Regex(@"\)\s*as\s*rowindex\s*,", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

		/// <summary>
		/// 根据PagingInfo信息，返回分页后的实体集合
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="pageInfo">分页信息</param>
		/// <returns>实体集合</returns>
		public List<T> ToPageList<T>(PagingInfo pageInfo) where T : class, new()
		{

			//--需要配置的SQL语句
			//select row_number() over (order by UpCount asc) as RowIndex, 
			//    Title, Tag, [Description], Creator, CreateTime, UpCount, ReadCount, ReplyCount
			//from   CaoItem
			//where CreateTime < @CreateTime

			//--在运行时，将会生成下面二条SQL

			//select * from (
			//select row_number() over (order by UpCount asc) as RowIndex, 
			//    Title, Tag, [Description], Creator, CreateTime, UpCount, ReadCount, ReplyCount
			//from   CaoItem
			//where CreateTime < @CreateTime
			//) as t1
			//where  RowIndex > (@PageSize * @PageIndex) and RowIndex <= (@PageSize * (@PageIndex+1))

			//select  count(*) from   ( select 
			//-- 去掉 select row_number() over (order by UpCount asc) as RowIndex,
			//    Title, Tag, [Description], Creator, CreateTime, UpCount, ReadCount, ReplyCount
			//from   CaoItem as p 
			//where CreateTime < @CreateTime
			//) as t1


			// 为了方便得到 count 的语句，先直接定位 ") as RowIndex," 
			// 然后删除这之前的部分，将 select  count(*) from   (select 加到SQL语句的前面。
			// 所以，这里就检查SQL语句是否符合要求。

			//string flag = ") as RowIndex,";
			//int p = xmlCommandText.IndexOf(flag, StringComparison.OrdinalIgnoreCase);
			//if( p <= 0 )
			//    throw new InvalidOperationException("XML中配置的SQL语句不符合分页语句的要求。");

			string xmlCommandText = _query.ToString();

			Match match = _pagingRegex.Match(xmlCommandText);

			if( match.Success == false )
				throw new InvalidOperationException("XML中配置的SQL语句不符合分页语句的要求。");
			int p = match.Index;
			

			// 获取命令参数数组
			SqlParameter[] parameters1 = _query.Command.Parameters.Cast<SqlParameter>().ToArray();
			_query.Command.Parameters.Clear();	// 断开参数对象与原命令的关联。

			// 克隆参数数组，因为参数对象只能属于一个命令对象。
			SqlParameter[] parameters2 = (from pp in parameters1
										  select new SqlParameter {
											  ParameterName = pp.ParameterName,
											  SqlDbType = pp.SqlDbType,
											  Size = pp.Size,
											  Scale = pp.Scale,
											  Value = pp.Value,
											  Direction = pp.Direction
										  }).ToArray();
			


			// 生成 SELECT 命令
			string selectCommandText = string.Format(@"select * from ( {0} ) as t1 
where  RowIndex > (@PageSize * @PageIndex) and RowIndex <= (@PageSize * (@PageIndex+1))", xmlCommandText);

			CPQuery query1 = CPQuery.From(selectCommandText, parameters1);

			query1.Command.Parameters.Add(new SqlParameter {
				ParameterName = "@PageIndex",
				SqlDbType = System.Data.SqlDbType.Int,
				Value = pageInfo.PageIndex
			});
			query1.Command.Parameters.Add(new SqlParameter {
				ParameterName = "@PageSize",
				SqlDbType = System.Data.SqlDbType.Int,
				Value = pageInfo.PageSize
			});



			// 生成 COUNT 命令
			string getCountText = string.Format("select  count(*) from   (select {0}  ) as t1",
							xmlCommandText.Substring(p + match.Length));

			CPQuery query2 = CPQuery.From(getCountText, parameters2);


			// 执行二次数据库操作（在一个连接中）
			using( ConnectionScope scope = new ConnectionScope() ) {
				List<T> list = query1.ToList<T>();
				pageInfo.TotalRecords = query2.ExecuteScalar<int>();

				return list;
			}
		}

		/// <summary>
		/// 基本的分页信息。
		/// </summary>
		public class PagingInfo
		{
			/// <summary>
			/// 分页序号，从0开始计数
			/// </summary>
			public int PageIndex { get; set; }
			/// <summary>
			/// 分页大小
			/// </summary>
			public int PageSize { get; set; }
			/// <summary>
			/// 从相关查询中获取到的符合条件的总记录数
			/// </summary>
			public int TotalRecords { get; set; }


			/// <summary>
			/// 计算总页数
			/// </summary>
			/// <returns>总页数</returns>
			public int CalcPageCount()
			{
				if( this.PageSize == 0 || this.TotalRecords == 0 )
					return 0;

				return (int)Math.Ceiling((double)this.TotalRecords / (double)this.PageSize);
			}
		}

		#region IDbExecute 成员

		/// <summary>
		/// 执行命令,并返回影响函数
		/// </summary>
		/// <returns>影响行数</returns>
		public int ExecuteNonQuery()
		{
			return _query.ExecuteNonQuery();
		}
		/// <summary>
		/// 执行命令,返回第一行,第一列的值,并将结果转换为T类型
		/// </summary>
		/// <typeparam name="T">返回值类型</typeparam>
		/// <returns>结果集的第一行,第一列</returns>
		public T ExecuteScalar<T>()
		{
			return _query.ExecuteScalar<T>();
		}
		/// <summary>
		/// 执行查询,并将结果集填充到DataSet
		/// </summary>
		/// <returns>数据集</returns>
		public System.Data.DataSet FillDataSet()
		{
			return _query.FillDataSet();
		}
		/// <summary>
		/// 执行命令,并将结果集填充到DataTable
		/// </summary>
		/// <returns>数据表</returns>
		public System.Data.DataTable FillDataTable()
		{
			return _query.FillDataTable();
		}
		/// <summary>
		/// 执行命令,将第一列的值填充到类型为T的行集合中
		/// </summary>
		/// <typeparam name="T">返回值类型</typeparam>
		/// <returns>结果集的第一列集合</returns>
		public List<T> FillScalarList<T>()
		{
			return _query.FillScalarList<T>();
		}
		/// <summary>
		/// 执行命令,将结果集转换为实体集合
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <returns>实体集合</returns>
		public List<T> ToList<T>() where T : class, new()
		{
			return _query.ToList<T>();
		}
		/// <summary>
		/// 执行命令,将结果集转换为实体
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <returns>实体</returns>
		public T ToSingle<T>() where T : class, new()
		{
			return _query.ToSingle<T>();
		}

		#endregion
	}
}
