using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
	public class FeedbackController : Controller
	{

		public IActionResult OverviewFeedback()
		{
			return View();
		}

		public IActionResult FormFeedback()
		{

			return View();
		}

		[HttpPost]
		public IActionResult FormFeedback(FormFeedbackViewModel model)
		{

			return View();
		}
	}
}
