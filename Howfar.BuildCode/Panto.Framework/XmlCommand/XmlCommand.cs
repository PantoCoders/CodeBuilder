using Panto.Framework.Exceptions;
using Panto.Map.Extensions.DAL;
using OptimizeReflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Panto.Framework
{
    /// <summary>
    /// Xml命令
    /// </summary>
    public class XmlCommand : IDbExecute
    {
        private CPQuery _query;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">命令名称</param>
        /// <param name="argsObject">参数</param>
        public XmlCommand(string name, object argsObject)
            : this(name, argsObject, null)
        { 
        }

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        ///// <param name="name">命令名称</param>
        ///// <param name="argsObject">参数</param>
        ///// <param name="replaces">替换的键值对集合</param>
        //public XmlCommand(string name, object argsObject, Dictionary<string, string> replaces)
        //{
        //    if (string.IsNullOrEmpty(name))
        //        throw new ArgumentNullException("name");

        //    ClownFish.XmlCommand command = ClownFish.XmlCommandManager.GetCommand(name);
        //    if (command == null)
        //        throw new ArgumentOutOfRangeException("name", string.Format("指定的XmlCommand名称 {0} 不存在。", name));

        //    // 根据XML的定义以及传入参数，生成SqlParameter数组
        //    SqlParameter[] parameters = GetParameters(command, argsObject);

        //    // 创建CPQuery实例
        //    StringBuilder commandText = new StringBuilder(command.CommandText);
        //    if (replaces != null)
        //    {
        //        foreach (KeyValuePair<string, string> kvp in replaces)
        //        {
        //            commandText.Replace(kvp.Key, kvp.Value);
        //        }
        //    }

        //    _query = Panto.Map.Extensions.DAL.CPQuery.From(commandText.ToString(), parameters);
        //    _query.Command.CommandTimeout = command.Timeout;
        //    _query.Command.CommandType = command.CommandType;
        //}

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">XmlCommand名称</param>
        /// <param name="argsObject">参数</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        public XmlCommand(string name, object argsObject, params object[] tables)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            ClownFish.XmlCommand command = ClownFish.XmlCommandManager.GetCommand(name);
            if (command == null)
                throw new ArgumentOutOfRangeException("name", string.Format("指定的XmlCommand名称 {0} 不存在。", name));

            // 根据XML的定义以及传入参数，生成SqlParameter数组
            SqlParameter[] parameters = GetParameters(command, argsObject);

            // 创建CPQuery实例
            var commandText = new StringBuilder(command.CommandText).ToString();
            if (tables != null && tables.Length > 0)
            {
                commandText = string.Format(commandText, tables);
            }

            _query = Panto.Map.Extensions.DAL.CPQuery.From(commandText, parameters);
            _query.Command.CommandTimeout = command.Timeout;
            _query.Command.CommandType = command.CommandType;

        }

        /// <summary>
        /// 获得一个XmlCommand实例
        /// </summary>
        /// <param name="name">XmlCommand名称</param>
        /// <param name="argsObject">参数</param>
        /// <returns>XmlCommand实例</returns>
        public static XmlCommand From(string name, object argsObject)
        {
            return new XmlCommand(name, argsObject);
        }

        ///// <summary>
        ///// 获得一个XmlCommand实例
        ///// </summary>
        ///// <param name="name">XmlCommand名称</param>
        ///// <param name="argsObject">参数</param>
        ///// <param name="replaces">替换的键值对集合</param>
        ///// <returns>XmlCommand实例</returns>
        //public static XmlCommand From(string name, object argsObject, Dictionary<string, string> replaces)
        //{
        //    return new XmlCommand(name, argsObject, replaces);
        //}

        /// <summary>
        /// 获取一个XmlCommand实例
        /// </summary>
        /// <param name="name">XmlCommand名称</param>
        /// <param name="argsObject">参数</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns></returns>
        public static XmlCommand From(string name, object argsObject, params object[] tables)
        {
            return new XmlCommand(name, argsObject, tables);
        }

        /// <summary>
        /// 获得一个XmlCommand实例
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="type">XmlCommand类型</param>
        /// <param name="argsObject">参数</param>
        /// <returns>XmlCommand实例</returns>
        public static StandardXmlCommand<T> From<T>(XmlCommandType type, object argsObject) where T : class, new()
        {
            return new StandardXmlCommand<T>(type, argsObject);
        }

        /// <summary>
        /// 获得一个XmlCommand实例
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="type">XmlCommand类型</param>
        /// <param name="argsObject">参数</param>
        /// <param name="filter">过滤条件（参数化sql的过滤条件语句）</param>
        /// <returns>XmlCommand实例</returns>
        public static StandardXmlCommand<T> From<T>(XmlCommandType type, object argsObject, string filter) where T : class, new()
        {
            return new StandardXmlCommand<T>(type, argsObject, filter);
        }

        /// <summary>
        /// 获取Sql参数集合
        /// </summary>
        /// <param name="command">XmlCommand</param>
        /// <param name="argsObject">参数</param>
        /// <returns>Sql参数集合</returns>
        private SqlParameter[] GetParameters(ClownFish.XmlCommand command, object argsObject)
        {
            if (argsObject == null || command.Parameters.Count == 0)
                return new SqlParameter[0];

            // 将XML定义的参数，转成SqlParameter数组。
            SqlParameter[] parameters = (from p in command.Parameters
                                         let p2 = new SqlParameter
                                         {
                                             ParameterName = p.Name,
                                             DbType = p.Type,
                                             Direction = p.Direction,
                                             Size = p.Size
                                         }
                                         select p2).ToArray();

            PropertyInfo[] properties = argsObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // 为每个SqlParameter赋值。
            foreach (PropertyInfo pInfo in properties)
            {
                string name = "@" + pInfo.Name;

                // 如果传入了在XML中没有定义的参数项，则会抛出异常。
                SqlParameter p = parameters.FirstOrDefault(x => string.Compare(x.ParameterName, name, StringComparison.OrdinalIgnoreCase) == 0);
                if (p == null)
                    throw new ArgumentException(string.Format("传入的参数对象中，属性 {0} 没有在MXL定义对应的参数名。", pInfo.Name));

                p.Value = pInfo.FastGetValue(argsObject) ?? DBNull.Value;
            }

            return parameters;
        }

        private Regex _pagingRegex = new Regex(@"\)\s*as\s*rowindex\s*,", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// 获取实体分页列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="pageInfo">分页信息</param>
        /// <returns>实体分页列表</returns>
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

            string xmlCommandText = _query.ToString();

            Match match = _pagingRegex.Match(xmlCommandText);

            if (match.Success == false)
                throw new InvalidOperationException("XML中配置的SQL语句不符合分页语句的要求。");
            int p = match.Index;

            // 获取命令参数数组
            SqlParameter[] parameters1 = _query.Command.Parameters.Cast<SqlParameter>().ToArray();
            _query.Command.Parameters.Clear();	// 断开参数对象与原命令的关联。

            // 克隆参数数组，因为参数对象只能属于一个命令对象。
            SqlParameter[] parameters2 = (from pp in parameters1
                                          select new SqlParameter
                                          {
                                              ParameterName = pp.ParameterName,
                                              SqlDbType = pp.SqlDbType,
                                              Size = pp.Size,
                                              Scale = pp.Scale,
                                              Value = pp.Value,
                                              Direction = pp.Direction
                                          }).ToArray();



            // 生成 SELECT 命令
            string selectCommandText = string.Format(@"select * from ( {0} ) as t1 where RowIndex > (@PageSize * @PageIndex) and RowIndex <= (@PageSize * (@PageIndex+1))", xmlCommandText);

            Panto.Map.Extensions.DAL.CPQuery query1 = Panto.Map.Extensions.DAL.CPQuery.From(selectCommandText, parameters1);

            query1.Command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@PageIndex",
                SqlDbType = System.Data.SqlDbType.Int,
                Value = pageInfo.PageIndex
            });
            query1.Command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@PageSize",
                SqlDbType = System.Data.SqlDbType.Int,
                Value = pageInfo.PageSize
            });



            // 生成 COUNT 命令
            string getCountText = string.Format("select  count(*) from   (select {0}  ) as t1",
                            xmlCommandText.Substring(p + match.Length));

            Panto.Map.Extensions.DAL.CPQuery query2 = Panto.Map.Extensions.DAL.CPQuery.From(getCountText, parameters2);


            // 执行二次数据库操作（在一个连接中）
            using (Panto.Map.Extensions.DAL.ConnectionScope scope = new Panto.Map.Extensions.DAL.ConnectionScope())
            {
                List<T> list = query1.ToList<T>();
                pageInfo.TotalRecords = query2.ExecuteScalar<int>();

                return list;
            }
        }

        #region IDbExecute 成员

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <returns>执行结果</returns>
        public int ExecuteNonQuery()
        {
            return _query.ExecuteNonQuery();
        }

        /// <summary>
        /// 获取一个标量
        /// </summary>
        /// <typeparam name="T">标量类型</typeparam>
        /// <returns>标量</returns>
        public T ExecuteScalar<T>()
        {
            return _query.ExecuteScalar<T>();
        }

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <returns>DataSet</returns>
        public System.Data.DataSet FillDataSet()
        {
            return _query.FillDataSet();
        }

        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <returns>DataTable</returns>
        public System.Data.DataTable FillDataTable()
        {
            return _query.FillDataTable();
        }

        /// <summary>
        /// 获取标量列表
        /// </summary>
        /// <typeparam name="T">标量类型</typeparam>
        /// <returns>标量列表</returns>
        public List<T> FillScalarList<T>()
        {
            return _query.FillScalarList<T>();
        }

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>实体列表</returns>
        public List<T> ToList<T>() where T : class, new()
        {
            return _query.ToList<T>();
        }

        /// <summary>
        /// 获取单个实体
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
