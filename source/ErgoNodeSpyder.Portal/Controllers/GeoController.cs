using Microsoft.AspNetCore.Mvc;

namespace ErgoNodeSpyder.Portal.Controllers
{
    public class GeoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
