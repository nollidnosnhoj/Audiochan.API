namespace Audiochan.Core.Features.Auth.Login
{
    public record AuthResultViewModel
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public long RefreshTokenExpires { get; init; }
    }
}