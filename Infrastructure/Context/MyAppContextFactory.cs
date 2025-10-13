using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context
{
    public class MyAppContextFactory(IConfiguration configuration) : IDesignTimeDbContextFactory<MyAppContext>
    {
        public MyAppContext CreateDbContext(string[] args)
        {
            var cs = configuration.GetConnectionString("DefaultConnection");
             var optionsBuilder = new DbContextOptionsBuilder<MyAppContext>();
            optionsBuilder.UseSqlServer(cs);
            return new MyAppContext(optionsBuilder.Options);
        }
    }
}
