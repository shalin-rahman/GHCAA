using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GHCAA.Domain.Enums;
using PersonaGroups = GHCAA.Domain.Constants.Elections.PersonaGroups;

namespace GHCAA.Infrastructure.Data;

// Spec 023 (37.12b). Inserts any default election persona missing by name. An existing row keeps
// whatever a SuperAdmin set, so an edit survives a redeploy. The one exception is a default persona
// with no permissions at all, which gets its default set (95.10). The ElectionAppointments migration
// may insert four of these first, which is why this cannot stop at a non-empty table.
public static class ElectionPersonaSeeder
{
    // FORM ER-04, docs/Elections/05-Election-Forms-and-Templates.md. Copied word for word.
    public const string NeutralityDeclarationText =
        "I hereby solemnly declare that I shall:\n\n" +
        "Remain impartial in every act and decision.\n" +
        "Not campaign for or against any candidate.\n" +
        "Not favour any candidate, panel or group.\n" +
        "Not disclose confidential information.\n" +
        "Perform my duties honestly and to the best of my ability.";

    public static async Task EnsureAsync(ApplicationDbContext db, ILogger logger)
    {
        const ElectionPermission all = ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit
            | ElectionPermission.ManageSetup | ElectionPermission.ManageVoterRoll | ElectionPermission.DecideNominations
            | ElectionPermission.AppointOfficials | ElectionPermission.SetBallotKey | ElectionPermission.ChangePhase
            | ElectionPermission.Count | ElectionPermission.Declare | ElectionPermission.DecideAppeals | ElectionPermission.Approve;
        const ElectionPermission chiefCommissioner = all & ~ElectionPermission.SetBallotKey & ~ElectionPermission.DecideAppeals;

        var now = DateTime.UtcNow;

        ElectionPersona Row(string name, string group, string description, ElectionPermission permissions,
            bool takesOver, bool showOnPublicBoard, int minCount, int? maxCount, int sortOrder) => new()
            {
                Name = name,
                GroupName = group,
                Description = description,
                Permissions = permissions,
                MinCount = minCount,
                MaxCount = maxCount,
                ShowOnPublicBoard = showOnPublicBoard,
                TakesOverFromAdmin = takesOver,
                DeclarationText = NeutralityDeclarationText,
                SortOrder = sortOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

        var defaults = new List<ElectionPersona>
        {
            Row("ECSC Member", PersonaGroups.SearchCommittee, "Screens and recommends candidates before nominations open.",
                ElectionPermission.ViewDashboard, false, true, 3, 5, 10),
            Row("Chief Commissioner", PersonaGroups.ElectionCommission, "Runs the election commission and can act in place of the admin.",
                chiefCommissioner, true, true, 1, 1, 20),
            Row("Commissioner", PersonaGroups.ElectionCommission, "Sits on the election commission alongside the Chief Commissioner.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit | ElectionPermission.ManageSetup
                    | ElectionPermission.ManageVoterRoll | ElectionPermission.DecideNominations | ElectionPermission.AppointOfficials
                    | ElectionPermission.ChangePhase | ElectionPermission.Declare | ElectionPermission.Approve,
                true, true, 2, 4, 30),
            Row("Returning Officer", PersonaGroups.Officials, "Holds the ballot key and runs the count.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit | ElectionPermission.DecideNominations
                    | ElectionPermission.SetBallotKey | ElectionPermission.Count | ElectionPermission.Approve,
                false, true, 1, 1, 40),
            Row("Assistant Returning Officer", PersonaGroups.Officials, "Helps the Returning Officer with nominations.",
                ElectionPermission.ViewDashboard | ElectionPermission.DecideNominations, false, true, 0, null, 50),
            Row("Presiding Officer", PersonaGroups.Officials, "Oversees a polling station on election day.",
                ElectionPermission.ViewDashboard, false, true, 0, null, 60),
            Row("Polling Officer", PersonaGroups.Officials, "Staffs a polling station on election day.",
                ElectionPermission.ViewDashboard, false, true, 0, null, 70),
            Row("Scrutineer", PersonaGroups.Officials, "Checks nominations for eligibility.",
                ElectionPermission.ViewDashboard | ElectionPermission.DecideNominations, false, true, 0, null, 80),
            Row("Counting Supervisor", PersonaGroups.Officials, "Oversees the vote count.",
                ElectionPermission.ViewDashboard | ElectionPermission.Count, false, true, 0, null, 90),
            Row("Technical Administrator", PersonaGroups.Officials, "Keeps the voting system running.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit, false, true, 0, null, 100),
            Row("Cybersecurity Auditor", PersonaGroups.Officials, "Reviews the election for security issues.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit, false, true, 0, null, 110),
            Row("Security Officer", PersonaGroups.Officials, "Handles physical security for the election.",
                ElectionPermission.ViewDashboard, false, false, 0, null, 120),
            Row("Observer", PersonaGroups.Observers, "Watches the election without taking part in it.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit, false, true, 0, null, 130),
            Row("Appeal Tribunal Member", PersonaGroups.AppealTribunal, "Decides appeals raised against election decisions.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit | ElectionPermission.DecideAppeals,
                false, true, 3, 3, 140)
        };

        var existing = await db.ElectionPersonas.ToListAsync();
        var existingNames = existing.Select(x => x.Name).ToHashSet();
        var missing = defaults.Where(x => !existingNames.Contains(x.Name)).ToList();
        db.ElectionPersonas.AddRange(missing);

        // A persona with no permissions cannot even open the election dashboard, so an empty set is
        // never a deliberate choice. Only rows still named as a default are filled; a renamed or
        // custom persona has no known default.
        var filled = 0;
        foreach (var row in existing.Where(x => x.Permissions == ElectionPermission.None))
        {
            var match = defaults.FirstOrDefault(x => x.Name == row.Name);
            if (match is null)
                continue;
            row.Permissions = match.Permissions;
            row.UpdatedAt = now;
            filled++;
        }

        if (missing.Count == 0 && filled == 0)
            return;

        await db.SaveChangesAsync();
        if (filled > 0)
            logger.LogInformation("Gave {Count} election persona(s) their default permissions.", filled);
    }
}
