using fin_api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base (options)
        {
        }

        public DbSet<Transacao> Transactions { get; set; }
        public DbSet<Categoria> Categories { get; set; }
        public DbSet<UserHiddenCategory> UserHiddenCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserHiddenCategory>()
                .HasKey(uhc => new { uhc.UserId, uhc.CategoryId });

            builder.Entity<UserHiddenCategory>()
                .HasOne(uhc => uhc.Category)
                .WithMany()
                .HasForeignKey(uhc => uhc.CategoryId);
        }

    }
}
