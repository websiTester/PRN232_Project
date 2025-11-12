using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ReviewRepository2 : IReviewRepository2
    {
        private readonly CloneEbayDbContext _context;

        public ReviewRepository2(CloneEbayDbContext context)
        {
            _context = context;
        }
        public async Task<Review> AddReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review?> GetExistingReviewAsync(int sellerId, int productId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewerId == sellerId && r.ProductId == productId);
        }

        public async Task<OrderItem?> GetOrderItemWithOrderAsync(int orderId, int productId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductId == productId);
        }

        public async Task<Product?> GetProductByIdAndSellerIdAsync(int productId, int sellerId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == sellerId);
        }

        public async Task<IEnumerable<Review>> GetReviewsReceivedByBuyerAsync(int buyerId)
        {
            var query = from order in _context.OrderTables
                        join orderItem in _context.OrderItems on order.Id equals orderItem.OrderId
                        join product in _context.Products on orderItem.ProductId equals product.Id
                        join review in _context.Reviews on product.Id equals review.ProductId
                        where order.BuyerId == buyerId &&
                              review.ReviewerId == product.SellerId
                        select review;

            return await query.Distinct().ToListAsync();
        }
    }
}
