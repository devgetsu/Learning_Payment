using Click_Integration.Entities;
using Microsoft.EntityFrameworkCore;

namespace Click_Integration
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<ClickTransaction> ClickTransactions { get; set; }
    }
}
