using Dapper;
using ProjectManageServer.Common;
using ProjectManageServer.Model;
using ProjectManageServer.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace ProjectManageServer.DataAccess
{
    public class AuthorizationDataAccess
    {

        #region  NO correspondence
        /// <summary>
        /// 验证用户名和密码
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public static UsersModel GetUserModel(UsersModel userModel)
        {
            string sql = @"Select usercode,password,username From flc_user Where UserCode=@UserCode";

            var param = new DynamicParameters();

            param.Add("@UserCode", userModel.UserCode);

            UsersModel usersModels = AppDataBase.QuerySingle<UsersModel>(sql, param);

            usersModels.ConvertDescription();

            return usersModels;
        }

        public static UsersModel GetUserModel(string UserCode)
        {
            string sql = @"Select usercode,password,username From flc_user Where UserCode=:UserCode";

            var param = new DynamicParameters();

            param.Add(":UserCode", UserCode);

            UsersModel usersModels = AppDataBase.QuerySingle<UsersModel>(sql, param);

            usersModels.ConvertDescription();

            return usersModels;
        }

        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RoleModel> GetRoles()
        {
            string sql = "Select AutoID,RoleCode,RoleName From Roles";
 
            IEnumerable<RoleModel> roleModels = AppDataBase.Query<RoleModel>(sql);

            return roleModels;
        }

        /// <summary>
        /// 获取权限菜单
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public static IEnumerable<MenuRolesModel> GetAuthorizationMenuInfo(string userCode,string roleCode)
        {
            string sql = @"Select mr.RoleCode,m.MenuCode,MenuName,MenuIcon,ParentCode,MenuPath,Component,Title,AlwaysShow
                                From Menus m
                                Inner Join MenusRole mr on m.MenuCode = mr.MenuCode
                                Inner Join Users u on u.RoleCode = mr.RoleCode
                            Where mr.RoleCode=@RoleCode And u.UserCode=@UserCode";

            var param = new DynamicParameters();

            param.Add("@RoleCode", roleCode);

            param.Add("@UserCode", userCode);

            IEnumerable<MenuRolesModel> menuRoles = AppDataBase.Query<MenuRolesModel>(sql, param);

            IEnumerable<MenuRolesModel> menuRolesModels = from x in menuRoles where object.Equals(x.ParentCode, null) select x;

            foreach (MenuRolesModel item in menuRolesModels)
            {
                item.Children = (from x in menuRoles where x.ParentCode == item.MenuCode select x);
            }

            return menuRolesModels;
        }

        #endregion



       




    }
}
