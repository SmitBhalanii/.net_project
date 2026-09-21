using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Business)]
public class BusinessDashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
