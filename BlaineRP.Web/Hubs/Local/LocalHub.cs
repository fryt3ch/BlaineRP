using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlaineRP.Web.Hubs.Local
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LocalHub : Hub
    {
        public async Task SendMessage(string message)
        {
            Console.WriteLine(message);
        }

        public override async Task OnConnectedAsync()
        {
            string? userId = Context.UserIdentifier;

            if (userId == null)
            {
                Context.Abort();

                return;
            }

            Console.WriteLine($"{userId} connected!");

            await base.OnConnectedAsync();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
        }

        public class UserIdProvider : IUserIdProvider
        {
            public virtual string? GetUserId(HubConnectionContext connection)
            {
                return connection.User?.Identity?.Name;
            }
        }
    }
}
