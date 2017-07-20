using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using Panto.Map.Extensions.Exception;


namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 增加MockResult，允许在数据访问时指定模拟数据，绕过真实的SQLSERVER操作。
    /// BY FISH LI
    /// </summary>
    internal class ConnectionManager : IDisposable
    {
        private Stack<TransactionStackItem> _transactionModes = new Stack<TransactionStackItem>();

        public ConnectionManager()
        {
        }

        private ConnectionInfo OpenTopStackInfo()
        {
            TransactionStackItem item = _transactionModes.Peek();

            ConnectionInfo info = item.Info;

            // 打开连接，并根据需要开启事务
            if (info.Connection == null)
            {
                info.Connection = new SqlConnection(info.ConnectionString);

                info.Connection.Open();

                EventManager.FireConnectionOpened(info.Connection);
            }

            if (item.EnableTranscation && info.Transaction == null)
            {
                info.Transaction = info.Connection.BeginTransaction();
            }

            return info;
        }

        public T ExecuteCommand<T>(SqlCommand command, Func<SqlCommand, T> func)
        {
            if (command == null)
                throw new ArgumentNullException("command");


            // 如果存在模拟数据，则直接用模拟数据做为返回结果。
            object mockResult = Panto.Map.Extensions.Test.MockResult.GetResult();
            if (mockResult != null)
                return (T)mockResult;

            ConnectionInfo info = OpenTopStackInfo();

            // 设置命令的连接以及事务对象
            command.Connection = info.Connection;

            if (info.Transaction != null)
                command.Transaction = info.Transaction;

            object userData = EventManager.FireBeforeExecute(command);
            try
            {
                T result = func(command);

                EventManager.FireAfterExecute(command, userData);

                return result;
            }
            catch (System.Exception ex)
            {
                EventManager.FireOnException(command, ex, userData);
                throw;
            }
            finally
            {
                // 让命令与连接，事务断开，避免这些资源外泄。
                command.Connection = null;
                command.Transaction = null;
            }
        }



        public SqlBulkCopy CreateSqlBulkCopy(SqlBulkCopyOptions copyOptions)
        {
            ConnectionInfo info = OpenTopStackInfo();

            SqlConnection conn = info.Connection as SqlConnection;

            if (conn == null)
            {
                throw new InvalidOperationException("只支持在SqlServer环境下使用SqlBulkCopy");
            }

            SqlTransaction tran = info.Transaction as SqlTransaction;

            return new SqlBulkCopy(conn, copyOptions, tran);
        }


        internal void PushTransactionMode(TransactionMode mode, string connectionString, string providerName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName");
            }

            TransactionStackItem stackItem = new TransactionStackItem();
            stackItem.Mode = mode;

            foreach (TransactionStackItem item in _transactionModes)
            {
                if (item.Info.ConnectionString == connectionString && item.Info.ProviderName == providerName)
                {
                    stackItem.Info = item.Info;
                    stackItem.EnableTranscation = item.EnableTranscation;
                    stackItem.CanClose = false;
                    break;
                }
            }

            if (stackItem.Info == null)
            {
                ConnectionInfo info = new ConnectionInfo();
                info.ConnectionString = connectionString;
                info.ProviderName = providerName;
                stackItem.Info = info;
                stackItem.CanClose = true;
            }

            if (mode == TransactionMode.Required)
            {
                stackItem.EnableTranscation = true;
            }

            _transactionModes.Push(stackItem);
        }

        internal bool PopTransactionMode()
        {
            if (_transactionModes.Count == 0)
            {
                return false;
            }

            TransactionStackItem current = _transactionModes.Pop();

            if (current.Mode == TransactionMode.Required)
            {

                bool required = false;

                foreach (TransactionStackItem item in _transactionModes)
                {
                    if (item.Info.IsSame(current.Info) && item.Mode == TransactionMode.Required)
                    {
                        required = true;
                        break;
                    }
                }

                if (required == false)
                {
                    if (current.Info.Transaction != null)
                    {

                        ////MySQL的事务如果不提交,必须显示调用Rollback。
                        ////Dispose不包含回滚事务动作
                        //if( current.Info.IsCommit == false ) {
                        //    current.Info.Transaction.Rollback();
                        //}

                        //current.Info.Transaction.Dispose();
                        //由于MySQL中,没有重写Dispose(disposing)方法.
                        //导致调用DBTransaction类使用了基类的空方法.没有正确释放事务
                        IDisposable ids = current.Info.Transaction as IDisposable;
                        ids.Dispose();

                        current.Info.Transaction = null;
                    }
                }

            }

            if (current.CanClose && current.Info.Connection != null)
            {
                //current.Info.Connection.Dispose();
                //current.Info.Connection = null;

                //为了确保使用子类的Dispose方法.此处转换为接口调用.
                IDisposable ids = current.Info.Connection as IDisposable;
                ids.Dispose();

                current.Info.Connection = null;
            }

            return _transactionModes.Count != 0;

            //TransactionMode mode = _transactionModes.Pop();

            //if( _enableTranscation && mode == TransactionMode.Required) {

            //    //父级是否包含需要开启事务的场景
            //    bool required = _transactionModes.Contains(TransactionMode.Required);

            //    //父级不包含开启事务的场景,也就是最外层才进行销毁
            //    if( required == false ) {
            //        _enableTranscation = false;
            //        if( _transcation != null ) {
            //            _transcation.Dispose();
            //            _transcation = null;
            //        }
            //    }
            //}

            ////是否到达栈底.到达栈底后不能继续出栈,外部需要调用本类的Dispose行为
            //return _transactionModes.Count != 0;
        }

        public void Commit()
        {

            TransactionStackItem current = _transactionModes.Peek();

            if (current.Info.Connection == null && current.Mode == TransactionMode.Required)
            {
                return;
            }

            if (current.Info.Transaction == null && current.Mode == TransactionMode.Required)
            {
                return;
                //throw new InvalidOperationException("当前的作用域不支持事务操作。");
            }

            if (current.Mode != TransactionMode.Required)
            {
                throw new InvalidOperationException("未在构造函数中指定TransactionMode.Required参数,不能调用Commit方法");
            }

            current = _transactionModes.Pop();

            bool required = false;

            foreach (TransactionStackItem item in _transactionModes)
            {
                if (item.Info.IsSame(current.Info) && item.Mode == TransactionMode.Required)
                {
                    required = true;
                    break;
                }
            }

            _transactionModes.Push(current);

            if (required == false)
            {
                current.Info.Transaction.Commit();
            }

            ////取出栈顶元素进行判断
            //TransactionMode mode = _transactionModes.Peek();

            ////如果启用了事务,且事务段内不执行任何代码,直接Commit().这种场景应该是允许的.
            ////对于内部实现,就相当于连接对象都没有创建,所以此处直接返回
            //if( mode == TransactionMode.Required && _connection == null )
            //    return;	

            //if( _transcation == null )
            //    throw new InvalidOperationException("当前的作用域不支持事务操作。");

            //if( mode != TransactionMode.Required ) 
            //    throw new InvalidOperationException("未在构造函数中指定TransactionMode.Required参数,不能调用Commit方法");


            ////取出当前元素才能查找父级.
            //mode = _transactionModes.Pop();

            //if( _enableTranscation && mode == TransactionMode.Required) {

            //    //父级是否包含需要开启事务的场景
            //    bool required = _transactionModes.Contains(TransactionMode.Required);

            //    if( required == false ) {
            //        _transcation.Commit();
            //    }
            //}

            ////处理完毕后压将当前事务模式压回栈内
            //_transactionModes.Push(mode);
        }

        public void Rollback(string message)
        {
            TransactionStackItem item = _transactionModes.Peek();
            if (item.Info.Transaction == null)
                throw new InvalidOperationException("当前的作用域不支持事务操作。");

            throw new RollbackException(message);
        }

        public ConnectionInfo GetTopStackInfo()
        {
            if (_transactionModes.Count > 0)
            {
                return _transactionModes.Peek().Info;
            }

            return null;
        }


        public void Dispose()
        {
            foreach (TransactionStackItem item in _transactionModes)
            {

                if (item.Info.Transaction != null)
                {
                    item.Info.Transaction.Dispose();
                    item.Info.Transaction = null;
                }

                if (item.Info.Connection != null)
                {
                    item.Info.Connection.Dispose();
                    item.Info.Connection = null;
                }
            }

            //_transactionModes.Clear();

            //if( _connection != null ) {
            //    _connection.Dispose();
            //    _connection = null;
            //}

            //if( _transcation != null ) {
            //    _transcation.Dispose();
            //    _transcation = null;
            //}
        }
    }
}
