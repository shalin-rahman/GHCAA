using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GHCAA.Infrastructure.Data;
using GHCAA.Domain;

// Purpose: Diagnostic script to check Member statuses
public static class DbDiagnostics 
{
    public static async Task Run(ApplicationDbContext db)
    {
        var total = await db.Members.CountAsync();
        var active = await db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Active);
        var applied = await db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Applied);
        var archived = await db.Members.CountAsync(m => m.IsArchived);

        Console.WriteLine($"Total Members: {total}");
        Console.WriteLine($"Active Members: {active}");
        Console.WriteLine($"Applied Members: {applied}");
        Console.WriteLine($"Archived Members: {archived}");
        
        if (active == 0 && applied > 0)
        {
            Console.WriteLine("CRITICAL: Members exist but none are 'Active'. Force-activating 10 members for testing...");
            var toActivate = await db.Members.Where(m => m.Status == Enums.MembershipStatus.Applied).Take(10).ToListAsync();
            foreach(var m in toActivate) m.Status = Enums.MembershipStatus.Active;
            await db.SaveChangesAsync();
            Console.WriteLine("Done.");
        }
    }
}
