using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace ProjectManageServer.Common.Filter
{
    public class CustomerTokenValidate : ISecurityTokenValidator
    {
        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            ClaimsPrincipal principal;

            try
            {
                validatedToken = null;

                var token = new JwtSecurityToken(securityToken);

                //获取到Token的一切信息
                var payload = token.Payload;
                 
                var _AutoID = (from t in payload where t.Key == "AutoID" select t.Value).FirstOrDefault();

                var _RoleCode = (from t in payload where t.Key == "RoleCode" select t.Value).FirstOrDefault();

                var _Usercode = (from t in payload where t.Key == "UserCode" select t.Value).FirstOrDefault();

                var _Passwords = (from t in payload where t.Key == "Passwords" select t.Value).FirstOrDefault();

                var _UserNick = (from t in payload where t.Key == "UserNick" select t.Value).FirstOrDefault(); 

                var issuer = token.Issuer;

                var key = token.SecurityKey;

                var audience = token.Audiences;

                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme); 

                identity.AddClaim(new Claim("AutoID", _AutoID.ToString()));

                identity.AddClaim(new Claim("RoleCode", _RoleCode.ToString()));

                identity.AddClaim(new Claim("UserCode", _Usercode.ToString()));

                identity.AddClaim(new Claim("Passwords", _Passwords.ToString()));

                identity.AddClaim(new Claim("UserNick", _UserNick.ToString())); 

                principal = new ClaimsPrincipal(identity);
            }
            catch
            {
                validatedToken = null;

                principal = null;
            }

            return principal;
        }
    }
}
