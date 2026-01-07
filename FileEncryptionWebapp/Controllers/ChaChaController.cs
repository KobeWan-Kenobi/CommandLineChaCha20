using Microsoft.AspNetCore.Mvc;

namespace FileEncryptionWebApp.Controllers
{
    public class ChaChaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
