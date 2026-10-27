using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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

        private string GenerateJwtToken(string id, string email, string name, string role)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "FUNewsManagementSystem_Secret_Key_PRN232_2024_Assignment01!@#$%";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "FUNewsManagementAPI";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "FUNewsManagementClient";
            var duration = int.TryParse(_configuration["Jwt:DurationInMinutes"], out var d) ? d : 120;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duration),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
                string.Equals(request.Email.Trim(), adminEmail.Trim(), StringComparison.OrdinalIgnoreCase) &&
                request.Password == adminPassword)
            {
                var token = GenerateJwtToken("0", adminEmail, adminName, "Admin");
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Admin logged in successfully",
                    Token = token,
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

                var accountName = account.AccountName ?? "Staff Member";
                var email = account.AccountEmail ?? request.Email;
                var token = GenerateJwtToken(account.AccountID.ToString(), email, accountName, roleName);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    AccountID = account.AccountID,
                    AccountName = accountName,
                    AccountEmail = email,
                    Role = account.AccountRole ?? 2,
                    RoleName = roleName
                });
            }

            return Unauthorized(new { message = "You are not allowed to access this function!" });
        }
    }
}
