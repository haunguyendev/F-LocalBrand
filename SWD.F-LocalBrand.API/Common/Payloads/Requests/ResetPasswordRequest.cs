namespace SWD.F_LocalBrand.API.Common.Payloads.Requests
{
    public class ResetPasswordRequest
    {
        //public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
