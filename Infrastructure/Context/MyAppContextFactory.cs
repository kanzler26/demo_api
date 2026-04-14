using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace Infrastructure.Context;

// 🔥 Этот класс нужен ТОЛЬКО для dotnet ef migrations/add/update
public class MyAppContextDesignTimeFactory : IDesignTimeDbContextFactory<MyAppContext>
{
    public MyAppContext CreateDbContext(string[] args)
    {
        // 1. Собираем конфигурацию из appsettings*.json
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "..", "..", ".."))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // 2. Получаем строку подключения
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        // 3. Настраиваем DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<MyAppContext>();
        optionsBuilder.UseNpgsql(connectionString,
            sql => sql.MigrationsAssembly(typeof(MyAppContext).Assembly.FullName));

        return new MyAppContext(optionsBuilder.Options);
    }
}