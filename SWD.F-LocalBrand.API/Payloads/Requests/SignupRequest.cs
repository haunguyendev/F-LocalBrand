namespace SWD.F_LocalBrand.API.Payloads.Requests;

public class SignupRequest
{
    public string UserName { get; set; }

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; }

    public string Address { get; set; }

    public string Image { get; set; } = null!;

    public int RoleId { get; set; }
}