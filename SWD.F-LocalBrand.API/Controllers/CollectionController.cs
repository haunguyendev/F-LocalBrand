using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var collection = await _collectionService.GetCollectionById(id);
            if (collection != null)
            {
                return Ok(collection);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
