using System;
using System.Threading.Tasks;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using _25_TranHoangDung_Assignment01_FrontEnd.Helpers;
using _25_TranHoangDung_Assignment01_FrontEnd.Services;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Controllers
{
    public class ReportsController : Controller
    {
        private readonly NewsApiClient _apiClient;

        public ReportsController(NewsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        private bool EnsureAdmin(out IActionResult? redirectResult)
        {
            var user = UserSession.FromHttpContext(HttpContext);
            if (!user.IsLoggedIn)
            {
                redirectResult = RedirectToAction("Login", "Auth");
                return false;
            }
            if (!user.IsAdmin)
            {
                TempData["ErrorMessage"] = "Access denied! Only Administrator can view report statistics.";
                redirectResult = RedirectToAction("Index", "Home");
                return false;
            }
            redirectResult = null;
            return true;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (!EnsureAdmin(out var redirect)) return redirect!;

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                TempData["ErrorMessage"] = "Validation Error: Start Date cannot be later than End Date.";
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                return View(new ReportStatisticsDto());
            }

            var stats = await _apiClient.GetReportStatisticsAsync(startDate, endDate);

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(stats ?? new ReportStatisticsDto());
        }
    }
}
