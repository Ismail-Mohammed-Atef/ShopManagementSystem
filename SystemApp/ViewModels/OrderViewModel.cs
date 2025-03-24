namespace SystemApp.ViewModels
{
    public class OrderViewModel
    {
        public List<OrderProductViewModel> Products { get; set; } = new List<OrderProductViewModel>();
        public List<SelectedProductViewModel> SelectedProducts { get; set; } = new List<SelectedProductViewModel>();
        public decimal TotalAmount { get; set; } // Add this property
    }
}
