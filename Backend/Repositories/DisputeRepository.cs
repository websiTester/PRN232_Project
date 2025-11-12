using Backend.DTOs.Dispute;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class DisputeRepository : IDisputeRepository
    {
        private readonly CloneEbayDbContext _context;

        public DisputeRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<List<DisputeListItemDto>> GetDisputesByBuyerAsync(int buyerId)
        {
            // 1. Lấy các dispute của buyer, include đủ nav để dùng
            var disputes = await _context.Disputes
                .Where(d => d.RaisedBy == buyerId)
                .Include(d => d.Order)
                    .ThenInclude(o => o.Buyer)                   // User buyer
                .Include(d => d.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Seller)          // User seller
                .ToListAsync();

            // 2. Map sang DTO hoàn toàn bằng C#
            var result = disputes.Select(d =>
            {
                var order = d.Order;

                var products = order?.OrderItems?
                    .Select(oi => new DisputeProductDto
                    {
                        ProductId = (int)oi.ProductId,
                        ProductTitle = oi.Product.Title,
                        SellerName = oi.Product.Seller.Username
                    })
                    .ToList() ?? new List<DisputeProductDto>();

                return new DisputeListItemDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId ?? 0,
                    BuyerName = order?.Buyer?.Username,
                    SellerName = products.Select(p => p.SellerName).Distinct().FirstOrDefault(),
                    Description = d.Description,
                    Status = d.Status,
                    SubmittedDate = d.SubmittedDate,
                    Products = products,
                    Resolution = d.Resolution
                };
            })
                // ❶ Sắp xếp: status 1 & 3 lên trước, còn lại xuống dưới
                .OrderBy(x => (x.Status.Equals("1") || x.Status.Equals("3")) ? 0 : 1)
                // ❷ Trong mỗi nhóm vẫn sort theo ngày gửi (mới nhất trước)
                .ThenByDescending(x => x.SubmittedDate ?? DateTime.MinValue)
                .ToList();


            return result;
        }

        public async Task<List<DisputeListItemDto>> GetDisputesBySellerAsync(int sellerId)
        {
            // 1. Lấy dispute mà trong order có ít nhất 1 product thuộc seller đó
            var disputes = await _context.Disputes
                .Include(d => d.Order)
                    .ThenInclude(o => o.Buyer)
                .Include(d => d.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Seller)
                .Where(d => d.Order.OrderItems
                    .Any(oi => oi.Product.SellerId == sellerId) && (d.Status.Equals("1")||d.Status.Equals("2")))
                .ToListAsync();

            // 2. Map sang DTO
            var result = disputes.Select(d =>
            {
                var order = d.Order;

                var products = order?.OrderItems?
                    .Where(oi => oi.Product.SellerId == sellerId)
                    .Select(oi => new DisputeProductDto
                    {
                        ProductId = (int)oi.ProductId,
                        ProductTitle = oi.Product.Title,
                        SellerName = oi.Product.Seller.Username
                    })
                    .ToList() ?? new List<DisputeProductDto>();

                return new DisputeListItemDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId ?? 0,
                    BuyerName = order?.Buyer?.Username,
                    SellerName = products.Select(p => p.SellerName).Distinct().FirstOrDefault(),
                    Description = d.Description,
                    Status = d.Status,
                    SubmittedDate = d.SubmittedDate,
                    Products = products,
                    Resolution = d.Resolution
                };
            })
                .OrderBy(x => (x.Status.Equals("1")) ? 0 : 1)
                .OrderByDescending(x => x.SubmittedDate)
                .ToList();


            return result;
        }
    }

}

