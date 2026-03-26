using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ScrummerQL.Data
{
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ScrummerQLDbContext>
    {
        public ScrummerQLDbContext CreateDbContext(string[] args)
        {
            var (_, _, connectionString) = EnvConfig.Config();

            var optionsBuilder = new DbContextOptionsBuilder<ScrummerQLDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ScrummerQLDbContext(optionsBuilder.Options);
        }
    }
}
