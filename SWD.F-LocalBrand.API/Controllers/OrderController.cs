using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Common.Payloads.Responses;
using SWD.F_LocalBrand.Business.Services;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        //get list order have payment status is true
        [HttpGet("orders/withPaymentStatusTrue")]
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
        [HttpGet("byStatus/{status}")]
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
    }
}
