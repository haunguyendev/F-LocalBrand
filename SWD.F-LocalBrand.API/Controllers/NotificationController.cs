using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        //post push notification
        [HttpPost("notification")]
        [AllowAnonymous]
        public async Task<IActionResult> PushNoti([FromBody] NotificationRequest notificationRequest)
        {
            var response = await _notificationService.SendNotification(notificationRequest.Token, notificationRequest.Title, notificationRequest.Body);
            return Ok(response);
        }


        //get notification by customer id
        [HttpGet("notifications")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get notifiations",
            Description = "Retrieves a list of notifications."
            )]
        [SwaggerResponse(StatusCodes.Status200OK, "Notifications retrieved successfully", typeof(ApiResult<ListNotification>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        public async Task<IActionResult> GetNotification([FromQuery] NotificationModel notificationModel)
        {
            try
            {
                if(notificationModel.Status == null)
                {
                    return BadRequest(ApiResult<string>.Error("Status is required"));
                }
                var listNotification = await _notificationService.GetNotificationFromRedis(notificationModel);
                return Ok(ApiResult<ListNotification>.Succeed(new ListNotification
                {
                    Notifications = listNotification
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResult<object>.Fail(ex));
            }
        }
    }
}
