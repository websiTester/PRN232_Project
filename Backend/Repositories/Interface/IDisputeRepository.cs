using Backend.Models;

namespace Backend.Repositories.Interface
{
    public interface IDisputeRepository
    {
        Task<bool> HasPendingDisputeAsync(int orderId);

        Task<Dispute> GetLatestByOrderIdAsync(int orderId);

        Task AddAsync(Dispute dispute);

        Task<OrderTable> GetByIdAndBuyerAsync(int orderId, int buyerId);

        Task<IEnumerable<OrderTable>> GetDisputableOrdersAsync(int buyerId);

    }



    // Unit of Work: Đảm bảo các thay đổi được lưu đồng bộ
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
