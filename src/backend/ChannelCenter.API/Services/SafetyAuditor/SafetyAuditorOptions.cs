namespace ChannelCenter.API.Services.SafetyAuditor;

public class SafetyAuditorOptions
{
    public const string SectionName = "SafetyAuditor";

    public string BaseUrl { get; set; } = "https://channel-center-ai-service-dmccgqh9bddyfhcy.eastasia-01.azurewebsites.net";
    public string InternalServiceKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public int MaxResponseBytes { get; set; } = 262_144;
}
