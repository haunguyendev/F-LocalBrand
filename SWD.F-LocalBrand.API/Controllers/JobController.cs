using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.BackgroundJob.Jobs;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        public JobController(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;

        }

        [HttpPost]
        [Route("create-daily-report-mail")]
        public ActionResult CreateRecurringJob()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            int hour = 18; // 6 PM
            int minute = 00;

            string cronExpression = $"{minute} {hour} * * *";

            //RecurringJob.AddOrUpdate<SendMailJob>("RecurringJob1", x => x.PushMail(), cronExpression, timeZoneInfo);
            RecurringJob.AddOrUpdate<SendMailJob>("RecurringJob1", x => x.PushMail(), "*/15 * * * * *", TimeZoneInfo.Local);

            return Ok();
        }
    }
}
