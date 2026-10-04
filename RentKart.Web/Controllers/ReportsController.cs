using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportingService _reportingService;
    private readonly IBusinessService _businessService;

    public ReportsController(IReportingService reportingService, IBusinessService businessService)
    {
        _reportingService = reportingService;
        _businessService = businessService;
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Admin(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddMonths(-1);

            var report = await _reportingService.GetAdminDashboardAsync(start, end);
            return View(report);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to load admin analytics.";
            return RedirectToAction("Index", "AdminDashboard");
        }
    }

    [Authorize(Roles = RoleNames.Business)]
    public async Task<IActionResult> Business(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var vendorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(vendorId)) return Unauthorized();
            var business = await _businessService.GetBusinessByUserIdAsync(vendorId);
            if (business == null)
            {
                return RedirectToAction("Index", "BusinessDashboard");
            }

            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddMonths(-1);

            var report = await _reportingService.GetBusinessDashboardAsync(business.Id, start, end);
            return View(report);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to load business analytics.";
            return RedirectToAction("Index", "BusinessDashboard");
        }
    }

    [Authorize(Roles = RoleNames.Customer)]
    public async Task<IActionResult> Customer()
    {
        try
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var report = await _reportingService.GetCustomerStatisticsAsync(customerId);
            return View(report);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to load customer statistics.";
            return RedirectToAction("Index", "CustomerDashboard");
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> ExportAdmin(DateTime? startDate, DateTime? endDate)
    {
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddMonths(-1);
        var report = await _reportingService.GetAdminDashboardAsync(start, end);

        var csv = new StringBuilder();
        csv.AppendLine("Category,Equipment Count,Rental Count,Revenue");
        foreach (var cat in report.TopCategories)
        {
            csv.AppendLine($"\"{cat.CategoryName}\",{cat.EquipmentCount},{cat.RentalCount},{cat.Revenue}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Admin_Category_Report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
    }
}
