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

                ValifyResponse response = await IsValidTokenAsync(id, token);

                if (!response.Result) {
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsync(response.Message);
                    return;
                }
            }

            await _next(context);
        }

        private async Task<ValifyResponse> IsValidTokenAsync(string id, string accessToken) {
            Console.WriteLine("토큰 검증 함수 실행");

            var client = new HttpClient();
            var url = $"https://classhubsso.azurewebsites.net/api/token/verify?user_id={id}&accessToken={accessToken}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ValifyResponse>(content);

            Console.WriteLine("검증 결과" + result.Result);
            return result;
        }

        public class ValifyResponse {
            public bool Result { get; set; }
            public string Message { get; set; }
        }
    }

    public static class TokenValidationMiddlewareExtensions {
        public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder app) {
            return app.UseMiddleware<TokenValidationMiddleware>();
        }
    }
}
