using ClassHub.Client;
using ClassHub.Client.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClassHub.Client {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped<HttpInterceptor>();
            builder.Services.AddScoped(sp => new HttpClient(new HttpInterceptorHandler(sp.GetRequiredService<HttpInterceptor>(), new HttpClientHandler())) { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddSingleton<NavMenuTitleService>();

            builder.Services.AddScoped<SSOAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<SSOAuthenticationStateProvider>());

            builder.Services.AddAuthorizationCore();
            builder.Services.AddBlazoredModal();

            await builder.Build().RunAsync();
        }
    }
}