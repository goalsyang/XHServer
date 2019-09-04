using Dapper;
using ProjectManageServer.Common;
using ProjectManageServer.Model;
using ProjectManageServer.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System;
using ProjectManageServer.Model.Authorization;

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


        #region correspondence

        /// <summary>
        /// 获取全部菜单权限
        /// </summary>
        /// <param name="role"></param>
        /// <param name="lan"></param>
        public GetAllAuthByRole GetAllAuthByRole(string role, string lan)
        {
            //尽管role没有用到，考虑客户端调用的时候 传值了，为了减少更改
            //所有还是留着了

            lan = string.IsNullOrEmpty(lan) ? "zn_CN" : lan;

            GetAllAuthByRole getAllAuthByRole = new GetAllAuthByRole();

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = @"select * from v_FLC_MENU  
                                                    where lan = :lan order by disp_order ";
                    var parm = new DynamicParameters();
                    parm.Add(":lan", lan);

                    getAllAuthByRole.dtNodes = AppDataBase.Query<V_FLC_MENU>(sql, parm, null, dbConnection);

                    sql = @"select t.*,t1.value from FLC_OBJ_OPERATION t
                            left join FLC_LANG t1 on t.operation_id = t1.key order by obj_code,page,btn_index";

                    getAllAuthByRole.dtOperation = AppDataBase.Query<FLC_OBJ_OPERATION>(sql, null, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return getAllAuthByRole;
        }

        /// <summary>
        /// 对角色设置权限
        /// </summary>
        /// <param name="setAuthToRole"></param>
        /// <returns></returns>
        public string SetAuthToRole(SetAuthToRole setAuthToRole)
        {
            string Role = setAuthToRole.Role;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {

                    string sql = "Delete FLC_AUTHORIZATION_ROLE where role_code = :role_code";

                    var para = new DynamicParameters();
                    para.Add(":role_code", Role);

                    AppDataBase.Execute(sql, para, transaction, dbConnection);
                    sql = @"Insert into FLC_AUTHORIZATION_ROLE (id,obj_code, page, operation_id, role_code,menu_id,is_operation)
                        values(:id,:obj_code, :page, :operation_id, :role_code,:menu_id,:is_operation)";

                    foreach (FLC_AUTHORIZATION_ROLE dr in setAuthToRole.fLC_AUTHORIZATION_ROLEs)
                    {
                        int id = MethodGetSerial.getSerialNumInt("FLC_AUTHORIZATION_ROLE", transaction, dbConnection);

                        para = new DynamicParameters();
                        para.Add(":id", id);
                        para.Add(":obj_code", dr.OBJ_CODE);
                        para.Add(":page", dr.PAGE);
                        para.Add(":operation_id", dr.OPERATION_ID);
                        para.Add(":role_code", dr.ROLE_CODE);
                        para.Add(":menu_id", dr.MENU_ID);
                        para.Add(":is_operation", dr.IS_OPERATION);

                        AppDataBase.Execute(sql, para, transaction, dbConnection);
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return "ok";
        }

        /// <summary>
        /// 通过用户编码获取菜单
        /// </summary>
        /// <param name="user_code"></param>
        /// <param name="lan"></param>
        /// <param name="bAdmin"></param>
        /// <returns></returns>
        public IEnumerable<V_FLC_MENU> GetMenuAuthByUser(string user_code, string lan, string bAdmin)
        {
            string sql = "";
            lan = string.IsNullOrEmpty(lan) ? "zn_CN" : lan;
            if (user_code == "admins_group")
                bAdmin = "1";

            IEnumerable<V_FLC_MENU> v_FLC_MENUs = null;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    //查看该用户是否拥有管理员角色
                    sql = @" select count(1) from DATA_ROLE_USER where user_code='" + user_code + "' and role_code='admins_group'";
                    string count = AppDataBase.ExecuteScalar(sql, dbConnection, null, transaction).ToString();
                    if (count != "0")
                        bAdmin = "1";

                    var para = new DynamicParameters();
                    para.Add(":lan", lan);
                    para.Add(":user_code", user_code);

                    sql = @"select t1.* from FLC_MENU_AUTH t  left join v_FLC_MENU t1 on t.menu_id = t1.menucode
                        where lan = :lan and t.role_code in   (select role_code from DATA_ROLE_USER where user_code = :user_code) 
                        and is_sys = 0 and is_enable = 1 and is_show=1  order by disp_order";

                    if (bAdmin == "1")
                    {
                        sql = @"select t1.* from v_FLC_MENU t1 where lan = :lan 
                                and is_sys = 0 and is_enable = 1 and is_show=1 and is_admin=0 order by disp_order";
                    }

                    v_FLC_MENUs = AppDataBase.Query<V_FLC_MENU>(sql, para, transaction, dbConnection);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return v_FLC_MENUs;
        }

        /// <summary>
        /// 通过角色编码获取菜单
        /// </summary>
        /// <param name="role_code"></param>
        /// <param name="lan"></param>
        /// <returns></returns>
        public IEnumerable<V_FLC_MENU> GetMenuAuthByRole(string role_code, string lan)
        {
            string bAdmin = "0";
            if (role_code == "admins_group")
            {
                bAdmin = "1";
            }
            lan = string.IsNullOrEmpty(lan) ? "zn_CN" : lan;
            IEnumerable<V_FLC_MENU> v_FLC_MENUs = null;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string sql = @"select t1.* from FLC_MENU_AUTH t 
                        left join v_FLC_MENU t1 on t.menu_id = t1.menucode
                        where lan = :lan and t.role_code = :role_code and  menugrade = 0  order by disp_order";

                    if (bAdmin == "1")
                    {
                        sql = @"select t.* from v_FLC_MENU t  where lan = :lan  order by disp_order ";
                    }

                    var parm = new DynamicParameters();
                    parm.Add(":lan", lan);
                    parm.Add(":role_code", role_code);

                    v_FLC_MENUs = AppDataBase.Query<V_FLC_MENU>(sql, parm, transaction, dbConnection);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return v_FLC_MENUs;
        }

        /// <summary>
        /// 给角色设置菜单权限
        /// </summary>
        /// <param name="saveMenuAuthToRole"></param>
        /// <returns></returns>
        public string SaveMenuAuthToRole(SaveMenuAuthToRole saveMenuAuthToRole)
        {
            string role_code = saveMenuAuthToRole.role_code;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string sql = "";
                    var parm = new DynamicParameters();
                    sql = "Delete flc_menu_auth where role_code = :role_code";
                    parm.Add(":role_code", role_code);

                    AppDataBase.Execute(sql, parm, transaction, dbConnection);
                    sql = "Insert into flc_menu_auth(id, menu_id, role_code) values(:id, :menu_id, :role_code)";

                    foreach (FLC_MENU_AUTH item in saveMenuAuthToRole.fLC_MENU_AUTHs)
                    {
                        int id = MethodGetSerial.getSerialNumInt("flc_menu_auth", transaction, dbConnection);
                        parm.Add(":id", id);
                        parm.Add(":menu_id", item.MENU_ID);
                        AppDataBase.Execute(sql, parm, transaction, dbConnection);
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return "ok";
        }



        #endregion


    }
}
