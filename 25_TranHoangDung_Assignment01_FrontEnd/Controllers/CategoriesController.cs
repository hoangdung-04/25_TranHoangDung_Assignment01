using System.Threading.Tasks;
using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using _25_TranHoangDung_Assignment01_FrontEnd.Helpers;
using _25_TranHoangDung_Assignment01_FrontEnd.Services;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly NewsApiClient _apiClient;

        public CategoriesController(NewsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        private bool EnsureStaff(out IActionResult? redirectResult)
        {
            var user = UserSession.FromHttpContext(HttpContext);
            if (!user.IsLoggedIn)
            {
                redirectResult = RedirectToAction("Login", "Auth");
                return false;
            }
            if (!user.IsStaff && !user.IsAdmin)
            {
                TempData["ErrorMessage"] = "Access denied! Only Staff or Admin can manage categories.";
                redirectResult = RedirectToAction("Index", "Home");
                return false;
            }
            redirectResult = null;
            return true;
        }

        public async Task<IActionResult> Index(string? search, bool? activeOnly)
        {
            if (!EnsureStaff(out var redirect)) return redirect!;

            var categories = await _apiClient.GetCategoriesAsync(activeOnly);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                categories = categories.FindAll(c => c.CategoryName.ToLower().Contains(s) ||
                                                     (c.CategoryDesciption != null && c.CategoryDesciption.ToLower().Contains(s)));
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentActiveOnly = activeOnly;
            ViewBag.AllCategories = await _apiClient.GetCategoriesAsync();

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryJson(short id)
        {
            if (!EnsureStaff(out var redirect)) return Unauthorized();

            var category = await _apiClient.GetCategoryByIdAsync(id);
            if (category == null) return NotFound(new { message = "Category not found" });

            return Json(new
            {
                categoryID = category.CategoryID,
                categoryName = category.CategoryName,
                categoryDesciption = category.CategoryDesciption,
                parentCategoryID = category.ParentCategoryID,
                isActive = category.IsActive
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckDelete(short id)
        {
            if (!EnsureStaff(out var redirect)) return Unauthorized();

            var check = await _apiClient.CheckDeleteCategoryAsync(id);
            return Json(new { canDelete = check.CanDelete, message = check.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!EnsureStaff(out var redirect)) return redirect!;

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation Error: {errors}";
                return RedirectToAction(nameof(Index));
            }

            var result = await _apiClient.CreateCategoryAsync(category);
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
        public async Task<IActionResult> Edit(short id, Category category)
        {
            if (!EnsureStaff(out var redirect)) return redirect!;

            category.CategoryID = id;
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation Error: {errors}";
                return RedirectToAction(nameof(Index));
            }

            var result = await _apiClient.UpdateCategoryAsync(id, category);
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
        public async Task<IActionResult> Delete(short id)
        {
            if (!EnsureStaff(out var redirect)) return redirect!;

            var result = await _apiClient.DeleteCategoryAsync(id);
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
    }
}
