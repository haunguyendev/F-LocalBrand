using Microsoft.Extensions.Logging;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.BackgroundJob.Jobs
{
    public class SendMailJob
    {
        private readonly ILogger _logger;
        private readonly EmailService _emailService;
        public SendMailJob(ILogger<SendMailJob> logger, EmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;

            
        }

        public void WriteLog(string logMessage)
        {
            _logger.LogInformation($"{DateTime.Now:yyyy-MM-dd hh:mm:ss tt} {logMessage}");

        }

        public async Task PushMail()
        {
            MailData mailRequest = new MailData();
            mailRequest.EmailToName = "haunt150603@gmail.com";
            mailRequest.EmailToId = "haunt150603@gmail.com";
            mailRequest.EmailSubject = "Test Mail background jobs";
            mailRequest.EmailBody = GetHtml();
            await _emailService.SendEmailAsync(mailRequest);


        }
        private string GetHtml()
        {
            string emailContent = @"
<div style=""width: 80%; margin: auto; padding: 20px; border: 1px solid #ccc; border-radius: 5px;"">
<h1 style=""color: #333;"">Hurry! Limited Time Offer Ending Soon!</h1>
<p style=""color: #666;"">Dear User,</p>
<p style=""color: #666;"">We hope this email finds you well.</p>
<p style=""color: #666;"">This is a friendly reminder that our special offer on Minimal API is ending soon.</p>
<p style=""color: #666;"">Offer Details:</p>
<ul style=""color: #666;"">
    <li>Discount: 50%</li>
    <li>Validity: 5 Days</li>
</ul>
<p style=""color: #666;"">Don't miss out on this opportunity to enroll in Minimal API  at a discounted rate.</p>
<p style=""color: #666;"">Act fast and secure your spot before the offer expires on 14-04-2024.</p>
<p style=""color: #666;"">For more information or to enroll, visit C#corner.</p>
<p style=""color: #666;"">Thank you for choosing C# Corner!</p>
<p style=""color: #666;"">Best Regards,</p>
<p style=""color: #666;"">JobinS<br>Content creator<br>C# Corner</p>
</div>";
            return emailContent;
        }
    }
}
