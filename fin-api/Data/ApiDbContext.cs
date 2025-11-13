using fin_api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Data
{
    public class ApiDbContext : IdentityDbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base (options)
        {
        }

        public DbSet<Transacao> Transactions { get; set; }
        public DbSet<Categoria> Categories { get; set; }

    }
}
