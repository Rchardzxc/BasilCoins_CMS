using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using z.Data;

namespace InSys.Helper
{
    public class EncryptionHandler
    {
        public static string CreateClientToken(HttpRequest Request, IHttpContextAccessor accessor)
        {
            var ip = accessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("::1", "127.0.0.1");
            var ticks = DateTime.UtcNow.Ticks;

            var enc1 = string.Join(":", new string[] { ip, ticks.ToString() });

            var hashLeft = CryptoJS.Encrypt(enc1, Config.Encryption.Key, Config.Encryption.Salt);
            var hashRight = string.Join(":", new string[] { Config.Encryption.UID, ticks.ToString(), Config.Encryption.Key.CompressToUTF16(), Config.Encryption.Salt.CompressToUTF16() });

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(":", hashRight, hashLeft)));
        }

        /// <param name="source">raw value</param>
        /// <param name="key">value from SQL NEWID()</param>
        public static string EncryptUserPassword(string source, string key)
        {
            string hash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = GetHash(key + source);
            }
            return hash;
        }
        private static string GetHash(string input)
        {
            var sBuilder = new StringBuilder();
            using (SHA256 hashAlgorithm = SHA256.Create())
            {
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
            }
            return sBuilder.ToString();
        }
        /// <param name="input">raw value</param>
        /// <param name="hash">hash from database</param>
        public static bool VerifyHash(string input, string hash, string encryptionKey)
        {
            bool output = false;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var hashOfInput = GetHash(encryptionKey + input);
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                output = comparer.Compare(hashOfInput, hash) == 0;
            }
            return output;
        }

        //GENERATION OF ENCRYPTED UNIQUE ID
        public static string GenerateEncryptedKey()
        {
            string key = "";
            key = CryptoJS.Encrypt(Guid.NewGuid().ToString().Replace("-", "").ToUpperInvariant(), Config.Encryption.Key, Config.Encryption.Salt);
            return key;
        }
        public static string DecryptUserKey(string encryptedKey)
        {
            string key = "";
            key = CryptoJS.Decrypt(encryptedKey, Config.Encryption.Key, Config.Encryption.Salt);
            return key;
        }

        //ACCESS RIGHTS
        public static bool GetRights(string access, string code)
        {
            StringBuilder sb = new StringBuilder();
            if (code.IsNull("").ToString().Length == 0) return false;
            for (var i = 0; i < code.Length; i++)
            {
                byte[] a = Encoding.ASCII.GetBytes(code.Substring(i, 1));
                a[0] = (byte)(a[0] ^ 42);
                sb.Append(Encoding.ASCII.GetString(a));
            }
            return sb.ToString().IndexOf(access) >= 0 ? true : false;
        }
        public static string EncryptRights(AccessRights access, tMenus menu)
        {
            string code = "";
            string strToHash = menu.Code.IsNull("").ToString() + menu.Name.IsNull("").ToString();

            if (access.HasView) code = "view-";
            if (access.HasNew) code = code + "new-";
            if (access.HasEdit) code = code + "edit-";
            if (access.HasDelete) code = code + "delete-";
            if (access.PostJob) code = code + "postjob-";

            string hash = "";
            using (MD5 md5 = MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(strToHash))
                ).Replace("-", string.Empty);
            }
            code = code + hash;

            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < code.Length; i++)
            {
                byte[] a = Encoding.ASCII.GetBytes(code.Substring(i, 1));
                a[0] = (byte)(a[0] ^ 42);
                sb.Append(Encoding.ASCII.GetString(a));
            }
            return sb.ToString();
        }
    }
}
