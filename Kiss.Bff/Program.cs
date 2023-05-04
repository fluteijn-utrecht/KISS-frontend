using Microsoft.EntityFrameworkCore;
using Kiss.Bff.NieuwsEnWerkinstructies.Data;
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
    builder.Services.AddKissProxy();
    builder.Services.AddKvk(builder.Configuration["KVK_BASE_URL"], builder.Configuration["KVK_API_KEY"]);
    builder.Services.AddHaalCentraal(builder.Configuration["HAAL_CENTRAAL_BASE_URL"], builder.Configuration["HAAL_CENTRAAL_API_KEY"]);
    var connStr = $"Username={builder.Configuration["POSTGRES_USER"]};Password={builder.Configuration["POSTGRES_PASSWORD"]};Host={builder.Configuration["POSTGRES_HOST"]}Database={builder.Configuration["POSTGRES_DB"]};Port={builder.Configuration["POSTGRES_PORT"]}";
    builder.Services.AddDbContext<NieuwsEnWerkinstructiesDbContext>(o => o.UseNpgsql(connStr));

    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext());

    var app = builder.Build();

    // Configure the HTTP request pipeline.

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseKissStaticFiles();
    app.UseKissSecurityHeaders();

    app.UseStrictSameSiteExternalAuthenticationMiddleware();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapKissAuthEndpoints();
    app.MapControllers();
    app.MapKissProxy();
    app.MapFallbackToIndexHtml();

    if (builder.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NieuwsEnWerkinstructiesDbContext>();
        db.Database.Migrate(); // apply the migrations
    }

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
