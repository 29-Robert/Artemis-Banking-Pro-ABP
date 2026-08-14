using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApi.Controllers
{
    public class CommerceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
