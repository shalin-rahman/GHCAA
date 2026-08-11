using System;
using System.Linq;
using System.Threading.Tasks;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GHCAA.Domain.Models;

namespace GHCAA.Tools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("GHCAA.API/appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("PgSqlConnection");
            var optionsBuilder = new DbContextOptionsBuilder<PgSqlApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using (var db = new PgSqlApplicationDbContext(optionsBuilder.Options))
            {
                var users = await db.Users.Select(u => new { u.Id, u.Username }).ToListAsync();
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.Id}, Username: {user.Username}");
                }
            }
        }
    }
}
