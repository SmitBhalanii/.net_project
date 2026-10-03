using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers
{
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
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                TempData["ErrorMessage"] = "Start date cannot be after end date.";
                return RedirectToAction("Admin");
            }

            var model = await _reportingService.GetAdminDashboardAsync(startDate, endDate);
            return View(model);
        }

        [Authorize(Roles = RoleNames.Business)]
        public async Task<IActionResult> Business(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                TempData["ErrorMessage"] = "Start date cannot be after end date.";
                return RedirectToAction("Business");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var business = await _businessService.GetBusinessByUserIdAsync(userId);
            if (business == null) return Forbid();

            var model = await _reportingService.GetBusinessDashboardAsync(business.Id, startDate, endDate);
            return View(model);
        }

        [Authorize(Roles = RoleNames.Customer)]
        public async Task<IActionResult> Customer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var model = await _reportingService.GetCustomerStatisticsAsync(userId);
            return View(model);
        }
    }
}
