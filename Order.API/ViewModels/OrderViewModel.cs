namespace Order.API.ViewModels;

public class OrderViewModel
{
    public int BuyerId { get; set; }
    public List<OrderItemViewModel>? OrderItems { get; set; }
}