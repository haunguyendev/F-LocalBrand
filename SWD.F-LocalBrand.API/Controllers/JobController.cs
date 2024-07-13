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
        [Route("CreateBackgroundJob")]
        public ActionResult CreateBackgroundJob()
        {
            //BackgroundJob.Enqueue(() => Console.WriteLine("Background job Triggered"));

            _backgroundJobClient.Enqueue<SendMailJob>(x => x.WriteLog("Background Job Triggered"));
            return Ok();
        }


        [HttpPost]
        [Route("CreateScheduledJob")]
        public ActionResult CreateScheduledJob()
        {
            var sheduleDate = DateTime.UtcNow.AddSeconds(5);
            var dateTimeOffSet = new DateTimeOffset(sheduleDate);
            //BackgroundJob.Schedule(() => Console.WriteLine("Scheduled Job Triggered"), dateTimeOffSet);
            _backgroundJobClient.Schedule<SendMailJob>(x => x.WriteLog("Scheduled Job Triggered"), dateTimeOffSet);
            return Ok();

        }

        [HttpPost]
        [Route("CreateContinuationJob")]
        public ActionResult CreateContinuationJob()
        {
            var sheduleDate = DateTime.UtcNow.AddSeconds(5);
            var dateTimeOffSet = new DateTimeOffset(sheduleDate);
            var jobId = _backgroundJobClient.Schedule(() => Console.WriteLine("Scheduled Job 2 Triggered"), dateTimeOffSet);
            var job2Id = _backgroundJobClient.ContinueJobWith<SendMailJob>(jobId, x => x.WriteLog("Continuation Job 1 Triggered"));
            var job3Id = _backgroundJobClient.ContinueJobWith<SendMailJob>(job2Id, x => x.WriteLog("Continuation Job 2 Triggered"));
            var job4Id = _backgroundJobClient.ContinueJobWith<SendMailJob>(job3Id, x => x.WriteLog("Continuation Job 3 Triggered"));


            return Ok();
        }

        [HttpPost]
        [Route("CreateRecurringJob")]
        public ActionResult CreateRecurringJob()
        {

            RecurringJob.AddOrUpdate<SendMailJob>("RecurringJob1", x => x.PushMail(), "*/1 * * * *");


            return Ok();

        }
    }
}
