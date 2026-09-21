using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Customer)]
public class CustomerDashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
