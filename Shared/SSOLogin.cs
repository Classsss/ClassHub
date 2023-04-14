namespace ClassHub.Shared
{
    public class AccessTokenRequest
    {
        public string? AuthorizationCode { get; set; }

        public string? Role { get; set; }
    }

    public class AccessTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Role { get; set; }
    }
}