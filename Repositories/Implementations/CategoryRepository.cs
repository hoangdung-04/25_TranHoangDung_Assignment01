using System.Collections.Generic;
using BusinessObjects;
using Repositories.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        public List<Category> GetCategories(bool? activeOnly = null) => CategoryDAO.Instance.GetCategories(activeOnly);

        public Category? GetCategoryById(short id) => CategoryDAO.Instance.GetCategoryById(id);

        public Category AddCategory(Category category) => CategoryDAO.Instance.AddCategory(category);

        public Category UpdateCategory(Category category) => CategoryDAO.Instance.UpdateCategory(category);

        public bool DeleteCategory(short id) => CategoryDAO.Instance.DeleteCategory(id);

        public bool HasNewsArticles(short categoryId) => CategoryDAO.Instance.HasNewsArticles(categoryId);

        public bool HasSubCategories(short categoryId) => CategoryDAO.Instance.HasSubCategories(categoryId);
    }
}
