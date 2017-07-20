using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32;
using OptimizeReflection;
using System.Text.RegularExpressions;

namespace Panto.Framework
{
    /// <summary>
    /// Sql助手
    /// </summary>
    public class SqlHelper
    {
        private static string _strConn = string.Empty;
        public static void Init(string strConn)
        {
            _strConn = strConn;
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="connectName">连接字符串名称</param>
        /// <returns>连接字符串</returns>
        public static string GetConnString(string connectName)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\mysoft\" + connectName, false);

            if (key == null)
                throw new Exception("未配置注册表信息");

            return string.Format("server={0},{1};User ID={2}; Password={3}; database={4}", key.GetValue("ServerName", "").ToString(), key.GetValue("ServerProt", "").ToString(), key.GetValue("UserName", ""), SecurityHelper.DecryptPwd(key.GetValue("SaPassword", "").ToString()), key.GetValue("DBName", "").ToString());
        }

        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameterList">parameterList</param>
        /// <returns>执行结果</returns>
        public static int ExecuteNonQuery(string strConn, string sql, SqlParameter[] parameterList)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (parameterList != null)
                {
                    cmd.Parameters.AddRange(parameterList);
                }
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>执行结果</returns>
        public static int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(_strConn, sql, null);
        }

        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="obj">参数对象</param>
        /// <returns>执行结果</returns>
        public static int ExecuteNonQuery(string sql, object obj)
        {
            return ExecuteNonQuery(_strConn, sql, GetParameters(sql, obj));
        }


        public static string ExecuteNonQuery(string[] sqls)
        {
            string result = "ok";
            using (SqlConnection conn = new SqlConnection(_strConn))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    SqlTransaction st = conn.BeginTransaction();
                    int i = 0;
                    try
                    {
                        for (; i < sqls.Length; i++)
                        {
                            cmd.CommandText = sqls[i];
                            cmd.ExecuteNonQuery();
                        }
                        st.Commit();
                        result = "ok";
                    }
                    catch (Exception ex)
                    {
                        st.Rollback();
                        result = i.ToString() + "|" + ex.Message;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取数据表 
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>数据表</returns>
        public static DataTable GetDataTable(string strConn, string sql, SqlParameter[] parameterList)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (parameterList != null)
                {
                    cmd.Parameters.AddRange(parameterList);
                }
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlDataAdapter sa = new SqlDataAdapter(cmd);

                sa.Fill(dt);
                return dt;
            }
        }
        /// <summary>
        /// 获取数据表 
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>数据表</returns>
        public static DataTable GetDataTable(string strConn, string sql)
        {
            return GetDataTable(strConn, sql, null);
        }
        /// <summary>
        /// 获取数据表 
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>数据表</returns>
        public static DataTable GetDataTable(string sql, object obj)
        {
            return GetDataTable(_strConn, sql, GetParameters(sql, obj));
        }

        /// <summary>
        /// 获取数据表 
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>数据表</returns>
        public static DataTable GetDataTable(string sql)
        {
            return GetDataTable(_strConn, sql, null);
        }
        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="cmd">Sql命令</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(SqlCommand cmd)
        {
            using (SqlConnection conn = cmd.Connection)
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                DataTable dt = new DataTable();
                SqlDataAdapter sa = new SqlDataAdapter(cmd);

                sa.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string strConn, string sql, SqlParameter[] parameterList)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (parameterList != null)
                {
                    cmd.Parameters.AddRange(parameterList);
                }

                SqlDataAdapter sa = new SqlDataAdapter(cmd);

