using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}