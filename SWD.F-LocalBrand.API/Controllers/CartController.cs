using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Exceptions;
using SWD.F_LocalBrand.API.Payloads.Requests.Cart;
using SWD.F_LocalBrand.Business.DTO.Cart;
using SWD.F_LocalBrand.Business.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        public CartController(CartService cartService)
        {
            _cartService = cartService;
            }


        #region api get customer cart
        [HttpGet("get-customer-cart")]
        [SwaggerOperation(
        Summary = "Get customer cart",
        Description = "Retrieves the cart for the logged-in customer."
    )]
        [SwaggerResponse(200, "Cart retrieved successfully", typeof(ApiResult<CartResponseModel>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(500, "An error occurred while retrieving the cart")]
        public async Task<ActionResult<ApiResult<CartResponseModel>>> GetCustomerCart()
        {
            try
            {
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    return Unauthorized(ApiResult<string>.Error("Authorization header is missing or invalid."));
                }

                token = token.ToString().Split()[1];

                if (string.IsNullOrWhiteSpace(token))
                {
                    return Unauthorized(ApiResult<string>.Error("Authorization header is missing or invalid."));
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var customerClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId);

                if (customerClaim == null)
                {
                    return Unauthorized(ApiResult<string>.Error("Unauthorized: No customer ID found in token."));
                }

                var customerId = int.Parse(customerClaim.Value);
                var cart = await _cartService.GetCartByCustomerIdAsync(customerId);

                if (cart == null)
                {
                    return Ok(ApiResult<string>.Succeed("Your cart has no items"));
                }

                return Ok(ApiResult<CartResponseModel>.Succeed(cart));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }

        #endregion

        #region api add-to-cart
        [HttpPost("add-to-cart")]
        
        [SwaggerOperation(
            Summary = "Add a product to the cart",
            Description = "Adds a product to the cart with the specified quantity."
        )]
        [SwaggerResponse(200, "Product added to cart successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(500, "An error occurred while adding the product to the cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
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

                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    throw new BadRequestException("Authorization header is missing or invalid.");
                }

                token = token.ToString().Split()[1];

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new BadRequestException("Authorization header is missing or invalid.");
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var customerClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId);

                if (customerClaim == null)
                {
                    return Unauthorized(ApiResult<string>.Error("Unauthorized: No customer ID found in token."));
                }

                var customerId = int.Parse(customerClaim.Value);
                await _cartService.AddToCartAsync(customerId, request.ProductId, request.Quantity);

                return Ok(ApiResult<string>.Succeed("Product added to cart successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
        #region api update cart
        [HttpPut("update-cart")]
        [Authorize]
        [SwaggerOperation(
    Summary = "Update the cart",
    Description = "Updates the cart by replacing the existing products with the new ones provided."
)]
        [SwaggerResponse(200, "Cart updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(500, "An error occurred while updating the cart")]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartRequest request)
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

                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    throw new BadRequestException("Authorization header is missing or invalid.");
                }

                token = token.ToString().Split()[1];

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new BadRequestException("Authorization header is missing or invalid.");
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var customerClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId);

                if (customerClaim == null)
                {
                    return Unauthorized(ApiResult<string>.Error("Unauthorized: No customer ID found in token."));
                }

                var customerId = int.Parse(customerClaim.Value);
                var cartUpdateModel = request.MapToModel();
                await _cartService.UpdateCartAsync(customerId, cartUpdateModel);

                return Ok(ApiResult<string>.Succeed("Cart updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
        #region api detele item in cart
        [HttpDelete("delete-cart-item")]
        [Authorize]
        [SwaggerOperation(
    Summary = "Delete an item from the cart",
    Description = "Deletes an item from the cart based on the product ID."
)]
        [SwaggerResponse(200, "Item deleted from cart successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(500, "An error occurred while deleting the item from the cart")]
        public async Task<IActionResult> DeleteCartItem([FromBody] DeleteCartItemRequest request)
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

                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    throw new BadRequestException("Authorization header is missing or invalid.");
                }

                token = token.ToString().Split()[1];

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new BadRequestException("Authorization header is missing or invalid.");
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var customerClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId);

                if (customerClaim == null)
                {
                    return Unauthorized(ApiResult<string>.Error("Unauthorized: No customer ID found in token."));
                }

                var customerId = int.Parse(customerClaim.Value);
                await _cartService.DeleteCartItemAsync(customerId, request.ProductId);

                return Ok(ApiResult<string>.Succeed("Item deleted from cart successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
    }
}
