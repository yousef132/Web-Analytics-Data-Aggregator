namespace DataAggergator.Application.Dtos
{
    // overall summary for all pages and dates
    public record TopLevelOverviewDto : BaseOverViewDto
    {

    }
    public record RegisterRequest(string Email, string Password, string Name);
    public record LoginRequest(string Email, string Password);
    public record RefreshTokenRequest(string RefreshToken);
}
