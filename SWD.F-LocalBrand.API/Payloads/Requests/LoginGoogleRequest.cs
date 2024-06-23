using Microsoft.IdentityModel.Tokens;

namespace SWD.F_LocalBrand.API.Payloads.Requests
{
    public class LoginGoogleRequest
    {
        public string IdToken { get; set; }
    }
}
