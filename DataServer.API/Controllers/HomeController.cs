using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataServer.API.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        [HttpGet("data")]
        public IActionResult Index()
        {
            var userName = HttpContext.User.Identity.Name;
            var userEmail = HttpContext.User.FindFirstValue(ClaimTypes.Email);

            return Ok(new { Name = userName, Email = userEmail});
        }
    }
}
