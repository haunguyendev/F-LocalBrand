using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SWD.F_LocalBrand.API.Payloads.Requests;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        //post push notification
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PushNOti([FromBody] NotificationRequest notificationRequest)
        {
            var response = await _notificationService.SendNotification(notificationRequest.Token, notificationRequest.Title, notificationRequest.Body);
            return Ok(response);
        }
    }
}
