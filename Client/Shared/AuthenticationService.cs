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
    public class SSOAuthenticationStateProvider : AuthenticationStateProvider //인증 상태를 제공하는 Provider 클래스
    {
        private readonly IJSRuntime jsRuntime;

        public SSOAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() //Access Token으로 부터 인증 상태를 도출하고 반환하는 메소드
        {
            var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            //Console.WriteLine("토큰 : " + accessToken);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ClassHubOnTheBuilding"); //대칭키 암호화

            try
            {
                var validationParameters = new TokenValidationParameters //JWT토큰 검증 정보 
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken validatedToken;
                var claimsPrincipal = tokenHandler.ValidateToken(accessToken, validationParameters, out validatedToken);

                //토큰으로부터 정보를 뽑아냄
                var code = claimsPrincipal.FindFirst("code").Value;
                var name = claimsPrincipal.FindFirst(ClaimTypes.Name).Value;
                var role = claimsPrincipal.FindFirst(ClaimTypes.Role).Value;

                return new AuthenticationState(claimsPrincipal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        public void StateChanged() //인증상태 업데이트가 필요할 때 호출하는 메소드 (예를 들면 로그인, 로그아웃)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}