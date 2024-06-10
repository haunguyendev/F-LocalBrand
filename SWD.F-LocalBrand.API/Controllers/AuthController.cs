

using System.IdentityModel.Tokens.Jwt;
using F_LocalBrand.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Common.Payloads.Requests;
using SWD.F_LocalBrand.API.Common.Payloads.Responses;
using SWD.F_LocalBrand.API.Exceptions;
using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IdentityService _identityService;
    private readonly UserService _userService;
    private readonly IValidator<SignupRequest> _signupValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(IdentityService identityService, UserService userService, IValidator<SignupRequest> signupValidator, IValidator<LoginRequest> loginValidator)
    {
        _identityService = identityService;
        _userService = userService;
        _signupValidator = signupValidator;
        _loginValidator = loginValidator;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] SignupRequest req)
    {
        var validationResult = _signupValidator.Validate(req);
        if(validationResult.IsValid)
        {
            var handler = new JwtSecurityTokenHandler();
            var res = await _identityService.Signup(req);
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
    [HttpPost]
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
    [HttpPost]
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
    [HttpGet]
    public async Task<IActionResult> CheckToken()
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
}