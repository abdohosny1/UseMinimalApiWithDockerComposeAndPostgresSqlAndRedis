using Microsoft.EntityFrameworkCore;
using UseDockerComposeWithPostgresSqlAndRedis.Database;

namespace UseDockerComposeWithPostgresSqlAndRedis.Extension
{
    public static class MigrationExtension
    {
        public static void ApplyMigration(this IApplicationBuilder app)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();

            using ApplicationDbContext dbContext = 
                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.Migrate();

        }
    }
}
