using System;
using System.Linq;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories.Interfaces;

namespace _25_TranHoangDung_Assignment01_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ODataController
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        [EnableQuery]
        public IActionResult Get([FromQuery] bool? activeOnly)
        {
            var categories = _categoryRepository.GetCategories(activeOnly);
            return Ok(categories.AsQueryable());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(short id)
        {
            var category = _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound(new { message = $"not found value" });
            }
            return Ok(category);
        }

        [HttpGet("CheckDelete/{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult CheckDelete(short id)
        {
            var category = _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound(new { message = $"not found value." });
            }

            var hasArticles = _categoryRepository.HasNewsArticles(id);
            var hasSubCategories = _categoryRepository.HasSubCategories(id);

            bool canDelete = !hasArticles && !hasSubCategories;
            string message = "Category can be safely deleted.";

            if (hasArticles)
            {
                message = "Cannot delete category because it has associated news articles.";
            }
            else if (hasSubCategories)
            {
                message = "Cannot delete category because it has child categories.";
            }

            return Ok(new { canDelete, message });
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Create([FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = _categoryRepository.AddCategory(category);
                return CreatedAtAction(nameof(GetById), new { id = created.CategoryID }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Update(short id, [FromBody] Category category)
        {
            if (id != category.CategoryID)
            {
                return BadRequest(new { message = "Category ID in URL does not match body." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = _categoryRepository.UpdateCategory(category);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Delete(short id)
        {
            try
            {
                var deleted = _categoryRepository.DeleteCategory(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Category with ID {id} not found." });
                }

                return Ok(new { message = "Category deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
