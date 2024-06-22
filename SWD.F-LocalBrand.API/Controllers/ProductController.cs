using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests.Product;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService productService;

        public ProductController(ProductService productService)
        {
            this.productService = productService;
        }

        //Get all product
        [HttpGet("/list-product")]
        public async Task<IActionResult> GetAllProduct()
        {
            try
            {
                var listProduct = await productService.GetAllProductsAsync();
                if (listProduct == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any product!"));
                    return NotFound(resultFail);
                }
                return Ok(ApiResult<ListProductResponse>.Succeed(new ListProductResponse
                {
                    Products = listProduct
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [HttpGet("/list-product/{pageSize}/{pageNumber}")]
        public async Task<IActionResult> GetAllProduct(int pageSize, int pageNumber)
        {
            var listProduct = await productService.GetAllProductsAsync(pageNumber, pageSize);
            if (listProduct == null)
            {
                var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any product!"));
                return NotFound(resultFail);
            }
            return Ok(ApiResult<ListProductWithPageAndIncludeRelatedReponse>.Succeed(new ListProductWithPageAndIncludeRelatedReponse
            {
                Products = listProduct
            }));
        }

        //get product by category id
        [HttpGet("category-with-products/{categoryId}")]
        public async Task<IActionResult> GetProductByCategoryId(int categoryId)
        {
            try
            {
                var listProduct = await productService.GetProductsByCategoryIdAsync(categoryId);
                if (listProduct == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any product in this category!"));
                    return NotFound(resultFail);
                }
                return Ok(ApiResult<ListProductResponse>.Succeed(new ListProductResponse
                {
                    Products = listProduct
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        //get product by category id and have paging
        [HttpGet("category-with-products/{categoryId}/{pageIndex}/{pageSize}")]
        public async Task<IActionResult> GetProductByCategoryIdPaging(int categoryId, int pageIndex, int pageSize)
        {
            try
            {
                var listProduct = await productService.GetProductsByCategoryIdPagingAsync(categoryId, pageIndex, pageSize);
                if (listProduct == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any product in this category!"));
                    return NotFound(resultFail);
                }
                return Ok(ApiResult<ListProductResponse>.Succeed(new ListProductResponse
                {
                    Products = listProduct
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }


        //get product by id and compapility of them
        [HttpGet("/product-product-recommend/{productId}")]
        public async Task<IActionResult> GetProductWithRecommendations(int productId)
        {
            try
            {
                var product = await productService.GetProductWithRecommendationsAsync(productId);
                if (product == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any product with this id!"));
                    return NotFound(resultFail);
                }
                return Ok(ApiResult<ProductResponse>.Succeed(new ProductResponse
                {
                    Product = product
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }
        #region create product 
        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="request">The product creation request.</param>
        /// <returns>A response indicating the result of the creation operation.</returns>
        /// <response code="200">Returns the newly created product.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If there was an internal server error.</response>
        [HttpPost("create-product")]
        [SwaggerOperation(
        Summary = "Create a new product",
        Description = "Creates a new product with the provided details. The input model must contain valid data as specified in the constraints."
    )]
        [SwaggerResponse(200, "Product created successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(500, "An error occurred while creating the product")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
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
                var productModel = request.MapToModel();
                await productService.CreateProductAsync(productModel);

                return Ok(ApiResult<string>.Succeed("Product created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }


        /*
         * JSON valid  example
         {
  "productName": "Sample Product",
  "categoryId": 1,
  "campaignId": 2,
  "gender": "Unisex",
  "price": 29.99,
  "description": "This is a sample product description.",
  "stockQuantity": 100,
  "imageUrl": "http://example.com/images/sample-product.jpg",
  "size": 42,
  "color": "Red",
  "status": "Active"
}
         -- Json invalid validation
        {
 {
  "productName": "",
  "categoryId": 0,
  "campaignId": 0,
  "gender": "Unknown",
  "price": -10,
  "description": "",
  "stockQuantity": -5,
  "imageUrl": "",
  "size": -1,
  "color": ""
 
}

}
         */
        #endregion

        #region api update product detail 
        [HttpPut("update-product")]
        [SwaggerOperation(
           Summary = "Update product details",
           Description = "Updates the details of an existing product. Example of a valid request: {\"id\":1,\"productName\":\"New Product Name\",\"categoryId\":1,\"campaignId\":1,\"gender\":\"Male\",\"price\":100.00,\"description\":\"New description\",\"stockQuantity\":50,\"imageUrl\":\"http://example.com/image.jpg\",\"size\":42,\"color\":\"Red\",\"status\":\"Active\"}"
       )]
        [SwaggerResponse(StatusCodes.Status200OK, "Product updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the product", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request)
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
                var productModel = request.MapToModel();
                var updateResult = await productService.UpdateProductAsync(productModel);

                if (updateResult == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Product not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Product updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        /**
         Json with invalid value - 
        {
  "id": 0,
  "productName": "",
  "categoryId": 0,
  "campaignId": 0,
  "gender": "Other",
  "price": -1,
  "description": "",
  "stockQuantity": -10,
  "imageUrl": "invalid-url",
  "size": 0,
  "color": "",
  "status": "Unknown"
}

        Json with valid value - 
        {
  "id": 1,
  "productName": "New Product Name",
  "categoryId": 1,
  "campaignId": 1,
  "gender": "Male",
  "price": 100.00,
  "description": "New description",
  "stockQuantity": 50,
  "imageUrl": "http://example.com/image.jpg",
  "size": 42,
  "color": "Red",
  "status": "Active"
}
         
         */

        #endregion

        #region api delete-product
        [HttpDelete("{productId}")]
        [SwaggerOperation(
       Summary = "Delete a product",
       Description = "Deletes a product by changing its status to 'Deleted'. A valid product ID is required.")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                await productService.DeleteProductAsync(productId);
                return Ok(ApiResult<string>.Succeed("Product deleted successfully"));
            }
            catch (EntryPointNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion


        //get product by order id
        [HttpGet("/by-order/{orderId}/products")]
        public async Task<IActionResult> GetProductByOrderId(int orderId)
        {
            try
            {
                var listProduct = await productService.GetProductsByOrderIdAsync(orderId);
                if (listProduct == null)
                {
                    var resultFail = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Do not have any product in this order!"));
                    return NotFound(resultFail);
                }
                return Ok(ApiResult<ListProductResponse>.Succeed(new ListProductResponse
                {
                    Products = listProduct
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
        }

  }
}
