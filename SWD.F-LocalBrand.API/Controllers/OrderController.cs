using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests.Order;
using SWD.F_LocalBrand.API.Payloads.Responses;
using SWD.F_LocalBrand.Business.DTO.Order;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
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
    }
}
