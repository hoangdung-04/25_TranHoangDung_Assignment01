using System;
using System.Collections.Generic;

namespace BusinessObjects.DTOs
{
    public class ReportQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ReportStatisticsDto
    {
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
        public Dictionary<string, int> ArticlesByCategory { get; set; } = new Dictionary<string, int>();
        public List<NewsArticleDto> Articles { get; set; } = new List<NewsArticleDto>();
    }
}
