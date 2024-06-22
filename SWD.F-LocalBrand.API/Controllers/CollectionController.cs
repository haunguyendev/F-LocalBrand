using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests.Collection;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly CollectionService _collectionService;

        public CollectionController(CollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        //Get collection by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollectionById(int id)
        {
            try
            {
                var collection = await _collectionService.GetCollectionById(id);
                if (collection != null)
                {
                    return Ok(ApiResult<CollectionResponse>.Succeed(new CollectionResponse
                    {
                        Collection = collection
                    }));
                }
                else
                {
                    return NotFound(ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Collection does not exist")));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
        }

        #region api create collection
        [HttpPost("create")]
        [SwaggerOperation(
            Summary = "Create a new collection",
            Description = "Creates a new collection with the provided name."
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Collection created successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Collection name already exists", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while creating the collection", typeof(ApiResult<object>))]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionRequest request)
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
                var collectionModel = request.MapToModel();
                var collectionName = await _collectionService.CreateCollectionAsync(collectionModel);
                return CreatedAtAction(nameof(GetCollectionById), ApiResult<object>.Succeed(new { Message = "Collection created successfully", CollectionName = collectionName }));
            }
            catch (ArgumentException ex)
            {
                return Conflict(ApiResult<object>.Error(new { Message = ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }


        #endregion
    }
}
