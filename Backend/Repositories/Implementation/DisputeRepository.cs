using Backend.Models;
using Backend.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementation
{
    public class DisputeRepository : IDisputeRepository
    {
        private readonly CloneEbayDbContext _context;

        public DisputeRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Dispute dispute)
        {
            await _context.Disputes.AddAsync(dispute);
        }

        public async Task<OrderTable> GetByIdAndBuyerAsync(int orderId, int buyerId)
        {
            return await _context.OrderTables
                        .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == buyerId);
        }

        public async Task<IEnumerable<OrderTable>> GetDisputableOrdersAsync(int buyerId)
        {
            var disputableStatuses = new[] { "Completed", "Shipped" };
            return await _context.OrderTables
            .Where(o => o.BuyerId == buyerId && disputableStatuses.Contains(o.Status))

            // Lấy thông tin cần thiết để hiển thị
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Seller)
                        .ThenInclude(s => s.Stores)

            // Lấy các đơn hàng chưa có khiếu nại nào đang "Pending"
            .Where(o => !_context.Disputes.Any(d => d.OrderId == o.Id && d.Status == "Pending"))

            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        }

        public async Task<Dispute> GetLatestByOrderIdAsync(int orderId)
        {
            return await _context.Disputes
                     .Where(d => d.OrderId == orderId)
                     .OrderByDescending(d => d.SubmittedDate) // Lấy cái mới nhất
                     .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPendingDisputeAsync(int orderId)
        {
            return await _context.Disputes
                        .AnyAsync(d => d.OrderId == orderId && d.Status == "1");
        }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly CloneEbayDbContext _context;
        public UnitOfWork(CloneEbayDbContext context) => _context = context;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
