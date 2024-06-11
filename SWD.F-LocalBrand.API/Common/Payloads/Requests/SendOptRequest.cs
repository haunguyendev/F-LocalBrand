namespace SWD.F_LocalBrand.API.Common.Payloads.Requests
{
    public class SendOtpRequest
    {
        public string Email { get; set; } = null!;

        public bool IsResend { get; set; } // True if it's a resend request
    }
}
