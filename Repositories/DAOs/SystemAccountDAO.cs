using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace Repositories.DAOs
{
    public class SystemAccountDAO
    {
        private static SystemAccountDAO? _instance;
        private static readonly object _instanceLock = new object();

        private SystemAccountDAO() { }

        public static SystemAccountDAO Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new SystemAccountDAO();
                    }
                    return _instance;
                }
            }
        }

        public List<SystemAccount> GetAccounts(string? search = null, int? role = null)
        {
            using var context = new FUNewsManagementDbContext();
            var query = context.SystemAccounts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(a => (a.AccountName != null && a.AccountName.ToLower().Contains(s)) ||
                                         (a.AccountEmail != null && a.AccountEmail.ToLower().Contains(s)));
            }

            if (role.HasValue)
            {
                query = query.Where(a => a.AccountRole == role.Value);
            }

            return query.OrderBy(a => a.AccountID).ToList();
        }

        public SystemAccount? GetAccountById(short id)
        {
            using var context = new FUNewsManagementDbContext();
            return context.SystemAccounts
                .Include(a => a.NewsArticles)
                .FirstOrDefault(a => a.AccountID == id);
        }

        public SystemAccount? GetAccountByEmail(string email)
        {
            using var context = new FUNewsManagementDbContext();
            return context.SystemAccounts
                .FirstOrDefault(a => a.AccountEmail != null && a.AccountEmail.ToLower() == email.Trim().ToLower());
        }

        public bool HasCreatedNewsArticles(short accountId)
        {
            using var context = new FUNewsManagementDbContext();
            return context.NewsArticles.Any(n => n.CreatedByID == accountId);
        }

        public short GetNextAccountId()
        {
            using var context = new FUNewsManagementDbContext();
            if (!context.SystemAccounts.Any())
            {
                return 1;
            }
            return (short)(context.SystemAccounts.Max(a => a.AccountID) + 1);
        }

        public SystemAccount AddAccount(SystemAccount account)
        {
            using var context = new FUNewsManagementDbContext();
            if (account.AccountID <= 0)
            {
                account.AccountID = GetNextAccountId();
            }

            if (context.SystemAccounts.Any(a => a.AccountEmail != null && a.AccountEmail.ToLower() == account.AccountEmail!.ToLower()))
            {
                throw new InvalidOperationException($"An account with email '{account.AccountEmail}' already exists.");
            }

            context.SystemAccounts.Add(account);
            context.SaveChanges();
            return account;
        }

        public SystemAccount UpdateAccount(SystemAccount account)
        {
            using var context = new FUNewsManagementDbContext();
            var existing = context.SystemAccounts.Find(account.AccountID);
            if (existing == null)
            {
                throw new InvalidOperationException($"Account with ID {account.AccountID} not found.");
            }

            // Check email uniqueness if email changed
            if (!string.IsNullOrWhiteSpace(account.AccountEmail) && 
                account.AccountEmail.ToLower() != existing.AccountEmail?.ToLower())
            {
                if (context.SystemAccounts.Any(a => a.AccountID != account.AccountID && 
                                                    a.AccountEmail != null && 
                                                    a.AccountEmail.ToLower() == account.AccountEmail.ToLower()))
                {
                    throw new InvalidOperationException($"An account with email '{account.AccountEmail}' already exists.");
                }
                existing.AccountEmail = account.AccountEmail;
            }

            existing.AccountName = account.AccountName;
            if (account.AccountRole.HasValue)
            {
                existing.AccountRole = account.AccountRole;
            }

            if (!string.IsNullOrWhiteSpace(account.AccountPassword))
            {
                existing.AccountPassword = account.AccountPassword;
            }

            context.SaveChanges();
            return existing;
        }

        public bool DeleteAccount(short id)
        {
            using var context = new FUNewsManagementDbContext();
            if (HasCreatedNewsArticles(id))
            {
                throw new InvalidOperationException("Cannot delete this account because it has already created news articles.");
            }

            var account = context.SystemAccounts.Find(id);
            if (account == null)
            {
                return false;
            }

            context.SystemAccounts.Remove(account);
            context.SaveChanges();
            return true;
        }
    }
}
