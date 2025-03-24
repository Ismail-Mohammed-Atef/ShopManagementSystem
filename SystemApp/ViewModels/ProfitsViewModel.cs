using DataLayer.Models;

namespace SystemApp.ViewModels
{
    public class ProfitsViewModel
    {
        public DateTime SelectedDate { get; set; }
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public double CumulativeDailyProfit { get; set; }
        public double MonthlyProfit { get; set; }
        public List<OrderProfitViewModel> DailyOrders { get; set; } // Replace DailyTransactions
        public List<ExtraPayments> DailyExtraPayments { get; set; }
        public decimal DailyExtraPaymentsTotal { get; set; }
        public double MonthlyExtraPaymentsTotal { get; set; }
    }
}
