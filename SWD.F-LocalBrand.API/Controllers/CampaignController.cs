using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sprache;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Responses;
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
                return Ok(ApiResult<CampaignResponse>.Succeed(new CampaignResponse
                {
                    Campaign = campaign
                }));
            }
            else
            {
                var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Campaign does not exist"));
                return NotFound(resultFail);
            }
        }
    }
}
