using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectManageServer.Common;
using ProjectManageServer.Interface;
using ProjectManageServer.Model;

namespace ProjectManageServer.Controllers
{
    [Route("api/[controller]/[action]")] 
    public class AuthorizationController : Controller, Microsoft.AspNetCore.Hosting.IHostingEnvironment
    {

        #region 私有字段

        private JwtSettings setting;

        private readonly IAuthorization _authorization;

        string IHostingEnvironment.EnvironmentName { get; set; }
        string IHostingEnvironment.ApplicationName { get ; set; }
        string IHostingEnvironment.WebRootPath { get { return aa; } set { aa = value; } }

        string aa = "";
        IFileProvider IHostingEnvironment.WebRootFileProvider { get ; set ; }
        string IHostingEnvironment.ContentRootPath { get ; set ; }
        IFileProvider IHostingEnvironment.ContentRootFileProvider { get ; set; }

        #endregion

        public AuthorizationController(IOptions<JwtSettings> options, IAuthorization authorization)
        {
            this._authorization = authorization;

            this.setting = options.Value;
        }



        [HttpPost]
        public string Test()
        {
            return "成功";
        }

        [HttpPost]
        public AjaxRspJson Authorize(UsersModel model)
        {
            if (ModelState.IsValid)
            {
                UsersModel userModel = new UsersModel();
                string result = _authorization.Authorization<UsersModel>(model, out userModel);

                if (result == "OK")
                {
                    var claims = new Claim[] {
                        new Claim("UserCode", userModel.UserCode),
                        new Claim("Passwords", userModel.PassWord),
                        new Claim("UserName", userModel.UserName)};

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));

                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var endtime = DateTime.Now.AddMinutes(60);

                    var expiretime = (endtime - DateTime.Parse("1970-1-1")).TotalMilliseconds;

                    var token = new JwtSecurityToken(setting.Issuer, setting.Audience, claims, DateTime.Now, endtime, creds);

                    return new AjaxRspJson() { RspCode = RspStatus.Successed, ObjectData = new { Token = new JwtSecurityTokenHandler().WriteToken(token), Expires = expiretime }, RspMsg = "获取登录信息成功!" };
                }
                else
                {
                    return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = result };
                }
            }
            else
            {
                return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = "登录失败,请稍后重试!" };
            }

            //return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = "登录失败,请稍后重试!" };
        }

        [HttpGet]
        public AjaxRspJson Authorize(string UserCode, string PassWord)
        {
            if (ModelState.IsValid)
            {
                UsersModel userModel = new UsersModel();

                string result = _authorization.Authorization<UsersModel>(UserCode, PassWord, out userModel);

                if (result == "OK")
                {
                    var claims = new Claim[] {
                        new Claim("UserCode", userModel.UserCode),
                        new Claim("Passwords", userModel.PassWord),
                        new Claim("UserName", userModel.UserName)};

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));

                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var endtime = DateTime.Now.AddMinutes(60);

                    var expiretime = (endtime - DateTime.Parse("1970-1-1")).TotalMilliseconds;

                    var token = new JwtSecurityToken(setting.Issuer, setting.Audience, claims, DateTime.Now, endtime, creds);

                    return new AjaxRspJson() { RspCode = RspStatus.Successed, ObjectData = new { Token = new JwtSecurityTokenHandler().WriteToken(token), Expires = expiretime }, RspMsg = "获取登录信息成功!" };
                }
                else
                {
                    return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = result };
                }
            }
            else
            {
                return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = "登录失败,请稍后重试!" };
            }
        }



        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public AjaxRspJson GetUserInfo()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;

            var UserCode = claimIdentity.FindFirst(a => a.Type == "UserCode").Value;

            var UserNick = claimIdentity.FindFirst(a => a.Type == "UserNick").Value;

            var RoleCode = claimIdentity.FindFirst(a => a.Type == "RoleCode").Value;

            var RoleList = _authorization.GetRoles();

            Dictionary<string, object> dic = new Dictionary<string, object>();

            dic.Add("UserCode", UserCode);

            dic.Add("RoleCode", RoleCode);

            dic.Add("UserNick", UserNick);

            dic.Add("RoleArray", RoleList);

            return new AjaxRspJson { RspCode = RoleCode != null ? RspStatus.Successed : RspStatus.Failed, ObjectData = dic, RspMsg = RoleCode != null ? "查询成功" : "未找到该用户数据" };

        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public AjaxRspJson InitMenuInfo()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;

            var UserCode = claimIdentity.FindFirst(a => a.Type == "UserCode").Value; 

            var RoleCode = claimIdentity.FindFirst(a => a.Type == "RoleCode").Value;

            List<Dictionary<string, object>> menuList = _authorization.GetAuthorizationMenuInfo(UserCode, RoleCode);

            return new AjaxRspJson { RspCode = RoleCode != null ? RspStatus.Successed : RspStatus.Failed, ObjectData = menuList, RspMsg = RoleCode != null ? "获取权限菜单成功" : "获取权限菜单失败" };

        }

        /// <summary>
        /// 所有的错误异常处理
        /// </summary>
        /// <returns></returns>
        public AjaxRspJson AllErrorHandler()
        { 
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            var error = feature?.Error;

            AppLog.WriteLog(error);

            return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = error.Message };
        }
    }

}