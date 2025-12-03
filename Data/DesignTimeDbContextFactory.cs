using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace cs2price_prediction.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Строка подключения ДЛЯ dotnet ef (с хоста)
            // Подключаемся к контейнеру Postgres на порту 5435
            var connectionString =
                "Host=localhost;Port=5435;Database=cs2db;Username=cs2_user;Password=cs2_password";

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
