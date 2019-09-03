using ProjectManageServer.Model.Login;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Interface
{
    public interface ILogin
    {
        string Login<T>(T entity, out T t) where T : UserModel;

        string ChangePwd(string UserCode, string NewPwd);

        LoadMenuUnionCompany LoadMenu(string UserCode, string Language);

    }
}
