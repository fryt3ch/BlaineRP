using System.Text;
namespace BlaineRP.Server.UtilsT.Cryptography
{
    public static class MD5
    {

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

    }
}