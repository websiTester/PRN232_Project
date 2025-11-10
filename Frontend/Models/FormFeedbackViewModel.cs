using Backend.Models;

namespace Frontend.Models
{
	public class FormFeedbackViewModel
	{
		public OrderTable OrderTable { get; set; }
		public int PositiveRate { get; set; }
		public int? DeliveryOnTime { get; set; }
		public int? ExactSame { get; set; }
		public int? Communication { get; set; }

	}
}
