using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BlaineRP.Web.Hubs.Local
{
    public class Service
    {
        public class AuthOptions
        {
            public const string TOKEN_ISSUER = "BlaineRolePlayAPI";
            public const string TOKEN_AUDIENCE = "BlaineRolePlayWI";
            public const string TOKEN_ENC_KEY = "NKG8bF3CCRiEhmUhxXiU4Yv46%r|9DxSeVHe";

            public const int TOKEN_EXPIRE_SECS = 24 * 60 * 60;

            public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TOKEN_ENC_KEY));
        }

        public record class User(string Id, string Password);

        private static List<User> _allUsers = new List<User>()
        {
            new User("brp_server_1", "EHc%K?k0t*$u"),
        };

        public static User? GetUser(string userId, string password) => _allUsers.Where(x => x.Id == userId && x.Password == password).FirstOrDefault();
    }
}
