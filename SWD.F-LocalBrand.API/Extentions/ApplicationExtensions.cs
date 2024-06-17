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
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
           


            app.UseCors("CORS");

            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapHub<MessageHub>("/messagehub");

            app.MapControllers();


        }
    }
}
