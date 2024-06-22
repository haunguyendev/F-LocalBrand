namespace SWD.F_LocalBrand.API.Payloads.Responses;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}