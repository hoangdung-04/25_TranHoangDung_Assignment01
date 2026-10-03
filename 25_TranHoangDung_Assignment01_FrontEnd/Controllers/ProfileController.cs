using System.Threading.Tasks;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using _25_TranHoangDung_Assignment01_FrontEnd.Helpers;
using _25_TranHoangDung_Assignment01_FrontEnd.Services;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Controllers
{
    public class ProfileController : Controller
    {
        private readonly NewsApiClient _apiClient;

        public ProfileController(NewsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var user = UserSession.FromHttpContext(HttpContext);
            if (!user.IsLoggedIn)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (user.IsAdmin)
            {
                // Admin profile is virtual from appsettings.json
                var adminProfile = new UserProfileDto
                {
                    AccountID = 0,
                    AccountName = user.AccountName,
                    AccountEmail = user.AccountEmail,
                    AccountRole = 0
                };
                return View(adminProfile);
            }

            var account = await _apiClient.GetAccountByIdAsync(user.AccountID);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var profile = new UserProfileDto
            {
                AccountID = account.AccountID,
                AccountName = account.AccountName,
                AccountEmail = account.AccountEmail,
                AccountRole = account.AccountRole
            };

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserProfileDto dto)
        {
            var user = UserSession.FromHttpContext(HttpContext);
            if (!user.IsLoggedIn)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (user.IsAdmin)
            {
                TempData["ErrorMessage"] = "Default Admin profile is managed via configuration.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View("Index", dto);
            }

            dto.AccountID = user.AccountID;
            var result = await _apiClient.UpdateProfileAsync(user.AccountID, dto);
            if (result.Success)
            {
                // Update session name if changed
                if (!string.IsNullOrEmpty(dto.AccountName))
                {
                    user.AccountName = dto.AccountName;
                    user.SaveToSession(HttpContext);
                }
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
