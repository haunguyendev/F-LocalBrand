using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Responses.User;
using SWD.F_LocalBrand.Business.DTO.User;
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
        #region Get all users with filter
        [HttpGet("filter")]
        [SwaggerOperation(
            Summary = "Get users with filter",
            Description = "Retrieves a list of users based on the provided filter criteria.")]
        [SwaggerResponse(200, "Users retrieved successfully", typeof(ApiResult<ListUserResponse>))]
        [SwaggerResponse(500, "An error occurred while retrieving the users", typeof(ApiResult<object>))]
        public async Task<IActionResult> GetUsersWithFilter([FromQuery] UserFilterModel request)
        {
            try
            {
                var users = await _userService.GetAllUsersWithFilterAsync(request);
                return Ok(ApiResult<ListUserResponse>.Succeed(new ListUserResponse
                {
                    Users = users
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
    }
}
