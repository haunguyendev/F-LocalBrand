using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly CampaignService _campaignService;

        public CampaignController(CampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        //Get campaign by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCampaignById(int id)
        {
            var campaign = await _campaignService.GetCampaignById(id);
            if (campaign != null)
            {
                return Ok(campaign);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
