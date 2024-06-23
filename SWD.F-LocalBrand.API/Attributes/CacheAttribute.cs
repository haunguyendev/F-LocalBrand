using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SWD.F_LocalBrand.Business.Settings;
using System.Text;

namespace SWD.F_LocalBrand.API.Attributes
{
    public class CacheAttribute : Attribute, IAsyncActionFilter
    {

        private readonly int _timeToLiveSeconds;

        public CacheAttribute(int timeToLiveSeconds)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //View cache whether it is already in cache
            var cacheCofiguration = context.HttpContext.RequestServices.GetRequiredService<RedisConnection>();
            if(!cacheCofiguration.Enable) // check in appsettings.json whether cache is enabled or not
            {
                await next(); // If not enable, In line, I run into Controller
                return;
            }
            

            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

            //Get cache key form url of api request
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

            //Get cache response from cache
            var cacheReponse = await cacheService.GetCachedResponseAsync(cacheKey);

            //check if cache response is not null, return cache response
            if (!string.IsNullOrEmpty(cacheReponse))
            {
                var contentResult = new ContentResult
                {
                    Content = cacheReponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }

            var executedContext = await next();
            if (executedContext.Result is OkObjectResult okObjectResult)
            {
                await cacheService.SetCacheResponseAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(_timeToLiveSeconds));
            }
            
        }

        //Generate cache key from request
        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            return keyBuilder.ToString();
        }
    }
}
