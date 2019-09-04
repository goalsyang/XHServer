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

        /// <summary>
        /// 设置对象操作给角色
        /// </summary>
        /// <param name="saveOperationAuthToRole"></param>
        /// <returns></returns>
        public string SaveOperationAuthToRole(SaveOperationAuthToRole saveOperationAuthToRole)
        {
            string role_code = saveOperationAuthToRole.role_code;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string sql = "";
                    var parm = new DynamicParameters();
                    sql = "Delete FLC_OPERATION_AUTH where role_code = :role_code";
                    parm.Add(":role_code", role_code);

                    AppDataBase.Execute(sql, parm, transaction, dbConnection);
                    sql = @"Insert into FLC_OPERATION_AUTH(id, obj_code, page, operation_id, role_code) 
                values(:id, :obj_code, :page, :operation_id, :role_code)";
                    foreach (var dr in saveOperationAuthToRole.fLC_OPEERATION_AUTHs)
                    {                      
                        int id = MethodGetSerial.getSerialNumInt("FLC_OPERATION_AUTH", transaction, dbConnection);

                        parm.Add(":id", id);
                        parm.Add(":obj_code", dr.OBJ_CODE);
                        parm.Add(":page", dr.PAGE);
                        parm.Add(":operation_id", dr.OPERATION_ID);

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

        /// <summary>
        /// 返回能够操作的角色列表
        /// </summary>
        /// <param name="user_code"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetRoleListByUser(string user_code)
        {
           IEnumerable<dynamic> data_role = null;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = " select * from data_role where role_code!='admins_group' ";
                    var parm = new DynamicParameters();
                    parm.Add(":user_code", user_code);
                    data_role = AppDataBase.Query<dynamic>(sql, parm, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return data_role;
        }

        public IEnumerable<FLC_MENU_AUTH> GetRoleMenuAuthByRole(string role_code)
        {
            IEnumerable<FLC_MENU_AUTH> fLC_MENU_s = null;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = "select * from FLC_MENU_AUTH where role_code = :role_code";
                    var parm = new DynamicParameters();
                    parm.Add(":role_code", role_code);
                    fLC_MENU_s = AppDataBase.Query<FLC_MENU_AUTH>(sql, parm, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return fLC_MENU_s;
        }

        /// <summary>
        /// 按钮列表
        /// </summary>
        /// <param name="user_code"></param>
        /// <param name="lan"></param>
        /// <returns></returns>
        public GetOperationAuthByUser GetOperationAuthByUser(string user_code, string lan)
        {
            GetOperationAuthByUser getOperationAuthByUser = new GetOperationAuthByUser();

            lan = string.IsNullOrEmpty(lan) ? "zn_CN" : lan;
            string bAdmin = "0";
            if (user_code == "admin")
                bAdmin = "1";
            string sql = string.Empty;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    var parm = new DynamicParameters();
                    parm.Add(":lan", lan);
                    parm.Add(":user_code", user_code);

                    string sql2 = @" select count(1) from DATA_ROLE_USER where user_code='" + user_code + "' and role_code in('admins_group' ,'DATA-MANAGER')";//查看该用户是否拥有管理员角色
                    string count = AppDataBase.ExecuteScalar(sql, dbConnection, null, transaction).ToString();
                    if (count != "0")
                        bAdmin = "1";

                    sql = @"select t.*,t1.value from flc_object t left join FLC_LANG t1 on t.obj_code = t1.key  where  
                    ((t.is_enable = 1 and t.is_show = 1)or(t.is_model = 0))
                    and t1.lan = :lan  and t.obj_code in (select distinct obj_code from FLC_OPERATION_AUTH df 
                    where role_code in ( select  ROLE_CODE from DATA_ROLE_USER where USER_CODE= :user_code))    order by t.id ";

                    if (bAdmin == "1")
                    {
                        sql = @"select t1.* from v_FLC_MENU t1 where lan = :lan 
                                and is_sys= 0 and is_enable = 1 and is_show = 1 order by disp_order";
                    }

                    getOperationAuthByUser.objects = AppDataBase.Query<dynamic>(sql, parm, transaction, dbConnection);

                    sql = @"select t.*,t1.value 
                            from flc_operation_auth t_m
                            left join FLC_OBJ_OPERATION  t on t_m.obj_code = t.obj_code 
                            and t_m.page = t.page and t_m.operation_id = t.operation_id
                            left join FLC_LANG t1 on t.operation_id = t1.key 
                            where t1.lan = :lan and t_m.role_code in 
                            (Select role_code from DATA_ROLE_USER where user_code = :user_code)
                            order by t_m.obj_code,t_m.page,btn_index";


                    if (bAdmin == "1")
                    {
                        sql = @"select t.*,t1.value from FLC_OBJ_OPERATION  t
                            left join FLC_LANG t1 on t.operation_id = t1.key 
                            where t1.lan = :lan
                            order by obj_code,page,btn_index";
                    }

                    getOperationAuthByUser.operation = AppDataBase.Query<FLC_OBJ_OPERATION>(sql, parm, transaction, dbConnection);


                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }



            return getOperationAuthByUser;
        }

        /// <summary>
        /// 角色的按钮权限
        /// </summary>
        /// <param name="role_code"></param>
        /// <param name="lan"></param>
        /// <returns></returns>
        public GetOperationAuthByRole GetOperationAuthByRole(string role_code, string lan)
        {
            GetOperationAuthByRole getOperationAuthBy = new GetOperationAuthByRole();

            lan = string.IsNullOrEmpty(lan) ? "zn_CN" : lan;
            string bAdmin = "0";
            if (role_code == "admins_group")
                bAdmin = "1";
            string sql = string.Empty;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    var parm = new DynamicParameters();
                    parm.Add(":lan", lan);
                    parm.Add(":role_code", role_code);

                    sql = @"select t.*,t1.value from flc_object t left join FLC_LANG t1 on t.obj_code = t1.key  where  
                    ((t.is_enable = 1 and t.is_show = 1)or(t.is_model = 0))
                    and t1.lan = :lan  and t.obj_code in (select distinct obj_code from FLC_OPERATION_AUTH df 
                    where role_code = :role_code)    order by t.id  ";

                    if (bAdmin == "1")
                    {
                        sql = @"select t.*,t1.value from FLC_OBJECT t
left join FLC_LANG t1 on t.obj_code = t1.key
where lan = :lan";

                    }

                    getOperationAuthBy.objects = AppDataBase.Query<FLC_OBJECT>(sql, parm, transaction, dbConnection);

                    sql = @"select t.*,t1.value 
from flc_operation_auth t_m
left join FLC_OBJ_OPERATION  t on t_m.obj_code = t.obj_code 
and t_m.page = t.page and t_m.operation_id = t.operation_id
left join FLC_LANG t1 on t.operation_id = t1.key 
where t1.lan = :lan and t_m.role_code = :role_code
order by t_m.obj_code,t_m.page,btn_index";

                    if (bAdmin == "1")
                    {
                        sql = @"select t.*,t1.value from FLC_OBJ_OPERATION  t
                            left join FLC_LANG t1 on t.operation_id = t1.key 
                            where t1.lan = :lan
                            order by obj_code,page,btn_index";
                    }

                    getOperationAuthBy.operation = AppDataBase.Query<FLC_OBJ_OPERATION>(sql, parm, transaction, dbConnection);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return getOperationAuthBy;
        }

        public IEnumerable<dynamic> GetAdminRolebyUser(string user_code)
        {

            IEnumerable<dynamic> data_role = null;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = @" select *from DATA_ROLE_USER where user_code='" + user_code + "' and role_code='admins_group'";//查看该用户是否拥有管理员角色

                    data_role = AppDataBase.Query<dynamic>(sql, null, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return data_role;
        }

        #endregion


    }
}
