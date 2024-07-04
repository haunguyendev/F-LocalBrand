using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Responses.User;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }
        #region Get all list user
        [HttpGet("users")]
        [SwaggerOperation(
            Summary = "Get all users",
            Description = "Retrieves a list of all users.")]
        [SwaggerResponse(200, "All users retrieved successfully", typeof(ApiResult<ListUserResponse>))]
        [SwaggerResponse(500, "An error occurred while retrieving the users", typeof(ApiResult<object>))]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                ListUserResponse listUserResponse = new ListUserResponse()
                {
                    Users = users
                };
                
                
                return Ok(ApiResult<ListUserResponse>.Succeed(listUserResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
    }
}
