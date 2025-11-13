using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CloneEbayDbContext _context;

        public OrderRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task CreateSimpleOrderAsync(int buyerId, int productId, decimal unitPrice)
        {
            var newOrder = new OrderTable
            {
                BuyerId = buyerId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = unitPrice,
                Status = "Completed"
            };

            _context.OrderTables.Add(newOrder);
            await _context.SaveChangesAsync();

            var newOrderItem = new OrderItem
            {
                OrderId = newOrder.Id,
                ProductId = productId,
                Quantity = 1,
                UnitPrice = unitPrice
            };

            _context.OrderItems.Add(newOrderItem);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetPurchaseHistoryAsync(int buyerId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.BuyerId == buyerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsBySellerIdAsync(int sellerId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order) // Lấy thông tin Order cha
                    .ThenInclude(o => o.Buyer) // Từ Order -> lấy thông tin Buyer
                .Include(oi => oi.Order) // Lấy thông tin Order cha (lần nữa)
                    .ThenInclude(o => o.Feedbacks) // ✅ TỪ ORDER -> LẤY FEEDBACK LIÊN QUAN
                .Include(oi => oi.Product) // Lấy thông tin Product (để check sellerId)
                .Where(oi => oi.Product != null && oi.Product.SellerId == sellerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .ToListAsync();
        }
    }
}