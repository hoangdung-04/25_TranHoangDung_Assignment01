using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories.Interfaces;

namespace _25_TranHoangDung_Assignment01_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISystemAccountRepository _accountRepository;
        private readonly IConfiguration _configuration;

        public AuthController(ISystemAccountRepository accountRepository, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];
            var adminName = _configuration["AdminAccount:Name"] ?? "Administrator";

            // Check admin account from appsettings.json
            if (!string.IsNullOrEmpty(adminEmail) &&
                string.Equals(request.Email.Trim(), adminEmail.Trim(), System.StringComparison.OrdinalIgnoreCase) &&
                request.Password == adminPassword)
            {
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Admin logged in successfully",
                    AccountID = 0,
                    AccountName = adminName,
                    AccountEmail = adminEmail,
                    Role = 0,
                    RoleName = "Admin"
                });
            }

            // Check database system accounts
            var account = _accountRepository.GetAccountByEmail(request.Email);
            if (account != null && account.AccountPassword == request.Password)
            {
                var roleName = account.AccountRole switch
                {
                    1 => "Staff",
                    2 => "Lecturer",
                    _ => "User"
                };

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    AccountID = account.AccountID,
                    AccountName = account.AccountName ?? "Staff Member",
                    AccountEmail = account.AccountEmail ?? request.Email,
                    Role = account.AccountRole ?? 2,
                    RoleName = roleName
                });
            }

            return Unauthorized(new { message = "You are not allowed to access this function!" });
        }
    }
}
