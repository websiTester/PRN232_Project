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

        public async Task<IEnumerable<SellerSalesOrderDto>> GetSalesHistoryAsync(string sellerUsername)
        {
            var user = await GetUserFromUsername(sellerUsername);
            if (user == null || (user.Role != "Seller" && user.Role != "Supporter")) // Sửa logic role
            {
                return new List<SellerSalesOrderDto>();
            }

            var orderItems = await _orderRepository.GetOrderItemsBySellerIdAsync(user.Id);

            // 1. Lấy tất cả feedback của seller này MỘT LẦN
            //    (Giả định bạn đã thêm IFeedbackRepository, nếu không hãy dùng _context trực tiếp)
            //    Ở đây tôi sẽ dùng cách JOIN từ OrderItems cho đúng

            var groupedOrders = orderItems
                .GroupBy(oi => oi.Order)
                .Select(group =>
                {
                    var order = group.Key;

                    // 2. Tìm feedback cho đơn hàng NÀY
                    //    (Bảng Feedback có liên kết 1-1 với OrderTable, thông qua OrdersId)
                    var feedback = order.Feedbacks.FirstOrDefault(); // Lấy feedback đầu tiên (nếu có)

                    return new SellerSalesOrderDto
                    {
                        OrderId = order.Id,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.Status,
                        OrderTotalPrice = order.TotalPrice,

                        BuyerId = order.BuyerId ?? 0,
                        BuyerUsername = order.Buyer?.Username ?? "Unknown Buyer",

                        Items = group.Select(oi => new SellerSalesItemDto
                        {
                            ProductId = oi.ProductId ?? 0,
                            ProductTitle = oi.Product?.Title,
                            Quantity = oi.Quantity ?? 0,
                            UnitPrice = oi.UnitPrice
                        }).ToList(),

                        // 3. ĐIỀN DỮ LIỆU FEEDBACK
                        HasBuyerFeedback = feedback != null,
                        BuyerFeedbackId = feedback?.Id,
                        BuyerFeedbackRating = feedback?.AverageRating
                    };
                });

            return groupedOrders.ToList();
        }
    }
}