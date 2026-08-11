using System;
using System.Linq;
using System.Threading.Tasks;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GHCAA.Tools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Haragangian Global Directory Force-Sync Initialized...");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("GHCAA.API/appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using (var db = new ApplicationDbContext(optionsBuilder.Options))
            {
                Console.WriteLine("🛡️ Executing Bulk Promotion (Status 0 -> 1)...");
                
                var count = await db.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Members\" SET \"Status\" = 1 WHERE \"Status\" = 0 AND \"IsArchived\" = false");

                Console.WriteLine($"✅ SUCCESS: {count} alumni promoted to ACTIVE status.");
                Console.WriteLine("🌍 Your Global Directory is now LIVE.");
            }
        }
    }
}
