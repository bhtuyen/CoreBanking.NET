using Asp.Versioning;

namespace CoreBanking.API.Bootstraping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // Register application services here
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();

        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true; // Enable reporting of API versions in responses
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(), // Read version from URL segment
                new HeaderApiVersionReader("X-Version") // Read version from custom header
            );
        });

        return builder;
    }
}