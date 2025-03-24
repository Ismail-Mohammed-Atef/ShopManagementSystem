using DataLayer;
using DataLayer.Models;
using SystemApp.Repository;
using SystemApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SystemApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IExtraPaymentRepository _extraPaymentRepository;

        public OrderController(IOrderRepository orderRepository , IProductRepository productRepository , IExtraPaymentRepository extraPaymentRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _extraPaymentRepository = extraPaymentRepository;
        }
        [HttpGet]
        public async Task<IActionResult> SearchProducts(
            string searchName,
            string quantityFilter,
            int? quantity,
            int pageNumber = 1,
            int pageSize = 15)
        {
            var query =  _productRepository.GetAll();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(p => p.ProductName.Contains(searchName));
            }

            if (quantity.HasValue)
            {
                switch (quantityFilter)
                {
                    case "greater":
                        query = query.Where(p => p.Quantity > quantity);
                        break;
                    case "less":
                        query = query.Where(p => p.Quantity < quantity);
                        break;
                    case "equal":
                        query = query.Where(p => p.Quantity == quantity);
                        break;
                }
            }

            // Calculate the number of items to skip
            int skipAmount = (pageNumber - 1) * pageSize;

            // Apply pagination
            var paginatedProducts = await query
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.SellPrice,
                    p.Quantity
                })
                .Skip(skipAmount)
                .Take(pageSize)
                .ToListAsync();

            // Optionally, return the total count of items for the client to know how many pages exist
            var totalCount = await query.CountAsync();

            return Ok(new
            {
                Products = paginatedProducts,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }


        public async Task<IActionResult> Create()
        {
            var products = (await _productRepository.GetAllAsync())
                .Select(p => new OrderProductViewModel
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    SellPrice = p.SellPrice,
                    BuyPrice = p.buyPrice,
                    AvailableQuantity = p.Quantity
                })
                .ToList();

            return View(new OrderViewModel { Products = products });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderViewModel model)
        {
            if (model.SelectedProducts == null || !model.SelectedProducts.Any())
            {
                return BadRequest("No products selected.");
            }

            // Validate stock availability
            foreach (var item in model.SelectedProducts)
            {
                var product = await _productRepository.GetById(item.ProductId);
                if (product == null || product.Quantity < item.Quantity)
                {
                    return BadRequest($"Not enough stock for {product?.ProductName ?? "Unknown"}.");
                }
            }

            // Create a new order
            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount // Set the total amount from the model
            };

            // Add order details and update product quantities
            foreach (var item in model.SelectedProducts)
            {
                var product = await _productRepository.GetById(item.ProductId);
                if (product != null)
                {
                    // Add order detail
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductName = product.ProductName,
                        BuyPrice = product.buyPrice,
                        Quantity = item.Quantity,
                        SellPrice = item.SellPrice
                    });

                    // Update product quantity
                    product.Quantity -= item.Quantity;
                    
                }
            }

            // Save the order and order details
            _orderRepository.Insert(order);
             _orderRepository.Save();

            return Ok(new { success = true, message = "Order created successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> RefundOrder([FromBody] OrderViewModel model)
        {
            if (model.SelectedProducts == null || !model.SelectedProducts.Any())
            {
                return BadRequest("No products selected.");
            }

            // Validate stock availability for refund
            foreach (var item in model.SelectedProducts)
            {
                var product = await _productRepository.GetById(item.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product not found: {item.ProductId}.");
                }
            }

            // Create a new refund
            var refund = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount // Set the total amount from the model
            };

            // Add refund details and update product quantities
            foreach (var item in model.SelectedProducts)
            {
                var product = await _productRepository.GetById(item.ProductId);
                if (product != null)
                {
                    // Add refund detail
                    refund.OrderDetails.Add(new OrderDetail
                    {
                        ProductName = product.ProductName,
                        BuyPrice = product.buyPrice,
                        Quantity = -item.Quantity,
                        SellPrice = item.SellPrice
                    });

                    // Update product quantity
                    product.Quantity += item.Quantity;

                }
            }

            // Save the refund and refund details
            _orderRepository.Insert(refund);
            _orderRepository.Save();

            return Ok(new { success = true, message = "Refund processed successfully!" });
        }
        [HttpPost]
        public async Task<IActionResult> RefreshProducts()
        {
            var products = (await _productRepository.GetAllAsync())
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.SellPrice,
                    p.Quantity
                })
                .ToList();

            return Ok(products);
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
