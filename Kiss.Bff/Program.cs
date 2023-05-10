using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    builder.WebHost.ConfigureKestrel(x =>
    {
        x.AddServerHeader = false;
    });

    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddKissAuth(
        builder.Configuration["OIDC_AUTHORITY"],
        builder.Configuration["OIDC_CLIENT_ID"],
        builder.Configuration["OIDC_CLIENT_SECRET"],
        builder.Configuration["OIDC_KLANTCONTACTMEDEWERKER_ROLE"]
    );

    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext());

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.All
    });

    app.Use((context, next) =>
    {
        if (context.Request.Headers["x-forwarded-proto"] == "https")
        {
            context.Request.Scheme = "https";
        }
        return next();
    });

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseKissStaticFiles();
    //app.UseKissSecurityHeaders();

    //app.UseStrictSameSiteExternalAuthenticationMiddleware();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapKissAuthEndpoints();
    app.MapControllers();
    app.MapFallbackToIndexHtml();

    app.Run();
}
catch (Exception ex)
{
    var type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
