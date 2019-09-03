using Dapper;
using ProjectManageServer.Attributes;
using ProjectManageServer.Common;
using ProjectManageServer.Model;
using ProjectManageServer.Model.Login;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;

namespace ProjectManageServer.DataAccess
{
    public class LoginDataAccess
    {
        /// <summary>
        /// 登录函数
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public static UsersModel GetUserModel(UserModel userModel)
        {

            string sql = @"Selecat usercode,password,username From flc_user Where UserCode=:UserCode";

            var param = new DynamicParameters();

            param.Add(":UserCode", userModel.UserCode);

            UsersModel userModels = AppDataBase.QuerySingle<UsersModel>(sql, param);

            userModels.ConvertDescription();

            return userModels;
        }

        public static string ChangePwd(string UserCode, string NewPwd)
        {
            try
            {
                string sql = "Update DATA_UA_USER set password = :newPwd where CUSER_ID = :userCode";
                var param = new DynamicParameters();
                param.Add(":newPwd", NewPwd);
                param.Add(":userCode", UserCode);
                int num = AppDataBase.Execute(sql, param);
                return num.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static LoadMenuUnionCompany LoadMenu(string UserCode, string Language)
        {
            string sql = @"Select * from V_FLC_MENU where lan = :lan 
                                       and menucode in (select menu_id from FLC_MENU_AUTH where role_code 
                                       in (select role_code from DATA_ROLE_USER where user_code = :user_code))  and is_show=1 and is_admin in (0)
                                       order by disp_order";

            string sqladmins = @"select count(1) from data_role_user where role_code in('admins_group' ,'DATA-MANAGER')  and user_code='" + UserCode + "'";

            object o = AppDataBase.ExecuteScalar(sqladmins);

            if (o !=null && o.ToString() != "0")
            {
                sql = "select * from V_FLC_MENU where lan = :lan  and is_show=1 and is_sys <> 1 and is_admin in (0,1) order by disp_order";
            }
            if (System.IO.File.Exists("admin"))
                sql = @"select * from V_FLC_MENU where lan = :lan  and is_show=1 order by disp_order";

            var param = new DynamicParameters();

            param.Add(":user_code", UserCode);
            param.Add(":lan", Language);

            IEnumerable<LoadMenu> menuRoles = AppDataBase.Query<LoadMenu>(sql, param);

            sql = "select id,name from data_company where rownum=1";
            Company company = AppDataBase.QuerySingle<Company>(sql);

            LoadMenuUnionCompany loadMenuUnionCompany = new LoadMenuUnionCompany();         
            loadMenuUnionCompany.company = company;
            loadMenuUnionCompany.loadMenu = menuRoles;

            return loadMenuUnionCompany;

        }



    }
}
