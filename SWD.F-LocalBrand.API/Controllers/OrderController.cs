using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Exceptions;
using SWD.F_LocalBrand.API.Payloads.Requests.Order;
using SWD.F_LocalBrand.API.Payloads.Requests.OrderHistory;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.DTO.Order;
using SWD.F_LocalBrand.Business.DTO.VNPay;
using SWD.F_LocalBrand.Business.Services;
using SWD.F_LocalBrand.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly OrderHistoryService _orderHistoryService;

        public OrderController(OrderService orderService,OrderHistoryService orderHistoryService)
        {
            _orderService = orderService;
            _orderHistoryService = orderHistoryService;
        }

        //get product by order id
        [HttpGet("order/{orderId}/products")]
        public async Task<IActionResult> GetProductByOrderId(int orderId)
        {
            try
            {
                var listProduct = await _orderService.GetProductsByOrderIdAsync(orderId);
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
        //get list order have payment status is true
        [HttpGet("orders/payment-true")]
        public async Task<IActionResult> GetOrdersWithPaymentStatusTrue()
        {
            try
            {
                var orders = await _orderService.GetOrdersWithPaymentStatusTrueAsync();
                return Ok(ApiResult<ListOrderResponse>.Succeed(new ListOrderResponse
                {
                    Orders = orders
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        //get order or list order have status from request
        [HttpGet("order/status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(ApiResult<ListOrderResponse>.Succeed(new ListOrderResponse
                {
                    Orders = orders.ToList()
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        #region api order status
        [HttpPut("order/status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<object>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<ValidationProblemDetails>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResult<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResult<object>))]
        public IActionResult UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<ValidationProblemDetails>.Error(new ValidationProblemDetails(ModelState)));
            }

            try
            {
                var updateModel = new UpdateOrderStatusModel
                {
                    Id = request.Id,
                    OrderStatus = request.OrderStatus
                };

                var updateResult = _orderService.UpdateOrderStatus(updateModel);

                if (updateResult == null)
                {
                    return NotFound(ApiResult<object>.Error(new { Message = "Order not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Order status updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion


        #region api create order with payment


        [HttpPost("order")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Create a new order and initiate payment",
            Description = "Creates a new order with the provided products and initiates payment."
        )]
        [SwaggerResponse(200, "Order created successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(500, "An error occurred while creating the order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
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
                
                var result = await _orderService.CreateOrderQueuePaymentAsync(customerId, request.Products, request.PaymentMethod);

                if (!result.Success)
                {
                    return BadRequest(ApiResult<string>.Error(result.ErrorMessage));
                }

                return Ok(ApiResult<string>.Succeed(result.PaymentUrl));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion


        #region api update payment status 
        [HttpGet("order/check-payment")]
        [Authorize]
        [SwaggerOperation(
           Summary = "Update payment status",
           Description = "Updates the status of the payment and the corresponding order."
       )]
        [SwaggerResponse(200, "Payment status updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(500, "An error occurred while updating the payment status")]
        public async Task<IActionResult> UpdatePaymentStatus([FromQuery] UpdateVNPayModel request)
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

                var res = await _orderService.UpdatePaymentStatusAsync(request);

                return Ok(ApiResult<string>.Succeed(res));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion


        #region get orders with filter
        [HttpGet("orders/filter")]
        [SwaggerOperation(
                       Summary = "Get orders with filter",
                       Description = "Retrieves a list of orders based on the provided filter."
                   )]
        [SwaggerResponse(StatusCodes.Status200OK, "Orders retrieved successfully", typeof(ApiResult<ListOrderResponse>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        public async Task<IActionResult> GetOrders([FromQuery] OrderFilterModel request)
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

                var orders = await _orderService.GetAllOrdersWithFilterAsync(request);

                return Ok(ApiResult<ListOrderResponse>.Succeed(new ListOrderResponse
                {
                    Orders = orders
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion


        #region get order with role shipper, customer filter
        [HttpGet("orders/{status}")]
        [Authorize]
        [SwaggerOperation(
    Summary = "Get orders with role",
    Description = "Retrieves a list of orders based on the user's role."
)]
        [SwaggerResponse(StatusCodes.Status200OK, "Orders retrieved successfully", typeof(ApiResult<ListOrderResponse>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        public async Task<IActionResult> GetOrdersByRole(string status)
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

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized(ApiResult<string>.Error("Unauthorized: No role found in token."));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int customerId))
                {
                    return Unauthorized(ApiResult<string>.Error("Unauthorized: Invalid user ID."));
                }

                if (role == "Customer")
                {
                    var orders = await _orderService.GetOrdersWithFilterAsync(customerId, status);
                    return Ok(ApiResult<ListOrderResponse>.Succeed(new ListOrderResponse { Orders = orders }));
                }
                else if (role == "Shipper")
                {
                    var orders = await _orderService.GetOrdersWithFilterAsync(null, status);
                    return Ok(ApiResult<ListOrderResponse>.Succeed(new ListOrderResponse { Orders = orders }));
                }
                else
                {
                    return Unauthorized(ApiResult<string>.Error("Unauthorized: Invalid role."));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }

        #endregion


        #region api update history order
        [HttpPut("/order/{orderId}/status")]
        [SwaggerOperation(
        Summary = "Update order status",
        Description = "Updates the status of an order following the defined status transition rules."
    )]
        [SwaggerResponse(StatusCodes.Status200OK, "Order status updated successfully", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Order not found", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Invalid status transition", typeof(ApiResult<object>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while updating the order status", typeof(ApiResult<object>))]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderHistoryStatusRequest request)
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
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(ApiResult<object>.Error(new { Message = "User role is not defined" }));
                }

                var updateResult = await _orderHistoryService.UpdateOrderStatusAsync(orderId, request.Status, userRole);

                if (!updateResult)
                {
                    return Conflict(ApiResult<object>.Error(new { Message = "Invalid status transition or order not found" }));
                }

                return Ok(ApiResult<object>.Succeed(new { Message = "Order status updated successfully" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        #endregion
    }
}
