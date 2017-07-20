using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Panto.Framework
{
    /// <summary>
    /// 加密助手
    /// </summary>
    public class SecurityHelper
    {
        #region DES

        private const string DES_KEY = "12346578";
        private const string DES_IV = "abcdefgh";

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="source">明文</param>
        /// <returns>密文</returns>
        public static string EncryptDes(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(DES_KEY);
                des.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(DES_IV);
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                var sw = new StreamWriter(cs);
                sw.Write(source);
                sw.Flush();
                cs.FlushFinalBlock();
                sw.Flush();

                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="source">密文</param>
        /// <returns>明文</returns>
        public static string DecryptDes(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = Encoding.UTF8.GetBytes(DES_KEY);
                des.IV = Encoding.UTF8.GetBytes(DES_IV);

                var buffer = Convert.FromBase64String(source);
                var ms = new MemoryStream(buffer);
                var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
                var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
        }

        #endregion

        #region Password

        /// <summary>
        /// 密码解密
        /// </summary>
        /// <param name="inStr">密文</param>
        /// <returns>明文</returns>
        public static string DecryptPwd(string inStr)
        {
            int num2;
            string str2 = "";
            int num6 = Strings.Len(Strings.Trim(inStr));
            int num3 = num6 % 3;
            int num4 = num6 % 9;
            int num5 = num6 % 5;
            if ((((double)num6) / 2.0) == Conversion.Int((double)(((double)num6) / 2.0)))
            {
                num2 = num4 + num5;
            }
            else
            {
                num2 = num3 + num5;
            }
            int num7 = num6;
            for (int i = 1; i <= num7; i++)
            {
                str2 = str2 + StringType.FromChar(Strings.Chr(Strings.Asc(Strings.Mid(inStr, (num6 + 1) - i, 1)) + num2));
                if (num2 == (num3 + num5))
                {
                    num2 = num4 + num5;
                }
                else
                {
                    num2 = num3 + num5;
                }
            }
            return (str2 + Strings.Space(Strings.Len(inStr) - num6));
        }

        /// <summary>
        /// 密码加密
        /// </summary>
        /// <param name="inStr">明文</param>
        /// <returns>密文</returns>
        public static string EncryptPwd(string inStr)
        {
            string str2 = "";
            int num6 = Strings.Len(Strings.Trim(inStr));
            int num3 = num6 % 3;
            int num4 = num6 % 9;
            int num5 = num6 % 5;
            int num2 = num3 + num5;
            int num7 = num6;
            for (int i = 1; i <= num7; i++)
            {
                str2 = str2 + StringType.FromChar(Strings.Chr(Strings.Asc(Strings.Mid(inStr, (num6 + 1) - i, 1)) - num2));
                if (num2 == (num3 + num5))
                {
                    num2 = num4 + num5;
                }
                else
                {
                    num2 = num3 + num5;
                }
            }
            return (str2 + Strings.Space(Strings.Len(inStr) - num6));
        }

        #endregion


        #region Ras

        /// <summary>
        /// RAS加密(支持长字符加密)
        /// </summary>
        /// <param name="xmlPublicKey">公钥</param>
        /// <param name="EncryptString">明文</param>
        /// <returns>密文</returns>

        public static string RSAEncrypt(string xmlPublicKey, string EncryptString)
        {
            byte[] PlainTextBArray;
            byte[] CypherTextBArray;
            string Result = String.Empty;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPublicKey);
            int t = (int)(Math.Ceiling((double)EncryptString.Length / (double)50));
            //分割明文
            for (int i = 0; i <= t - 1; i++) {

                PlainTextBArray = (new UnicodeEncoding()).GetBytes(EncryptString.Substring(i * 50, EncryptString.Length - (i * 50) > 50 ? 50 : EncryptString.Length - (i * 50)));
                CypherTextBArray = rsa.Encrypt(PlainTextBArray, false);
                Result += Convert.ToBase64String(CypherTextBArray) + "ThisIsSplitMysoft";
            }
            return Result;
        }
        /// <summary>
        /// RAS解密(支持长字符解密)
        /// </summary>
        /// <param name="xmlPrivateKey">私钥</param>
        /// <param name="DecryptString">密文</param>
        /// <returns>明文</returns>
        public static string RSADecrypt(string xmlPrivateKey, string DecryptString)
        {
            byte[] PlainTextBArray;
            byte[] DypherTextBArray;
            string Result = String.Empty;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPrivateKey);

            //分割密文
            string[] mis = DecryptString.Split(new string[] { "ThisIsSplitMysoft" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < mis.Length; i++) {
                PlainTextBArray = Convert.FromBase64String(mis[i]);
                DypherTextBArray = rsa.Decrypt(PlainTextBArray, false);
                Result += (new UnicodeEncoding()).GetString(DypherTextBArray);
            }
            return Result;
        }

        /// <summary>
        /// 产生公钥和私钥对
        /// </summary>
        /// <returns>string[] 0:私钥;1:公钥</returns>
        public static string[] RSAKey()
        {
            string[] keys = new string[2];
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            keys[0] = rsa.ToXmlString(true);
            keys[1] = rsa.ToXmlString(false);
            return keys;
        }

        #endregion
    }

    
}

