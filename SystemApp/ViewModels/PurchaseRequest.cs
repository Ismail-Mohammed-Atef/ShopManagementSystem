namespace SystemApp.ViewModels
{
    public class PurchaseRequest
    {
        public List<ProductPurchaseVM> Products { get; set; }
        public decimal TotalInvoice { get; set; }
        public string Note { get; set; }
    }
}
