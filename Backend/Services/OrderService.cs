using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories;
// using Microsoft.EntityFrameworkCore; // <-- XÓA BỎ using này

namespace Backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        // SỬA: Dùng Interface
        private readonly IUserRepository _userRepository;
        // THÊM: Cần IReviewRepository để kiểm tra
        private readonly IReviewRepository _reviewRepository;
        // private readonly CloneEbayDbContext _context; // <-- XÓA BỎ DbContext

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository, // <-- SỬA: Inject interface
            IReviewRepository reviewRepository // <-- THÊM: Inject interface
                                               // CloneEbayDbContext context) // <-- XÓA
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _reviewRepository = reviewRepository; // <-- THÊM
            // _context = context; // <-- XÓA
        }

        private async Task<User?> GetUserFromUsername(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<bool> CreateQuickBuyOrderAsync(string buyerUsername, int productId)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return false;

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null || product.Price == null) return false;

            await _orderRepository.CreateSimpleOrderAsync(user.Id, productId, product.Price.Value);
            return true;
        }

        public async Task<IEnumerable<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(string buyerUsername)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null)
            {
                return new List<PurchaseHistoryItemDto>();
            }

            var orderItems = await _orderRepository.GetPurchaseHistoryAsync(user.Id);

            var dtos = new List<PurchaseHistoryItemDto>();

            foreach (var item in orderItems)
            {
                if (item.Product == null || item.Order == null) continue;

                // SỬA: Dùng Repository, không dùng DbContext trực tiếp
                bool hasReviewed = await _reviewRepository.HasUserReviewedProductAsync(user.Id, item.Product.Id);

                dtos.Add(new PurchaseHistoryItemDto
                {
                    OrderItemId = item.Id,
                    OrderId = item.OrderId ?? 0,
                    ProductId = item.Product.Id,
                    ProductTitle = item.Product.Title,
                    ProductImage = item.Product.Images,
                    UnitPrice = item.UnitPrice,
                    OrderDate = item.Order.OrderDate ?? DateTime.MinValue,
                    HasReviewed = hasReviewed
                });
            }

            return dtos;
        }
    }
}