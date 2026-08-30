using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class SetMay2026BatchMemberPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The May 2026 alumni batch (AddMay2026AlumniRegistrationBatch) was inserted with
            // PhotoPath = NULL because the passport-size photos supplied for the batch weren't
            // matched to their members yet. They've now been matched, compressed (max 1920px,
            // JPEG quality 85, 350KB target) via the same pipeline LocalFileStorageService uses
            // for live uploads, and saved under wwwroot/uploads/members/{id}/photo/. Only set
            // PhotoPath when it's still NULL, so this never overwrites a real photo a member
            // uploaded themselves since the batch import landed.
            //
            // Hand-written raw SQL rather than the scaffolded UpdateData calls: `dotnet ef
            // migrations add` here always also emits non-deterministic churn on unrelated
            // Users.SecurityStamp/PasswordHash and EmailTemplates.LastUpdated rows (see
            // gotcha_pending_model_changes_seed) plus an unrelated pending EventPhotos diff from
            // other in-flight work — none of which belongs in this migration.
            migrationBuilder.Sql(@"
UPDATE ""Members"" SET ""PhotoPath"" = CASE ""Id""
    WHEN 782 THEN 'uploads/members/782/photo/f9099dcfaff349948e6b12ee7cfe4dda_782.jpg'
    WHEN 783 THEN 'uploads/members/783/photo/d27fb15afa314b1598eaf0d20ecdbb7c_783.jpg'
    WHEN 784 THEN 'uploads/members/784/photo/fe75b2fa157a463ca3ccf6b7e581ee26_784.jpg'
    WHEN 785 THEN 'uploads/members/785/photo/0a4a900add6e486c837e72618a0353dd_785.jpg'
    WHEN 786 THEN 'uploads/members/786/photo/d63098c41bd244d8b834414bd1f159d6_786.jpg'
    WHEN 787 THEN 'uploads/members/787/photo/b61b947193df4f70a121be7ff34ae0a9_787.jpg'
    WHEN 788 THEN 'uploads/members/788/photo/d00bb56e5b5043ed945ed8b1f3b9eb70_788.jpg'
    WHEN 790 THEN 'uploads/members/790/photo/5e80a71905e74b28873112367de087a7_790.jpg'
    WHEN 793 THEN 'uploads/members/793/photo/3a7ba6d5764a4e8bb749d544af4463da_793.jpg'
    WHEN 794 THEN 'uploads/members/794/photo/ecbb83cc728d4aeda4c065cd8b7a681f_794.jpg'
    WHEN 796 THEN 'uploads/members/796/photo/6de348a3f8c44763bd10b7c5c8e4aa60_796.jpg'
    WHEN 797 THEN 'uploads/members/797/photo/d68f02e62d8d4e7b902a7509dfdfec59_797.jpg'
    WHEN 798 THEN 'uploads/members/798/photo/99c29593f6424208948c6de07abeaaac_798.jpg'
    WHEN 803 THEN 'uploads/members/803/photo/9e2f5508134f4766835dbe748d460610_803.jpg'
    WHEN 809 THEN 'uploads/members/809/photo/26a90c7c9ab6485cafae603fb43dca60_809.jpg'
    WHEN 810 THEN 'uploads/members/810/photo/a46261a83ea04f1c9bdc093f3fe2ce7c_810.jpg'
    WHEN 811 THEN 'uploads/members/811/photo/23aa42b590be4187a90b8a1d084c7aea_811.jpg'
    WHEN 813 THEN 'uploads/members/813/photo/470bf436ce5f469cbaab798c8c9e06cd_813.jpg'
    WHEN 814 THEN 'uploads/members/814/photo/4e26c71995bf4511a352b37567c7e732_814.jpg'
    WHEN 817 THEN 'uploads/members/817/photo/1fb1df8c598246a79b2114eece9e7a6d_817.jpg'
    WHEN 818 THEN 'uploads/members/818/photo/4777f1f55e5b4514b931c792d86a628e_818.jpg'
    WHEN 819 THEN 'uploads/members/819/photo/cda95f7590a84fdbbdc242b45022b0ce_819.jpg'
    WHEN 820 THEN 'uploads/members/820/photo/acab823657ab4d338aa6ad1eeb23b9a9_820.jpg'
    WHEN 821 THEN 'uploads/members/821/photo/e648c4c526b64fb4ad6257e67d0d2d5d_821.jpg'
    WHEN 822 THEN 'uploads/members/822/photo/6cb2894576a84cc9b1e652f12b56ee70_822.jpg'
    WHEN 823 THEN 'uploads/members/823/photo/3199ea6b1e5e4e818a2207f3450c3c54_823.jpg'
    WHEN 824 THEN 'uploads/members/824/photo/b728893af6354cef8fc24da075982956_824.jpg'
    WHEN 825 THEN 'uploads/members/825/photo/80172ed7c7064b28bc5f19ad4bfc6a04_825.jpg'
    WHEN 826 THEN 'uploads/members/826/photo/403970da235b407b85bec3ad3fd99c36_826.jpg'
    END
WHERE ""Id"" IN (782,783,784,785,786,787,788,790,793,794,796,797,798,803,809,810,811,813,814,817,818,819,820,821,822,823,824,825,826)
  AND ""PhotoPath"" IS NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Content-based, not blanket NULL — only revert rows this migration actually set,
            // so a member who uploaded a real photo after this migration ran doesn't lose it.
            migrationBuilder.Sql(@"
UPDATE ""Members"" SET ""PhotoPath"" = NULL
WHERE ""Id"" IN (782,783,784,785,786,787,788,790,793,794,796,797,798,803,809,810,811,813,814,817,818,819,820,821,822,823,824,825,826)
  AND ""PhotoPath"" LIKE 'uploads/members/%/photo/%';
");
        }
    }
}
