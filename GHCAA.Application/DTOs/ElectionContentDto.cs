using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    // 37.14s. A public list leaves Body null; one page or form, or an admin read, carries it.
    public class ElectionContentDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public ElectionContentKind Kind { get; set; }
        public ElectionPageFormat? Format { get; set; }
        public string? Group { get; set; }
        public string Title { get; set; } = null!;
        public string? Summary { get; set; }
        public string? Body { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class UpsertElectionContentDto
    {
        [Required]
        [MaxLength(Constants.Elections.ContentKeyMaxLength)]
        [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "Key may hold only letters, digits and hyphens.")]
        public string Key { get; set; } = null!;

        // The JSON enum converter also takes plain numbers, so an unnamed value has to be refused here.
        [EnumDataType(typeof(ElectionContentKind))]
        public ElectionContentKind Kind { get; set; }

        // Required for a page and left null for a form. The service checks this.
        [EnumDataType(typeof(ElectionPageFormat))]
        public ElectionPageFormat? Format { get; set; }

        [MaxLength(Constants.Elections.ContentGroupMaxLength)]
        public string? Group { get; set; }

        [Required]
        [MaxLength(Constants.Elections.ContentTitleMaxLength)]
        public string Title { get; set; } = null!;

        [MaxLength(Constants.Elections.ContentSummaryMaxLength)]
        public string? Summary { get; set; }

        [Required]
        [MaxLength(Constants.Elections.ContentBodyMaxLength)]
        public string Body { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public bool IsPublished { get; set; } = true;
    }

    // The stored layout of one form. Field labels in Header print as ruled lines on a blank form.
    public sealed class ElectionFormLayout
    {
        public static readonly IReadOnlySet<string> SectionKinds =
            new HashSet<string> { "fields", "text", "checklist", "declaration", "table" };

        public List<string> Header { get; set; } = [];
        public List<ElectionFormSection> Sections { get; set; } = [];
        public List<string> Signatures { get; set; } = [];
        public bool Seal { get; set; }
    }

    // Items are labels, lines or ticks by kind. For a table they are the column headings, Rows
    // holds the first-column labels and Blank the number of empty rows under them.
    public sealed class ElectionFormSection
    {
        public string? Name { get; set; }
        public string Kind { get; set; } = null!;
        public List<string> Items { get; set; } = [];
        public List<string>? Rows { get; set; }
        public int? Blank { get; set; }
    }
}
