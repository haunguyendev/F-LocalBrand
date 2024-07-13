using Microsoft.Extensions.Logging;
using SWD.F_LocalBrand.Business.DTO.Report;
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
        private readonly OrderService _orderService;
        private readonly UserService _userService;
        public SendMailJob(ILogger<SendMailJob> logger, EmailService emailService,OrderService orderService,UserService userService)
        {
            _logger = logger;
            _emailService = emailService;
            _orderService = orderService;
            _userService = userService;

            
        }

        public void WriteLog(string logMessage)
        {
            _logger.LogInformation($"{DateTime.Now:yyyy-MM-dd hh:mm:ss tt} {logMessage}");

        }

        public async Task PushMail()
        {
            var report = await _orderService.GetDailyReportDataAsync(DateOnly.FromDateTime(DateTime.Now));
            var userAdmins = await _userService.GetUserByRoleAsync("Admin");

            foreach (var admin in userAdmins)
            {
                MailData mailRequest = new MailData();
                mailRequest.EmailToName = admin.UserName;
                mailRequest.EmailToId = admin.Email;
                mailRequest.EmailSubject = "Report Daily F-LocalBrand!";
                mailRequest.EmailBody = CreateEmailContent(report);
                await _emailService.SendEmailAsync(mailRequest);
            }


        }
        private string CreateEmailContent(ReportData reportData)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            font-size: 14px;
            line-height: 1.5;
            color: #333;
            margin: 0;
            padding: 0;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        th, td {
            padding: 10px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }
        th {
            background-color: #f2f2f2;
        }
    </style>
</head>
<body>
    <h1>Sales Report</h1>
   ");
            sb.AppendLine(@"<table>
        <tr>
            <th>Total Orders</th>");
            sb.AppendLine($"{reportData.TotalOrders}");
            sb.AppendLine(@"</tr>
        <tr>
            <th>Total Revenue</th>");
            sb.AppendLine($"<td>{reportData.TotalRevenue:C}</td>");
            sb.AppendLine(@" </tr>
    </table>");
            sb.AppendLine(@"<h2>Order Status Counts</h2>
    <table>
        <tr>
            <th>Status</th>
            <th>Count</th>
        </tr>");

            foreach (var kvp in reportData.OrderStatusCounts)
            {
                sb.AppendLine($"        <tr><td>{kvp.Key}</td><td>{kvp.Value}</td></tr>");
            }

            sb.AppendLine(@"
    </table>
    
    <h2>Top Selling Products</h2>
    <ul>");

            foreach (var product in reportData.TopSellingProducts)
            {
                sb.AppendLine($"        <li>{product}</li>");
            }

            sb.AppendLine(@"
    </ul>
    
    <h2>Payment Status Counts</h2>
    <table>
        <tr>
            <th>Status</th>
            <th>Count</th>
        </tr>");

            foreach (var kvp in reportData.PaymentStatusCounts)
            {
                sb.AppendLine($"        <tr><td>{kvp.Key}</td><td>{kvp.Value}</td></tr>");
            }

            sb.AppendLine(@"
    </table>
    
    <h2>Shipping Status Counts</h2>
    <table>
        <tr>
            <th>Status</th>
            <th>Count</th>
        </tr>");

            foreach (var kvp in reportData.ShippingStatusCounts)
            {
                sb.AppendLine($"        <tr><td>{kvp.Key}</td><td>{kvp.Value}</td></tr>");
            }

            sb.AppendLine(@"
    </table>
</body>
</html>");

            return sb.ToString();
        }
    }
}
