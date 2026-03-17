using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class MemberImportService : IMemberImportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _storage;
        private readonly IUserService _userService;
        private readonly ILogger<MemberImportService> _logger;

        public MemberImportService(
            ApplicationDbContext db,
            IFileStorageService storage,
            IUserService userService,
            ILogger<MemberImportService> logger)
        {
            _db = db;
            _storage = storage;
            _userService = userService;
            _logger = logger;
        }

        public async Task<MemberImportResultDto> ImportMembersAsync(MemberImportRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = new MemberImportResultDto();
            Dictionary<string, string> mapping;
            Dictionary<string, string> defaultValues;

            try
            {
                mapping = JsonSerializer.Deserialize<Dictionary<string, string>>(request.ColumnMappingJson) ?? new();
                defaultValues = JsonSerializer.Deserialize<Dictionary<string, string>>(request.DefaultValuesJson) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize mapping or default values");
                result.Errors.Add("System Error: Invalid mapping configuration.");
                return result;
            }
            
            using var workbook = new XLWorkbook(request.ExcelFile.OpenReadStream());
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                result.Errors.Add("The Excel file has no worksheets.");
                return result;
            }

            var rowsUsed = worksheet.RangeUsed()?.RowsUsed();
            if (rowsUsed == null || rowsUsed.Count() <= 1)
            {
                result.Errors.Add("The Excel file is empty or only contains headers.");
                return result;
            }

            var rows = rowsUsed.Skip(1); // Skip header

            // Robust Header Extraction: Handle duplicates by taking the first occurrence
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var headerRow = worksheet.Row(1);
            foreach (var cell in headerRow.CellsUsed())
            {
                var headerName = cell.Value.ToString().Trim();
                if (!string.IsNullOrEmpty(headerName) && !headers.ContainsKey(headerName))
                {
                    headers.Add(headerName, cell.Address.ColumnNumber);
                }
            }

            // Pre-fetch all members for O(1) lookup and to support updates
            var allExistingMembers = await _db.Members.ToListAsync(cancellationToken);
            var existingMembersByNID = new Dictionary<string, Member>(StringComparer.OrdinalIgnoreCase);
            var existingEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existingMobiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var m in allExistingMembers)
            {
                if (!string.IsNullOrWhiteSpace(m.NID)) existingMembersByNID[m.NID] = m;
                if (!string.IsNullOrWhiteSpace(m.Email)) existingEmails.Add(m.Email);
                if (!string.IsNullOrWhiteSpace(m.MobileNo)) existingMobiles.Add(m.MobileNo);
            }

            // Also track within-batch to catch intra-batch duplicates
            var batchEmails  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var batchNIDs    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var batchMobiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var batchMembers = new List<(Member Member, string? ExternalId, int RowNum, bool IsUpdate)>();

            foreach (var row in rows)
            {
                try
                {
                    string rowNID = string.Empty;
                    foreach (var map in mapping)
                    {
                        if (map.Value.Equals(nameof(Member.NID)) && headers.TryGetValue(map.Key, out var colIndex))
                        {
                            rowNID = row.Cell(colIndex).Value.ToString().Trim();
                            break;
                        }
                    }

                    bool isUpdate = false;
                    Member member;

                    if (!string.IsNullOrWhiteSpace(rowNID) && existingMembersByNID.TryGetValue(rowNID, out var existingMember))
                    {
                        member = existingMember;
                        isUpdate = true;
                        
                        // Like insert, set default flags to update
                        member.Status = Enums.MembershipStatus.Active;
                        member.EmailVerified = true;
                        member.ApprovedDate = DateTime.UtcNow;
                        member.AppliedDate = DateTime.UtcNow;
                        member.LastUpdateDate = DateTime.UtcNow;
                        if (member.DateOfBirth == default) 
                        {
                            member.DateOfBirth = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(rowNID) && batchNIDs.Contains(rowNID))
                    {
                        result.Errors.Add($"Row {row.RowNumber()}: Duplicate NID '{rowNID}' within the uploaded file — skipped.");
                        result.FailureCount++;
                        continue;
                    }
                    else
                    {
                        member = new Member
                        {
                            Status        = Enums.MembershipStatus.Active,
                            EmailVerified = true,
                            AppliedDate   = DateTime.UtcNow,
                            ApprovedDate  = DateTime.UtcNow,
                            LastUpdateDate = DateTime.UtcNow,
                            // PostgreSQL requires Kind=Utc; default(DateTime) is Unspecified
                            DateOfBirth   = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                            FullName      = "",
                            NID           = string.IsNullOrWhiteSpace(rowNID) ? "" : rowNID,
                            Email         = ""
                        };
                    }

                    string? externalId = null;
                    string? originalEmail = isUpdate ? member.Email : null;
                    string? originalMobile = isUpdate ? member.MobileNo : null;

                    // Apply column mapping from Excel
                    foreach (var map in mapping)
                    {
                        if (headers.TryGetValue(map.Key, out var colIndex))
                        {
                            var cellValue = row.Cell(colIndex).Value.ToString().Trim();
                            if (map.Value.Equals("ID", StringComparison.OrdinalIgnoreCase))
                            { externalId = cellValue; continue; }
                            SetPropertyValue(member, map.Value, cellValue);
                        }
                    }

                    // Apply user-configured default values for any still-empty fields
                    foreach (var def in defaultValues)
                    {
                        if (!string.IsNullOrEmpty(def.Value) && string.IsNullOrEmpty(GetPropertyValue(member, def.Key)))
                            SetPropertyValue(member, def.Key, def.Value);
                    }

                    var rowTag = $"Row {row.RowNumber()}";

                    // ── REQUIRED: FullName (cannot be auto-generated meaningfully) ──────────
                    if (string.IsNullOrWhiteSpace(member.FullName))
                    {
                        result.Errors.Add($"{rowTag}: Missing Full Name — skipped.");
                        result.FailureCount++;
                        continue;
                    }

                    // ── AUTO-FILL non-nullable string fields with traceable defaults ────────
                    if (string.IsNullOrWhiteSpace(member.FatherName))          { member.FatherName = $"{Constants.Defaults.ImportPrefix}-Father-{row.RowNumber()}";                result.Errors.Add($"{rowTag}: FatherName missing — set to '{member.FatherName}'"); }
                    if (string.IsNullOrWhiteSpace(member.MotherName))          { member.MotherName = $"{Constants.Defaults.ImportPrefix}-Mother-{row.RowNumber()}";                result.Errors.Add($"{rowTag}: MotherName missing — set to '{member.MotherName}'"); }
                    if (string.IsNullOrWhiteSpace(member.PresentAddress))      { member.PresentAddress = $"{Constants.Defaults.ImportPrefix}-{Constants.Defaults.UnknownValue}";                             result.Errors.Add($"{rowTag}: PresentAddress missing — set to '{member.PresentAddress}'"); }
                    if (string.IsNullOrWhiteSpace(member.PermanentAddress))    { member.PermanentAddress = member.PresentAddress;                      result.Errors.Add($"{rowTag}: PermanentAddress missing — copied from PresentAddress"); }
                    if (string.IsNullOrWhiteSpace(member.EmergencyContactName)) { member.EmergencyContactName = $"{Constants.Defaults.ImportPrefix}-{Constants.Defaults.UnknownValue}";                      result.Errors.Add($"{rowTag}: EmergencyContactName missing — set to '{member.EmergencyContactName}'"); }
                    if (string.IsNullOrWhiteSpace(member.EmergencyContactRelation)) { member.EmergencyContactRelation = Constants.Defaults.UnknownValue;                    result.Errors.Add($"{rowTag}: EmergencyContactRelation missing — set to '{Constants.Defaults.UnknownValue}'"); }
                    if (string.IsNullOrWhiteSpace(member.EmergencyContactPhone)) { member.EmergencyContactPhone = $"{Constants.Defaults.ImportPrefix}-{Constants.Defaults.UnknownValue}";                   result.Errors.Add($"{rowTag}: EmergencyContactPhone missing — set to '{member.EmergencyContactPhone}'"); }
                    if (string.IsNullOrWhiteSpace(member.HighestCertificate))  { member.HighestCertificate = Enums.Degree.HSC.ToString();                                   result.Errors.Add($"{rowTag}: HighestCertificate missing — defaulted to '{Enums.Degree.HSC}'"); }
                    if (string.IsNullOrWhiteSpace(member.HighestCertificateGroup))   { member.HighestCertificateGroup = Constants.Defaults.UnknownValue;                    result.Errors.Add($"{rowTag}: HighestCertificateGroup missing — defaulted to '{Constants.Defaults.UnknownValue}'"); }
                    if (string.IsNullOrWhiteSpace(member.HighestCertificateSubject)) { member.HighestCertificateSubject = "None";                     result.Errors.Add($"{rowTag}: HighestCertificateSubject missing — defaulted to 'None'"); }
                    if (string.IsNullOrWhiteSpace(member.GHCLastCertificate))  { member.GHCLastCertificate = Enums.Degree.HSC.ToString();                                   result.Errors.Add($"{rowTag}: GHCLastCertificate missing — defaulted to '{Enums.Degree.HSC}'"); }
                    if (string.IsNullOrWhiteSpace(member.GHCLastCertificateGroup))   { member.GHCLastCertificateGroup = Constants.Defaults.UnknownValue;                    result.Errors.Add($"{rowTag}: GHCLastCertificateGroup missing — defaulted to '{Constants.Defaults.UnknownValue}'"); }
                    if (string.IsNullOrWhiteSpace(member.GHCLastCertificateSubject)) { member.GHCLastCertificateSubject = "None";                     result.Errors.Add($"{rowTag}: GHCLastCertificateSubject missing — defaulted to 'None'"); }
                    if (string.IsNullOrWhiteSpace(member.ProfessionalSector))  { member.ProfessionalSector = Constants.Defaults.UnknownValue;                               result.Errors.Add($"{rowTag}: ProfessionalSector missing — defaulted to '{Constants.Defaults.UnknownValue}'"); }
                    if (string.IsNullOrWhiteSpace(member.Designation))         { member.Designation = Constants.Defaults.UnknownValue;                                       result.Errors.Add($"{rowTag}: Designation missing — defaulted to '{Constants.Defaults.UnknownValue}'"); }
 
                    // ── UNIQUE: Email (auto-generate from NID if missing) ─────────────────
                    if (string.IsNullOrWhiteSpace(member.Email))
                    {
                        var id = !string.IsNullOrWhiteSpace(member.NID) ? member.NID.Trim() : $"row{row.RowNumber()}";
                        member.Email = $"{Constants.Defaults.ImportEmailBase}+{id}@gmail.com";
                        result.Errors.Add($"{rowTag}: Email missing — assigned '{member.Email}'");
                    }
 
                    // ── UNIQUE: NID (generate traceable placeholder if missing) ───────────
                    if (string.IsNullOrWhiteSpace(member.NID))
                    {
                        member.NID = $"{Constants.Defaults.ImportPrefix}-{row.RowNumber()}-{DateTime.UtcNow.Ticks % 100000}";
                        result.Errors.Add($"{rowTag}: NID missing — assigned placeholder '{member.NID}'");
                    }
 
                    // ── UNIQUE: MobileNo (generate traceable placeholder if missing) ──────
                    if (string.IsNullOrWhiteSpace(member.MobileNo))
                    {
                        member.MobileNo = $"{Constants.Defaults.ImportPrefix}-{row.RowNumber()}-{DateTime.UtcNow.Ticks % 100000}";
                        result.Errors.Add($"{rowTag}: MobileNo missing — assigned placeholder '{member.MobileNo}'");
                    }

                    // ── DUPLICATE RESOLUTION (DB + intra-batch) ──────────────────────────────
                    if (!isUpdate && existingMembersByNID.ContainsKey(member.NID))
                    {
                        // Match by NID found but wasn't caught earlier? (Shouldn't happen with current logic, but safety first)
                        member = existingMembersByNID[member.NID];
                        isUpdate = true;
                    }

                    if (!string.IsNullOrWhiteSpace(member.Email))
                    {
                        string baseEmail = member.Email;
                        int suffix = 1;
                        while (existingEmails.Contains(member.Email) || batchEmails.Contains(member.Email))
                        {
                            if (isUpdate && member.Email == originalEmail) break; // Existing member keeping their email is fine

                            var parts = baseEmail.Split('@');
                            if (parts.Length == 2)
                            {
                                member.Email = $"{parts[0]}+{suffix}@{parts[1]}";
                                suffix++;
                            }
                            else
                            {
                                member.Email = $"{baseEmail}_{suffix}";
                                suffix++;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(member.MobileNo))
                    {
                        string baseMobile = member.MobileNo;
                        int suffix = 1;
                        while (existingMobiles.Contains(member.MobileNo) || batchMobiles.Contains(member.MobileNo))
                        {
                            if (isUpdate && member.MobileNo == originalMobile) break; // Existing member keeping their mobile is fine
                            member.MobileNo = $"{baseMobile}{suffix}";
                            suffix++;
                        }
                    }

                    // Register in batch-tracking sets
                    if (!isUpdate) batchNIDs.Add(member.NID);
                    if (!string.IsNullOrWhiteSpace(member.Email)) batchEmails.Add(member.Email);
                    if (!string.IsNullOrWhiteSpace(member.MobileNo)) batchMobiles.Add(member.MobileNo);

                    batchMembers.Add((member, externalId, row.RowNumber(), isUpdate));
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row.RowNumber()}: Data error - {ex.Message}");
                    result.FailureCount++;
                }
            }

            if (!batchMembers.Any()) return result;

            // Step 1: Bulk Save Members to generate IDs
            foreach (var item in batchMembers)
            {
                if (!item.IsUpdate)
                {
                    _db.Members.Add(item.Member);
                }
            }

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save member batch");
                result.Errors.Add("Database Error: Failed to save member records. Please check for unique constraints.");
                return result;
            }

            // Step 2: Handle related entities and per-member operations sequentially
            // (EF Core DbContext is not thread-safe — cannot use Task.WhenAll here)
            var passingYearCounts = new Dictionary<int, int>();

            foreach (var item in batchMembers)
            {
                try
                {
                    // Generate Membership Number if missing: Format GHC-NID
                    if (string.IsNullOrWhiteSpace(item.Member.MembershipNumber))
                    {
                        item.Member.MembershipNumber = $"{Constants.Defaults.MembershipPrefix}{item.Member.NID}";
                        _db.Members.Update(item.Member);
                    }

                    // EC Position is intentionally NOT synced during import

                    // Handle Photo: match by ExternalId (Registration ID), NID, or MobileNo
                    var validPhotoNames = new[] { item.ExternalId, item.Member.NID, item.Member.MobileNo }
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToArray();

                    if (validPhotoNames.Any())
                    {
                        var photoFile = request.Photos.FirstOrDefault(p =>
                        {
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(p.FileName);
                            return validPhotoNames.Contains(nameWithoutExt, StringComparer.OrdinalIgnoreCase);
                        });

                        if (photoFile != null)
                        {
                            try
                            {
                                var path = await _storage.SaveFileAsync(photoFile.OpenReadStream(), photoFile.FileName, item.Member.Id, Enums.FileUploadType.Photo, cancellationToken);
                                item.Member.PhotoPath = path;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to save photo for member {MemberId}", item.Member.Id);
                            }
                        }
                    }

                    // Create or Update User Account using NID as both Username and default Password
                    var username = item.Member.NID;
                    var defaultPassword = item.Member.NID;

                    var existingUser = await _db.Users
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.MemberId == item.Member.Id, cancellationToken);

                    if (existingUser == null)
                    {
                        await _userService.CreateUserAccountAsync(
                            item.Member.Id,
                            username,
                            defaultPassword,
                            cancellationToken);

                        existingUser = await _db.Users
                            .Include(u => u.Roles)
                            .FirstOrDefaultAsync(u => u.MemberId == item.Member.Id, cancellationToken);
                    }
                    else
                    {
                        existingUser.Username = username;
                        existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                        _db.Users.Update(existingUser);
                    }

                    // Add default Member Role if missing
                    if (existingUser != null && !existingUser.Roles.Any(r => r.Name == Constants.Roles.Member))
                    {
                        var memberRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.Member, cancellationToken);
                        if (memberRole != null)
                        {
                            existingUser.Roles.Add(memberRole);
                            _db.Users.Update(existingUser);
                        }
                    }

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing post-import step for row {RowNum}", item.RowNum);
                    // Member was already batch-saved; count it as a partial success
                    // to avoid confusion but log the secondary error
                }
            }

            // Final save: persist PhotoPath updates and ECMember entries
            await _db.SaveChangesAsync(cancellationToken);

            return result;
        }

        private string? GetPropertyValue(Member member, string propName)
        {
            return propName switch
            {
                nameof(Member.FullName) => member.FullName,
                nameof(Member.Email) => member.Email,
                nameof(Member.MobileNo) => member.MobileNo,
                nameof(Member.NID) => member.NID,
                nameof(Member.FatherName) => member.FatherName,
                nameof(Member.MotherName) => member.MotherName,
                nameof(Member.DateOfBirth) => member.DateOfBirth.ToString("yyyy-MM-dd"),
                nameof(Member.GHCLastCertificatePassingYear) => member.GHCLastCertificatePassingYear.ToString(),
                nameof(Member.MembershipNumber) => member.MembershipNumber,
                nameof(Member.Designation) => member.Designation,
                nameof(Member.ProfessionalSector) => member.ProfessionalSector,
                nameof(Member.PresentAddress) => member.PresentAddress,
                nameof(Member.PermanentAddress) => member.PermanentAddress,
                nameof(Member.EmergencyContactName) => member.EmergencyContactName,
                nameof(Member.EmergencyContactRelation) => member.EmergencyContactRelation,
                nameof(Member.EmergencyContactPhone) => member.EmergencyContactPhone,
                nameof(Member.HighestCertificate) => member.HighestCertificate,
                nameof(Member.HighestCertificateGroup) => member.HighestCertificateGroup,
                nameof(Member.HighestCertificateSubject) => member.HighestCertificateSubject,
                nameof(Member.GHCLastCertificate) => member.GHCLastCertificate,
                nameof(Member.GHCLastCertificateGroup) => member.GHCLastCertificateGroup,
                nameof(Member.GHCLastCertificateSubject) => member.GHCLastCertificateSubject,
                nameof(Member.HSCAdmissionYear) => member.HSCAdmissionYear.ToString(),
                nameof(Member.GHCAdmissionYear) => member.GHCAdmissionYear.ToString(),
                nameof(Member.Category) => member.Category.ToString(),
                _ => null
            };
        }

        private void SetPropertyValue(Member member, string propName, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            switch (propName)
            {
                case nameof(Member.FullName): member.FullName = value; break;
                case nameof(Member.Email): member.Email = value.Trim().ToLower(); break;
                case nameof(Member.MobileNo): member.MobileNo = value; break;
                case nameof(Member.NID): member.NID = value; break;
                case nameof(Member.FatherName): member.FatherName = value; break;
                case nameof(Member.MotherName): member.MotherName = value; break;
                case nameof(Member.GHCLastCertificatePassingYear): 
                    if (int.TryParse(value, out var yr)) member.GHCLastCertificatePassingYear = yr; break;
                case nameof(Member.HSCAdmissionYear): 
                    if (int.TryParse(value, out var yrHsc)) member.HSCAdmissionYear = yrHsc; break;
                case nameof(Member.GHCAdmissionYear): 
                    if (int.TryParse(value, out var yrGhc)) member.GHCAdmissionYear = yrGhc; break;
                case nameof(Member.DateOfBirth):
                    if (DateTime.TryParse(value, out var dob))
                        // Treat Excel dates as local/unspecified; store as UTC for PostgreSQL
                        member.DateOfBirth = DateTime.SpecifyKind(dob, DateTimeKind.Utc);
                    break;
                case nameof(Member.MembershipNumber): member.MembershipNumber = value; break;
                case nameof(Member.Designation): member.Designation = value; break;
                case nameof(Member.ProfessionalSector): member.ProfessionalSector = value; break;
                case nameof(Member.PresentAddress): member.PresentAddress = value; break;
                case nameof(Member.PermanentAddress): member.PermanentAddress = value; break;
                case nameof(Member.HighestCertificate): member.HighestCertificate = value; break;
                case nameof(Member.HighestCertificateGroup): member.HighestCertificateGroup = value; break;
                case nameof(Member.HighestCertificateSubject): member.HighestCertificateSubject = value; break;
                case nameof(Member.GHCLastCertificate): member.GHCLastCertificate = value; break;
                case nameof(Member.GHCLastCertificateGroup): member.GHCLastCertificateGroup = value; break;
                case nameof(Member.GHCLastCertificateSubject): member.GHCLastCertificateSubject = value; break;
                case nameof(Member.EmergencyContactName): member.EmergencyContactName = value; break;
                case nameof(Member.EmergencyContactRelation): member.EmergencyContactRelation = value; break;
                case nameof(Member.EmergencyContactPhone): member.EmergencyContactPhone = value; break;
                case nameof(Member.Gender):
                    if (Enum.TryParse<Enums.Gender>(value, true, out var gender)) member.Gender = gender; break;
                case nameof(Member.BloodGroup):
                    if (Enum.TryParse<Enums.BloodGroup>(value, true, out var bg)) member.BloodGroup = bg; break;
                case nameof(Member.MembershipType):
                    if (Enum.TryParse<Enums.MembershipType>(value, true, out var mt)) member.MembershipType = mt; break;
                case nameof(Member.ECPosition):
                    if (Enum.TryParse<Enums.ECPosition>(value, true, out var pos)) member.ECPosition = pos; break;
                case nameof(Member.Category):
                    if (Enum.TryParse<Enums.MemberCategory>(value, true, out var cat)) member.Category = cat; break;
            }
        }
    }
}
