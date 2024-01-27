using Microsoft.EntityFrameworkCore;
using UseDockerComposeWithPostgresSqlAndRedis.Entities;

namespace UseDockerComposeWithPostgresSqlAndRedis.Database
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
              : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
