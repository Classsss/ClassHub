using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClassHub.Client.Shared {
    public class AuthenticationService {
        public bool IsLoggedIn { get; set; }
    }

    public static class UserInfo {
        public static async Task<string> GetUserNameAsync(IJSRuntime jsRuntime) {
            var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            if (accessToken != null) {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("ClassHubOnTheBuilding"); //대칭키 암호화

                try {
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
                    var name = claimsPrincipal.FindFirst(ClaimTypes.Name).Value;

                    return name;
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    return "";
                }
            }

            return "";
        }
        public static async Task<int> GetUserIdAsync(IJSRuntime jsRuntime) {
            var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            if (accessToken != null) {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("ClassHubOnTheBuilding"); //대칭키 암호화

                try {
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
                    var user_id = claimsPrincipal.FindFirst("user_id").Value;

                    return int.Parse(user_id);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    return 0;
                }
            }

            return 0;
        }

        public static async Task<string> GetRoleAsync(IJSRuntime jsRuntime) {
            var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");
            if (accessToken != null) {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("ClassHubOnTheBuilding"); //대칭키 암호화
                try {
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
                    var role = claimsPrincipal.FindFirst(ClaimTypes.Role).Value;
                    return role;
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }

            return null;
        }

        public static async Task<DateTime?> GetExpirationTimeAsync(IJSRuntime jsRuntime) {
            var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            if (accessToken != null) {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("ClassHubOnTheBuilding");

                try {
                    var validationParameters = new TokenValidationParameters {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    SecurityToken validatedToken;
                    tokenHandler.ValidateToken(accessToken, validationParameters, out validatedToken);

                    if (validatedToken is JwtSecurityToken jwtToken) {
                        return jwtToken.ValidTo;
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

            return null;
        }
    }

    public class SSOAuthenticationStateProvider : AuthenticationStateProvider //인증 상태를 제공하는 Provider 클래스
    {
        private readonly IJSRuntime jsRuntime;

        public SSOAuthenticationStateProvider(IJSRuntime jsRuntime) {
            this.jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() //Access Token으로 부터 인증 상태를 도출하고 반환하는 메소드
        {
            var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            //Console.WriteLine("토큰 : " + accessToken);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ClassHubOnTheBuilding"); //대칭키 암호화

            try {
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
                var user_id = claimsPrincipal.FindFirst("user_id").Value;
                var name = claimsPrincipal.FindFirst(ClaimTypes.Name).Value;
                var role = claimsPrincipal.FindFirst(ClaimTypes.Role).Value;

                //학번/교번과 이름을 로컬 스토리지에 저장
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", "userID", user_id);
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", "Name", name);
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", "Role", role);

                return new AuthenticationState(claimsPrincipal);
            } catch (Exception ex) {
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