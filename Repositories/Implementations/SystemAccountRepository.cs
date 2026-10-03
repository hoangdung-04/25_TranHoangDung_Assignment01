using System.Collections.Generic;
using BusinessObjects;
using Repositories.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class SystemAccountRepository : ISystemAccountRepository
    {
        public List<SystemAccount> GetAccounts(string? search = null, int? role = null) => 
            SystemAccountDAO.Instance.GetAccounts(search, role);

        public SystemAccount? GetAccountById(short id) => 
            SystemAccountDAO.Instance.GetAccountById(id);

        public SystemAccount? GetAccountByEmail(string email) => 
            SystemAccountDAO.Instance.GetAccountByEmail(email);

        public SystemAccount AddAccount(SystemAccount account) => 
            SystemAccountDAO.Instance.AddAccount(account);

        public SystemAccount UpdateAccount(SystemAccount account) => 
            SystemAccountDAO.Instance.UpdateAccount(account);

        public bool DeleteAccount(short id) => 
            SystemAccountDAO.Instance.DeleteAccount(id);

        public bool HasCreatedNewsArticles(short accountId) => 
            SystemAccountDAO.Instance.HasCreatedNewsArticles(accountId);

        public short GetNextAccountId() => 
            SystemAccountDAO.Instance.GetNextAccountId();
    }
}
