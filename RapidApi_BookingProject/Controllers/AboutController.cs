using Microsoft.AspNetCore.Mvc;

namespace RapidApi_BookingProject.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
