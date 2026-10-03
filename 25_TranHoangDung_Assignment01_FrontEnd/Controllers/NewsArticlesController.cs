using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using _25_TranHoangDung_Assignment01_FrontEnd.Helpers;
using _25_TranHoangDung_Assignment01_FrontEnd.Services;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Controllers
{
    public class NewsArticlesController : Controller
    {
        private readonly NewsApiClient _apiClient;

        public NewsArticlesController(NewsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        private bool EnsureStaffOrAdmin(out IActionResult? redirectResult, out UserSession user)
        {
            user = UserSession.FromHttpContext(HttpContext);
            if (!user.IsLoggedIn)
            {
                redirectResult = RedirectToAction("Login", "Auth");
                return false;
            }
            if (!user.IsStaff && !user.IsAdmin)
            {
                TempData["ErrorMessage"] = "Access denied! Only Staff or Admin can manage news articles.";
                redirectResult = RedirectToAction("Index", "Home");
                return false;
            }
            redirectResult = null;
            return true;
        }

        public async Task<IActionResult> Index(bool? status, string? search, short? categoryId)
        {
            if (!EnsureStaffOrAdmin(out var redirect, out _)) return redirect!;

            var newsList = await _apiClient.GetAllNewsAsync(status, search, categoryId);
            ViewBag.Categories = await _apiClient.GetCategoriesAsync();
            ViewBag.Tags = await _apiClient.GetTagsAsync();
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(newsList);
        }

        public async Task<IActionResult> History()
        {
            if (!EnsureStaffOrAdmin(out var redirect, out var user)) return redirect!;

            var newsList = await _apiClient.GetNewsHistoryAsync(user.AccountID);
            ViewBag.Categories = await _apiClient.GetCategoriesAsync();
            ViewBag.Tags = await _apiClient.GetTagsAsync();

            return View(newsList);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewsJson(string id)
        {
            if (!EnsureStaffOrAdmin(out _, out _)) return Unauthorized();

            var news = await _apiClient.GetNewsByIdAsync(id);
            if (news == null) return NotFound(new { message = "News article not found" });

            return Json(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsArticleCreateDto dto)
        {
            if (!EnsureStaffOrAdmin(out var redirect, out var user)) return redirect!;

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation Error: {errors}";
                return RedirectToAction(nameof(Index));
            }

            dto.CreatedByID = user.AccountID;
            var result = await _apiClient.CreateNewsAsync(dto);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NewsArticleUpdateDto dto, string? returnUrl)
        {
            if (!EnsureStaffOrAdmin(out var redirect, out var user)) return redirect!;

            dto.NewsArticleID = id;
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation Error: {errors}";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction(nameof(Index));
            }

            dto.UpdatedByID = user.AccountID;
            var result = await _apiClient.UpdateNewsAsync(id, dto);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, string? returnUrl)
        {
            if (!EnsureStaffOrAdmin(out var redirect, out _)) return redirect!;

            var result = await _apiClient.DeleteNewsAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}
