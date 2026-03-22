namespace OrderApi.Services
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(Order order);

        Task CancelOrderAsync(int orderId, string reason);
    }
}
