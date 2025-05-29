using Microsoft.AspNetCore.Mvc;

namespace fantasydg.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}