using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectManageServer.Common;
using ProjectManageServer.Interface;
using ProjectManageServer.Model;
using ProjectManageServer.Model.Login;

namespace ProjectManageServer.Controllers
{
    [Route("Login/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private JwtSettings setting;
        private readonly ILogin _login;

        public LoginController(IOptions<JwtSettings> options, ILogin login)
        {
            this._login = login;

            this.setting = options.Value;
        }

        [HttpGet]
        public AjaxRspJson Login(UserModel model)
        {        
            if (ModelState.IsValid)
            {
                UserModel userModel = new UserModel();
                string result = _login.Login<UserModel>(model, out userModel);

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

        [HttpGet]
        public AjaxRspJson ChangePwd(string UserCode, string NewPwd)
        {
            try
            {

                string result = _login.ChangePwd(UserCode, NewPwd);
                if (result == "OK")
                    return new AjaxRspJson { RspCode = RspStatus.Successed, ObjectData = null, RspMsg = "修改密码成功!" };
                else
                    return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = result };

            }
            catch (Exception ex)
            {
                return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = ex.Message };
            }
        }

        [HttpGet]
        public AjaxRspJson GetFileServerInfo()
        {
            try
            {

                System.Collections.Specialized.NameValueCollection appsetting = System.Configuration.ConfigurationManager.AppSettings;
                string FileServerAddress = appsetting["FileServerAddress"].ToString();
                string FileServerPort = appsetting["FileServerPort"].ToString();
                FileServerInfo fileServerInfo = new FileServerInfo();
                fileServerInfo.FileServerAddress = FileServerAddress;
                fileServerInfo.FileServerPort = FileServerPort;
                return new AjaxRspJson { RspCode = RspStatus.Successed, ObjectData = fileServerInfo, RspMsg = "获取文件服务成功!" };

            }
            catch (Exception ex)
            {
                return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = ex.Message };
            }
        }

        [HttpGet]
        public AjaxRspJson GetU8connString()
        {
            try
            {

                System.Collections.Specialized.NameValueCollection appsetting = System.Configuration.ConfigurationManager.AppSettings;
                string U8DBInstance = appsetting["U8DBInstance"].ToString();
                string U8DBLibname = appsetting["U8DBLibname"].ToString();
                string U8User = appsetting["U8User"].ToString();
                string U8Password = appsetting["U8Password"].ToString();

                U8connString u8ConnString = new U8connString();
                u8ConnString.U8DBInstance = U8DBInstance;
                u8ConnString.U8DBLibname = U8DBLibname;
                u8ConnString.U8User = U8User;
                u8ConnString.U8Password = U8Password;

                return new AjaxRspJson { RspCode = RspStatus.Successed, ObjectData = u8ConnString.U8connStrings, RspMsg = "获取文件服务成功!" };

            }
            catch (Exception ex)
            {
                return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = ex.Message };
            }
        }

        [HttpGet]
        public AjaxRspJson LoadMenu(string Language,string UserCode)
        {
            try
            {

                LoadMenuUnionCompany loadMenuUnion = _login.LoadMenu(UserCode, Language);
                return new AjaxRspJson { RspCode = RspStatus.Successed, ObjectData = loadMenuUnion, RspMsg = "" };

            }
            catch (Exception ex)
            {
                return new AjaxRspJson { RspCode = RspStatus.Failed, ObjectData = null, RspMsg = ex.Message };
            }
        }

    }
}
