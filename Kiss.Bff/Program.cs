using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.CreateBootstrapLogger();

Log.Information("Starting up");

try
{
	// Add services to the container.

	builder.Services.AddControllers();
	builder.Services.AddHsts(x =>
	{
		x.MaxAge = TimeSpan.FromDays(7 * 26);
		x.IncludeSubDomains = true;
		x.Preload = true;
	});

	builder.Host.UseSerilog((ctx, services, lc) => lc
		.ReadFrom.Configuration(builder.Configuration)
		.Enrich.FromLogContext());

	var app = builder.Build();

	// Configure the HTTP request pipeline.

	app.UseHttpsRedirection();

	app.UseSerilogRequestLogging();

	if (!app.Environment.IsDevelopment())
	{
		app.UseHsts();
		app.UseExceptionHandler("/Error");
	}

	app.UseAuthorization();

	app.MapControllers();

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
