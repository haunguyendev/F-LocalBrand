using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests.Collection;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.Common.Shared;
using SWD.F_LocalBrand.Business.DTO.Collection;
using SWD.F_LocalBrand.Business.DTO.Order;
using SWD.F_LocalBrand.Business.Services;
using SWD.F_LocalBrand.Data.Models;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly CollectionService _collectionService;

        public CollectionController(CollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        //Get collection by id
        [HttpGet("collection/{collectionId}")]
        public async Task<IActionResult> GetCollectionById(int collectionId)
        {
            try
            {
                var collection = await _collectionService.GetCollectionById(collectionId);
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
        [HttpPost("collection")]
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
                return Ok(ApiResult<string>.Succeed($"Collection {collectionName} add successfully!"));
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


        #region api update collection
        [HttpPut("collection")]
        [SwaggerOperation(
           Summary = "Update collection details",
           Description = "Updates the details of an existing collection. Campaign and CollectionProducts are optional."
       )]
        [SwaggerResponse(StatusCodes.Status200OK, "Collection updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Collection not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the collection", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateCollection([FromBody] CollectionUpdateRequest request)
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
                if(request.Status != CollectionStatusTypeEnum.Active && request.Status != CollectionStatusTypeEnum.Inactive && request.Status != CollectionStatusTypeEnum.Deleted)
                {
                    return BadRequest(ApiResult<object>.Error(new { Message = "Invalid status" }));
                }
                var collectionModel = request.MapToModel();
                var updatedCollection = await _collectionService.UpdateCollectionAsync(collectionModel);

                if (updatedCollection == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Collection not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Collection updated successfully", Collection = updatedCollection }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion


        #region get all collection
        [HttpGet("collections")]
        [SwaggerOperation(
            Summary = "Get collections",
            Description = "Get all collections"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Collections get successfully", typeof(ApiResult<CollectionsResponse>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Collection not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while get collections", typeof(ApiResult<object>))]
        public async Task<IActionResult> GetAllCollections()
        {
            try
            {
                var collections = await _collectionService.GetCollections();
                if (collections == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Collections not found" }));
                }

                return Ok(ApiResult<CollectionsResponse>.Succeed(new CollectionsResponse{ 
                    Collections = collections 
                }));
            }
            catch (Exception e){ 
                return StatusCode(500, ApiResult<object>.Fail(e));
            }
        }
        #endregion


        #region get collections with filter
        [HttpGet("collections/filter")]
        [SwaggerOperation(
                       Summary = "Get collections with filter",
                       Description = "Get all collections with filter"
                   )]
        [SwaggerResponse(StatusCodes.Status200OK, "Collections get successfully", typeof(ApiResult<CollectionsResponse>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Collection not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while get collections", typeof(ApiResult<object>))]
        public async Task<IActionResult> GetCollectionsWithFilter([FromQuery] CollectionFilterModel request)
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
                var collections = await _collectionService.GetAllCollectionsWithFilterAsync(request);
                if (collections == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Collections not found" }));
                }

                return Ok(ApiResult<CollectionsResponse>.Succeed(new CollectionsResponse
                {
                    Collections = collections
                }));
            }
            catch (Exception e)
            {
                return StatusCode(500, ApiResult<object>.Fail(e));
            }
        }
        #endregion


        #region api update status collection 
        [HttpPut("collection/{collectionId}/status")]
        [SwaggerOperation(
       Summary = "Update collection status",
       Description = "Updates the status of a collection to 'Active' or 'Inactive'."
   )]
        [SwaggerResponse(StatusCodes.Status200OK, "Collection status updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Collection not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the collection status", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateCollectionStatus(int collectionId, [FromBody] UpdateCollectionStatusRequest request)
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
                var updateResult = await _collectionService.UpdateCollectionStatusAsync(collectionId, request.Status);

                if (!updateResult)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Collection not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Collection status updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion


        #region api delete collection
        [HttpDelete("collection/{collectionId}")]
        [SwaggerOperation(
       Summary = "Delete a collection",
       Description = "Deletes a collection by updating its status to 'Deleted' if no products are using it."
   )]
        [SwaggerResponse(StatusCodes.Status200OK, "Collection deleted successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Collection not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Collection is in use by one or more products", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while deleting the collection", typeof(ApiResult<object>))]
        public async Task<IActionResult> DeleteCollection(int collectionId)
        {
            try
            {
                if (await _collectionService.IsCollectionInUseAsync(collectionId))
                {
                    return Conflict(ApiResult<object>.Error(new { Message = "Collection is in use by one or more products" }));
                }

                var deleteResult = await _collectionService.DeleteCollectionAsync(collectionId);

                if (!deleteResult)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Collection not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Collection deleted successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion

    }
}
