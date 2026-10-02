using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBusinessController : Controller
{
    private readonly IBusinessService _businessService;

    public AdminBusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    public async Task<IActionResult> Index()
    {
        var businesses = await _businessService.GetAllBusinessesAsync();
        return View(businesses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, BusinessApprovalStatus status)
    {
        await _businessService.UpdateBusinessStatusAsync(id, status);
        TempData["SuccessMessage"] = $"Business status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }
}
