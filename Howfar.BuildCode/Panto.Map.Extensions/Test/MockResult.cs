using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Map.Extensions.Test
{
	/// <summary>
	/// 用于在数据访问时，绕过对数据库的访问，直接返回结果。
	/// 注意：这个类仅仅用于单元测试，它的所有方法都不是线程安全的，不能用于多线程环境。
	/// </summary>
	public static class MockResult
	{
		private static Queue s_queue = null;

		/// <summary>
		/// 为后面的数据访问操作指定返回结果，如果需要为后续的多个数据访问操作指定返回值，需要多次调用这个方法。
		/// 每次执行真实的数据访问时，将会使用这个方法提供的模拟数据，当多次调用时，会形成一个先进先出的队列，
		/// 每次数据访问时，会从队列中取出（并从队列中移除）一个模拟结果直接做为返回值。
		/// 注意：这个类仅仅用于单元测试，它的所有方法都不是线程安全的，不能用于多线程环境。
		/// </summary>
		/// <param name="result"></param>
		public static void PushResult(object result)
		{
			if( s_queue == null )
				s_queue = new Queue(10);

			s_queue.Enqueue(result);
		}

		/// <summary>
		/// 清除之前没有用过的模拟数据。
		/// 注意：这个类仅仅用于单元测试，它的所有方法都不是线程安全的，不能用于多线程环境。
		/// </summary>
		public static void Clear()
		{
			if( s_queue == null )
				return;

			s_queue.Clear();
		}

		/// <summary>
		/// 从队列的尾队获取一个可以做为数据访问返回值的模拟数据
		/// </summary>
		/// <returns></returns>
		internal static object GetResult()
		{
			if( s_queue == null )
				return null;

			if( s_queue.Count > 0 )
				return s_queue.Dequeue();

			return null;
		}
	}
}
