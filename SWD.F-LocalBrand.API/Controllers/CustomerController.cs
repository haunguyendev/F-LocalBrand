using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Common.Payloads.Requests;
using SWD.F_LocalBrand.API.Common.Payloads.Responses;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Business.Services;
using System.Text.RegularExpressions;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly CustomerService _customerService;
        private readonly EmailService _emailService;

        public CustomerController(IMemoryCache cache, CustomerService customerService, EmailService emailService)
        {
            _cache = cache;
            _customerService = customerService;
            _emailService = emailService;
        }

        private async Task SendOtpAsync(string email, string subject)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set(email, otp, TimeSpan.FromMinutes(10));
            var mailData = new MailData
            {
                EmailToId = email,
                EmailToName = email,
                EmailSubject = subject,
                EmailBody = $"Your OTP is: {otp}"
            };

            await _emailService.SendEmailAsync(mailData);
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            if (!regex.IsMatch(request.Email))
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Invalid email format."));
                return BadRequest(result);
            }
            var checkCustomer = await _customerService.GetCustomerByUsername(request.Email);
            if (checkCustomer.Value)
            {
                if (request.IsResend)
                {
                    if (!_cache.TryGetValue(request.Email, out string _))
                    {
                        var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Email not found. Please initiate the forget password process first."));
                        return NotFound(result);
                    }
                }

                await SendOtpAsync(request.Email, request.IsResend ? "Resend OTP" : "Reset Password OTP");

                var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "OTP sent successfully." });
                return Ok(response);
            }
            return NotFound(ApiResult<Dictionary<string, string[]>>.Fail(new Exception("User is not found")));

        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            if (!regex.IsMatch(request.Email))
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Invalid email format."));
                return BadRequest(result);
            }
            if (_cache.TryGetValue(request.Email, out string otp) && otp == request.Otp)
            {
                _cache.Remove(request.Email);
                var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "OTP verified. You can now reset your password." });
                return Ok(response);
            }
            return Unauthorized(ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "Invalid OTP" }));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var checkCustomer = await _customerService.GetCustomerByUsername(request.Email);
            if (request.Password != request.ConfirmPassword)
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Password and confirm password do not match."));
                return BadRequest(result);
            }
            if (checkCustomer.Value)
            {
                var updateResult = await _customerService.UpdatePass(request.Email, request.Password);
                if (!updateResult)
                {
                    var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Failed to update password."));
                    return BadRequest(result);
                }
                else
                {
                    var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "Password updated successfully." });
                    return Ok(response);
                }
            }
            return NotFound(ApiResult<Dictionary<string, string[]>>.Fail(new Exception("User is not found")));
        }

        [HttpGet("{customerId}/orders/{orderId}/histories")]
        public async Task<IActionResult> GetOrderHistories(int customerId, int orderId)
        {
            var orderHistories = await _customerService.GetOrderHistoryByCustomerId(customerId, orderId);
            return Ok(orderHistories);
        }

        //Get customer by id with customer products
        [HttpGet("customer-product/{customerId}")]
        public async Task<IActionResult> GetCustomerProductByCustomerId(int customerId)
        {
            var customer = await _customerService.GetCustomerProductByCustomerId(customerId);
            return Ok(customer);
        }
    }
}
