using Microsoft.IdentityModel.Tokens;

namespace SWD.F_LocalBrand.API.Common.Payloads.Requests
{
    public class LoginGoogleRequest
    {
        public string IdToken { get; set; }
    }
}
