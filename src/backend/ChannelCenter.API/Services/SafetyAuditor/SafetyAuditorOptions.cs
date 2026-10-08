namespace ChannelCenter.API.Services.SafetyAuditor;

public class SafetyAuditorOptions
{
    public const string SectionName = "SafetyAuditor";

    public string BaseUrl { get; set; } = "https://proud-stone-0c1f07700.1.azurestaticapps.net";
    public string InternalServiceKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public int MaxResponseBytes { get; set; } = 262_144;
}
