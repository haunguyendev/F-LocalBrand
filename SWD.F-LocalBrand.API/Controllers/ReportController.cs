using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly OrderService _reportService;
        public ReportController(OrderService orderService)

        {
            _reportService = orderService;
            
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport([FromQuery] DateOnly reportDate)
        {
            var report = await _reportService.GetDailyReportDataAsync(reportDate);
            return Ok(report);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            var report = await _reportService.GetMonthlyReportDataAsync(year, month);
            return Ok(report);
        }

        [HttpGet("quarterly")]
        public async Task<IActionResult> GetQuarterlyReport([FromQuery] int year, [FromQuery] int quarter)
        {
            var report = await _reportService.GetQuarterlyReportDataAsync(year, quarter);
            return Ok(report);
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
        {
            var report = await _reportService.GetYearlyReportDataAsync(year);
            return Ok(report);
        }


    }
}
