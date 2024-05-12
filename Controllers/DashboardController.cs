using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
