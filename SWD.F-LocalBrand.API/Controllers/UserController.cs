using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests.User;
using SWD.F_LocalBrand.API.Payloads.Responses.User;
using SWD.F_LocalBrand.Business.DTO.User;
using SWD.F_LocalBrand.Business.Services;
using System.Security.Claims;

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
        #region api update user detail
        [Authorize]
        [HttpPut("update")]
        [SwaggerOperation(
    Summary = "Update user details",
    Description = "Updates the details of an existing user account."
)]
        [SwaggerResponse(StatusCodes.Status200OK, "User account updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the user account", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateUserAccount([FromForm] UserDetailUpdateRequest request)
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
                if (request.Email != null)
                {
                    var userById = await _userService.GetUserById(userId);
                    if (userById.Email != request.Email)
                    {
                        if (await _userService.EmailExistsAsync(request.Email))
                        {
                            return Conflict(ApiResult<string>.Error("Email already exists"));
                        }
                    }
                }

                var userModel = request.MapToModel(userId);
                var updateResult = await _userService.UpdateUserAsync(userModel);

                if (updateResult == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "User not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "User account updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }


        #endregion
        #region api update status user 
        [HttpPut("status")]
        [SwaggerOperation(
    Summary = "Update user status",
    Description = "Updates the status of a specified user. The request must contain a valid user ID and a valid status value.")]
        [SwaggerResponse(200, "User status updated successfully")]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(404, "User not found")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusRequest request)
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
                await _userService.UpdateUserStatusAsync(request.UserId, request.Status);
                return Ok(ApiResult<string>.Succeed("User status updated successfully"));
            }
            catch (EntryPointNotFoundException ex)
            {
                return NotFound(ApiResult<object>.Fail(ex));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<object>.Fail(ex));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }

        #endregion
        #region api update user role
        [HttpPut("role")]
        [SwaggerOperation(
    Summary = "Change user role",
    Description = "Changes the role of a specified user. The request must contain a valid user ID and a valid role ID.")]
        [SwaggerResponse(200, "User role changed successfully")]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(404, "User or role not found")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleRequest request)
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
                await _userService.ChangeUserRoleAsync(request.UserId, request.RoleName);
                return Ok(ApiResult<string>.Succeed("User role changed successfully"));
            }
            catch (EntryPointNotFoundException ex)
            {
                return NotFound(ApiResult<object>.Fail(ex));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion

        #region update device id for user
        [Authorize]
        [HttpPut("update/deviceId/{deviceId}")]
        [SwaggerOperation(
                       Summary = "Update device ID",
                       Description = "Updates the device ID of the current user."
                   )]
        [SwaggerResponse(StatusCodes.Status200OK, "Device ID updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the user account", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateDeviceId(string deviceId)
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
                var result = await _userService.UpdateDeviceId(userId, deviceId);
                if (!result)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "User not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "User account updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }

        #endregion
    }

}
