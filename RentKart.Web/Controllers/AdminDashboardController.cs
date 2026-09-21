using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminDashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