                sa.Fill(ds);
                return ds;
            }
        }
        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string sql)
        {
            return GetDataSet(_strConn, sql, null);
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string sql, object obj)
        {
            return GetDataSet(_strConn, sql, GetParameters(sql, obj));
        }


        /// <summary>
        /// 执行返回一个标量
        /// </summary>
        /// <param name="cmdText">执行内容</param>
        /// <returns>标量</returns>
        public static T ExecScalar<T>(string cmdText)
        {
            return ExecScalar<T>(_strConn, CommandType.Text, cmdText, null);
        }
        /// <summary>
        /// 执行返回一个标量
        /// </summary>
        /// <param name="cmdText">执行内容</param>
        /// <param name="cmdType">CommandType</param>
        /// <returns>标量</returns>
        public static T ExecScalar<T>(string cmdText, object obj)
        {
            return ExecScalar<T>(_strConn, CommandType.Text, cmdText, GetParameters(cmdText, obj));
        }
        /// <summary>
        /// 执行返回一个标量
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="cmdType">CommandType</param>
        /// <param name="cmdText">执行内容</param>
        /// <returns>标量</returns>
        public static T ExecScalar<T>(string strConn, CommandType cmdType, string cmdText)
        {
            return ExecScalar<T>(strConn, cmdType, cmdText, null);
        }

        /// <summary>
        /// 执行返回一个标量
        /// </summary>
        /// <param name="strConn">连接字符串</param>
        /// <param name="cmdType">CommandType</param>
        /// <param name="cmdText">执行内容</param>
        /// <param name="cmdParams">参数</param>
        /// <returns>标量</returns>
        public static T ExecScalar<T>(string strConn, CommandType cmdType, string cmdText, SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.CommandType = cmdType;

                if (cmdParams != null && cmdParams.Length > 0)
                {
                    cmd.Parameters.AddRange(cmdParams);
                }
                object obj = cmd.ExecuteScalar();
                if (obj != null && obj.ToString().ToLower() != "NULL".ToLower())
                {
                    return (T)obj;
                }
                else
                {
                    return default(T);
                }

            }
        }

        public static List<T> ToList<T>(string sql)
        {
            DataTable dt = GetDataTable(sql);

            return dt.ToEntity<T>();

        }

        /// <summary>
        /// 获取Sql参数集合
        /// </summary>
        /// <param name="parameterList">Sql参数集合</param>
        /// <param name="argsObject">参数</param>
        /// <returns>Sql参数集合</returns>
        private static SqlParameter[] GetParameters(string sql, object argsObject)
        {
            List<SqlParameter> parameterList = new List<SqlParameter>();
            if (argsObject == null)
                return new SqlParameter[0];

            if (!string.IsNullOrEmpty(sql))
            {

                Regex reg = new Regex(@"[^@@](?<p>@\w+)");
                PropertyInfo[] argProps = argsObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (Match m in reg.Matches(sql))
                {
                    // 跳过同名参数
                    if (parameterList.Count(p => p.ParameterName == m.Groups["p"].Value) > 0)
                        continue;

                    var argProp = argProps.FirstOrDefault(p => "@" + p.Name == m.Groups["p"].Value);
                    parameterList.Add(new SqlParameter()
                    {
                        ParameterName = m.Groups["p"].Value,
                        DbType = CSharp2DbType(argProp.PropertyType),
                        Direction = ParameterDirection.Input,
                        Size = 0
                    });
                }
            }

            SqlParameter[] parameters = parameterList.ToArray();

            PropertyInfo[] properties = argsObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // 为每个SqlParameter赋值。
            foreach (PropertyInfo pInfo in properties)
            {
                string name = "@" + pInfo.Name;

                // 如果传入了没有定义的参数项，则会抛出异常。
                SqlParameter p = parameters.FirstOrDefault(x => string.Compare(x.ParameterName, name, StringComparison.OrdinalIgnoreCase) == 0);
                if (p == null)
                    throw new ArgumentException(string.Format("传入的参数对象中，属性 {0} 没有定义对应的参数名。", pInfo.Name));

                p.Value = pInfo.FastGetValue(argsObject) ?? DBNull.Value;
            }

            return parameters;
        }

        /// <summary>
        /// C#类型转换为数据库类型
        /// </summary>
        /// <param name="type">C#类型</param>
        /// <returns>数据库类型</returns>
        private static DbType CSharp2DbType(Type type)
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
    }
}
