using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Data;

// Spec 023 (37.12b). Inserts any default election persona missing by name. An existing row is
// never updated, so a SuperAdmin edit survives a redeploy. The ElectionAppointments migration
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
            Row("ECSC Member", "Search Committee", "Screens and recommends candidates before nominations open.",
                ElectionPermission.ViewDashboard, false, true, 3, 5, 10),
            Row("Chief Commissioner", "Election Commission", "Runs the election commission and can act in place of the admin.",
                chiefCommissioner, true, true, 1, 1, 20),
            Row("Commissioner", "Election Commission", "Sits on the election commission alongside the Chief Commissioner.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit | ElectionPermission.ManageSetup
                    | ElectionPermission.ManageVoterRoll | ElectionPermission.DecideNominations | ElectionPermission.AppointOfficials
                    | ElectionPermission.ChangePhase | ElectionPermission.Declare | ElectionPermission.Approve,
                true, true, 2, 4, 30),
            Row("Returning Officer", "Officials", "Holds the ballot key and runs the count.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit | ElectionPermission.DecideNominations
                    | ElectionPermission.SetBallotKey | ElectionPermission.Count | ElectionPermission.Approve,
                false, true, 1, 1, 40),
            Row("Assistant Returning Officer", "Officials", "Helps the Returning Officer with nominations.",
                ElectionPermission.ViewDashboard | ElectionPermission.DecideNominations, false, true, 0, null, 50),
            Row("Presiding Officer", "Officials", "Oversees a polling station on election day.",
                ElectionPermission.ViewDashboard, false, true, 0, null, 60),
            Row("Polling Officer", "Officials", "Staffs a polling station on election day.",
                ElectionPermission.ViewDashboard, false, true, 0, null, 70),
            Row("Scrutineer", "Officials", "Checks nominations for eligibility.",
                ElectionPermission.ViewDashboard | ElectionPermission.DecideNominations, false, true, 0, null, 80),
            Row("Counting Supervisor", "Officials", "Oversees the vote count.",
                ElectionPermission.ViewDashboard | ElectionPermission.Count, false, true, 0, null, 90),
            Row("Technical Administrator", "Officials", "Keeps the voting system running.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit, false, true, 0, null, 100),
            Row("Cybersecurity Auditor", "Officials", "Reviews the election for security issues.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit, false, true, 0, null, 110),
            Row("Security Officer", "Officials", "Handles physical security for the election.",
                ElectionPermission.ViewDashboard, false, false, 0, null, 120),
            Row("Observer", "Observers", "Watches the election without taking part in it.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit, false, true, 0, null, 130),
            Row("Appeal Tribunal Member", "Appeal Tribunal", "Decides appeals raised against election decisions.",
                ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit | ElectionPermission.DecideAppeals,
                false, true, 3, 3, 140)
        };

        var existing = await db.ElectionPersonas.Select(x => x.Name).ToListAsync();
        var missing = defaults.Where(x => !existing.Contains(x.Name)).ToList();
        if (missing.Count == 0)
            return;

        db.ElectionPersonas.AddRange(missing);
        await db.SaveChangesAsync();
    }
}
