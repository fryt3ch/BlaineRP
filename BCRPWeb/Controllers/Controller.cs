using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BCRPWeb.Controllers
{
    public class Controller : ControllerBase
    {
        private IHubContext<Hubs.Local.LocalHub> localHubContext;

        public Controller(IHubContext<Hubs.Local.LocalHub> hubContext)
        {
            this.localHubContext = hubContext;
        }

        [HttpPost("/hubs/local/auth")]
        public IActionResult Auth(string userId, string password)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
                return BadRequest();

            var user = Hubs.Local.Service.GetUser(userId, password);

            if (user == null)
                return Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var jwt = new JwtSecurityToken
            (
                issuer: Hubs.Local.Service.AuthOptions.TOKEN_ISSUER,
                audience: Hubs.Local.Service.AuthOptions.TOKEN_AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromSeconds(Hubs.Local.Service.AuthOptions.TOKEN_EXPIRE_SECS)),
                signingCredentials: new SigningCredentials(Hubs.Local.Service.AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
            };

            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(response));
        }
    }
}