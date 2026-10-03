using System.Threading.Tasks;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using _25_TranHoangDung_Assignment01_FrontEnd.Helpers;
using _25_TranHoangDung_Assignment01_FrontEnd.Services;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Controllers
{
    public class AuthController : Controller
    {
        private readonly NewsApiClient _apiClient;

        public AuthController(NewsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var user = UserSession.FromHttpContext(HttpContext);
            if (user.IsLoggedIn)
            {
                if (user.IsAdmin) return RedirectToAction("Index", "Accounts");
                return RedirectToAction("Index", "NewsArticles");
            }
            return View(new LoginRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _apiClient.LoginAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            // Save to session
            var session = new UserSession
            {
                AccountID = result.AccountID,
                AccountName = result.AccountName,
                AccountEmail = result.AccountEmail,
                Role = result.Role,
                RoleName = result.RoleName
            };
            session.SaveToSession(HttpContext);

            TempData["SuccessMessage"] = $"Welcome back, {result.AccountName} ({result.RoleName})!";

            if (result.Role == 0) // Admin
            {
                return RedirectToAction("Index", "Accounts");
            }
            else if (result.Role == 1) // Staff
            {
                return RedirectToAction("Index", "NewsArticles");
            }
            else
            {
                // Lecturer
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Logout()
        {
            UserSession.ClearSession(HttpContext);
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}
