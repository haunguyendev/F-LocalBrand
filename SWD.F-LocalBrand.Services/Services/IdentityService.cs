using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Data.DataAccess;
using SWD.F_LocalBrand.Business.Settings;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Business.Common.Payloads.Requests;
using SWD.F_LocalBrand.Data.Models;


namespace F_LocalBrand.Services;

public class IdentityService
{
    private readonly JwtSettings _jwtSettings;
    private readonly SwdFlocalBrandContext _context;

    public IdentityService(IOptions<JwtSettings> jwtSettingsOptions, SWD_FLocalBrandContext context)
    {
        _jwtSettings = jwtSettingsOptions.Value;
        _context = context;
    }


    //Signup for User
    public async Task<LoginResult> Signup(SignupRequest req)
    {
        var user = _context.Users.Where(c => c.UserName == req.UserName).FirstOrDefault();
        if (user is not null)
        {
            throw new BadRequestException("username or email already exists");
        }

        var createUser = await _context.AddAsync(new User
        {
            UserName = req.UserName,
            Password = SecurityUtil.Hash(req.Password),
            //Password = SecurityUtil.Hash("supersuperpasshashed"),
            Email = req.Email,
            Phone = req.Phone,
            Address = req.Address,
            RoleId = req.RoleId,
            RegistrationDate = DateOnly.FromDateTime(DateTime.Now),
        });
        
        var res = await _context.SaveChangesAsync();
        if(res > 0)
        {
            return new LoginResult
            {
                Authenticated = true,
                Token = CreateJwtToken(createUser.Entity)
            };
        }
        else
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null
            };
        }
    }


    //Login for User
    public LoginResult Login(string username, string password)
    {
        var user = _context.Users.Where(c => c.UserName == username).FirstOrDefault();


        if (user is null)
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
            };
        }
        var userRole = _context.Roles.Where(ur => ur.RoleId == user.RoleId).FirstOrDefault();

        user.Role = userRole!;

        var hash = SecurityUtil.Hash(password);
        if (!user.Password.Equals(hash))
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
            };
        }

        return new LoginResult
        {
            Authenticated = true,
            Token = CreateJwtToken(user),
        };
    }

    //Generate JWT Token for User
    private SecurityToken CreateJwtToken(User user)
    {
        var utcNow = DateTime.UtcNow;
        var userRole = _context.Roles.Where(u => u.id == user.RoleId).FirstOrDefault();
        if (userRole is null) throw new BadRequestException("Role not found");
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.UserId.ToString()),
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

    //Generate JWT Token for Customer
    private SecurityToken CreateJwtTokenCustomer(Customer customer)
    {
        var utcNow = DateTime.UtcNow;
        //var userRole = _context..FindByCondition(u => u.RoleId == user.RoleID).FirstOrDefault();
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, customer.CustomerId.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new(JwtRegisteredClaimNames.Email, customer.Email),
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

    
}