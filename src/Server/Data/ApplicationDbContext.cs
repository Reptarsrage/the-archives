using Microsoft.EntityFrameworkCore;

using TheArchives.Server.Models.Dto;

namespace TheArchives.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Content> Content { get; set; } = default!;

        public DbSet<Tag> Tags { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Content>()
                .HasMany(s => s.Tags)
                .WithMany(c => c.Content)
                .UsingEntity<ContentTag>(
                    j => j
                        .HasOne(pt => pt.Tag)
                        .WithMany(t => t.ContentTags)
                        .HasForeignKey(pt => pt.TagId),
                    j => j
                        .HasOne(pt => pt.Content)
                        .WithMany(p => p.ContentTags)
                        .HasForeignKey(pt => pt.ContentId));

            modelBuilder.Entity<Content>()
               .HasIndex(u => u.Url)
               .IsUnique();

            modelBuilder.Entity<Tag>()
               .HasIndex(u => u.Label)
               .IsUnique();
        }
    }
}