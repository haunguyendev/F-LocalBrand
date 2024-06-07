

using SWD.F_LocalBrand.API.Extentions;

var builder = WebApplication.CreateBuilder(args);
try { 
DotNetEnv.Env.Load();
builder.Host.AddAppConfigurations();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseInfrastructure();





app.Run();
} catch(Exception e)
{
    
}