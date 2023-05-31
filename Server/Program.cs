using Azure.Identity;
using ClassHub.Server.Controllers;
using ClassHub.Server.Middleware;
using Microsoft.Extensions.Azure;
namespace ClassHub {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();
            builder.Services.AddAzureClients(clientBuilder => {
                // Add a KeyVault client
                clientBuilder.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
                // Add a Storage account client
                clientBuilder.AddBlobServiceClient(builder.Configuration.GetSection("Storage"));
                // Use DefaultAzureCredential by default
                clientBuilder.UseCredential(new DefaultAzureCredential());
            });

            var app = builder.Build();

            // Register middleware
            app.UseMiddleware<TokenValidationMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseWebAssemblyDebugging();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
    
            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
            app.MapHub<RealTimeSubmitHubController>("/realtimesubmithub");
            app.MapHub<LectureHubController>("/lecturehub");
            app.Run();
        }
    }
}