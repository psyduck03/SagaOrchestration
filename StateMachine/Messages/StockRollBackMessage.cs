namespace Stock.API.Messages;

public class StockRollBackMessage
{
    public List<OrderItemMessage>? OrderItems { get; set; }
}