using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;


namespace Panto.Map.Extensions.DAL
{

	/// <summary>
	/// 事务栈成员
	/// </summary>
	internal class TransactionStackItem
	{
		public ConnectionInfo Info { get; set; }
		public TransactionMode Mode { get; set; }
		public bool EnableTranscation { get; set; }
		public bool CanClose { get; set; }
		
	}

	/// <summary>
	/// 连接信息
	/// </summary>
	internal class ConnectionInfo
	{
		public string ConnectionString { get; set; }
		public string ProviderName { get; set; }
		public SqlConnection Connection { get; set; }
		public SqlTransaction Transaction { get; set; }

		public bool IsSame(ConnectionInfo info)
		{
			if( info == null ) {
				throw new ArgumentNullException("info");
			}

			return ConnectionString == info.ConnectionString && ProviderName == info.ProviderName;
		}
	}
}
