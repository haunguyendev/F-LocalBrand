using Hangfire.Dashboard;
using System.IdentityModel.Tokens.Jwt;

namespace SWD.F_LocalBrand.API.Extentions
{
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {      
            return true;
        }
    }
}
