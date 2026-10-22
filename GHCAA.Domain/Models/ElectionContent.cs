using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    // 37.14s. One election page (Regulations, manuals, forms handbook) or the layout of one form.
    // Seeded from profiles/<org>/election-content and edited by an admin after that, so the seeder
    // only adds keys it cannot find.
    public class ElectionContent
    {
        public int Id { get; set; }

        // A page key such as "regulations", or a form code such as "ER-27C".
        public string Key { get; set; } = null!;
        public ElectionContentKind Kind { get; set; }

        // Pages only.
        public ElectionPageFormat? Format { get; set; }
        public string? Group { get; set; }
        public string? Summary { get; set; }

        public string Title { get; set; } = null!;

        // Markdown for a page. For a form, the layout JSON: header, sections, signatures, seal.
        // Both may hold {{OrgFullName}}, {{Head}} and the other tokens ElectionContentService fills in.
        public string Body { get; set; } = null!;

        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
        public DateTime? LastModified { get; set; }
        public int? UpdatedByAdminId { get; set; }
    }
}
