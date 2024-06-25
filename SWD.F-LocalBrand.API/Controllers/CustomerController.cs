using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests;
using SWD.F_LocalBrand.API.Payloads.Requests.Customer;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Business.Services;
using System.Security.Claims;
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
            try
            {
                var orderHistories = await _customerService.GetOrderHistoryByCustomerId(customerId, orderId);
                if (orderHistories == null)
                {
                    var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Customer not found"));
                    return NotFound(result);
                }
                return Ok(ApiResult<OrderResponse>.Succeed(new OrderResponse
                {
                    Order = orderHistories
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<Dictionary<string, string[]>>.Fail(ex));
            }
            
        }

        //Get customer by id with customer products
        [HttpGet("customer-product/{customerId}")]
        public async Task<IActionResult> GetCustomerProductByCustomerId(int customerId)
        {
            var customer = await _customerService.GetCustomerProductByCustomerId(customerId);
            if (customer == null)
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Customer not found"));
                return NotFound(result);
            }
            return Ok(ApiResult<ListCustomerProductResponse>.Succeed(new ListCustomerProductResponse
            {
                CustomerProducts = customer
            }));
        }

        //Get customer product by customer id (see product recommended of products)
        [HttpGet("customer-product/{customerId}/recommended")]
        public async Task<IActionResult> GetCustomerProductRecommended(int customerId)
        {
            var customer = await _customerService.GetCustomerProductAndProductRecommendByCustomerId(customerId);
            if (customer == null)
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Customer not found"));
                return NotFound(result);
            }
            return Ok(ApiResult<ListCustomerProductResponse>.Succeed(new ListCustomerProductResponse
            {
                CustomerProducts = customer
            }));
        }
        #region api update user account
        [Authorize]
        [HttpPut("update-customer-account")]
        [SwaggerOperation(
           Summary = "Update customer account details",
           Description = "Updates the details of an existing customer account."
       )]
        [SwaggerResponse(StatusCodes.Status200OK, "Customer account updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Customer not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the customer account", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateCustomerAccount([FromBody] CustomerUpdateAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
                {
                    { "Errors", errors.ToArray() }
                }));
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var customerModel = request.MapToModel(userId);
                var updateResult = await _customerService.UpdateCustomerAsync(customerModel);

                if (updateResult == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Customer not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Customer account updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion

        #region api update user gmail
        [Authorize]
        [HttpPut("update-customer-gmail")]
        [SwaggerOperation(
           Summary = "Update customer Gmail details",
           Description = "Updates the details of an existing customer Gmail. Example of a valid request: {\"fullName\":\"New FullName\",\"image\":\"http://example.com/image.jpg\",\"phone\":\"1234567890\",\"address\":\"New Address\"}"
       )]
        [SwaggerResponse(StatusCodes.Status200OK, "Customer Gmail updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Customer not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the customer Gmail", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateCustomerGmail([FromBody] CustomerUpdateGmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
                {
                    { "Errors", errors.ToArray() }
                }));
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var customerModel = request.MapToModel(userId);
                var updateResult = await _customerService.UpdateCustomerAsync(customerModel);

                if (updateResult == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Customer not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Customer Gmail updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }

        #endregion
        #region register customer api
        [HttpPost("register-customer")]
        [SwaggerOperation(
            Summary = "Register a new customer",
            Description = "Registers a new customer with the provided details. The input model must contain valid data as specified in the constraints."
        )]
        [SwaggerResponse(200, "Customer registered successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(409, "Username already exists")]
        [SwaggerResponse(500, "An error occurred while registering the customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                   .Select(e => e.ErrorMessage)
                                                   .ToList();
                    return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
                    {
                        { "Errors", errors.ToArray() }
                    }));
                }

                if ( _customerService.UsernameExistsAsync(request.UserName))
                {
                    return Conflict(ApiResult<string>.Error("Username already exists"));
                }

                var customerModel = request.MapToModel();
                await _customerService.RegisterCustomerAsync(customerModel);

                return Ok(ApiResult<string>.Succeed("Customer registered successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion



    }
}

