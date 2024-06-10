
using Serilog;
using SWD.F_LocalBrand.API.Extentions;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();



try
{
    Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
    Log.Information("Start F-LocalBrand API up");


    
    builder.Host.AddAppConfigurations();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Host.UseSerilog((ctx, lc) =>

    lc.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration));

    var app = builder.Build();
    app.UseInfrastructure();
    app.Run();
    
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, "Unhandled exception");

}
finally
{
    Log.Information("Shut down F-LocalBrand API complete");
    Log.CloseAndFlush();
}