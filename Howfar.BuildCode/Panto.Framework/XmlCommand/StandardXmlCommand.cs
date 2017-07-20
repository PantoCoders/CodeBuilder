using Panto.Framework.Exceptions;
using Panto.Framework.MVC;
using Panto.Map.Extensions.DAL;
using OptimizeReflection;
using System;
using System.Collections;
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
    /// 标准的XmlCommand
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class StandardXmlCommand<T> where T : class, new()
    {
        private CPQuery _query;

        /// <summary>
        /// 缓存用户的Hashtable
        /// </summary>
        private static Hashtable s_table = Hashtable.Synchronized(new Hashtable(1024));

        private EntitySqlInfo _sqlInfo;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">命令类型</param>
        /// <param name="argsObject">参数</param>
        public StandardXmlCommand(XmlCommandType type, object argsObject)
            : this(type, argsObject, string.Empty)
        { }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">命令类型</param>
        /// <param name="argsObject">参数</param>
        /// <param name="filter">过滤条件（参数化sql的过滤条件语句）</param>
        public StandardXmlCommand(XmlCommandType type, object argsObject, string filter)
        {
            _sqlInfo = GetEntitySqlInfo();
            var sql = new StringBuilder();
            sql.AppendFormat("select {0} from {1} where 1=1", _sqlInfo.Properties.Join(","), _sqlInfo.TableName + " " + _sqlInfo.ExtendTables.Join(" "));
            List<SqlParameter> parameterList = new List<SqlParameter>();
            switch (type)
            {
                case XmlCommandType.GetSingle:
                    sql.AppendFormat(" and {0}=@oid", _sqlInfo.PrimaryKey);
                    parameterList.Add(new SqlParameter()
                    {
                        ParameterName = "@oid",
                        DbType = DbType.Guid,
                        Direction = ParameterDirection.Input,
                        Size = 0
                    });
                    break;
                case XmlCommandType.GetList:
                case XmlCommandType.GetPageList:
                    break;
                case XmlCommandType.GetListByIds:
                    sql.AppendFormat(" and {0} IN (SELECT AllItem FROM dbo.fn_split(@oids,','))", _sqlInfo.PrimaryKey);
                    parameterList.Add(new SqlParameter()
                    {
                        ParameterName = "@oids",
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input,
                        Size = 0
                    });
                    break;
                case XmlCommandType.GetCount:
                    sql = new StringBuilder();
                    sql.AppendFormat("select count(1) from {0} where 1=1", _sqlInfo.TableName);
                    break;
            }

            if (!string.IsNullOrEmpty(filter))
            {
                sql.AppendFormat(" and {0}", filter);
                Regex reg = new Regex(@"[^@@](?<p>@\w+)");

                if (argsObject != null)
                {
                    PropertyInfo[] argProps = argsObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                    foreach (Match m in reg.Matches(filter))
                    {
                        // 跳过同名参数
                        if (parameterList.Count(p => p.ParameterName == m.Groups["p"].Value) > 0)
                            continue;

                        var argProp = argProps.FirstOrDefault(p => "@" + p.Name == m.Groups["p"].Value);
                        parameterList.Add(new SqlParameter()
                        {
                            ParameterName = m.Groups["p"].Value,
                            DbType = CSharp2DbType(argProp.PropertyType.GetRealType()),
                            Direction = ParameterDirection.Input,
                            Size = 0
                        });
                    }
                }
            }

            SqlParameter[] parameters = GetParameters(parameterList, argsObject);

            _query = CPQuery.From(sql.ToString(), parameters);
            _query.Command.CommandTimeout = 30;
            _query.Command.CommandType = CommandType.Text;
        }

        /// <summary>
        /// 获取实体sql信息
        /// </summary>
        /// <returns>实体sql信息</returns>
        private EntitySqlInfo GetEntitySqlInfo()
        {
            EntitySqlInfo _info = (EntitySqlInfo)s_table[typeof(T).FullName];
            //_info = null
            if (_info == null)
            {
                // 用于支持扩展列的表名
                _info = new EntitySqlInfo();
                //_info.TableName = string.Format("[{0}]", typeof(T).Name);
                _info.TableName = string.Format("{0}", typeof(T).Name);
                // 如果有别名，则使用别名
                DataEntityAttribute attribute = typeof(T).GetMyAttribute<DataEntityAttribute>();

                if (attribute != null && !string.IsNullOrEmpty(attribute.Alias))
                {
                    //_info.TableName = string.Format("[{0}]", attribute.Alias);
                    _info.TableName = string.Format("{0}", attribute.Alias);
                }
                // 获取字段名
                foreach(PropertyInfo prop in typeof(T).GetProperties())
                {
                    TransferProperty(prop, _info);
                }

                s_table[typeof(T).FullName] = _info;
            }

            return _info;
        }

        /// <summary>
        /// C#类型转换为数据库类型
        /// </summary>
        /// <param name="type">C#类型</param>
        /// <returns>数据库类型</returns>
        private DbType CSharp2DbType(Type type)
        {
            switch (type.ToString())
            {
                case "System.Int64": return DbType.Int64;
                case "System.Byte[]": return DbType.Binary;
                case "System.Boolean": return DbType.Boolean;
                case "System.String": return DbType.String;
                case "System.DateTime": return DbType.DateTime;
                case "System.Decimal": return DbType.Decimal;
                case "System.Double": return DbType.Double;
                case "System.Int32": return DbType.Int32;
                case "System.Single": return DbType.Single;
                case "System.Int16": return DbType.Int16;
                case "System.Byte": return DbType.Byte;
                case "System.Guid": return DbType.Guid;
                case "System.TimeSpan": return DbType.Time;
                default:
                    throw new NotSupportedException("不支持的数据类型:" + type.ToString());
            }
        }

        /// <summary>
        /// 获取Sql参数集合
        /// </summary>
        /// <param name="parameterList">Sql参数集合</param>
        /// <param name="argsObject">参数</param>
        /// <returns>Sql参数集合</returns>
        private SqlParameter[] GetParameters(List<SqlParameter> parameterList, object argsObject)
        {
            if (argsObject == null || parameterList.Count == 0)
                return new SqlParameter[0];

            SqlParameter[] parameters = parameterList.ToArray();

            PropertyInfo[] properties = argsObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // 为每个SqlParameter赋值。
            foreach (PropertyInfo pInfo in properties)
            {
                string name = "@" + pInfo.Name;

                // 如果传入了没有定义的参数项，则跳过。
                SqlParameter p = parameters.FirstOrDefault(x => string.Compare(x.ParameterName, name, StringComparison.OrdinalIgnoreCase) == 0);
                if (p == null)
                    continue;

                p.Value = pInfo.FastGetValue(argsObject) ?? DBNull.Value;
            }

            return parameters;
        }

        /// <summary>
        /// 转换属性
        /// </summary>
        /// <param name="info">属性</param>
        /// <param name="entity">实体Sql信息</param>
        private void TransferProperty(PropertyInfo info, EntitySqlInfo entity)
        {            
            DataColumnAttribute attrColumn = info.GetMyAttribute<DataColumnAttribute>();
            if (attrColumn != null)
            {
                var prop = "";
                if (!string.IsNullOrEmpty(attrColumn.Alias))
                {
                    prop = string.Format("{0}.[{1}]", entity.TableName, attrColumn.Alias);
                }
                else
                {
                    prop = string.Format("{0}.[{1}]", entity.TableName, info.Name);
                }
                entity.Properties.Add(prop);
                if (attrColumn.PrimaryKey)
                {
                    entity.PrimaryKey = prop;
                }
            }
            else
            {
                ExtendColumnAttribute attrExtColumn = info.GetMyAttribute<ExtendColumnAttribute>();
                if (attrExtColumn != null && attrExtColumn.ColumnType != 0)
                {
                    switch (attrExtColumn.ColumnType)
                    {
                        case ExtendColumnTypeEnum.KeyParamOption_Code:
                            entity.ExtendTables.Add(string.Format("LEFT JOIN dbo.KeyParamOption AS {0} ON [{0}].[Code] = {1}.[{2}] AND [{0}].[ParamGroup] = '{3}'", info.Name, entity.TableName, attrExtColumn.RelationColumn, attrExtColumn.Expression));
                            entity.Properties.Add(string.Format("[{0}].[Name] AS {0}", info.Name));
                            break;
                        case ExtendColumnTypeEnum.KeyParamOption_Guid:
                            entity.ExtendTables.Add(string.Format("LEFT JOIN dbo.KeyParamOption AS {0} ON [{0}].[KeyParamOptionID] = {1}.[{2}]", info.Name, entity.TableName, attrExtColumn.RelationColumn));
                            entity.Properties.Add(string.Format("[{0}].[Name] AS {0}", info.Name));
                            break;
                        case ExtendColumnTypeEnum.UserName_Guid:
                            entity.ExtendTables.Add(string.Format("LEFT JOIN {0}.dbo.ORG_UserInfo AS {1} ON [{1}].[UserGUID] = {2}.[{3}]", ConnStringHelper.GetDBName("SystemCore"), info.Name, entity.TableName, attrExtColumn.RelationColumn));
                            entity.Properties.Add(string.Format("[{0}].[UserName_Chn] AS {0}", info.Name));
                            break;
                        case ExtendColumnTypeEnum.UserName_Code:
                            entity.ExtendTables.Add(string.Format("LEFT JOIN {0}.dbo.ORG_UserInfo AS {1} ON [{1}].[UserCode] = {2}.[{3}]", ConnStringHelper.GetDBName("SystemCore"), info.Name, entity.TableName, attrExtColumn.RelationColumn));
                            entity.Properties.Add(string.Format("[{0}].[UserName_Chn] AS {0}", info.Name));
                            break;
                        case ExtendColumnTypeEnum.BusinessUnit_Guid:
                            entity.ExtendTables.Add(string.Format("LEFT JOIN {0}.dbo.ORG_BusinessUnit AS {1} ON [{1}].[BUGUID] = {2}.[{3}]", ConnStringHelper.GetDBName("SystemCore"), info.Name, entity.TableName, attrExtColumn.RelationColumn));
                            entity.Properties.Add(string.Format("[{0}].[BUName] AS {0}", info.Name));
                            break;
                        case ExtendColumnTypeEnum.BusinessUnit_Code:
                            entity.ExtendTables.Add(string.Format("LEFT JOIN {0}.dbo.ORG_BusinessUnit AS {1} ON [{1}].[BUCode] = {2}.[{3}]", ConnStringHelper.GetDBName("SystemCore"), info.Name, entity.TableName, attrExtColumn.RelationColumn));
                            entity.Properties.Add(string.Format("[{0}].[BUName] AS {0}", info.Name));
                            break;
                        case ExtendColumnTypeEnum.Common:
                            entity.ExtendTables.Add(attrExtColumn.Expression);
                            entity.Properties.Add(attrExtColumn.RelationColumn);
                            break;
                    }
                }
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
        /// <typeparam name="T1">标量类型</typeparam>
        /// <returns>标量</returns>
        public T1 ExecuteScalar<T1>()
        {
            return _query.ExecuteScalar<T1>();
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
        /// <typeparam name="T1">标量类型</typeparam>
        /// <returns>标量列表</returns>
        public List<T1> FillScalarList<T1>()
        {
            return _query.FillScalarList<T1>();
        }

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <returns>实体列表</returns>
        public List<T> ToList()
        {
            return _query.ToList<T>();
        }

        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <returns>实体</returns>
        public T ToSingle()
        {
            return _query.ToSingle<T>();
        }

        private Regex _pagingRegex = new Regex(@"\)\s*as\s*rowindex\s*,", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// 获取实体分页列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="pageInfo">标准分页信息</param>
        /// <returns>实体分页列表</returns>
        public List<T> ToPageList(StandardPagingInfo pageInfo)
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

            if (string.IsNullOrEmpty(pageInfo.OrderSeq))
            {
                pageInfo.OrderSeq = _sqlInfo.PrimaryKey;
            }

            string xmlCommandText = _query.ToString().Replace("select", "select ROW_NUMBER() OVER ( ORDER BY " + pageInfo.OrderSeq + " ) as RowIndex, ");

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

        #endregion
    }
}
