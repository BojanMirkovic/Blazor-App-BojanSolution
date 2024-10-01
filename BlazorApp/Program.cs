using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Shared_Layer.ApiServices.Authentication;
using Shared_Layer.ApiServices.UserCRUD;
namespace BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiLocation"]) });//api https address

            //Add services IUserService,... from Shared-Layer
            builder.Services.AddScoped<IUserServices, UserServices>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

            builder.Services.AddBlazoredLocalStorage();
            // Enable Blazor's built-in authorization
            builder.Services.AddAuthorizationCore();

            await builder.Build().RunAsync();
        }
    }
}
