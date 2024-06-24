using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWD.F_LocalBrand.API.Common;
using SWD.F_LocalBrand.API.Payloads.Requests.Payment;
using SWD.F_LocalBrand.Business.Services;
using SWD.F_LocalBrand.Data.Models;

namespace SWD.F_LocalBrand.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        public PaymentController(PaymentService paymentService)
        {
            _paymentService= paymentService;
            
        }

        [HttpPost("create")]
        [SwaggerOperation(
       Summary = "Create a new payment",
       Description = "Creates a new payment with the provided details."
   )]
        [SwaggerResponse(StatusCodes.Status200OK, "Payment created successfully", typeof(ApiResult<Payment>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request", typeof(ApiResult<Dictionary<string, string[]>>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An error occurred while creating the payment", typeof(ApiResult<object>))]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateRequest request)
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
                var paymentModel = request.MapToModel();
                var payment = await _paymentService.CreatePaymentAsync(paymentModel);
                return Ok(ApiResult<Payment>.Succeed(payment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }

    }
}
