using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            var listProduct = await productService.GetAllProductsAsync();
            return Ok(listProduct);
        }

        //get product by category id
        [HttpGet("category-with-products/{categoryId}")]
        public async Task<IActionResult> GetProductByCategoryId(int categoryId)
        {
            var listProduct = await productService.GetProductsByCategoryIdAsync(categoryId);
            return Ok(listProduct);
        }

        //get product by category id and have paging
        [HttpGet("{categoryId}/{pageIndex}/{pageSize}")]
        public async Task<IActionResult> GetProductByCategoryIdPaging(int categoryId, int pageIndex, int pageSize)
        {
            var listProduct = await productService.GetProductsByCategoryIdPagingAsync(categoryId, pageIndex, pageSize);
            return Ok(listProduct);
        }


        //get product by id and compapility of them
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductWithRecommendations(int productId)
        {
            var product = await productService.GetProductWithRecommendationsAsync(productId);
            return Ok(product);
        }

        
    }
}
