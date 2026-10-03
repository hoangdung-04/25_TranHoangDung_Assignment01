using System.IO;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Repositories
{
    public class FUNewsManagementDbContext : DbContext
    {
        public FUNewsManagementDbContext()
        {
        }

        public FUNewsManagementDbContext(DbContextOptions<FUNewsManagementDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<NewsArticle> NewsArticles { get; set; } = null!;
        public virtual DbSet<NewsTag> NewsTags { get; set; } = null!;
        public virtual DbSet<SystemAccount> SystemAccounts { get; set; } = null!;
        public virtual DbSet<Tag> Tags { get; set; } = null!;

        public static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var config = builder.Build();
            var connStr = config.GetConnectionString("FUNewsManagementDB");

            if (string.IsNullOrEmpty(connStr))
            {
                // Fallback check parent folder or default server
                connStr = config.GetConnectionString("DefaultConnection") 
                          ?? "Server=.;Database=FUNewsManagement;Trusted_Connection=True;TrustServerCertificate=True;";
            }

            return connStr;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryID);
                entity.Property(e => e.CategoryID).ValueGeneratedOnAdd();
                entity.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CategoryDesciption).HasColumnName("CategoryDesciption").HasMaxLength(250).IsRequired();

                entity.HasOne(d => d.ParentCategory)
                    .WithMany(p => p.InverseParentCategory)
                    .HasForeignKey(d => d.ParentCategoryID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Category_Category");
            });

            modelBuilder.Entity<NewsArticle>(entity =>
            {
                entity.HasKey(e => e.NewsArticleID);
                entity.Property(e => e.NewsArticleID).HasMaxLength(20).IsRequired();
                entity.Property(e => e.NewsTitle).HasMaxLength(400);
                entity.Property(e => e.Headline).HasMaxLength(150).IsRequired();
                entity.Property(e => e.NewsContent).HasMaxLength(4000);
                entity.Property(e => e.NewsSource).HasMaxLength(400);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.NewsArticles)
                    .HasForeignKey(d => d.CategoryID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NewsArticle_Category");

                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.NewsArticles)
                    .HasForeignKey(d => d.CreatedByID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NewsArticle_SystemAccount");
            });

            modelBuilder.Entity<NewsTag>(entity =>
            {
                entity.HasKey(e => new { e.NewsArticleID, e.TagID });

                entity.HasOne(d => d.NewsArticle)
                    .WithMany(p => p.NewsTags)
                    .HasForeignKey(d => d.NewsArticleID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NewsTag_NewsArticle");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.NewsTags)
                    .HasForeignKey(d => d.TagID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NewsTag_Tag");
            });

            modelBuilder.Entity<SystemAccount>(entity =>
            {
                entity.HasKey(e => e.AccountID);
                entity.Property(e => e.AccountID).ValueGeneratedNever();
                entity.Property(e => e.AccountName).HasMaxLength(100);
                entity.Property(e => e.AccountEmail).HasMaxLength(70);
                entity.Property(e => e.AccountPassword).HasMaxLength(70);
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.TagID);
                entity.Property(e => e.TagID).ValueGeneratedNever();
                entity.Property(e => e.TagName).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(400);
            });
        }
    }
}
