namespace SWD.F_LocalBrand.API.Payloads.Requests
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
