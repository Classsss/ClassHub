using ClassHub.Server.Controllers;
using ClassHub.Server.Controllers.ClassHub.Server.Controllers;
using Microsoft.AspNetCore.ResponseCompression;

namespace ClassHub {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();

            var app = builder.Build();

           

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
            app.Run();
        }
    }
}