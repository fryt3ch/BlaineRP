using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlaineRP.Web
{
    public class Program
    {
        public static void Main(params string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAuthorization();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Hubs.Local.Service.AuthOptions.TOKEN_ISSUER,
                    ValidateAudience = true,
                    ValidAudience = Hubs.Local.Service.AuthOptions.TOKEN_AUDIENCE,
                    ValidateLifetime = true,
                    IssuerSigningKey = Hubs.Local.Service.AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = (context) =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/local"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddSignalR(options =>
            {

            }).AddHubOptions<Hubs.Local.LocalHub>(options =>
            {

            }).AddNewtonsoftJsonProtocol();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            app.MapHub<Hubs.Local.LocalHub>("/hubs/local", (options) =>
            {
                options.CloseOnAuthenticationExpiration = true;
            });

            app.UseRouting();

            app.UseAuthentication();   // добавление middleware аутентификации 
            app.UseAuthorization();   // добавление middleware авторизации 

            app.Run();
        }
    }
}
