using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace _25_TranHoangDung_Assignment01_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly INewsArticleRepository _newsArticleRepository;

        public ReportsController(INewsArticleRepository newsArticleRepository)
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
                CategoryName = n.Category?.CategoryName ?? "Unknown",
                NewsStatus = n.NewsStatus,
                CreatedByID = n.CreatedByID,
                CreatedByName = n.CreatedBy?.AccountName ?? "Unknown",
                UpdatedByID = n.UpdatedByID,
                ModifiedDate = n.ModifiedDate,
                TagIDs = n.NewsTags?.Select(nt => nt.TagID).ToList() ?? new List<int>(),
                TagNames = n.NewsTags?.Select(nt => nt.Tag?.TagName ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>()
            };
        }

        [HttpGet("Statistics")]
        public IActionResult GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return BadRequest(new { message = "Start Date cannot be after End Date." });
            }

            var articles = _newsArticleRepository.GetNewsArticlesByPeriod(startDate, endDate);
            var dtos = articles.Select(MapToDto).ToList();

            var categoryCounts = new Dictionary<string, int>();
            foreach (var a in dtos)
            {
                var catName = a.CategoryName ?? "Uncategorized";
                if (!categoryCounts.ContainsKey(catName))
                {
                    categoryCounts[catName] = 0;
                }
                categoryCounts[catName]++;
            }

            var result = new ReportStatisticsDto
            {
                TotalArticles = dtos.Count,
                ActiveArticles = dtos.Count(a => a.NewsStatus == true),
                InactiveArticles = dtos.Count(a => a.NewsStatus == false),
                ArticlesByCategory = categoryCounts,
                Articles = dtos
            };

            return Ok(result);
        }
    }
}
