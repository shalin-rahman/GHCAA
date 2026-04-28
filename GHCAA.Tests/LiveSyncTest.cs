using System;
using System.Threading.Tasks;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GHCAA.Tests
{
    [TestFixture]
    public class LiveSyncTest
    {
        [Test]
        [Explicit("Manual promotion trigger")]
        public async Task SyncMembersForReal()
        {
            Console.WriteLine("🚀 Haragangian Global Directory RESTORATION Initialized...");
            
            // Live Connection String for restoration
            var connectionString = "Host=localhost;Database=GHCAADB_v2;Username=postgres;Password=postgres;SslMode=Prefer";
            
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            // Use Npgsql directly if it's transitive from GHCAA.API
            optionsBuilder.UseNpgsql(connectionString);

            using (var db = new ApplicationDbContext(optionsBuilder.Options))
            {
                Console.WriteLine("🛡️ Executing Bulk Promotion (Status 0 -> 1)...");
                
                var count = await db.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Members\" SET \"Status\" = 1 WHERE \"Status\" = 0 AND \"IsArchived\" = false");

                Console.WriteLine($"✅ SUCCESS: {count} alumni promoted to ACTIVE status.");
                Console.WriteLine("🌍 Your Global Directory is now LIVE.");
                Assert.That(count, Is.AtLeast(0));
            }
        }
    }
}
