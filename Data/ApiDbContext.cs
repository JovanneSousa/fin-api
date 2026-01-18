using fin_api.Models;
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
        public DbSet<Icon> Icon { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserHiddenCategory>(entity =>
            {
                entity.HasKey(uhc => new { uhc.UserId, uhc.CategoryId });
                entity.HasOne(uhc => uhc.Usuario)
                .WithMany()
                .HasForeignKey(uhc => uhc.UserId);

                entity.HasOne(uhc => uhc.Category)
                .WithMany()
                .HasForeignKey(uhc => uhc.CategoryId);
            });

            builder.Entity<Categoria>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.DefaultIcon)
                    .WithMany(i => i.Categorias)
                    .HasForeignKey(c => c.DefaultIconId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Icon>()
                .HasKey(i => i.Id);

            builder.Entity<Usuario>()
                .HasKey(u => u.Id);

            builder.Entity<CategoriaUsuario>(entity =>
            {
                entity.HasKey(cu => new { cu.UserId, cu.CategoriaId });
                entity.HasOne(c => c.Categoria)
                    .WithMany()
                    .HasForeignKey(c => c.CategoriaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Icon)
                    .WithMany()
                    .HasForeignKey(i => i.IconId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(u => u.Usuario)
                    .WithMany()
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
