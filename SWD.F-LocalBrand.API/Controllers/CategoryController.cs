using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.Business.Services;

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
        [HttpGet]
        public IActionResult GetCategoriesWithProducts()
        {
            var list = categoryService.GetAll();
            return Ok(list);
        }

        //get all categories
        [HttpGet("categories")]
        public IActionResult GetAllCategories()
        {
            var list = categoryService.GetAllCategories();
            return Ok(list);
        }
    }
}
