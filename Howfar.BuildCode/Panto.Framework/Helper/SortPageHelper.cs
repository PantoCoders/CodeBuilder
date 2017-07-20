using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Panto.Map.Extensions.DAL;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Panto.Framework.Exceptions;

namespace Panto.Framework
{
    /// <summary>
    /// 分页排序助手(适用于获取需要动态传递排序字段的分页列表，单表或固定排序字段的情况请用XmlCommand和StandardXmlCommand中的ToPageList方法。)
    /// </summary>
    public class SortPageHelper
    {
        /// <summary>
        /// 获取分页数据表
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>分页数据表</returns>
        public static DataTable GetData(string sql, SqlParameter[] parameters, StandardPagingInfo pagingInfo, params object[] tables)
        {
            return GetValidCPQuery(sql, parameters, pagingInfo, tables).FillDataTable();
        }

        /// <summary>
        /// 获取分页数据表
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>分页数据表</returns>
        public static DataTable GetData(string sql, object parameters, StandardPagingInfo pagingInfo, params object[] tables)
        {
            return GetValidCPQuery(sql, parameters, pagingInfo, tables).FillDataTable();
        }

        /// <summary>
        /// 获取实体分页列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>实体分页列表</returns>
        public static List<T> GetList<T>(string sql, SqlParameter[] parameters, StandardPagingInfo pagingInfo, params object[] tables) where T : class,new()
        {
            return GetValidCPQuery(sql, parameters, pagingInfo, tables).ToList<T>();
        }

        /// <summary>
        /// 获取实体分页列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>实体分页列表</returns>
        public static List<T> GetList<T>(string sql, object parameters, StandardPagingInfo pagingInfo, params object[] tables) where T : class,new()
        {
            return GetValidCPQuery(sql, parameters, pagingInfo, tables).ToList<T>();
        }


        /// <summary>
        /// 获取分页数据表(指定数据库连接字符串)
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="strConn">数据库连接字符串</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>分页数据表</returns>
        public static DataTable GetData(string sql, SqlParameter[] parameters, string strConn, StandardPagingInfo pagingInfo, params object[] tables)
        {
            using (ConnectionScope scope = new ConnectionScope(TransactionMode.Inherits, strConn))
            {
                return GetValidCPQuery(sql, parameters, pagingInfo, tables).FillDataTable();
            }
        }

        /// <summary>
        /// 获取分页数据表(指定数据库连接字符串)
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="strConn">数据库连接字符串</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>分页数据表</returns>
        public static DataTable GetData(string sql, object parameters, string strConn, StandardPagingInfo pagingInfo, params object[] tables)
        {
            using (ConnectionScope scope = new ConnectionScope(TransactionMode.Inherits, strConn))
            {
                return GetValidCPQuery(sql, parameters, pagingInfo, tables).FillDataTable();
            }
        }

        /// <summary>
        /// 获取实体分页列表(指定数据库连接字符串)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="strConn">数据库连接字符串</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>实体分页列表</returns>
        public static List<T> GetList<T>(string sql, SqlParameter[] parameters, string strConn, StandardPagingInfo pagingInfo, params object[] tables) where T : class,new()
        {
            using (ConnectionScope scope = new ConnectionScope(TransactionMode.Inherits, strConn))
            {
                return GetValidCPQuery(sql, parameters, pagingInfo, tables).ToList<T>();
            }
        }

