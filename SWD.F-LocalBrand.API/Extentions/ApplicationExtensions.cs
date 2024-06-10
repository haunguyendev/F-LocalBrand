using SWD.F_LocalBrand.API.Hubs;
using SWD.F_LocalBrand.API.Middlewares;
using SWD.F_LocalBrand.Data.DataAccess;

namespace SWD.F_LocalBrand.API.Extentions
{
    public static class ApplicationExtensions
    {
        public static void UseInfrastructure(this WebApplication app)
        {
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();

            //app.UseAuthorization();

            //app.MapControllers();
            ////app.UseHttpsRedirection();  //for product

            //app.UseAuthorization();
            if(app.Environment.IsDevelopment())
{
                //await using (var scope = app.Services.CreateAsyncScope())
                //{
                //    var dbContext = scope.ServiceProvider.GetRequiredService<>(SwdFlocalBrandContext);
                //    await dbContext.Database.MigrateAsync();
                //}

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CORS");

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapHub<MessageHub>("/messagehub");

            app.MapControllers();


        }
    }
}
