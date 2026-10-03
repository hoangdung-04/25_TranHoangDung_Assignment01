using System;
using System.Collections.Generic;
using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface INewsArticleRepository
    {
        List<NewsArticle> GetNewsArticles(bool? status = null, string? search = null, short? categoryId = null, short? createdById = null);
        NewsArticle? GetNewsArticleById(string id);
        List<NewsArticle> GetNewsArticlesByPeriod(DateTime? startDate, DateTime? endDate);
        NewsArticle AddNewsArticle(NewsArticle article, List<int>? tagIds);
        NewsArticle UpdateNewsArticle(NewsArticle article, List<int>? tagIds);
        bool DeleteNewsArticle(string id);
    }
}
