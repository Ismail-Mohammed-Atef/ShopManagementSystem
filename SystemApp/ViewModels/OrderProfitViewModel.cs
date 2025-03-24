namespace SystemApp.ViewModels
{
    public class OrderProfitViewModel
    {
        public int OrderId { get; set; }
        public string OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderDetailProfitViewModel> OrderDetails { get; set; }
    }
}
