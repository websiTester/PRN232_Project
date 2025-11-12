namespace Backend.DTOs.Dispute
{
    public class DisputeListItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        public string BuyerName { get; set; }
        public string SellerName { get; set; } // có thể để nullable, vì mỗi dispute có thể nhiều seller
        public string Description { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; }
        public DateTime? SubmittedDate { get; set; }

        public List<DisputeProductDto> Products { get; set; } = new();
    }
}
