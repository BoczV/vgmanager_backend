namespace VGManager.Api;

public static class Constants
{
    public static class SettingKeys
    {
        public const string HealthChecksSettings = nameof(HealthChecksSettings);
        public const string CorsSettings = nameof(CorsSettings);
    }

    public static class Cors
    {
        public static string AllowSpecificOrigins { get; set; } = "_allowSpecificOrigins";
    }
}
