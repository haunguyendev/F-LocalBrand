

using System.IdentityModel.Tokens.Jwt;
using F_LocalBrand.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Exceptions;
using SWD.F_LocalBrand.API.Payloads.Requests;
using SWD.F_LocalBrand.API.Payloads.Requests.Customer;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IdentityService _identityService;
    private readonly UserService _userService;
    private readonly IValidator<SignupRequest> _signupValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly CustomerService _customerService;

    public AuthController(IdentityService identityService, UserService userService, IValidator<SignupRequest> signupValidator, IValidator<LoginRequest> loginValidator, CustomerService customerService)
    {
        _identityService = identityService;
        _userService = userService;
        _signupValidator = signupValidator;
        _loginValidator = loginValidator;
        _customerService = customerService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Signup([FromForm] SignupRequest req)
    {
        var validationResult = _signupValidator.Validate(req);
        if(validationResult.IsValid)
        {
            var handler = new JwtSecurityTokenHandler();
            var signupModel = req.MapToModel();
            var res = await _identityService.Signup(signupModel);
            if (!res.Authenticated)
            {
                //var resultFail = new SignupResponse
                //{
                //    //AccessToken = "Sign up fail",
                //    AccessToken = "Sign up fail"
                //};
                //return BadRequest(ApiResult<SignupResponse>.Succeed(resultFail));
                var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Sign up fail"));
                return BadRequest(resultFail);
            }
            var result = new SignupResponse
            {
                AccessToken = handler.WriteToken(res.Token),
                RefreshToken = handler.WriteToken(res.RefreshToken)
            };

            return Ok(ApiResult<SignupResponse>.Succeed(result));
        }
        else
        {
            var problemDetails = validationResult.ToProblemDetails();
            return BadRequest(problemDetails);
        }
        
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var validationResult = _loginValidator.Validate(req);
        if (validationResult.IsValid)
        {
            var loginResult = _identityService.Login(req.Username, req.Password);
            if (!loginResult.Authenticated)
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Username or password is invalid"));
                return BadRequest(result);
            }

            var handler = new JwtSecurityTokenHandler();
            var res = new LoginResponse
            {
                AccessToken = handler.WriteToken(loginResult.Token),
                RefreshToken = handler.WriteToken(loginResult.RefreshToken)
            };
            return Ok(ApiResult<LoginResponse>.Succeed(res));
            
        }
        else
        {
            var problemDetails = validationResult.ToProblemDetails();
            return BadRequest(problemDetails);
        }
        
    }
    //Recive refresh token and return new access token
    [AllowAnonymous]
    [HttpPost("access-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest req)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(req.RefreshToken);
        
        var newToken = _identityService.CreateNewToken(token);
        var res = new LoginResponse
        {
            AccessToken = handler.WriteToken(newToken),
            RefreshToken = req.RefreshToken
        };
        return Ok(ApiResult<LoginResponse>.Succeed(res));
    }

    // Check if the token is valid
    [Authorize]
    [HttpGet("user-info")]
    
    public async Task<IActionResult> GetUserInformation()
    {
        Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Split()[1];
        // Here goes your token validation logic
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BadRequestException("Authorization header is missing or invalid.");
        }
        // Decode the JWT token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Check if the token is expired
        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            throw new BadRequestException("Token has expired.");
        }

        string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        var user =await _userService.GetUserByEmail(email);
        if (user == null) 
        {
            return BadRequest("email is in valid");
        }

        // If token is valid, return success response
        return Ok(ApiResult<CheckTokenResponse>.Succeed(new CheckTokenResponse {
            User = user
        }));
    }
    [AllowAnonymous]
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] LoginGoogleRequest request)
    {
        var handler = new JwtSecurityTokenHandler();
        var res = await _identityService.LoginGoolge(request.IdToken);
        if (!res.Authenticated)
        {
            var resultFail = new SignupResponse
            {
                AccessToken = "Sign up fail"
            };
            return BadRequest(ApiResult<SignupResponse>.Succeed(resultFail));
        }
        var result = new SignupResponse
        {
            AccessToken = handler.WriteToken(res.Token),
            RefreshToken = handler.WriteToken(res.RefreshToken)

        };

        return Ok(ApiResult<SignupResponse>.Succeed(result));
    }

    [AllowAnonymous]
    [HttpPost("customer-login")]
    public IActionResult LoginCustomer([FromBody] LoginRequest req)
    {
        var validationResult = _loginValidator.Validate(req);
        if (validationResult.IsValid)
        {
            var loginResult = _identityService.LoginCustomer(req.Username, req.Password);
            if (!loginResult.Authenticated)
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Username or password is invalid"));
                return BadRequest(result);
            }

            var handler = new JwtSecurityTokenHandler();
            var res = new LoginResponse
            {
                AccessToken = handler.WriteToken(loginResult.Token),
                RefreshToken = handler.WriteToken(loginResult.RefreshToken)
            };
            return Ok(ApiResult<LoginResponse>.Succeed(res));

        }
        else
        {
            var problemDetails = validationResult.ToProblemDetails();
            return BadRequest(problemDetails);
        }

    }

    #region register customer api
    [HttpPost("customer/register-customer")]
    [SwaggerOperation(
        Summary = "Register a new customer",
        Description = "Registers a new customer with the provided details. The input model must contain valid data as specified in the constraints."
    )]
    [SwaggerResponse(200, "Customer registered successfully", typeof(ApiResult<object>))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(409, "Username already exists")]
    [SwaggerResponse(500, "An error occurred while registering the customer")]
    public async Task<IActionResult> RegisterCustomer([FromForm] CustomerCreateRequest request)
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

            if (_customerService.UsernameCusExistsAsync(request.UserName))
            {
                return Conflict(ApiResult<string>.Error("Username already exists"));
            }
            if (_customerService.EmailCusExistsAsync(request.Email))
            {
                return Conflict(ApiResult<string>.Error("Email already exists"));
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

    [Authorize]
    [HttpGet("customer-info")]

    public async Task<IActionResult> GetCustomerInformation()
    {
        Request.Headers.TryGetValue("Authorization", out var token);
        token = token.ToString().Split()[1];
        // Here goes your token validation logic
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BadRequestException("Authorization header is missing or invalid.");
        }
        // Decode the JWT token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Check if the token is expired
        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            throw new BadRequestException("Token has expired.");
        }

        string id = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
        var customer = await _customerService.GetCustomerById(int.Parse(id));
        if (customer == null)
        {
            return BadRequest("email is in valid");
        }

        // If token is valid, return success response
        return Ok(ApiResult<CheckTokenResponseCustomer>.Succeed(new CheckTokenResponseCustomer
        {
            Customer = customer
        }));
    }
}