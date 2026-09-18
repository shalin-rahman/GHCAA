namespace GHCAA.Application.DTOs
{
    // 82.115: one docs/TODO.md line, parsed for the admin tracker screen.
    public class DevTrackerItemDto
    {
        public string Id { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public string? DependsOn { get; set; }
        public string Summary { get; set; } = null!;
        public int WorkPackageNumber { get; set; }
        public string WorkPackageTitle { get; set; } = null!;
    }

    public class DevTrackerFilterDto
    {
        // "all" or "P0".."P4". Status is fixed to TODO/PARTIAL server-side — this screen is for
        // open work, not a full mirror of the file.
        public string? Priority { get; set; }
    }
}
