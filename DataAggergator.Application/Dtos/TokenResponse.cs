namespace DataAggergator.Application.Dtos
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

}
