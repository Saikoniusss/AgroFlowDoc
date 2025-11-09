using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Data
{
    public class DocflowDbContextFactory : IDesignTimeDbContextFactory<DocflowDbContext>
    {
        public DocflowDbContext CreateDbContext(string[] args)
        {
            // Путь к appsettings.json из проекта Api
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Api");
            // Загружаем конфигурацию из appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("AgroFlowConnection");

            var optionsBuilder = new DbContextOptionsBuilder<DocflowDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new DocflowDbContext(optionsBuilder.Options);
        }
    }
}