        /// <summary>
        /// 获取实体分页列表(指定数据库连接字符串)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="strConn">数据库连接字符串</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>实体分页列表</returns>
        public static List<T> GetList<T>(string sql, object parameters, string strConn, StandardPagingInfo pagingInfo, params object[] tables) where T : class,new()
        {
            using (ConnectionScope scope = new ConnectionScope(TransactionMode.Inherits, strConn))
            {
                return GetValidCPQuery(sql, parameters, pagingInfo, tables).ToList<T>();
            }
        }
        /// <summary>
        /// 获取有效的CPQuery对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="pagingInfo"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static CPQuery GetValidCPQuery(string sql, object parameters, StandardPagingInfo pagingInfo, params object[] tables)
        {
            CPQuery query = GetCPQuery(sql, parameters, pagingInfo, tables);
            if (pagingInfo.AutoValidPage)
            {
                if (query.FillDataTable().Rows.Count == 0 && pagingInfo.PageIndex > 0)
                {
                    //如果当前查询的页没有数据，但是设置的是允许自动定位页，则查找到最后一个有效的页面进行查询
                    int len = (int)Math.Ceiling((double)pagingInfo.TotalRecords / pagingInfo.PageSize);
                    pagingInfo.PageIndex = (len - 1) < 0 ? 0 : (len - 1);
                    query = GetCPQuery(sql, parameters, pagingInfo, tables);
                }
            }
            return query;
        }
        /// <summary>
        /// 获取有效的CPQuery对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="pagingInfo"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static CPQuery GetValidCPQuery(string sql, SqlParameter[] parameters, StandardPagingInfo pagingInfo, params object[] tables)
        {
            CPQuery query = GetCPQuery(sql, parameters, pagingInfo, tables);
            if (pagingInfo.AutoValidPage)
            {
                if (query.FillDataTable().Rows.Count == 0 && pagingInfo.PageIndex > 0)
                {
                    //如果当前查询的页没有数据，但是设置的是允许自动定位页，则查找到最后一个有效的页面进行查询
                    int len = (int)Math.Ceiling((double)pagingInfo.TotalRecords / pagingInfo.PageSize);
                    pagingInfo.PageIndex = (len - 1) < 0 ? 0 : (len - 1);
                    query = GetCPQuery(sql, parameters, pagingInfo, tables);
                }
            }
            return query;
        }
        /// <summary>
        /// 获取查询的CPQuery对象
        /// </summary>
        /// <param name="sql">查询SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns></returns>
        private static CPQuery GetCPQuery(string sql, object parameters, StandardPagingInfo pagingInfo, params object[] tables)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("sql");
            }

            if (string.IsNullOrEmpty(pagingInfo.OrderSeq))
            {
                throw new MyException("请在分页实体中指定 排序字段");
            }

            // 格式化数据库实例名
            if (tables != null && tables.Length > 0)
            {
                sql = string.Format(sql, tables);
            }

            // 生成 SELECT 命令
            string selectCommandText = string.Format(@"select * from ( 
                            select ROW_NUMBER() OVER(ORDER BY {3}) AS [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86],* 
                            from ({0}) as a1) as t1 where [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86] > ({1} * {2}) 
                            and [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86] <= ({1} * ({2}+1)) order by [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86]", sql, pagingInfo.PageSize, pagingInfo.PageIndex, pagingInfo.OrderSeq);

            CPQuery query1 = CPQuery.From(selectCommandText, parameters);
            // 生成 COUNT 命令
            string getCountText = string.Format(@"select count(*) from ({0}) as t1", sql);
            CPQuery query2 = CPQuery.From(getCountText, parameters);
            pagingInfo.TotalRecords = query2.ExecuteScalar<int>();

