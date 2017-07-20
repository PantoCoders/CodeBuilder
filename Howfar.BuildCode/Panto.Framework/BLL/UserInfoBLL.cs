using Panto.Framework.Entity;
using Panto.Map.Extensions.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panto.Framework.BLL
{
    public class UserInfoBLL : StandardBLL<UserInfo>
    {
        #region 标准方法

        /// <summary>
        /// 新增方法
        /// </summary>
        /// <param name="entity">待更新的实体</param>
        /// <returns>大于0表示成功，否则失败。</returns>
        public override int Insert(UserInfo entity)
        {
            return base.Insert(entity);
        }

        /// <summary>
        /// 修改方法
        /// </summary>
        /// <param name="entity">待修改的实体</param>
        /// <returns>大于0表示成功，否则失败。</returns>
        public override int Update(UserInfo entity)
        {
            return base.Update(entity);
        }

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="argsObject">参数</param>
        /// <param name="filter">过滤条件（参数化sql的过滤条件语句）</param>
        /// <returns>实体列表</returns>
        public override List<UserInfo> GetList(object argsObject, string filter)
        {
            return base.GetList(argsObject, filter);
        }

        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <param name="oid">主键</param>
        /// <returns>实体详细信息</returns>
        public UserInfo GetDetail(Guid oid)
        {
            return base.GetSingle(oid);
        }

        /// <summary>
        /// 获取个数
        /// </summary>
        /// <param name="argsObject">参数</param>
        /// <param name="filter">过滤条件（参数化sql的过滤条件语句）</param>
        /// <returns>个数</returns>
        public override int GetCount(object argsObject, string filter)
        {
            return base.GetCount(argsObject, filter);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <param name="pagingInfo">分页信息</param>
        /// <returns>分页列表</returns>
        public List<UserInfo> GetPageList(PagingInfo pagingInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 校验方法
        /// </summary>
        /// <param name="entity">实体信息</param>
        public override void CheckBefore(UserInfo entity)
        {
            base.CheckBefore(entity);
        }

        #endregion

        #region 扩展方法
        /// <summary>
        /// 自定义查询返回单实体
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UserInfo GetInfoBy(string filter, object query)
        {
            return CPQuery.From(string.Format(@"select * from v_user where {0}", filter), query).ToSingle<UserInfo>();
        }

        //修改密码
        public bool UpdatePassword(string table, string password, string idname, Guid idvalue)
        {
            string sql = string.Format(@"update {0} set Password='{1}' where {2}='{3}'", table, password, idname, idvalue);
            return CPQuery.From(sql).ExecuteNonQuery() > 0;
        }
        #endregion
    }
}
