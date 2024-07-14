using Hangfire.Dashboard;
using System.IdentityModel.Tokens.Jwt;

namespace SWD.F_LocalBrand.API.Extentions
{
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Kiểm tra token từ header Authorization
            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken != null && jsonToken.Claims.Any(c => c.Type == "role" && c.Value == "Admin"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
