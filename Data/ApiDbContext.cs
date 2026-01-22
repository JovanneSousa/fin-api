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
        public DbSet<IconeCategoriaUsuario> CategoriaUsuarios { get; set; }
        public DbSet<Cor> Cor { get; set; }
        public DbSet<CorCategoriaUsuario> CorCategoriaUsuarios { get; set; }

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

                entity.HasOne(c => c.Icone)
                    .WithMany(i => i.Categorias)
                    .HasForeignKey(c => c.IconId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Cor)
                    .WithMany(c => c.Categorias)
                    .HasForeignKey(c => c.CorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Cor>()
                .HasKey(c => c.Id);

            builder.Entity<Icon>()
                .HasKey(i => i.Id);

            builder.Entity<Usuario>()
                .HasKey(u => u.Id);

            builder.Entity<CorCategoriaUsuario>(entity =>
            {
                entity.HasKey(cc => new { cc.UserId, cc.CategoriaId });

                entity.HasOne(c => c.Categoria)
                    .WithMany(c => c.CorCategoriaUsuarios)
                    .HasForeignKey(c => c.CategoriaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Cor)
                    .WithMany()
                    .HasForeignKey(c => c.CorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(u => u.Usuario)
                   .WithMany()
                   .HasForeignKey(u => u.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<IconeCategoriaUsuario>(entity =>
            {
                entity.HasKey(cu => new { cu.UserId, cu.CategoriaId });

                entity.HasOne(c => c.Categoria)
                    .WithMany(c => c.IconeCategoriaUsuario)
                    .HasForeignKey(c => c.CategoriaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Icone)
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
