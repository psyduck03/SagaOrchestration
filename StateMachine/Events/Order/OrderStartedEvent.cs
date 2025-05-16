using Stock.API.Messages;

namespace StateMachine.Events.Order;

public class OrderStartedEvent
{
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage>? OrderItems { get; set; }
}