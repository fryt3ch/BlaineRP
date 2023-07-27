namespace BlaineRP.Server.UtilsT.Cryptography
{
    public static class BCrypt
    {
        public static string BCryptEncryptPassword(string input)
        {
            return global::BCrypt.Net.BCrypt.HashPassword(input, global::BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static bool BCryptIsPasswordValid(string input, string hash)
        {
            return global::BCrypt.Net.BCrypt.Verify(input, hash, false, global::BCrypt.Net.HashType.SHA384);
        }

    }
}