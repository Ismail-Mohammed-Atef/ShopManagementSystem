using DataLayer;
using DataLayer.Models;
using SystemApp.Repository;
using SystemApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace SystemApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PurchaseController : Controller
    {
        private readonly IPurchaseInvoiceRepository _purchaseInvoiceRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPurchaseDetailRepository _purchaseDetailRepository;
        private readonly IExtraPaymentRepository _extraPaymentRepository;
        private readonly IOrderRepository _orderRepository;

        public PurchaseController(IPurchaseInvoiceRepository purchaseInvoiceRepository , IProductRepository productRepository,IPurchaseDetailRepository purchaseDetailRepository,IExtraPaymentRepository extraPaymentRepository , IOrderRepository orderRepository)
        {
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _productRepository = productRepository;
            _purchaseDetailRepository = purchaseDetailRepository;
            _extraPaymentRepository = extraPaymentRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
           
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> SavePurchases([FromBody] PurchaseRequest requestData)
        {
            try
            {
                if (requestData == null || requestData.Products == null || !requestData.Products.Any())
                {
                    return Json(new { success = false, message = "Invalid request format or no products." });
                }

                decimal totalInvoice = requestData.TotalInvoice;
                var updatedProducts = requestData.Products;
                var note = requestData.Note;
                bool changesMade = false;
                List<object> updatedProductList = new List<object>();

                // Create a new purchase invoice
                var purchaseInvoice = new PurchaseInvoice
                {
                    Note = note,
                    TotalAmount = totalInvoice,
                    PurchaseDate = DateTime.UtcNow
                };
                _purchaseInvoiceRepository.Insert(purchaseInvoice);
                _purchaseInvoiceRepository.Save();  // Save invoice first to get ID

                foreach (var updatedProduct in updatedProducts)
                {
                    var product = await _productRepository.GetById(updatedProduct.id);
                    if (product != null && updatedProduct.quantity!=0)
                    {
                        bool isUpdated = false;

                        // Save individual product purchase details
                        var purchaseDetail = new PurchaseDetail
                        {
                            PurchaseInvoiceId = purchaseInvoice.Id,
                            ProductName = product.ProductName,
                            Quantity = updatedProduct.quantity,
                            BuyPrice = updatedProduct.buyPrice,
                            SellPrice = updatedProduct.sellPrice
                        };
                        _purchaseDetailRepository.Insert(purchaseDetail);

                        // Update stock quantity
                        double newQuantity = product.Quantity + updatedProduct.quantity;
                        if (product.Quantity != newQuantity)
                        {
                            product.Quantity = newQuantity;
                            isUpdated = true;
                        }

                        // Update buy price if changed
                        if (product.buyPrice != updatedProduct.buyPrice)
                        {
                            product.buyPrice = updatedProduct.buyPrice;
                            isUpdated = true;
                        }

                        // Update sell price if changed
                        if (product.SellPrice != updatedProduct.sellPrice)
                        {
                            product.SellPrice = updatedProduct.sellPrice;
                            isUpdated = true;
                        }

                        if (isUpdated)
                        {
                            updatedProductList.Add(new
                            {
                                id = product.Id,
                                newQuantity = product.Quantity,
                                newBuyPrice = product.buyPrice,
                                newSellPrice = product.SellPrice
                            });

                            changesMade = true;
                        }
                    }
                }

                if (changesMade)
                {
                    _purchaseDetailRepository.Save();
                    return Json(new { success = true, updatedProducts = updatedProductList });
                }

                return Json(new { success = false, message = "No changes detected." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        public async Task<IActionResult> GetPurchases(DateTime? startDate, DateTime? endDate)
        {
            // Query for purchase invoices
            IQueryable<PurchaseInvoice> purchaseQuery = _purchaseInvoiceRepository.GetAll()
                                                                .Include(p => p.PurchaseDetails);


            // Query for extra payments
            var extraPaymentsQuery = (await _extraPaymentRepository.GetAllAsync()).AsQueryable();

            // Apply date filter to both if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                purchaseQuery = purchaseQuery.Where(p => p.PurchaseDate >= startDate.Value && p.PurchaseDate <= endDate.Value);
                extraPaymentsQuery = extraPaymentsQuery.Where(e => e.Date >= startDate.Value && e.Date <= endDate.Value);
            }

            // Fetch data
            var purchases = await purchaseQuery
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            var extraPayments = extraPaymentsQuery
                .OrderByDescending(e => e.Date)
                .ToList();

            ViewBag.ExtraPayments = extraPayments;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(purchases);
        }
        [HttpGet]
        public async Task<IActionResult> GetFilteredPurchases(DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 15)
        {
            // Default to the current month if no dates are provided
            if (!startDate.HasValue || !endDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                endDate = startDate.Value.AddMonths(1).AddDays(-1);
            }

            // Query invoices within the date range
            var invoices =(await _purchaseInvoiceRepository.GetAllAsync()).AsQueryable()
                .Include(i => i.PurchaseDetails)
                .Where(i => i.PurchaseDate >= startDate && i.PurchaseDate <= endDate)
                .OrderByDescending(i => i.PurchaseDate)
                .ToList();

            // Pagination logic
            var totalInvoices = invoices.Count;
            var paginatedInvoices = invoices.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Return JSON response
            return Json(new
            {
                invoices = paginatedInvoices,
                totalInvoices,
                page,
                pageSize,
                startDate = startDate.Value.ToString("yyyy-MM-dd"),
                endDate = endDate.Value.ToString("yyyy-MM-dd")
            });
        }
        [HttpPost]
        public IActionResult SaveExtraPayments([FromBody] ExtraPaymentsRequest request)
        {
            if (request == null || request.ExtraPayments == null || !request.ExtraPayments.Any())
            {
                return Json(new { success = false, message = "No extra payments provided." });
            }

            foreach (var payment in request.ExtraPayments)
            {
                var extraPayment = new ExtraPayments
                {
                    Description = payment.Description,
                    Cost = payment.Price,
                    Date = DateTime.Now // Optional: Add timestamp
                };
                _extraPaymentRepository.Insert(extraPayment);
            }

            _extraPaymentRepository.Save();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeletePurchase(int id)
        {
            var purchase = _purchaseInvoiceRepository.GetAll()
                .Include(p => p.PurchaseDetails)
                .FirstOrDefault(p => p.Id == id);

            if (purchase == null)
            {
                TempData["ErrorMessage"] = "Purchase not found.";
                return NotFound();
            }

            _purchaseInvoiceRepository.Delete(purchase);
            _purchaseInvoiceRepository.Save();

            TempData["SuccessMessage"] = "Purchase deleted successfully!";
            return RedirectToAction("GetPurchases"); // Redirects to the purchase history view
        }

        [HttpPost]
        public IActionResult DeleteExtraPayment(int id)
        {
            var extraPayment = _extraPaymentRepository.GetAll()
                .FirstOrDefault(e => e.Id == id);

            if (extraPayment == null)
            {
                TempData["ErrorMessage"] = "Extra payment not found.";
                return NotFound();
            }

            _extraPaymentRepository.Delete(extraPayment);
            _extraPaymentRepository.Save();

            TempData["SuccessMessage"] = "Extra payment deleted successfully!";
            return RedirectToAction("GetPurchases"); // Redirects to the purchase history view
        }

    }
}
