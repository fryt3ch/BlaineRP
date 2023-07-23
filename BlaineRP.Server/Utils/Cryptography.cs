﻿using System;
using System.IO;
using System.Text;

namespace BlaineRP.Server
{
    internal class Cryptography
    {
        /// <summary>Метод для получения зашифрованной строки из исходной</summary>
        /// <param name="plainText">Строка, которую необходимо зашифровать</param>
        /// <param name="key">Ключ дешифрования</param>
        /// <returns>Зашифрованная строка в формате Base64</returns>
        public static string AesEncryptString(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                System.Security.Cryptography.ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream((Stream)memoryStream, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        /// <summary>Метод для получения исходной строки из зашифрованной</summary>
        /// <param name="cipherText">Строка, которую необходимо расшифровать</param>
        /// <param name="key">Ключ дешифрования</param>
        /// <returns>Расшифрованная строка</returns>
        public static string AesDecryptString(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream((Stream)memoryStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>Метод для получения строки, зашифрованной алгоритмом MD5</summary>
        /// <param name="input">Строка, которую необходимо зашифровать</param>
        /// <returns>Зашифрованная строка в нижнем регистре</returns>
        public static string MD5EncryptString(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("X2"));

                return sb.ToString().ToLower();
            }
        }

        public static string BCryptEncryptPassword(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static bool BCryptIsPasswordValid(string input, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(input, hash, false, BCrypt.Net.HashType.SHA384);
        }
    }
}
