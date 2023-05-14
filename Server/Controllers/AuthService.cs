namespace ClassHub.Server.Controllers {
    public static class AuthService {
        public static async Task<bool> isValidToken(int id, string token) {
            //토큰 검증과정
            var client = new HttpClient();
            var url = $"https://classhubsso.azurewebsites.net/api/token/verify?user_id={id}&accessToken={token}";
            var response = await client.GetAsync(url);

            return bool.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}
