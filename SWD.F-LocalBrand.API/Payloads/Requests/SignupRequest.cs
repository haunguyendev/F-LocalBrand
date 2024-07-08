using SWD.F_LocalBrand.Business.DTO;

namespace SWD.F_LocalBrand.API.Payloads.Requests;

public class SignupRequest
{
    public string UserName { get; set; }

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; }

    public string Address { get; set; }

    public IFormFile ImageUrl { get; set; } = null!;

    public int RoleId { get; set; }

    public SignupModel MapToModel()
    {
        return new SignupModel
        {
            UserName = UserName,
            Password = Password,
            Email = Email,
            Phone = Phone,
            Address = Address,
            Imageurl = ImageUrl,
            RoleId = RoleId
        };
    }   
}