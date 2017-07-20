using Panto.Framework.Entity;
using Panto.Framework.Login;
using Panto.Map.Extensions.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panto.Framework
{
    /// <summary>
    /// 权限类型
    /// </summary>
    public enum RightEnum
    {
        /// <summary>
        /// 系统
        /// </summary>
        System = 0,
        /// <summary>
        /// 模块
        /// </summary>
        Module = 1,
        /// <summary>
        /// 菜单
        /// </summary>
        Menu = 2,
        /// <summary>
        /// 功能点
        /// </summary>
        Action = 3
    }

    /// <summary>
    /// 授权级别
    /// </summary>
    public enum AuthorizeLevel
    {
        /// <summary>
        /// 任何人
        /// </summary>
        everyone,
        /// <summary>
        /// 登陆者
        /// </summary>
        logined,
        /// <summary>
        /// 授权者
        /// </summary>
        authorized
    }

    public class PermissionHelper
    {
        /// <summary>
        ///校验权限是否有权限
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="type"> 0 系统，1 模块，2 菜单，3 功能点</param>
        /// <returns></returns>
        public static bool ValidFuncPermission(string functionCode, RightEnum type)
        {
            UserInfo loginUser = UserManager.GetCurrentUserInfo();
            if (loginUser.IsAdmin)
            {
                return true;
            }
            else
            {
                var parameters = new
                {
                    UserID = loginUser.UserID,
                    FuncMenuCode = functionCode
                };

                var sql = @"SELECT COUNT(*) 
                            FROM (
                                SELECT ItemID FROM dbo.KH_StationRight st
                                INNER JOIN (
	                                SELECT StationID,EmployeeID FROM dbo.KH_StationUser
	                                WHERE SchoolID=@SchoolID
	                                UNION
	                                SELECT DefaultStationID,EmployeeID FROM dbo.KH_Employee
	                                WHERE SchoolID=@SchoolID
                                ) sr ON sr.StationID = st.StationID
                                WHERE sr.EmployeeID=@UserID AND st.ItemType=@ItemType
                                UNION
                                SELECT ItemID FROM dbo.KH_UserRight
                                WHERE UserID=@UserID AND ItemType=@ItemType
	                            UNION
                                SELECT ItemID FROM dbo.KH_RoleRight
                                WHERE  ItemType =@ItemType AND  RoleID IN (SELECT RoleID FROM dbo.KH_RoleUser WHERE UserID=@UserID) 
                            )m
                            INNER JOIN (
                             SELECT NodeID AS ItemID FROM dbo.PDRZ_SystemModule
                             WHERE NodeCode=@FunctionCode
                             UNION
                             SELECT MenuID FROM dbo.PDRZ_Menu
                             WHERE MenuCode=@FunctionCode
                             UNION
                             SELECT FunctionID FROM dbo.PDRZ_MenuFunction
                             WHERE FunctionCode=@FunctionCode
                            ) n ON n.ItemID = m.ItemID";
                return CPQuery.From(sql, new { SchoolID = loginUser.SchoolID, UserID = loginUser.UserID, ItemType = type, FunctionCode = functionCode }).ExecuteScalar<int>() > 0;
            }
        }

        /// <summary>
        /// 获取菜单下有权限的功能点
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public static List<string> GetMenuFunctionCode(string menuCode)
        {
            return GetMenuFunctionCodeList(menuCode).Select(t => t.Code).ToList();
        }

        /// <summary>
        /// 获取菜单下有权限的功能点
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public static List<PermissionInfo> GetMenuFunctionCodeList(string menuCode)
        {
            UserInfo loginUser = UserManager.GetCurrentUserInfo();
            string sql = string.Empty;
            if (loginUser.IsAdmin)
            {
                sql = @"SELECT mn.FunctionCode,mn.FunctionName,mn.OrderNo,mn.Event FROM dbo.PDRZ_Menu mu
                        INNER JOIN dbo.PDRZ_MenuFunction mn ON mn.MenuID = mu.MenuID
                        WHERE mu.MenuCode=@MenuCode ORDER BY mn.OrderNo ASC ";
            }
            else
            {
                sql = @"SELECT mn.FunctionCode,mn.FunctionName,mn.OrderNo,mn.Event FROM dbo.PDRZ_Menu mu
                        INNER JOIN dbo.PDRZ_MenuFunction mn ON mn.MenuID = mu.MenuID
                        WHERE mu.MenuCode=@MenuCode AND  mn.FunctionID IN 
                        (
                                SELECT ItemID FROM dbo.KH_StationRight st
                                INNER JOIN (
	                                SELECT StationID,EmployeeID FROM dbo.KH_StationUser
	                                WHERE SchoolID=@SchoolID
	                                UNION
	                                SELECT DefaultStationID,EmployeeID FROM dbo.KH_Employee
	                                WHERE SchoolID=@SchoolID
                                ) sr ON sr.StationID = st.StationID
                                WHERE sr.EmployeeID=@UserID AND st.ItemType=3
                                UNION
                                SELECT ItemID FROM dbo.KH_UserRight
                                WHERE UserID=@UserID AND ItemType=3
	                            UNION
                                SELECT ItemID FROM dbo.KH_RoleRight
                                WHERE  ItemType =3 AND  RoleID IN (SELECT RoleID FROM dbo.KH_RoleUser WHERE UserID=@UserID) 
                        ) ORDER BY mn.OrderNo ASC ";

            }
            DataTable dt = CPQuery.From(sql, new { SchoolID = loginUser.SchoolID, UserID = loginUser.UserID, MenuCode = menuCode }).FillDataTable();
            List<PermissionInfo> permList = new List<PermissionInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                permList.Add(new PermissionInfo()
                {
                    Code = dr["FunctionCode"].ToString(),
                    Name = dr["FunctionName"].ToString(),
                    Event = dr["Event"].ToString(),
                    Sort = string.IsNullOrWhiteSpace(dr["OrderNo"].ToString())
                    ? 0 : Convert.ToInt32(dr["OrderNo"]),
                });
            }
            return permList;

        }

        /// <summary>
        /// 获取菜单下有权限的功能点
        /// </summary>
        /// <param name="menuUrl"></param>
        /// <returns></returns>
        public static List<PermissionInfo> GetMenuFunctionCodeListByMenuUrl(string menuUrl)
        {
            UserInfo loginUser = UserManager.GetCurrentUserInfo();
            string sql = string.Empty;
            if (loginUser.IsAdmin)
            {
                sql = @"SELECT mn.FunctionCode,mn.FunctionName,mn.OrderNo,mn.Event FROM dbo.PDRZ_Menu mu
                        INNER JOIN dbo.PDRZ_MenuFunction mn ON mn.MenuID = mu.MenuID
                        WHERE mu.MenuUrl=@MenuUrl ORDER BY mn.OrderNo ASC ";
            }
            else
            {
                sql = @"SELECT mn.FunctionCode,mn.FunctionName,mn.OrderNo,mn.Event FROM dbo.PDRZ_Menu mu
                        INNER JOIN dbo.PDRZ_MenuFunction mn ON mn.MenuID = mu.MenuID
                        WHERE mu.MenuUrl=@MenuUrl AND  mn.FunctionID IN 
                        (
                                SELECT ItemID FROM dbo.KH_StationRight st
                                INNER JOIN (
	                                SELECT StationID,EmployeeID FROM dbo.KH_StationUser
	                                WHERE SchoolID=@SchoolID
	                                UNION
	                                SELECT DefaultStationID,EmployeeID FROM dbo.KH_Employee
	                                WHERE SchoolID=@SchoolID
                                ) sr ON sr.StationID = st.StationID
                                WHERE sr.EmployeeID=@UserID AND st.ItemType=3
                                UNION
                                SELECT ItemID FROM dbo.KH_UserRight
                                WHERE UserID=@UserID AND ItemType=3
	                            UNION
                                SELECT ItemID FROM dbo.KH_RoleRight
                                WHERE  ItemType =3 AND  RoleID IN (SELECT RoleID FROM dbo.KH_RoleUser WHERE UserID=@UserID) 
                        ) ORDER BY mn.OrderNo ASC ";

            }
            DataTable dt = CPQuery.From(sql, new { SchoolID = loginUser.SchoolID, UserID = loginUser.UserID, MenuUrl = menuUrl }).FillDataTable();
            List<PermissionInfo> permList = new List<PermissionInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                permList.Add(new PermissionInfo()
                {
                    Code = dr["FunctionCode"].ToString(),
                    Name = dr["FunctionName"].ToString(),
                    Event = dr["Event"].ToString(),
                    Sort = string.IsNullOrWhiteSpace(dr["OrderNo"].ToString())
                    ? 0 : Convert.ToInt32(dr["OrderNo"]),
                });
            }
            return permList;

        }
    }
}
