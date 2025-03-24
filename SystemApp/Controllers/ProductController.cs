using DataLayer;
using DataLayer.Models;
using SystemApp.Repository;
using SystemApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace SystemApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPurchaseInvoiceRepository _purchaseInvoiceRepository;
        private readonly IExtraPaymentRepository _extraPaymentRepository;

        public ProductController(IProductRepository productRepository ,IOrderRepository orderRepository , IPurchaseInvoiceRepository purchaseInvoiceRepository , IExtraPaymentRepository extraPaymentRepository) 
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _extraPaymentRepository = extraPaymentRepository;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 35)
        {
            int totalProducts =  (await _productRepository.GetAllAsync()).Count();
            var products = (await _productRepository.GetAllAsync())
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new PaginatedList<Product>
            {
                Items = products,
                TotalItems = totalProducts,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize)
            };

            return View(viewModel);
        }

       
        public IActionResult Add() => View();

        [HttpPost]
        public  IActionResult Add(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            _productRepository.Insert(product);
            _productRepository.Save();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var product = await _productRepository.GetById(id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmRemove(int id)
        {
            var product = await _productRepository.GetById(id);
            if (product != null)
            {
                _productRepository.Delete(product);
                _productRepository.Save();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetById(id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product updatedProduct)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedProduct);
            }

            var product = await _productRepository.GetById(id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            product.ProductName = updatedProduct.ProductName;
            product.buyPrice = updatedProduct.buyPrice;
            product.SellPrice = updatedProduct.SellPrice;
            product.Quantity = updatedProduct.Quantity;

            _productRepository.Save();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> FilterProducts(
              string searchTerm,
              string quantityFilter,
              int? quantity,
              int pageNumber = 1,
              int pageSize = 10)
        {
            var query =  _productRepository.GetAll();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.ProductName.Contains(searchTerm));
            }

            // Apply quantity filter
            if (quantity.HasValue)
            {
                switch (quantityFilter)
                {
                    case "greater":
                        query = query.Where(p => p.Quantity > quantity.Value);
                        break;
                    case "less":
                        query = query.Where(p => p.Quantity < quantity.Value);
                        break;
                    case "equal":
                        query = query.Where(p => p.Quantity == quantity.Value);
                        break;
                }
            }

            // Get total count of filtered products
            int totalCount = await query.CountAsync();

            // Apply pagination
            var filteredProducts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.buyPrice,
                    p.SellPrice,
                    p.Quantity
                })
                .ToListAsync();

            // Return the filtered and paginated results
            return Json(new
            {
                Products = filteredProducts,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        public IActionResult Profits(DateTime? selectedDate, int? selectedMonth, int? selectedYear)
        {
            DateTime today = DateTime.Now.Date;
            DateTime filterDate = selectedDate ?? today;

            // If no month/year is selected, default to the current month and year
            int month = selectedMonth ?? today.Month;
            int year = selectedYear ?? today.Year;

            // Fetch orders for the selected date
            var orders = _orderRepository.GetAll()
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderDate.Date == filterDate)
                .ToList();

            // Calculate cumulative profit for the selected day (before extra payments)
            double cumulativeDailyProfit = orders
                .Sum(o => o.OrderDetails.Sum(od => (od.SellPrice - od.BuyPrice) * od.Quantity));

            // Fetch extra payments for the selected date
            var dailyExtraPayments = _extraPaymentRepository.GetAll()
                .Where(ep => ep.Date.Date == filterDate)
                .ToList();

            // Calculate total daily extra payments
            double dailyExtraPaymentsTotal = (double)dailyExtraPayments.Sum(ep => ep.Cost);

            // Adjust daily profit by subtracting extra payments
            double netDailyProfit = cumulativeDailyProfit - dailyExtraPaymentsTotal;

            // Retrieve individual orders for the selected day
            var dailyOrders = orders
                .Select(o => new OrderProfitViewModel
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate.ToString("hh:mm tt"),
                    TotalAmount = o.TotalAmount,
                    OrderDetails = o.OrderDetails
                        .Select(od => new OrderDetailProfitViewModel
                        {
                            ProductName = od.ProductName,
                            Quantity = od.Quantity,
                            TotalSell = od.SellPrice * od.Quantity,
                            SellPrice = od.SellPrice,
                            ProfitPerItem = od.SellPrice - od.BuyPrice,
                            TotalProfit = (od.SellPrice - od.BuyPrice) * od.Quantity
                        })
                        .ToList()
                })
                .ToList();

            // Calculate cumulative profit for the selected month (before extra payments)
            var monthlyProfitBeforeExtra = ( _orderRepository.GetAll())
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
                .Sum(o => o.OrderDetails.Sum(od => (od.SellPrice - od.BuyPrice) * od.Quantity));

            // Fetch extra payments for the selected month
            var monthlyExtraPayments = ( _extraPaymentRepository.GetAll())
                .Where(ep => ep.Date.Year == year && ep.Date.Month == month)
                .Sum(ep => ep.Cost);

            // Adjust monthly profit by subtracting extra payments
            double netMonthlyProfit = monthlyProfitBeforeExtra - monthlyExtraPayments;

            // Prepare the view model
            var model = new ProfitsViewModel
            {
                SelectedDate = filterDate,
                SelectedMonth = month,
                SelectedYear = year,
                CumulativeDailyProfit = netDailyProfit, // Updated to net profit
                MonthlyProfit = netMonthlyProfit,       // Updated to net profit
                DailyOrders = dailyOrders,
                DailyExtraPayments = dailyExtraPayments, // New property for daily extra payments
                DailyExtraPaymentsTotal = (decimal)dailyExtraPaymentsTotal, // New property for total daily extra payments
                MonthlyExtraPaymentsTotal = monthlyExtraPayments   // New property for total monthly extra payments
            };

            return View(model);
        }
        public IActionResult MoneyFlow(DateTime? startDate, DateTime? endDate)
        {
            // Set default date range if not provided
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            // Part 1: Earnings and Profit from Sales
            double totalEarnings = ( _orderRepository.GetAll())
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .SelectMany(o => o.OrderDetails)
                .Sum(od => (od.SellPrice * od.Quantity));

            double totalProfitFromSales = ( _orderRepository.GetAll())
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .SelectMany(o => o.OrderDetails)
                .Sum(od => (od.SellPrice - od.BuyPrice) * od.Quantity);

            // Part 2: Spending on Purchasing Goods
            double totalSpendingOnPurchases = _purchaseInvoiceRepository.GetAll()
                  .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
                  .Sum(p => (double)p.TotalAmount);

            // Part 3: Extra Payments
            var totalExtraPayments = ( _extraPaymentRepository.GetAll())
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .Sum(e => e.Cost);

            // Calculate Net Profit
            var netProfit = totalEarnings - (double)totalSpendingOnPurchases - totalExtraPayments;

            // Create the ViewModel
            var financialSummary = new FinancialSummaryViewModel
            {
                TotalEarnings = totalEarnings,
                TotalProfitFromSales = totalProfitFromSales,
                TotalSpendingOnPurchases = (double)totalSpendingOnPurchases,
                TotalExtraPayments = totalExtraPayments,
                NetProfit = netProfit
            };

            // Pass dates to the view for filtering
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            return View(financialSummary);
        }
        [HttpPost]
        public IActionResult DeleteOrder(int orderId)
        {
            var order = _orderRepository.GetAll()
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("GetProfits");
            }

            _orderRepository.Delete(order);
            _orderRepository.Save();

            TempData["SuccessMessage"] = "Order deleted successfully!";
            return RedirectToAction("Profits", new { selectedDate = Request.Form["selectedDate"] }); // Pass selectedDate to maintain filter
        }
        [HttpGet]
        public IActionResult GetTotals()
        {
            var totalSellPrice = _productRepository.GetAll()
                .Sum(od => od.SellPrice * od.Quantity);

            var totalProfit = _productRepository.GetAll()
                .Sum(od => (od.SellPrice - od.buyPrice) * od.Quantity);
            var totalBuyPrice = _productRepository.GetAll()
                .Sum(od => od.buyPrice * od.Quantity);

            return Json(new
            {
                totalSellPrice = totalSellPrice,
                totalBuyPrice = totalBuyPrice,
                totalProfit = totalProfit
            });
        }
        [HttpPost]
        public IActionResult DeleteExtraPayment(int extraPaymentId)
        {
            var extraPayment = _extraPaymentRepository.GetAll()
                .FirstOrDefault(e => e.Id == extraPaymentId);

            if (extraPayment == null)
            {
                TempData["ErrorMessage"] = "Extra payment not found.";
                return NotFound();
            }

            _extraPaymentRepository.Delete(extraPayment);
            _extraPaymentRepository.Save();

            TempData["SuccessMessage"] = "Extra payment deleted successfully!";
            return RedirectToAction("Profits"); // Redirects to the purchase history view
        }

    }
}
