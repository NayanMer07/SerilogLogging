using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Services;
using Serilog.Context;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orders,
                                 ILogger<OrdersController> logger)
            => (_orders, _logger) = (orders, logger);

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            // Enrich ALL logs in this request with CorrelationId
            using (LogContext.PushProperty("CorrelationId",
                   HttpContext.TraceIdentifier))
            {
                _logger.LogInformation(
                    "POST /orders received for {CustomerEmail}",
                    order.CustomerEmail);

                var confirmed = await _orders.PlaceOrderAsync(order);
                return CreatedAtAction(nameof(GetOrder),
                       new { id = confirmed.Id }, confirmed);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            _logger.LogDebug("Fetching order {OrderId}", id);
            // ... return order
            return Ok(new { id });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id, string reason)
        {
            _logger.LogInformation("Cancel order {OrderId} due to {reason}", id, reason);
            // ... return order
            await _orders.CancelOrderAsync(id, reason);
            return Ok(new { id, reason });
        }
    }
}
