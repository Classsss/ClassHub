using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlazorMonaco;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace ClassHub.Client.Shared {
    public class AuthenticationService {
        public bool IsLoggedIn { get; set; }
    }
    public class SSOAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime jsRuntime;

        public SSOAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            Console.WriteLine("토큰 : " + accessToken);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ClassHubOnTheBuilding");

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken validatedToken;
                var claimsPrincipal = tokenHandler.ValidateToken(accessToken, validationParameters, out validatedToken);

                var code = claimsPrincipal.FindFirst("code").Value;
                var name = claimsPrincipal.FindFirst(ClaimTypes.Name).Value;
                var role = claimsPrincipal.FindFirst(ClaimTypes.Role).Value;

                Console.WriteLine($"Code: {code}");
                Console.WriteLine($"Name: {name}");
                Console.WriteLine($"Role: {role}");

                return new AuthenticationState(claimsPrincipal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
    }
}