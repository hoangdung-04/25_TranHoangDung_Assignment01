using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.DTOs;
using Microsoft.Extensions.Configuration;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Services
{
    public class NewsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public NewsApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5000/api/";
            if (!_baseUrl.EndsWith("/")) _baseUrl += "/";
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        private static async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    return msgProp.GetString() ?? response.ReasonPhrase ?? "An error occurred.";
                }
                return content;
            }
            catch
            {
                return response.ReasonPhrase ?? "An error occurred.";
            }
        }

        // ================= AUTH =================
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/Login", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result ?? new LoginResponse { Success = false, Message = "Failed to parse login response." };
            }

            var error = await GetErrorMessageAsync(response);
            return new LoginResponse { Success = false, Message = error };
        }

        // ================= NEWS ARTICLES =================
        public async Task<List<NewsArticleDto>> GetActiveNewsAsync(string? search = null, short? categoryId = null)
        {
            var url = "NewsArticles/Active?";
            if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
            if (categoryId.HasValue && categoryId.Value > 0) url += $"categoryId={categoryId.Value}&";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<NewsArticleDto>>() ?? new List<NewsArticleDto>();
            }
            return new List<NewsArticleDto>();
        }

        public async Task<List<NewsArticleDto>> GetAllNewsAsync(bool? status = null, string? search = null, short? categoryId = null, short? createdById = null)
        {
            var url = "NewsArticles?";
            if (status.HasValue) url += $"status={status.Value}&";
            if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
            if (categoryId.HasValue && categoryId.Value > 0) url += $"categoryId={categoryId.Value}&";
            if (createdById.HasValue) url += $"createdById={createdById.Value}&";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<NewsArticleDto>>() ?? new List<NewsArticleDto>();
            }
            return new List<NewsArticleDto>();
        }

        public async Task<NewsArticleDto?> GetNewsByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"NewsArticles/{Uri.EscapeDataString(id)}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NewsArticleDto>();
            }
            return null;
        }

        public async Task<List<NewsArticleDto>> GetNewsHistoryAsync(short creatorId)
        {
            var response = await _httpClient.GetAsync($"NewsArticles/History/{creatorId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<NewsArticleDto>>() ?? new List<NewsArticleDto>();
            }
            return new List<NewsArticleDto>();
        }

        public async Task<(bool Success, string Message, NewsArticleDto? Data)> CreateNewsAsync(NewsArticleCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("NewsArticles", dto);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<NewsArticleDto>();
                return (true, "News Article created successfully!", created);
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err, null);
        }

        public async Task<(bool Success, string Message, NewsArticleDto? Data)> UpdateNewsAsync(string id, NewsArticleUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"NewsArticles/{Uri.EscapeDataString(id)}", dto);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<NewsArticleDto>();
                return (true, "News Article updated successfully!", updated);
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err, null);
        }

        public async Task<(bool Success, string Message)> DeleteNewsAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"NewsArticles/{Uri.EscapeDataString(id)}");
            if (response.IsSuccessStatusCode)
            {
                return (true, "News Article deleted successfully!");
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err);
        }

        // ================= CATEGORIES =================
        public async Task<List<Category>> GetCategoriesAsync(bool? activeOnly = null)
        {
            var url = "Categories";
            if (activeOnly.HasValue) url += $"?activeOnly={activeOnly.Value}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Category>>() ?? new List<Category>();
            }
            return new List<Category>();
        }

        public async Task<Category?> GetCategoryByIdAsync(short id)
        {
            var response = await _httpClient.GetAsync($"Categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Category>();
            }
            return null;
        }

        public async Task<(bool CanDelete, string Message)> CheckDeleteCategoryAsync(short id)
        {
            var response = await _httpClient.GetAsync($"Categories/CheckDelete/{id}");
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<CheckDeleteResponse>();
                return (res?.CanDelete ?? false, res?.Message ?? "");
            }
            return (false, "Could not check deletion condition.");
        }

        public async Task<(bool Success, string Message, Category? Data)> CreateCategoryAsync(Category category)
        {
            var response = await _httpClient.PostAsJsonAsync("Categories", category);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<Category>();
                return (true, "Category created successfully!", created);
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err, null);
        }

        public async Task<(bool Success, string Message, Category? Data)> UpdateCategoryAsync(short id, Category category)
        {
            var response = await _httpClient.PutAsJsonAsync($"Categories/{id}", category);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<Category>();
                return (true, "Category updated successfully!", updated);
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err, null);
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(short id)
        {
            var response = await _httpClient.DeleteAsync($"Categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                return (true, "Category deleted successfully!");
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err);
        }

        // ================= ACCOUNTS =================
        public async Task<List<SystemAccount>> GetAccountsAsync(string? search = null, int? role = null)
        {
            var url = "Accounts?";
            if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
            if (role.HasValue) url += $"role={role.Value}&";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SystemAccount>>() ?? new List<SystemAccount>();
            }
            return new List<SystemAccount>();
        }

        public async Task<SystemAccount?> GetAccountByIdAsync(short id)
        {
            var response = await _httpClient.GetAsync($"Accounts/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SystemAccount>();
            }
            return null;
        }

        public async Task<(bool CanDelete, string Message)> CheckDeleteAccountAsync(short id)
        {
            var response = await _httpClient.GetAsync($"Accounts/CheckDelete/{id}");
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<CheckDeleteResponse>();
                return (res?.CanDelete ?? false, res?.Message ?? "");
            }
            return (false, "Could not check deletion condition.");
        }

        public async Task<(bool Success, string Message, SystemAccount? Data)> CreateAccountAsync(AccountCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("Accounts", dto);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<SystemAccount>();
                return (true, "Account created successfully!", created);
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err, null);
        }

        public async Task<(bool Success, string Message, SystemAccount? Data)> UpdateAccountAsync(short id, AccountUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"Accounts/{id}", dto);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<SystemAccount>();
                return (true, "Account updated successfully!", updated);
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err, null);
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(short id, UserProfileDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"Accounts/Profile/{id}", dto);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Profile updated successfully!");
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err);
        }

        public async Task<(bool Success, string Message)> DeleteAccountAsync(short id)
        {
            var response = await _httpClient.DeleteAsync($"Accounts/{id}");
            if (response.IsSuccessStatusCode)
            {
                return (true, "Account deleted successfully!");
            }
            var err = await GetErrorMessageAsync(response);
            return (false, err);
        }

        // ================= TAGS =================
        public async Task<List<Tag>> GetTagsAsync()
        {
            var response = await _httpClient.GetAsync("Tags");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Tag>>() ?? new List<Tag>();
            }
            return new List<Tag>();
        }

        // ================= REPORTS =================
        public async Task<ReportStatisticsDto?> GetReportStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var url = "Reports/Statistics?";
            if (startDate.HasValue) url += $"startDate={startDate.Value:yyyy-MM-dd}&";
            if (endDate.HasValue) url += $"endDate={endDate.Value:yyyy-MM-dd}&";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ReportStatisticsDto>();
            }
            return null;
        }

        private class CheckDeleteResponse
        {
            public bool CanDelete { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}
