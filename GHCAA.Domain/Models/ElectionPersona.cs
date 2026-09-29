using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models;

// Spec 023 (37.12b). A persona is a named role in the election process (Returning Officer,
// Scrutineer, and so on) with a fixed set of permissions and a declaration officials sign
// when they take it on. 37.12d will appoint members and non-members to these.
public class ElectionPersona
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ElectionPermission Permissions { get; set; }
    public int MinCount { get; set; }
    public int? MaxCount { get; set; }
    public bool ShowOnPublicBoard { get; set; }
    public bool TakesOverFromAdmin { get; set; }
    public string DeclarationText { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
