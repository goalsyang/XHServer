using ProjectManageServer.Common;
using ProjectManageServer.DataAccess;
using ProjectManageServer.Interface;
using ProjectManageServer.Model;
using System;
using System.Collections.Generic;

namespace ProjectManageServer.Bussiness
{
    public class AuthorizationBussiness : IAuthorization
    {
        /// <summary>
        /// 验证用户名密码
        /// </summary>
        /// <typeparam name="UsersModel"></typeparam>
        /// <param name="entity"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public string Authorization<T>(T entity, out T t) where T : UsersModel
        {
            t = default(T);

            //string passWords = entity.UserCode.EncryptMD5();

            var model = AuthorizationDataAccess.GetUserModel(entity);

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





        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RoleModel> GetRoles()
        {
            return AuthorizationDataAccess.GetRoles();
        }

        /// <summary>
        /// 获取权限菜单
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userCode"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetAuthorizationMenuInfo(string userCode, string roleCode)
        {
            IEnumerable<MenuRolesModel> rolesModels = AuthorizationDataAccess.GetAuthorizationMenuInfo(userCode, roleCode);

            List<Dictionary<string, object>> listKeyValuePairs = new List<Dictionary<string, object>>();

            foreach (var item in rolesModels)
            {
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

                keyValuePairs.Add("path", item.MenuPath);

                keyValuePairs.Add("component", item.Component);

                keyValuePairs.Add("redirect", item.MenuPath);

                keyValuePairs.Add("name", item.MenuName);

                keyValuePairs.Add("alwaysShow", item.AlwaysShow);
                 
                keyValuePairs.Add("title", item.Title);

                if (!string.IsNullOrEmpty(item.MenuIcon.NullValue()))
                {
                    keyValuePairs.Add("icon", item.MenuIcon);
                } 

                if (item.Children != null)
                {
                    List<Dictionary<string, object>> chirlden = new List<Dictionary<string, object>>();

                    foreach (var items in item.Children)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();

                        dic.Add("path", items.MenuName);

                        dic.Add("name", items.MenuName);

                        dic.Add("component", items.MenuPath);

                        dic.Add("title", items.Title);

                        if (!string.IsNullOrEmpty(items.MenuIcon.NullValue()))
                        {
                            dic.Add("icon", items.MenuIcon);
                        } 

                        chirlden.Add(dic);
                    }

                    keyValuePairs.Add("children",chirlden);
                }

                listKeyValuePairs.Add(keyValuePairs);
            }

            return listKeyValuePairs; 
        }

        string IAuthorization.Authorization<T>(T entity, out T t)
        {

            t = default(T);
            var model = AuthorizationDataAccess.GetUserModel(entity.UserCode);

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

        IEnumerable<RoleModel> IAuthorization.GetRoles()
        {
            throw new NotImplementedException();
        }

        string IAuthorization.Authorization<T>(string UserCode, string PassWord, out T t)
        {
            t = default(T);

            //string passWords = entity.UserCode.EncryptMD5();

            var model = AuthorizationDataAccess.GetUserModel(UserCode);

            if (model == null || String.IsNullOrEmpty(model.UserCode))
            {
                return "您输入的用户名不存在";
            }

            if (model.PassWord != PassWord)
            {
                return "您输入的密码不正确";
            }

            t = model as T;

            return "OK";
        }
    }
}
