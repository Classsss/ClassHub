using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ClassHub.Server.Middleware
{
    public class TokenValidationMiddleware {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext context) {
            var id = context.Request.Headers["UserId"].FirstOrDefault();
            var token = context.Request.Headers["AccessToken"].FirstOrDefault();

            if (id != null && token != null) {
                Console.WriteLine(id + " " + token);

                if (!(await IsValidTokenAsync(id, token))) {
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsync("Invalid token");
                    return;
                }
            }

            await _next(context);
        }

        private async Task<bool> IsValidTokenAsync(string id, string accessToken) {
            Console.WriteLine("토큰 검증 함수 실행");

            var client = new HttpClient();
            var url = $"https://classhubsso.azurewebsites.net/api/token/verify?user_id={id}&accessToken={accessToken}";
            var response = await client.GetAsync(url);

            Console.WriteLine("검증 결과" + bool.Parse(await response.Content.ReadAsStringAsync()));
            return bool.Parse(await response.Content.ReadAsStringAsync());
        }
    }

    public static class TokenValidationMiddlewareExtensions {
        public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder app) {
            return app.UseMiddleware<TokenValidationMiddleware>();
        }
    }
}
