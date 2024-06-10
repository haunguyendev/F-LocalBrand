using Microsoft.IdentityModel.Tokens;

namespace SWD.F_LocalBrand.Business.DTO.Auth;

public class LoginResult
{
    public bool Authenticated { get; set; }
    public SecurityToken? Token { get; set; }

    public SecurityToken? RefreshToken { get; set; }
}