using FluentValidation;
using SWD.F_LocalBrand.API.Common.Payloads.Requests;

namespace SWD.F_LocalBrand.API.Validation
{
    public class LoginValidation : AbstractValidator<LoginRequest>
    {
        public LoginValidation()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");  
        }
    }
}
