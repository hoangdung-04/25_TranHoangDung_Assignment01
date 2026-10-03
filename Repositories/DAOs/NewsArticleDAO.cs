using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace Repositories.DAOs
{
    public class NewsArticleDAO
    {
        private static NewsArticleDAO? _instance;
        private static readonly object _instanceLock = new object();

        private NewsArticleDAO() { }

        public static NewsArticleDAO Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new NewsArticleDAO();
                    }
                    return _instance;
                }
            }
        }

        public List<NewsArticle> GetNewsArticles(bool? status = null, string? search = null, short? categoryId = null, short? createdById = null)
        {
            using var context = new FUNewsManagementDbContext();
            var query = context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.NewsTags)
                    .ThenInclude(nt => nt.Tag)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(n => n.NewsStatus == status.Value);
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(n => n.CategoryID == categoryId.Value);
            }

            if (createdById.HasValue)
            {
                query = query.Where(n => n.CreatedByID == createdById.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(n => (n.NewsTitle != null && n.NewsTitle.ToLower().Contains(s)) ||
                                         (n.Headline != null && n.Headline.ToLower().Contains(s)) ||
                                         (n.NewsContent != null && n.NewsContent.ToLower().Contains(s)) ||
                                         (n.Category != null && n.Category.CategoryName.ToLower().Contains(s)));
            }

            return query.OrderByDescending(n => n.CreatedDate).ToList();
        }

        public NewsArticle? GetNewsArticleById(string id)
        {
            using var context = new FUNewsManagementDbContext();
            return context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.NewsTags)
                    .ThenInclude(nt => nt.Tag)
                .FirstOrDefault(n => n.NewsArticleID == id);
        }

        public List<NewsArticle> GetNewsArticlesByPeriod(DateTime? startDate, DateTime? endDate)
        {
            using var context = new FUNewsManagementDbContext();
            var query = context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.NewsTags)
                    .ThenInclude(nt => nt.Tag)
                .AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(n => n.CreatedDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(n => n.CreatedDate <= end);
            }

            return query.OrderByDescending(n => n.CreatedDate).ToList();
        }

        public NewsArticle AddNewsArticle(NewsArticle article, List<int>? tagIds)
        {
            using var context = new FUNewsManagementDbContext();
            if (string.IsNullOrWhiteSpace(article.NewsArticleID))
            {
                // Generate next ID
                article.NewsArticleID = Guid.NewGuid().ToString("N")[..8].ToUpper();
            }

            if (context.NewsArticles.Any(n => n.NewsArticleID == article.NewsArticleID))
            {
                throw new InvalidOperationException($"News Article with ID '{article.NewsArticleID}' already exists.");
            }

            article.CreatedDate ??= DateTime.Now;

            context.NewsArticles.Add(article);
            context.SaveChanges();

            if (tagIds != null && tagIds.Count > 0)
            {
                foreach (var tagId in tagIds.Distinct())
                {
                    context.NewsTags.Add(new NewsTag
                    {
                        NewsArticleID = article.NewsArticleID,
                        TagID = tagId
                    });
                }
                context.SaveChanges();
            }

            return GetNewsArticleById(article.NewsArticleID)!;
        }

        public NewsArticle UpdateNewsArticle(NewsArticle article, List<int>? tagIds)
        {
            using var context = new FUNewsManagementDbContext();
            var existing = context.NewsArticles
                .Include(n => n.NewsTags)
                .FirstOrDefault(n => n.NewsArticleID == article.NewsArticleID);

            if (existing == null)
            {
                throw new InvalidOperationException($"News Article with ID '{article.NewsArticleID}' not found.");
            }

            existing.NewsTitle = article.NewsTitle;
            existing.Headline = article.Headline;
            existing.NewsContent = article.NewsContent;
            existing.NewsSource = article.NewsSource;
            existing.CategoryID = article.CategoryID;
            existing.NewsStatus = article.NewsStatus;
            existing.UpdatedByID = article.UpdatedByID;
            existing.ModifiedDate = DateTime.Now;

            // Update Tags if provided
            if (tagIds != null)
            {
                // Remove tags no longer selected
                var tagsToRemove = existing.NewsTags.Where(nt => !tagIds.Contains(nt.TagID)).ToList();
                context.NewsTags.RemoveRange(tagsToRemove);

                // Add newly selected tags
                var currentTagIds = existing.NewsTags.Select(nt => nt.TagID).ToList();
                foreach (var tagId in tagIds.Distinct())
                {
                    if (!currentTagIds.Contains(tagId))
                    {
                        context.NewsTags.Add(new NewsTag
                        {
                            NewsArticleID = existing.NewsArticleID,
                            TagID = tagId
                        });
                    }
                }
            }

            context.SaveChanges();
            return GetNewsArticleById(article.NewsArticleID)!;
        }

        public bool DeleteNewsArticle(string id)
        {
            using var context = new FUNewsManagementDbContext();
            var article = context.NewsArticles
                .Include(n => n.NewsTags)
                .FirstOrDefault(n => n.NewsArticleID == id);

            if (article == null)
            {
                return false;
            }

            context.NewsTags.RemoveRange(article.NewsTags);
            context.NewsArticles.Remove(article);
            context.SaveChanges();
            return true;
        }
    }
}
