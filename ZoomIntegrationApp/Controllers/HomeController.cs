using Microsoft.AspNetCore.Mvc;

namespace ZoomIntegrationApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
