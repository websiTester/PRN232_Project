using Backend.Models;

namespace Backend.Repositories.Interface
{
    public interface IDisputeRepository
    {
        Task<OrderTable> GetOrderByIdAndBuyerAsync(int orderId, int buyerId);
        Task<IEnumerable<OrderTable>> GetDisputableOrdersAsync(int buyerId);

        // === Methods liên quan đến Dispute ===
        Task<Dispute> AddDisputeAsync(Dispute dispute);
        Task<bool> HasPendingDisputeAsync(int orderId);

    }



    // Unit of Work: Đảm bảo các thay đổi được lưu đồng bộ
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
