using System.Collections.Generic;
using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        List<Category> GetCategories(bool? activeOnly = null);
        Category? GetCategoryById(short id);
        Category AddCategory(Category category);
        Category UpdateCategory(Category category);
        bool DeleteCategory(short id);
        bool HasNewsArticles(short categoryId);
        bool HasSubCategories(short categoryId);
    }
}
