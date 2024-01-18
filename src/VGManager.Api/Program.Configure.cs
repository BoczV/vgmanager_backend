
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using VGManager.Api;
using VGManager.Api.HealthChecks;

static partial class Program
{
    internal static async Task<WebApplication> ConfigureAsync(WebApplication app, string specificOrigins)
    {
        var configuration = app.Configuration;

        var healthCheckSettings = configuration
                .GetSection(Constants.SettingKeys.HealthChecksSettings)
                .Get<HealthChecksSettings>()
                ?? throw new InvalidOperationException("HealthChecksSettings is missing from configuration.");

        app.UseHealthChecks("/health/startup", healthCheckSettings.Port, new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("startup")
        });

        app.UseHealthChecks("/health/liveness", healthCheckSettings.Port, new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("liveness")
        });

        app.UseHealthChecks("/health/readiness", healthCheckSettings.Port, new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("readiness")
        });

        RegisterStartupReadiness(app);

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseCors(specificOrigins);
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    private static void RegisterStartupReadiness(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        scope.ServiceProvider.GetRequiredService<StartupHealthCheck>().RegisterStartupReadiness();
    }
}
