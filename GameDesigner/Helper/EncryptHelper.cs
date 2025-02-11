﻿using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Net.Helper
{
    /// <summary>
    /// 加密解密帮助类
    /// </summary>
    public class EncryptHelper
    {
        /// <summary>
        /// 随机数形式加密法
        /// </summary>
        /// <param name="password"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ToEncrypt(int password, byte[] buffer)
        {
            return ToEncrypt(password, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 随机数形式加密法
        /// </summary>
        /// <param name="password"></param>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] ToEncrypt(int password, byte[] buffer, int index, int count)
        {
            if (password < 10000000)
                throw new Exception("密码值不能小于10000000");
            var random = new Random(password);
            for (int i = index; i < index + count; i++)
            {
                buffer[i] += (byte)random.Next(0, 255);
            }
            return buffer;
        }

        /// <summary>
        /// 随机数形式加密法--多密码
        /// </summary>
        /// <param name="passwords"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ToEncryptMulti(int[] passwords, byte[] buffer)
        {
            return ToEncryptMulti(passwords, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 随机数形式加密法--多密码
        /// </summary>
        /// <param name="passwords"></param>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] ToEncryptMulti(int[] passwords, byte[] buffer, int index, int count)
        {
            var randoms = new Random[passwords.Length];
            for (int i = 0; i < passwords.Length; i++)
            {
                if (passwords[i] < 10000000)
                    throw new Exception("密码值不能小于10000000");
                randoms[i] = new Random(passwords[i]);
            }
            for (int i = index; i < index + count; i++)
            {
                for (int j = 0; j < randoms.Length; j++)
                {
                    buffer[i] += (byte)randoms[j].Next(0, 255);
                }
            }
            return buffer;
        }

        /// <summary>
        /// 随机数形式解密法
        /// </summary>
        /// <param name="password"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ToDecrypt(int password, byte[] buffer)
        {
            return ToDecrypt(password, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 随机数形式解密法
        /// </summary>
        /// <param name="password"></param>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] ToDecrypt(int password, byte[] buffer, int index, int count)
        {
            if (password < 10000000)
                throw new Exception("密码值不能小于10000000");
            var random = new Random(password);
            for (int i = index; i < index + count; i++)
            {
                buffer[i] -= (byte)random.Next(0, 255);
            }
            return buffer;
        }

        /// <summary>
        /// 随机解密法 -- 多密码
        /// </summary>
        /// <param name="passwords"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ToDecryptMulti(int[] passwords, byte[] buffer)
        {
            return ToDecryptMulti(passwords, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 随机解密法 -- 多密码
        /// </summary>
        /// <param name="passwords"></param>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] ToDecryptMulti(int[] passwords, byte[] buffer, int index, int count)
        {
            var randoms = new Random[passwords.Length];
            for (int i = 0; i < passwords.Length; i++)
            {
                if (passwords[i] < 10000000)
                    throw new Exception("密码值不能小于10000000");
                randoms[i] = new Random(passwords[i]);
            }
            for (int i = index; i < index + count; i++)
            {
                for (int j = 0; j < randoms.Length; j++)
                {
                    buffer[i] -= (byte)randoms[j].Next(0, 255);
                }
            }
            return buffer;
        }

        /// <summary> 
        /// 加密字符串  
        /// </summary>
        /// <param name="encryptKey"></param> 
        /// <param name="text">要加密的字符串</param> 
        /// <returns>加密后的字符串</returns> 
        public static string DESEncrypt(string encryptKey, string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            var keyArray = Encoding.UTF8.GetBytes(encryptKey);
            var toEncryptArray = Encoding.UTF8.GetBytes(text);
            var rDel = new RijndaelManaged
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            var cTransform = rDel.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary> 
        /// 解密字符串  
        /// </summary>
        /// <param name="encryptKey"></param> 
        /// <param name="text">要解密的字符串</param> 
        /// <returns>解密后的字符串</returns>   
        public static string DESDecrypt(string encryptKey, string text)
        {
            if (text.Length < 2)
                return string.Empty;
            var keyArray = Encoding.UTF8.GetBytes(encryptKey);
            var toEncryptArray = Convert.FromBase64String(text);
            var rDel = new RijndaelManaged
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            var cTransform = rDel.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }

        public static string GetMD5(string text)
        {
            var bytValue = Encoding.UTF8.GetBytes(text);
            return GetMD5(bytValue);
        }

        public static string GetMD5(byte[] bytValue)
        {
            var md5 = new MD5CryptoServiceProvider();
            var bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            return sTemp.ToLower();
        }

        public static string GetMD5(Stream inputStream)
        {
            var md5 = new MD5CryptoServiceProvider();
            var bytHash = md5.ComputeHash(inputStream);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            return sTemp.ToLower();
        }

        public static string ToMD5(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                return GetMD5(stream);
            }
        }

        public static string ToSHA256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                var builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
