namespace ProjectManageServer.Common
{
    public class JwtSettings
    {

        /// <summary>
        /// 证书颁发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 允许使用的角色
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// 加密字符串
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

    }
}
