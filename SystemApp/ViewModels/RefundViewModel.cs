namespace SystemApp.ViewModels
{
    public class RefundViewModel
    {
        public List<OrderProductViewModel> Products { get; set; } = new List<OrderProductViewModel>();
        public List<SelectedProductViewModel> SelectedProducts { get; set; } = new List<SelectedProductViewModel>();
    }
}
