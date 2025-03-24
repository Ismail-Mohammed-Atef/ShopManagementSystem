namespace SystemApp.ViewModels
{
    public class OrderDetailProfitViewModel
    {
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public double SellPrice { get; set; }
        public double TotalSell { get; set; }
        public double ProfitPerItem { get; set; }
        public double TotalProfit { get; set; }
    }
}
