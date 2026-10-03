using System.Threading.Tasks;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using _25_TranHoangDung_Assignment01_FrontEnd.Helpers;
using _25_TranHoangDung_Assignment01_FrontEnd.Services;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Controllers
{
    public class AccountsController : Controller
    {
        private readonly NewsApiClient _apiClient;

        public AccountsController(NewsApiClient apiClient)
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
                TempData["ErrorMessage"] = "Access denied! Only Administrator can manage accounts.";
                redirectResult = RedirectToAction("Index", "Home");
                return false;
            }
            redirectResult = null;
            return true;
        }

        public async Task<IActionResult> Index(string? search, int? role)
        {
            if (!EnsureAdmin(out var redirect)) return redirect!;

            var accounts = await _apiClient.GetAccountsAsync(search, role);
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;

            return View(accounts);
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountJson(short id)
        {
            if (!EnsureAdmin(out var redirect)) return Unauthorized();

            var account = await _apiClient.GetAccountByIdAsync(id);
            if (account == null) return NotFound(new { message = "Account not found" });

            return Json(new
            {
                accountID = account.AccountID,
                accountName = account.AccountName,
                accountEmail = account.AccountEmail,
                accountRole = account.AccountRole
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckDelete(short id)
        {
            if (!EnsureAdmin(out var redirect)) return Unauthorized();

            var check = await _apiClient.CheckDeleteAccountAsync(id);
            return Json(new { canDelete = check.CanDelete, message = check.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateDto dto)
        {
            if (!EnsureAdmin(out var redirect)) return redirect!;

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation Error: {errors}";
                return RedirectToAction(nameof(Index));
            }

            var result = await _apiClient.CreateAccountAsync(dto);
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
        public async Task<IActionResult> Edit(short id, AccountUpdateDto dto)
        {
            if (!EnsureAdmin(out var redirect)) return redirect!;

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation Error: {errors}";
                return RedirectToAction(nameof(Index));
            }

            dto.AccountID = id;
            var result = await _apiClient.UpdateAccountAsync(id, dto);
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
            if (!EnsureAdmin(out var redirect)) return redirect!;

            var result = await _apiClient.DeleteAccountAsync(id);
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
