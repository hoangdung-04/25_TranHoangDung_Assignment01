using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace Repositories.DAOs
{
    public class CategoryDAO
    {
        private static CategoryDAO? _instance;
        private static readonly object _instanceLock = new object();

        private CategoryDAO() { }

        public static CategoryDAO Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new CategoryDAO();
                    }
                    return _instance;
                }
            }
        }

        public List<Category> GetCategories(bool? activeOnly = null)
        {
            using var context = new FUNewsManagementDbContext();
            var query = context.Categories.Include(c => c.ParentCategory).AsQueryable();
            if (activeOnly.HasValue)
            {
                query = query.Where(c => c.IsActive == activeOnly.Value);
            }
            return query.OrderBy(c => c.CategoryName).ToList();
        }

        public Category? GetCategoryById(short id)
        {
            using var context = new FUNewsManagementDbContext();
            return context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.InverseParentCategory)
                .Include(c => c.NewsArticles)
                .FirstOrDefault(c => c.CategoryID == id);
        }

        public bool HasNewsArticles(short categoryId)
        {
            using var context = new FUNewsManagementDbContext();
            return context.NewsArticles.Any(n => n.CategoryID == categoryId);
        }

        public bool HasSubCategories(short categoryId)
        {
            using var context = new FUNewsManagementDbContext();
            return context.Categories.Any(c => c.ParentCategoryID == categoryId && c.CategoryID != categoryId);
        }

        public Category AddCategory(Category category)
        {
            using var context = new FUNewsManagementDbContext();
            context.Categories.Add(category);
            context.SaveChanges();
            return category;
        }

        public Category UpdateCategory(Category category)
        {
            using var context = new FUNewsManagementDbContext();
            var existing = context.Categories.Find(category.CategoryID);
            if (existing == null)
            {
                throw new InvalidOperationException($"Category with ID {category.CategoryID} not found.");
            }

            existing.CategoryName = category.CategoryName;
            existing.CategoryDesciption = category.CategoryDesciption;
            existing.ParentCategoryID = category.ParentCategoryID;
            existing.IsActive = category.IsActive;

            context.SaveChanges();
            return existing;
        }

        public bool DeleteCategory(short id)
        {
            using var context = new FUNewsManagementDbContext();
            if (HasNewsArticles(id))
            {
                throw new InvalidOperationException("Cannot delete category because it has associated news articles.");
            }

            var category = context.Categories.Find(id);
            if (category == null)
            {
                return false;
            }

            context.Categories.Remove(category);
            context.SaveChanges();
            return true;
        }
    }
}
