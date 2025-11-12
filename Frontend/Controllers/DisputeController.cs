using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class DisputeController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
    }
}
