using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLA.Ordering.Web.Controllers;

[Authorize]
[Route("dashboard")]
public sealed class HomeController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View("Index");
}
