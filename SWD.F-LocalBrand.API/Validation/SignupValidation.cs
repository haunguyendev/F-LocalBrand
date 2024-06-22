using FluentValidation;
using SWD.F_LocalBrand.API.Payloads.Requests;

namespace SWD.F_LocalBrand.API.Validation
{
    public class SignupValidation : AbstractValidator<SignupRequest>
    {
        public SignupValidation()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required");
            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
            RuleFor(x => x.Image).NotEmpty().WithMessage("Image is required");
            RuleFor(x => x.RoleId).NotEmpty().WithMessage("Role is required");
        }

    }
}
