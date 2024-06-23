using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sprache;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests.Campaign;
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
        #region create campaig api
        [HttpPost("create-campaign")]
        [SwaggerOperation(
       Summary = "Create a new campaign",
       Description = "Creates a new campaign. Example of a valid request: {\"campaignName\":\"New Campaign Name\"}")]
        [SwaggerResponse(StatusCodes.Status201Created, "Campaign created successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Campaign name already exists", typeof(ApiResult<object>))]
        public async Task<IActionResult> CreateCampaign([FromBody] CampaignCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
            {
                { "Errors", errors.ToArray() }
            }));
            }

            if (await _campaignService.IsCampaignNameExistAsync(request.CampaignName))
            {
                return Conflict(ApiResult<object>.Error(new { Message = "Campaign name already exists" }));
            }

            var campaignModel = request.MapToModel();
            var createResult = await _campaignService.CreateCampaignAsync(campaignModel);

            return CreatedAtAction(nameof(CreateCampaign), new { id = createResult.Id }, ApiResult<object>.Succeed(new { Message = "Campaign created successfully", CampaignId = createResult.Id }));
        }
        #endregion
        #region update campaign api
        [HttpPut("update-campaign")]
        [SwaggerOperation(
    Summary = "Update campaign details",
    Description = "Updates the details of an existing campaign. Example of a valid request: {\"id\":1,\"campaignName\":\"New Campaign Name\",\"collectionIds\":[1,2,3]}"
)]
        [SwaggerResponse(StatusCodes.Status200OK, "Campaign updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Campaign or collection not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Campaign name already exists", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the campaign", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateCampaign([FromBody] CampaignUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
        {
            { "Errors", errors.ToArray() }
        }));
            }

            try
            {
                if (await _campaignService.IsCampaignNameExistAsync(request.CampaignName))
                {
                    return Conflict(ApiResult<object>.Error(new { Message = "Campaign name already exists" }));
                }

                var campaignModel = request.MapToModel();
                var updateResult = await _campaignService.UpdateCampaignAsync(campaignModel);

                if (updateResult == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Campaign not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Campaign updated successfully" }));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResult<object>.Error(new { Message = ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        /*
         * 
        Valid Json: 
        {
    "id": 1,
    "campaignName": "New Campaign Name",
    "collectionIds": [1, 2, 3]
}
        Invalid Json:
        {
    "id": 1,
    "campaignName": "Another Campaign Name",
    "collectionIds": [9999, 8888]
}
         **/
        #endregion
    }
}
