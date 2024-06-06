

using SWD.F_LocalBrand.API.Extentions;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
builder.Host.AddAppConfigurations();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseInfrastructure();
// Configure the HTTP request pipeline.




app.Run();
