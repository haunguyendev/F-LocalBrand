using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests;
using SWD.F_LocalBrand.API.Payloads.Requests.Category;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.Services;
using SWD.F_LocalBrand.Data.Models;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService categoryService;

        public CategoryController(CategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        //get categories with products of them
        [HttpGet("categories/products")]
        public IActionResult GetCategoriesWithProducts()
        {
            try
            {
                var list = categoryService.GetAllProductsWithCategories();
                if (list == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any category"));
                    return BadRequest(resultFail);
                }
                return Ok(ApiResult<ListCategoryResponse>.Succeed(new ListCategoryResponse
                {
                    Categories = list
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
        }

        //get all categories
        [HttpGet("categories")]
        public IActionResult GetAllCategories()
        {
            try
            {
                var list = categoryService.GetAllCategories();
                if (list == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any category"));
                    return BadRequest(resultFail);
                }
                return Ok(ApiResult<ListCategoryResponse>.Succeed(new ListCategoryResponse
                {
                    Categories = list
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
        }

        //get category with categpry id with products of them 
        [HttpGet("category/{categoryId}")]
        public IActionResult GetCategoryWithProducts(int categoryId)
        {
            try
            {
                var category = categoryService.GetCategoryWithProducts(categoryId);
                if (category == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Category does not exist"));
                    return NotFound(resultFail);
                }
                return Ok(ApiResult<CategoryResponse>.Succeed(new CategoryResponse
                {
                    Category = category
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
        }
        #region create category api
        [HttpPost("create-category")]
        [SwaggerOperation(
           Summary = "Create a new category",
           Description = "Creates a new category with the provided details. The input model must contain valid data as specified in the constraints."
       )]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
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
                var model = request.MapToModel();
                var category = await categoryService.CreateCategoryAsync(model);
                return Ok(ApiResult<Category>.Succeed(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        /**
         Json valid 
        {
        "categoryName": "Clothing",
        "description": "Apparel and garments"
        }
        Json invalid
        {
        "description": "Apparel and garments"
        }
         
         */
        #endregion
        #region update category api
        [HttpPut("update-category")]
        [SwaggerOperation(
           Summary = "Update category details",
           Description = "Updates the details of an existing category. Example of a valid request: {\"id\":1,\"categoryName\":\"New Category Name\",\"description\":\"New category description\"}"
       )]
        [SwaggerResponse(StatusCodes.Status200OK, "Category updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the category", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryUpdateRequest request)
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
                var categoryModel = request.MapToModel();
                var updateResult = await categoryService.UpdateCategoryAsync(categoryModel);

                if (updateResult == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Category not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Category updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
        #region delete category api
        [HttpDelete("delete-category")]
        [SwaggerOperation(
      Summary = "Delete a category",
      Description = "Updates the status of a category to 'Deleted' and updates the status of related products to 'Inactive'."
  )]
        [SwaggerResponse(StatusCodes.Status200OK, "Category deleted successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while deleting the category", typeof(ApiResult<object>))]
        public async Task<IActionResult> DeleteCategory([FromBody] CategoryDeleteRequest request)
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
                var deleteResult = await categoryService.DeleteCategoryAsync(request.Id);

                if (!deleteResult)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Category not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Category deleted successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
    }
}