            return query1;
        }

        /// <summary>
        /// 获取查询的CPQuery对象
        /// </summary>
        /// <param name="sql">查询SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns></returns>
        private static CPQuery GetCPQuery(string sql, SqlParameter[] parameters, StandardPagingInfo pagingInfo, params object[] tables)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("sql");
            }

            if (string.IsNullOrEmpty(pagingInfo.OrderSeq))
            {
                throw new MyException("请在分页实体中指定 排序字段");
            }

            // 格式化数据库实例名
            if (tables.Length > 0)
            {
                sql = string.Format(sql, tables);
            }

            // 生成 COUNT 命令
            string getCountText = string.Format(@"select count(*) from ({0}) as t1", sql);
            CPQuery query2 = CPQuery.From(getCountText, parameters);
            pagingInfo.TotalRecords = query2.ExecuteScalar<int>();
            //清除参数
            query2.Command.Parameters.Clear();

            // 生成 SELECT 命令
            string selectCommandText = string.Format(@"select * from ( 
                            select ROW_NUMBER() OVER(ORDER BY {3}) AS [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86],* 
                            from ({0}) as a1) as t1 where [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86] > ({1} * {2}) 
                            and [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86] <= ({1} * ({2}+1)) order by [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86]", sql, pagingInfo.PageSize, pagingInfo.PageIndex, pagingInfo.OrderSeq);

            CPQuery query1 = CPQuery.From(selectCommandText, parameters);
            return query1;
        }

        #region 特殊分页(动态拼接条件,解决性能问题)

        /// <summary>
        /// 获取特殊数据集分页列表
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="dict">扩展条件键值对(key:列名,value:left join语句)</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>分页数据集</returns>
        public static DataTable GetSpecialData(string sql, object parameters, StandardPagingInfo pagingInfo, Dictionary<string, string> dict, params object[] tables)
        {
            return GetSpecialQuery(sql, parameters, pagingInfo, dict, tables).FillDataTable();
        }

        /// <summary>
        /// 获取特殊实体分页列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="dict">扩展条件键值对(key:列名,value:left join语句)</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>实体分页列表</returns>
        public static List<T> GetSpecialList<T>(string sql, object parameters, StandardPagingInfo pagingInfo, Dictionary<string, string> dict, params object[] tables) where T : class,new()
        {
            return GetSpecialData(sql, parameters, pagingInfo, dict, tables).ToList<T>();
        }

        /// <summary>
        /// 获取数据集分页列表(指定数据库连接字符串)
        /// </summary>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="strConn">数据库连接字符串</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="dict">扩展条件键值对(key:列名,value:left join语句)</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>数据集分页列表</returns>
        public static DataTable GetSpecialData(string sql, object parameters, string strConn, StandardPagingInfo pagingInfo, Dictionary<string, string> dict, params object[] tables)
        {
            using (ConnectionScope scope = new ConnectionScope(TransactionMode.Inherits, strConn))
            {
                return GetSpecialQuery(sql, parameters, pagingInfo, dict, tables).FillDataTable();
            }
        }

        /// <summary>
        /// 获取实体分页列表(指定数据库连接字符串)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">执行SQL</param>
        /// <param name="parameters">参数</param>
        /// <param name="strConn">数据库连接字符串</param>
        /// <param name="pagingInfo">分页信息(OrderSeq传递排序字段：(Oid desc,Name asc))</param>
        /// <param name="dict">扩展条件键值对(key:列名,value:left join语句)</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>实体分页列表</returns>
        public static List<T> GetSpecialList<T>(string sql, object parameters, string strConn, StandardPagingInfo pagingInfo, Dictionary<string, string> dict, params object[] tables) where T : class,new()
        {
            return GetSpecialData(sql, parameters, pagingInfo, dict, tables).ToList<T>();
        }

        /// <summary>
        /// 获取CPQuery
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="pagingInfo">标准分页实体</param>
        /// <param name="dict">扩展条件键值对(key:列名,value:left join语句)</param>
        /// <param name="tables">联合查询用到的数据库实例名，可以用ConnStringHelper.GetDBName(string key)</param>
        /// <returns>CPQuery</returns>
        private static CPQuery GetSpecialQuery(string sql, object parameters, StandardPagingInfo pagingInfo, Dictionary<string, string> dict, params object[] tables)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("sql");
            }

            if (string.IsNullOrEmpty(pagingInfo.OrderSeq))
            {
                throw new MyException("请在分页实体中指定 排序字段");
            }

            // 格式化数据库实例名
            if (tables != null && tables.Length > 0)
            {
                sql = string.Format(sql, tables);
            }

            var extCols = "";
            var joinTables = "";
            if (dict != null && dict.Count > 0)
            {
                extCols = "," + dict.Keys.Join(" , ");
                joinTables = dict.Values.Join(" ");
            }

            // 生成 SELECT 命令
            var selectCommandText = string.Format(@"SELECT  a.* {4}
                                        FROM    ( SELECT    ROW_NUMBER() OVER ( ORDER BY {3} ) AS [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86] ,
                                                            *
                                                    FROM      ({0}) AS a
                                                ) AS a {5}
                                        WHERE   [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86] > ( {1} * {2} )
                                                AND [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86] <= ( {1} * ( {2} + 1 ) ) order by [RowIndex901ACBA3-1B21-4C1F-B55A-387BA69C1C86]", sql, pagingInfo.PageSize, pagingInfo.PageIndex, pagingInfo.OrderSeq, extCols, joinTables);

            CPQuery query1 = CPQuery.From(selectCommandText, parameters);
            // 生成 COUNT 命令
            string getCountText = string.Format(@"select count(*) from ({0}) as t1", sql);
            CPQuery query2 = CPQuery.From(getCountText, parameters);
            pagingInfo.TotalRecords = query2.ExecuteScalar<int>();

            return query1;
        }

        #endregion
    }
}
