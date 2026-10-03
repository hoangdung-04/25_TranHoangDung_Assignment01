using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    public class NewsArticleCreateDto
    {
        [Required(ErrorMessage = "NewsArticleID is required")]
        [StringLength(20, ErrorMessage = "NewsArticleID cannot exceed 20 characters")]
        public string NewsArticleID { get; set; } = null!;

        [StringLength(400, ErrorMessage = "News Title cannot exceed 400 characters")]
        public string? NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline is required")]
        [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
        public string Headline { get; set; } = null!;

        [StringLength(4000, ErrorMessage = "News Content cannot exceed 4000 characters")]
        public string? NewsContent { get; set; }

        [StringLength(400, ErrorMessage = "News Source cannot exceed 400 characters")]
        public string? NewsSource { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public short? CategoryID { get; set; }

        public bool? NewsStatus { get; set; } = true;

        public short? CreatedByID { get; set; }

        public List<int> TagIDs { get; set; } = new List<int>();
    }

    public class NewsArticleUpdateDto
    {
        [Required(ErrorMessage = "NewsArticleID is required")]
        public string NewsArticleID { get; set; } = null!;

        [StringLength(400, ErrorMessage = "News Title cannot exceed 400 characters")]
        public string? NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline is required")]
        [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
        public string Headline { get; set; } = null!;

        [StringLength(4000, ErrorMessage = "News Content cannot exceed 4000 characters")]
        public string? NewsContent { get; set; }

        [StringLength(400, ErrorMessage = "News Source cannot exceed 400 characters")]
        public string? NewsSource { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public short? CategoryID { get; set; }

        public bool? NewsStatus { get; set; }

        public short? UpdatedByID { get; set; }

        public List<int> TagIDs { get; set; } = new List<int>();
    }

    public class NewsArticleDto
    {
        public string NewsArticleID { get; set; } = null!;
        public string? NewsTitle { get; set; }
        public string Headline { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public string? NewsContent { get; set; }
        public string? NewsSource { get; set; }
        public short? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public bool? NewsStatus { get; set; }
        public short? CreatedByID { get; set; }
        public string? CreatedByName { get; set; }
        public short? UpdatedByID { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<int> TagIDs { get; set; } = new List<int>();
        public List<string> TagNames { get; set; } = new List<string>();
    }
}
