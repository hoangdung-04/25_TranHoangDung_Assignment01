using System;
using System.Collections.Generic;
using BusinessObjects;
using Repositories.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class NewsArticleRepository : INewsArticleRepository
    {
        public List<NewsArticle> GetNewsArticles(bool? status = null, string? search = null, short? categoryId = null, short? createdById = null) =>
            NewsArticleDAO.Instance.GetNewsArticles(status, search, categoryId, createdById);

        public NewsArticle? GetNewsArticleById(string id) =>
            NewsArticleDAO.Instance.GetNewsArticleById(id);

        public List<NewsArticle> GetNewsArticlesByPeriod(DateTime? startDate, DateTime? endDate) =>
            NewsArticleDAO.Instance.GetNewsArticlesByPeriod(startDate, endDate);

        public NewsArticle AddNewsArticle(NewsArticle article, List<int>? tagIds) =>
            NewsArticleDAO.Instance.AddNewsArticle(article, tagIds);

        public NewsArticle UpdateNewsArticle(NewsArticle article, List<int>? tagIds) =>
            NewsArticleDAO.Instance.UpdateNewsArticle(article, tagIds);

        public bool DeleteNewsArticle(string id) =>
            NewsArticleDAO.Instance.DeleteNewsArticle(id);
    }
}
