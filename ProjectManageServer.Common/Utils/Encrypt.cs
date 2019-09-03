using System;
using System.Security.Cryptography;
using System.Text;

namespace ProjectManageServer.Common
{
    public static class Encrypt
    {

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="pwds"></param>
        /// <returns></returns>
        public static string EncryptMD5(this string pwds)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(pwds));

                var strResult = BitConverter.ToString(result);

                return strResult.Replace("-", "");
            }
        }

    }
}
