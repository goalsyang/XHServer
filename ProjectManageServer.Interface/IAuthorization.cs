using ProjectManageServer.Model;
using System.Collections.Generic;

namespace ProjectManageServer.Interface
{
    public interface IAuthorization
    {

        string Authorization<T>(string UserCode, string PassWord,out T t) where T : UsersModel;

        string Authorization<T>(T entity, out T t) where T : UsersModel;

        IEnumerable<RoleModel> GetRoles();

        List<Dictionary<string, object>> GetAuthorizationMenuInfo(string userCode, string roleCode);

    }
}
