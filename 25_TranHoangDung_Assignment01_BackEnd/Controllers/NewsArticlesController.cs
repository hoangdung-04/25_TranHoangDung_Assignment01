using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories.Interfaces;

namespace _25_TranHoangDung_Assignment01_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsArticlesController : ODataController
    {
        private readonly INewsArticleRepository _newsArticleRepository;

        public NewsArticlesController(INewsArticleRepository newsArticleRepository)
        {
            _newsArticleRepository = newsArticleRepository;
        }

        private static NewsArticleDto MapToDto(NewsArticle n)
        {
            return new NewsArticleDto
            {
                NewsArticleID = n.NewsArticleID,
                NewsTitle = n.NewsTitle,
                Headline = n.Headline,
                CreatedDate = n.CreatedDate,
                NewsContent = n.NewsContent,
                NewsSource = n.NewsSource,
                CategoryID = n.CategoryID,
                CategoryName = n.Category?.CategoryName,
                NewsStatus = n.NewsStatus,
                CreatedByID = n.CreatedByID,
                CreatedByName = n.CreatedBy?.AccountName,
                UpdatedByID = n.UpdatedByID,
                ModifiedDate = n.ModifiedDate,
                TagIDs = n.NewsTags?.Select(nt => nt.TagID).ToList() ?? new List<int>(),
                TagNames = n.NewsTags?.Select(nt => nt.Tag?.TagName ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>()
            };
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        [EnableQuery]
        public IActionResult Get([FromQuery] bool? status, [FromQuery] string? search, [FromQuery] short? categoryId, [FromQuery] short? createdById)
        {
            var articles = _newsArticleRepository.GetNewsArticles(status, search, categoryId, createdById);
            var dtos = articles.Select(MapToDto).ToList();
            return Ok(dtos.AsQueryable());
        }

        [HttpGet("Active")]
        [AllowAnonymous]
        public IActionResult GetActive([FromQuery] string? search, [FromQuery] short? categoryId)
        {
            // Public endpoint: active news only
            var articles = _newsArticleRepository.GetNewsArticles(status: true, search: search, categoryId: categoryId);
            var dtos = articles.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(string id)
        {
            var article = _newsArticleRepository.GetNewsArticleById(id);
            if (article == null)
            {
                return NotFound(new { message = $"News Article with ID {id} not found." });
            }
            return Ok(MapToDto(article));
        }

        [HttpGet("History/{creatorId}")]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult GetHistory(short creatorId)
        {
            var articles = _newsArticleRepository.GetNewsArticles(createdById: creatorId);
            var dtos = articles.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Create([FromBody] NewsArticleCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var article = new NewsArticle
                {
                    NewsArticleID = dto.NewsArticleID,
                    NewsTitle = dto.NewsTitle,
                    Headline = dto.Headline,
                    CreatedDate = DateTime.Now,
                    NewsContent = dto.NewsContent,
                    NewsSource = dto.NewsSource,
                    CategoryID = dto.CategoryID,
                    NewsStatus = dto.NewsStatus ?? true,
                    CreatedByID = dto.CreatedByID,
                    UpdatedByID = dto.CreatedByID,
                    ModifiedDate = DateTime.Now
                };

                var created = _newsArticleRepository.AddNewsArticle(article, dto.TagIDs);
                return CreatedAtAction(nameof(GetById), new { id = created.NewsArticleID }, MapToDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Update(string id, [FromBody] NewsArticleUpdateDto dto)
        {
            if (id != dto.NewsArticleID)
            {
                return BadRequest(new { message = "Article ID in URL does not match body." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var article = new NewsArticle
                {
                    NewsArticleID = dto.NewsArticleID,
                    NewsTitle = dto.NewsTitle,
                    Headline = dto.Headline,
                    NewsContent = dto.NewsContent,
                    NewsSource = dto.NewsSource,
                    CategoryID = dto.CategoryID,
                    NewsStatus = dto.NewsStatus,
                    UpdatedByID = dto.UpdatedByID,
                    ModifiedDate = DateTime.Now
                };

                var updated = _newsArticleRepository.UpdateNewsArticle(article, dto.TagIDs);
                return Ok(MapToDto(updated));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Delete(string id)
        {
            try
            {
                var deleted = _newsArticleRepository.DeleteNewsArticle(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"News Article with ID {id} not found." });
                }

                return Ok(new { message = "News Article deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
