using ProjectManageServer.DataAccess;
using ProjectManageServer.Interface;
using ProjectManageServer.Model;
using ProjectManageServer.Model.Login;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Bussiness
{
    public class LoginBussiness : ILogin
    {
        public  string ChangePwd(string UserCode, string NewPwd)
        {
            var Result = LoginDataAccess.ChangePwd(UserCode, NewPwd);
            if (Result == "0")
                return "您输入的用户名不存在";
            else if (Result == "1")
                return "OK";
            else
                return Result;
        }

        public string Login<T>(T entity, out T t) where T : UserModel
        {
            t = default(T);

            var model = LoginDataAccess.GetUserModel(entity);

            if (model == null || String.IsNullOrEmpty(model.UserCode))
            {
                return "您输入的用户名不存在";
            }

            if (model.PassWord != entity.PassWord)
            {
                return "您输入的密码不正确";
            }

            t = model as T;

            return "OK";
        }

        public LoadMenuUnionCompany LoadMenu(string UserCode, string Language)
        {
            LoadMenuUnionCompany loadMenuUnionCompany = LoginDataAccess.LoadMenu(UserCode, Language);

            return loadMenuUnionCompany;
        }

    }
}
