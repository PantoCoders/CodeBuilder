using Panto.Map.Extensions.DAL;
using OptimizeReflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Panto.Framework.BLL
{
    /// <summary>
    /// 标准的实体业务逻辑类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class StandardBLL<T> where T : class, new()
    {
        /// <summary>
        /// 新增方法
        /// </summary>
        /// <param name="entity">待更新的实体</param>
        /// <returns>大于0表示成功，否则失败。</returns>
        public virtual int Insert(T entity)
        {
            var primaryKey = typeof(T).GetProperties().Where(p => p.GetCustomAttributes(typeof(DataColumnAttribute), false).Length > 0).FirstOrDefault(p => ((DataColumnAttribute)p.GetCustomAttributes(typeof(DataColumnAttribute), false)[0]).PrimaryKey);

            if (primaryKey != null)
            {
                // 如果主键有值，则不new主键
                if (primaryKey.FastGetValue(entity).ToString() == Guid.Empty.ToString())
                {
                    primaryKey.FastSetValue(entity, Guid.NewGuid());
                }
            }
            
            return (int)typeof(T).InvokeMember("Insert", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, entity, null);
        }

        /// <summary>
        /// 修改方法
        /// </summary>
        /// <param name="entity">待修改的实体</param>
        /// <returns>大于0表示成功，否则失败。</returns>
        public virtual int Update(T entity)
        {
            return (int)typeof(T).InvokeMember("Update", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, entity, null);
        }

        public virtual int Delete(T entity)
        {
            return (int)typeof(T).InvokeMember("Delete", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, entity, null);
        }

        /// <summary>
        /// 获取单个实体信息
        /// </summary>
        /// <param name="oid">主键</param>
        /// <returns>单个实体信息</returns>
        public T GetSingle(Guid oid)
        {
            return XmlCommand.From<T>(XmlCommandType.GetSingle, new { oid = oid }).ToSingle();
        }

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="argsObject">参数</param>
        /// <param name="filter">过滤条件（参数化sql的过滤条件语句）</param>
        /// <returns>实体列表</returns>
        public virtual List<T> GetList(object argsObject, string filter)
        {
            return XmlCommand.From<T>(XmlCommandType.GetList, argsObject, filter).ToList();
        }

        /// <summary>
        /// 通过Id字符串获取列表
        /// </summary>
        /// <param name="oids">Id字符串，以","分隔</param>
        /// <returns>实体列表</returns>
        public List<T> GetListByIds(string oids)
        {
            return XmlCommand.From<T>(XmlCommandType.GetListByIds, new { oids = oids }).ToList();
        }

        /// <summary>
        /// 获取个数
        /// </summary>
        /// <param name="argsObject">参数</param>
        /// <param name="filter">过滤条件（参数化sql的过滤条件语句）</param>
        /// <returns>个数</returns>
        public virtual int GetCount(object argsObject, string filter)
        {
            return XmlCommand.From<T>(XmlCommandType.GetCount, argsObject, filter).ExecuteScalar<int>();
        }

        /// <summary>
        /// 校验方法
        /// </summary>
        /// <param name="entity">实体信息</param>
        public virtual void CheckBefore(T entity)
        {
            
        }
    }
}
