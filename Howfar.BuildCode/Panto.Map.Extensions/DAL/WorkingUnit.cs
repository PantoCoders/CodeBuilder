using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;
using System.Reflection;
using Panto.Map.Extensions;


namespace Panto.Map.Extensions.DAL
{
	/// <summary>
	/// 实现工作单元的包装类型，用于实体的批量操作的延迟提交
	/// </summary>
	public sealed class WorkingUnit
	{
		private List<DBExecuteInfo> _list = new List<DBExecuteInfo>(8);

		/// <summary>
		/// 向工作单元中提交一个新增实体的请求
		/// </summary>
		/// <param name="entity"></param>
		public void Insert(BaseEntity entity)
		{
			if( entity == null )
				throw new ArgumentNullException("entity");

			CPQuery query = entity.GetCPQuery(3, entity);

			if( query == null )
				throw new InvalidOperationException("传入对象不能生成有效的SQL语句。");

			AddExecuteOjbect(query, false, entity.FuncBefore);
		}

		/// <summary>
		/// 向工作单元中提交一个删除实体的请求（不带并发冲突检查）
		/// </summary>
		/// <param name="entity"></param>
		public void Delete(BaseEntity entity)
		{
			if( entity == null )
				throw new ArgumentNullException("entity");

			CPQuery query = entity.GetCPQuery(4, entity); 
			AddExecuteOjbect(query, false, entity.FuncBefore);
		}

		/// <summary>
		/// 向工作单元中提交一个删除实体的请求（支持并发冲突检查）
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="concurrencyMode"></param>
		public void Delete(BaseEntity entity, ConcurrencyMode concurrencyMode)
		{
			if( entity == null )
				throw new ArgumentNullException("entity");

			int flag = concurrencyMode == ConcurrencyMode.TimeStamp ? 5 : 6;
			CPQuery query = entity.GetCPQuery(flag, entity);

			AddExecuteOjbect(query, true, entity.FuncBefore);
		}

		/// <summary>
		/// 向工作单元中提交一个更新实体的请求（不带并发冲突检查）
		/// </summary>
		/// <param name="entity"></param>
		public void Update(BaseEntity entity)
		{
			if( entity == null )
				throw new ArgumentNullException("entity");

			CPQuery query = entity.GetCPQuery(7, entity, entity.bakObject);

			AddExecuteOjbect(query, false, entity.FuncBefore);
		}

		/// <summary>
		/// 向工作单元中提交一个更新实体的请求（支持并发冲突检查）
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="original">用于并发检测的原始对象</param>
		/// <param name="concurrencyMode"></param>
		public void Update(BaseEntity entity, BaseEntity original, ConcurrencyMode concurrencyMode)
		{
			if( entity == null )
				throw new ArgumentNullException("entity");

			if( concurrencyMode == ConcurrencyMode.OriginalValue && object.ReferenceEquals(entity, original) )
				throw new ArgumentException("用于并发检测的原始对象不能是当前对象。");

			int flag = concurrencyMode == ConcurrencyMode.TimeStamp ? 8 : 9;
			CPQuery query = entity.GetCPQuery(flag, entity,original, entity.bakObject);

			AddExecuteOjbect(query, true, entity.FuncBefore);
		}

		private void AddExecuteOjbect(IDbExecute execute, bool concurrency, Func<CPQuery, bool> funcBefore)
		{
			if( execute == null )
				throw new ArgumentNullException("execute");

			DBExecuteInfo info = new DBExecuteInfo();
			info.DBExecute = execute;
			info.Concurrency = concurrency;
			info.FuncBefore = funcBefore;

			_list.Add(info);
		}

		/// <summary>
		/// 向工作单元中提交一个IDbExecute对象，用于延迟提交。
		/// </summary>
		/// <param name="execute"></param>
		public void AddExecuteOjbect(IDbExecute execute)
		{
			AddExecuteOjbect(execute, false, null);
		}

		/// <summary>
		/// 立即提交所有的操作请求
		/// 如果需要事务支持，请用using(ConnectionScope)的方式来实现
		/// </summary>
		public void Submit()
		{
			foreach( DBExecuteInfo info in _list ) {

				if( info.FuncBefore != null ) {
					CPQuery query = info.DBExecute as CPQuery;
					if( info.FuncBefore(query) == false ) {
						continue;
					}
				}

				int effectRows = info.DBExecute.ExecuteNonQuery();

				if( info.Concurrency && effectRows == 0 ) {
					throw new Panto.Map.Extensions.Exception.OptimisticConcurrencyException(
							"并发操作失败，本次操作没有更新任何记录，请确认当前数据行没有被其他用户更新或删除。");
				}
			}

			_list.Clear();
		}

		private class DBExecuteInfo
		{
			public bool Concurrency { get; set; }
			public IDbExecute DBExecute { get; set; }
			public Func<CPQuery, bool> FuncBefore { get; set; }
		}
	}

	
}
