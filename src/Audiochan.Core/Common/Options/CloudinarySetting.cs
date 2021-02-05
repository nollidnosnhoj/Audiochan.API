namespace Audiochan.Core.Common.Options
{
    public record CloudinarySetting
    {
        public string CloudName { get; init; }
        public string ApiKey { get; init; }
        public string ApiSecret { get; init; }
    }
}