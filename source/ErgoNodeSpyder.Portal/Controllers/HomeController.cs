using System;
using ErgoNodeSpyder.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ErgoNodeSpyder.Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("api")]
        public IActionResult Api()
        {
            return View();
        }

        [HttpGet]
        [Route("set-mode/{mode}")]
        public IActionResult SetMode(string mode)
        {
            if (!string.IsNullOrEmpty(mode) && "dark".Equals(mode))
            {
                Response.Cookies.Append("darkMode", "true", new CookieOptions{Expires = DateTimeOffset.UtcNow.AddYears(1)});
            }
            else if (!string.IsNullOrEmpty(mode) && "light".Equals(mode))
            {
                Response.Cookies.Delete("darkMode");
            }
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
