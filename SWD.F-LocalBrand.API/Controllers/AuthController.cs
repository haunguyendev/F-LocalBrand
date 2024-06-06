

using System.IdentityModel.Tokens.Jwt;
using F_LocalBrand.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Common.Payloads.Requests;
using SWD.F_LocalBrand.API.Common.Payloads.Responses;
using SWD.F_LocalBrand.API.Exceptions;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IdentityService _identityService;
    private readonly UserService _userService;

    public AuthController(IdentityService identityService, UserService userService)
    {
        _identityService = identityService;
        _userService = userService; 
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] SignupRequest req)
    {
        var handler = new JwtSecurityTokenHandler();
        var res = await _identityService.Signup(req);
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
            AccessToken = handler.WriteToken(res.Token)
        };

        return Ok(ApiResult<SignupResponse>.Succeed(result));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var loginResult = _identityService.Login(req.Email, req.Password);
        if (!loginResult.Authenticated)
        {
            var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Username or password is invalid"));
            return BadRequest(result);
        }

        var handler = new JwtSecurityTokenHandler();
        var res = new LoginResponse
        {
            AccessToken = handler.WriteToken(loginResult.Token),
        };
        return Ok(ApiResult<LoginResponse>.Succeed(res));
    }
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