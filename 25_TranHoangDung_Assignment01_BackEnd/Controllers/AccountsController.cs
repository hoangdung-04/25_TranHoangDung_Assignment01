using System;
using System.Linq;
using BusinessObjects;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories.Interfaces;

namespace _25_TranHoangDung_Assignment01_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ODataController
    {
        private readonly ISystemAccountRepository _accountRepository;

        public AccountsController(ISystemAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get([FromQuery] string? search, [FromQuery] int? role)
        {
            var accounts = _accountRepository.GetAccounts(search, role);
            return Ok(accounts.AsQueryable());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(short id)
        {
            var account = _accountRepository.GetAccountById(id);
            if (account == null)
            {
                return NotFound(new { message = $"Account with ID {id} not found." });
            }
            return Ok(account);
        }

        [HttpGet("CheckDelete/{id}")]
        public IActionResult CheckDelete(short id)
        {
            var account = _accountRepository.GetAccountById(id);
            if (account == null)
            {
                return NotFound(new { message = $"Account with ID {id} not found." });
            }

            var hasArticles = _accountRepository.HasCreatedNewsArticles(id);
            return Ok(new
            {
                canDelete = !hasArticles,
                message = hasArticles
                    ? "Cannot delete this account because it has already created news articles."
                    : "Account can be safely deleted."
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] AccountCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var account = new SystemAccount
                {
                    AccountName = dto.AccountName,
                    AccountEmail = dto.AccountEmail,
                    AccountRole = dto.AccountRole,
                    AccountPassword = dto.AccountPassword
                };

                var created = _accountRepository.AddAccount(account);
                return CreatedAtAction(nameof(GetById), new { id = created.AccountID }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(short id, [FromBody] AccountUpdateDto dto)
        {
            if (id != dto.AccountID)
            {
                return BadRequest(new { message = "Account ID in URL does not match body." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var account = new SystemAccount
                {
                    AccountID = dto.AccountID,
                    AccountName = dto.AccountName,
                    AccountEmail = dto.AccountEmail,
                    AccountRole = dto.AccountRole,
                    AccountPassword = dto.AccountPassword
                };

                var updated = _accountRepository.UpdateAccount(account);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("Profile/{id}")]
        public IActionResult UpdateProfile(short id, [FromBody] UserProfileDto dto)
        {
            if (id != dto.AccountID)
            {
                return BadRequest(new { message = "Account ID in URL does not match body." });
            }

            var existing = _accountRepository.GetAccountById(id);
            if (existing == null)
            {
                return NotFound(new { message = "Account not found." });
            }

            try
            {
                existing.AccountName = dto.AccountName;
                if (!string.IsNullOrWhiteSpace(dto.AccountPassword))
                {
                    existing.AccountPassword = dto.AccountPassword;
                }

                var updated = _accountRepository.UpdateAccount(existing);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(short id)
        {
            try
            {
                var deleted = _accountRepository.DeleteAccount(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Account with ID {id} not found." });
                }

                return Ok(new { message = "Account deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
