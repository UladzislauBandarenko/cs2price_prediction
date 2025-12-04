using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace cs2price_prediction.Data
{
    public class AdminDbContextFactory : IAdminDbContextFactory
    {
        private readonly IConfiguration _config;

        public AdminDbContextFactory(IConfiguration config)
        {
            _config = config;
        }

        public AppDbContext CreateAdminContext()
        {
            var adminConnStr =
                _config.GetConnectionString("AdminConnection") ??
                _config["ConnectionStrings:AdminConnection"] ??
                _config["ConnectionStrings__AdminConnection"];

            if (string.IsNullOrWhiteSpace(adminConnStr))
                throw new InvalidOperationException("Connection string 'AdminConnection' is not configured.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(adminConnStr);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
