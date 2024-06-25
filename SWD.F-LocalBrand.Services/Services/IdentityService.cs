using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SWD.F_LocalBrand.Data.DataAccess;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Data.Models;
using SWD.F_LocalBrand.Business.DTO.Auth;
using AutoMapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Business.Settings;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Business.Config;


namespace F_LocalBrand.Services;

public class IdentityService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ConfigEnv _configurationService;

    public IdentityService(ConfigEnv configurationService, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _jwtSettings = configurationService.LoadJwtSettings();
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }



    //Signup for User
    public async Task<LoginResult> Signup(SignupModel req)
    {
        var user = _unitOfWork.Users.FindByCondition(c => c.UserName == req.UserName || c.Email == req.Email).FirstOrDefault();
        if (user is not null)
        {
            throw new Exception("username or email already exists");
        }

        var createUser =new User
        {
            UserName = req.UserName,
            Password = SecurityUtil.Hash(req.Password),
            //Password = SecurityUtil.Hash("supersuperpasshashed"),
            Email = req.Email,
            Phone = req.Phone,
            Address = req.Address,
            RoleId = req.RoleId,
            RegistrationDate = DateOnly.FromDateTime(DateTime.Now),
        };
        var resCrteate = await _unitOfWork.Users.CreateAsync(createUser);
        if(resCrteate  <1) throw new Exception("Error while creating user");
        var res = await _unitOfWork.CommitAsync();
        if(res > 0)
        {
            return new LoginResult
            {
                Authenticated = true,
                Token = CreateJwtToken(createUser),
                RefreshToken = CreateJwtRefreshToken(createUser)
            };
        }
        else
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                RefreshToken = null
            };
        }
    }


    //Login for User
    public LoginResult Login(string username, string password)
    {
        var user = _unitOfWork.Users.FindByCondition(c => c.UserName == username).FirstOrDefault();


        if (user is null)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                RefreshToken = null
            };
        }
        var userRole = _unitOfWork.Roles.FindByCondition(ur => ur.Id == user.RoleId).FirstOrDefault();

        user.Role = userRole!;

        var hash = SecurityUtil.Hash(password);
        if (!user.Password.Equals(hash))
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                RefreshToken = null
            };
        }

        return new LoginResult
        {
            Authenticated = true,
            Token = CreateJwtToken(user),
            RefreshToken = CreateJwtRefreshToken(user)
        };
    }

    //Create new token with refresh token
    public SecurityToken CreateNewToken(JwtSecurityToken refreshToken)
    {
        var email = refreshToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var user = _unitOfWork.Users.FindByCondition(c => c.Email == email).FirstOrDefault();
        if (user == null)
        {
            throw new Exception("User not found");
        }
        var utcNow = DateTime.UtcNow;
        var userRole = _unitOfWork.Roles.FindByCondition(u => u.Id == user.RoleId).FirstOrDefault();
        if (userRole is null) throw new Exception("Role not found");
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, userRole.RoleName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Expires = utcNow.Add(TimeSpan.FromHours(1)),
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }


    //Generate JWT Token for User
    public SecurityToken CreateJwtToken(User user)
    {
        var utcNow = DateTime.UtcNow;
        var userRole = _unitOfWork.Roles.FindByCondition(u => u.Id == user.RoleId).FirstOrDefault();
        if (userRole is null) throw new Exception("Role not found");
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, userRole.RoleName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Expires = utcNow.Add(TimeSpan.FromHours(1)),
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    //Generate JWT Resfresh Token for User
    private SecurityToken CreateJwtRefreshToken(User user)
    {
        var utcNow = DateTime.UtcNow;
        var userRole = _unitOfWork.Roles.FindByCondition(u => u.Id == user.RoleId).FirstOrDefault();
        if (userRole is null) throw new Exception("Role not found");
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, userRole.RoleName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Expires = utcNow.Add(TimeSpan.FromHours(120)),
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    //Generate JWT Token for Customer
    public SecurityToken CreateJwtTokenCustomer(Customer user)
    {
        var utcNow = DateTime.UtcNow;
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, "Customer"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Expires = utcNow.Add(TimeSpan.FromHours(1)),
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
    public async Task<LoginResult> LoginGoolge(string token)
    {
        try
        {
            var credential = GoogleCredential.FromAccessToken(token);
            var oauth2Service = new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "F-LocalBrand",
            });
            Userinfo userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();
            var user = await _unitOfWork.Users.FindByCondition(u => u.Email == userInfo.Email).FirstOrDefaultAsync();
            if (user == null)
            {
                user = new User()
                {
                    Email = userInfo.Email,
                    UserName = userInfo.Name,
                    RoleId = 1,
                    RegistrationDate = DateOnly.FromDateTime(DateTime.Now)


                };
                await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.CommitAsync();
            }

            var tokenResponse = CreateJwtToken(user);
            var tokenRefreshResponse = CreateJwtRefreshToken(user);
            return new LoginResult
            {
                Authenticated = true,
                Token = tokenResponse,
                RefreshToken = tokenRefreshResponse
            };
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                RefreshToken = null
            };
        }


    }

    //Login for Customer
    public LoginResult LoginCustomer(string username, string password)
    {
        var user = _unitOfWork.Customers.FindByCondition(c => c.UserName == username).FirstOrDefault();


        if (user is null)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                RefreshToken = null
            };
        }

        var hash = SecurityUtil.Hash(password);
        if (!user.Password.Equals(hash))
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
                RefreshToken = null
            };
        }

        return new LoginResult
        {
            Authenticated = true,
            Token = CreateJwtTokenCustomer(user),
            RefreshToken = CreateJwtRefreshTokenCustomer(user)
        };
    }

    

    //Generate JWT Resfresh Token for Customer
    private SecurityToken CreateJwtRefreshTokenCustomer(Customer user)
    {
        var utcNow = DateTime.UtcNow;
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, "Customer"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Expires = utcNow.Add(TimeSpan.FromHours(120)),
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }


}