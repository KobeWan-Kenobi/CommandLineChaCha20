using Microsoft.AspNetCore.Mvc;

namespace FileEncryptionWebApp.Controllers
{
    public class EntriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
