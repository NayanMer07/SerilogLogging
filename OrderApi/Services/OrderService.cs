namespace OrderApi.Services;

public record Order(int Id, string CustomerEmail, decimal Total, string Status);

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger) => _logger = logger;

    public async Task<Order> PlaceOrderAsync(Order order)
    {
        // ✅ Structured: OrderId and CustomerEmail are queryable properties
        _logger.LogInformation(
            "Placing order {OrderId} for {CustomerEmail} total {Total:C}",
            order.Id, order.CustomerEmail, order.Total);

        try
        {
            // simulate DB save
            await Task.Delay(50);

            // Push additional context for all logs in this scope
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = order.Id,
                ["CustomerEmail"] = order.CustomerEmail
            });

            var confirmed = order with { Status = "Confirmed" };
            _logger.LogInformation(
                "Order {OrderId} confirmed successfully", confirmed.Id);

            return confirmed;
        }
        catch (Exception ex)
        {
            // ✅ Exception is captured as a structured property — not just a string
            _logger.LogError(ex,
                "Failed to place order {OrderId} for {CustomerEmail}",
                order.Id, order.CustomerEmail);
            throw;
        }
    }

    public async Task CancelOrderAsync(int orderId, string reason)
    {
        _logger.LogWarning(
            "Order {OrderId} cancelled — Reason: {CancellationReason}",
            orderId, reason);

        await Task.Delay(20);
    }
}