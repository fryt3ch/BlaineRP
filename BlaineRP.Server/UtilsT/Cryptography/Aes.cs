using System;
using System.IO;
using System.Text;
namespace BlaineRP.Server.UtilsT.Cryptography
{
    public static class Aes
    {
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
    }
}