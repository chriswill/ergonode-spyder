using Microsoft.AspNetCore.Mvc;

namespace ErgoNodeSpyder.Portal.Controllers
{
    [Route("geo")]
    public class GeoController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
