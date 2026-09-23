namespace ChannelCenter.API.Services.SafetyAuditor;

public class SafetyAuditorOptions
{
    public const string SectionName = "SafetyAuditor";

    public string BaseUrl { get; set; } = "http://localhost:8001";
    public string InternalServiceKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public int MaxResponseBytes { get; set; } = 262_144;
}
