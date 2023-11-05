namespace VGManager.Api;

public static class Constants
{
    public static class SettingKeys
    {
        public const string HealthChecksSettings = "HealthChecksSettings";
    }

    public static class Cors
    {
        public static string AllowSpecificOrigins { get; set; } = "_allowSpecificOrigins";
    }

    public static class ConnectionStringKeys
    {
        public const string PostgreSql = "VGManager_API";
    }

    public static class MigrationAssemblyNames
    {
        public const string PostgreSql = "VGManager.Migrations.PostgreSql";
    }
}
