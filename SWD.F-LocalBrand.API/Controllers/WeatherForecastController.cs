using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using SWD.F_LocalBrand.API.Attributes;
using SWD.F_LocalBrand.API.Payloads.Requests;
using SWD.F_LocalBrand.Business.Attributes;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Business.Services;
using System.Text.Json;

namespace SWD.F_LocalBrand.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly IConnectionMultiplexer _redis;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IResponseCacheService _responseCacheService;
        private readonly FirebaseService _firebaseService;

        private readonly UserService _userService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IResponseCacheService responseCacheService, EmailService emailService, FirebaseService firebaseService, UserService userService, IConnectionMultiplexer redis)
        {
            _logger = logger;
            _responseCacheService = responseCacheService;
            _emailService = emailService;
            _firebaseService = firebaseService;
            _userService = userService;
            _redis = redis;
        }

        [AllowAnonymous]
        [HttpGet]
        [Cache(1000)]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}
        public async Task<IActionResult> GetAsync(string keyword = null, int page = 1, int pageSize = 3)
        {
            var result = new List<WeatherForecast>()
            {
                new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TemperatureC = 20, Summary = "Hot" },
                new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)), TemperatureC = 25, Summary = "Cool" },
                new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)), TemperatureC = 30, Summary = "Warm" },
                new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)), TemperatureC = 35, Summary = "Cold" },
                new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(5)), TemperatureC = 40, Summary = "Freezing" }
            };

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await _responseCacheService.RemoveCacheRepsonseAsync("/weatherforecast|keyword");
            return Ok("Create");
        }

        [AllowAnonymous]
        [HttpPost("send-mail")]
        public async Task<IActionResult> SendMail([FromBody] MailData mailData)
        {
            var result = await _emailService.SendEmailAsync(mailData);
            if (!result)
            {
                return BadRequest("Send mail fail");
            }
            return Ok("Send mail success");
        }

        //[AllowAnonymous]
        //[HttpPost("firebase")]
        //public async Task<IActionResult> Firebase([FromForm] Test test)
        //{
        //    try
        //    {
        //        if (test.File == null || test.File.Length == 0)
        //        {
        //            return BadRequest(new { Message = "The file is empty", IsSuccess = false });
        //        }

        //        var result = await _userService.CreateUrl(test.File);
        //        return Ok(new { Result = result, IsSuccess = true });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { Message = ex.Message, IsSuccess = false });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = ex.Message, IsSuccess = false });
        //    }
        //}

        //Delete image from firebase
        [AllowAnonymous]
        [HttpDelete("firebase")]
        public async Task<IActionResult> DeleteFirebase([FromQuery] string pathFileName)
        {
            try
            {
                var result = await _firebaseService.DeleteFileFromFirebase(pathFileName);
                return Ok(new { Result = result, IsSuccess = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message, IsSuccess = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message, IsSuccess = false });
            }
        }

        [HttpPost("noti")]
        public async Task<IActionResult> AddNotification(string customerId, string orderId, string message)
        {
            var db = _redis.GetDatabase();
            var key = $"customer:{customerId}:notifications";

            var notification = new { CustomerId = customerId, OrderId = orderId, Message = message, Timestamp = DateTime.UtcNow};
            var serializedNotification = JsonSerializer.Serialize(notification);

            // Use a transaction to ensure atomicity
            var tran = db.CreateTransaction();
            await tran.ListRightPushAsync(key, serializedNotification);
            await tran.ListTrimAsync(key, -30, -1); // Keep only the last 30 items
            var committed = await tran.ExecuteAsync();

            if (committed)
            {
                return Ok();
            }

            return StatusCode(500, "Error saving notification");
        }

        [HttpGet("noti/{customerId}")]
        public async Task<IActionResult> GetNotifications(string customerId)
        {
            var db = _redis.GetDatabase();
            var key = $"customer:{customerId}:notifications";

            var notifications = await db.ListRangeAsync(key);

            var deserializedNotifications = notifications.Select(n => JsonSerializer.Deserialize<object>(n)).ToList();

            return Ok(deserializedNotifications);
        }
    }

}
