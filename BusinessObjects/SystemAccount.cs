using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObjects
{
    [Table("SystemAccount")]
    public class SystemAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AccountID { get; set; }

        [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters")]
        public string? AccountName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email address format")]
        [StringLength(70, ErrorMessage = "Account Email cannot exceed 70 characters")]
        public string? AccountEmail { get; set; }

        public int? AccountRole { get; set; }

        [StringLength(70, ErrorMessage = "Password cannot exceed 70 characters")]
        public string? AccountPassword { get; set; }

        [JsonIgnore]
        public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
    }
}
