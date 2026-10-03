using System.Collections.Generic;
using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface ISystemAccountRepository
    {
        List<SystemAccount> GetAccounts(string? search = null, int? role = null);
        SystemAccount? GetAccountById(short id);
        SystemAccount? GetAccountByEmail(string email);
        SystemAccount AddAccount(SystemAccount account);
        SystemAccount UpdateAccount(SystemAccount account);
        bool DeleteAccount(short id);
        bool HasCreatedNewsArticles(short accountId);
        short GetNextAccountId();
    }
}
