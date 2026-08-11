namespace GHCAA.Domain.Models
{
    public class OrganizationConfig
    {
        public int Id { get; set; }
        public string OrgId { get; set; } = "default";
        public int SchemaVersion { get; set; } = 1;
        public string ConfigJson { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedByAdminId { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
