using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GHCAA.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

public class SqlRunner
{
    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("GHCAA.API/appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("PgSqlConnection");
        Console.WriteLine($"Executing SQL on: {connectionString}");

        var sql = File.ReadAllText("restore_admin.sql");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        using var db = new ApplicationDbContext(optionsBuilder.Options);
        
        try 
        {
            await db.Database.ExecuteSqlRawAsync(sql);
            Console.WriteLine("SQL executed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
        }
    }
}
