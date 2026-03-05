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
            var mapping = JsonSerializer.Deserialize<Dictionary<string, string>>(request.ColumnMappingJson) ?? new();
            var defaultValues = JsonSerializer.Deserialize<Dictionary<string, string>>(request.DefaultValuesJson) ?? new();
            
            using var workbook = new XLWorkbook(request.ExcelFile.OpenReadStream());
            var worksheet = workbook.Worksheets.First();
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

            var headers = worksheet.Row(1).CellsUsed().ToDictionary(c => c.Value.ToString(), c => c.Address.ColumnNumber);

            foreach (var row in rows)
            {
                try
                {
                    var member = new Member
                    {
                        Status = Enums.MembershipStatus.Active,
                        EmailVerified = true,
                        AppliedDate = DateTime.UtcNow,
                        ApprovedDate = DateTime.UtcNow
                    };

                    string? externalId = null;

                    // Apply mapping from Excel
                    foreach (var map in mapping)
                    {
                        var excelColName = map.Key;
                        var systemPropName = map.Value;

                        if (headers.TryGetValue(excelColName, out var colIndex))
                        {
                            var cellValue = row.Cell(colIndex).Value.ToString();
                            
                            if (systemPropName.Equals("ID", StringComparison.OrdinalIgnoreCase))
                            {
                                externalId = cellValue;
                                continue;
                            }

                            SetPropertyValue(member, systemPropName, cellValue);
                        }
                    }

                    // Apply default values if property is still empty
                    foreach (var defaultValueEntry in defaultValues)
                    {
                        var propName = defaultValueEntry.Key;
                        var val = defaultValueEntry.Value;

                        if (!string.IsNullOrEmpty(val))
                        {
                            var currentVal = GetPropertyValue(member, propName);
                            if (string.IsNullOrEmpty(currentVal))
                            {
                                SetPropertyValue(member, propName, val);
                            }
                        }
                    }

                    // Basic validation
                    if (string.IsNullOrEmpty(member.Email) || string.IsNullOrEmpty(member.FullName))
                    {
                        result.Errors.Add($"Row {row.RowNumber()}: Missing Email or Full Name.");
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicates
                    if (await _db.Members.AnyAsync(m => m.Email == member.Email, cancellationToken))
                    {
                        result.Errors.Add($"Row {row.RowNumber()}: Member with email {member.Email} already exists.");
                        result.FailureCount++;
                        continue;
                    }

                    _db.Members.Add(member);
                    await _db.SaveChangesAsync(cancellationToken);

                    // Handle Photo if externalId is matched with file name
                    if (!string.IsNullOrEmpty(externalId))
                    {
                        var photoFile = request.Photos.FirstOrDefault(p => 
                            Path.GetFileNameWithoutExtension(p.FileName).Equals(externalId, StringComparison.OrdinalIgnoreCase));
                        
                        if (photoFile != null)
                        {
                            var path = await _storage.SaveFileAsync(photoFile.OpenReadStream(), photoFile.FileName, member.Id, Enums.FileUploadType.Photo, cancellationToken);
                            member.PhotoPath = path;
                            await _db.SaveChangesAsync(cancellationToken);
                        }
                    }

                    // Create User Account with default password
                    var defaultPassword = "UpdateMe123!"; // Simple default for imports
                    await _userService.CreateUserAccountAsync(member.Id, member.MembershipNumber ?? member.Email, defaultPassword, cancellationToken);

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    result.FailureCount++;
                }
            }

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
                nameof(Member.SubjectGroup) => member.SubjectGroup,
                nameof(Member.HSCAdmissionYear) => member.HSCAdmissionYear.ToString(),
                nameof(Member.GHCAdmissionYear) => member.GHCAdmissionYear.ToString(),
                nameof(Member.LastCertificateFromGHC) => member.LastCertificateFromGHC,
                _ => null
            };
        }

        private void SetPropertyValue(Member member, string propName, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            switch (propName)
            {
                case nameof(Member.FullName): member.FullName = value; break;
                case nameof(Member.Email): member.Email = value; break;
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
                    if (DateTime.TryParse(value, out var dob)) member.DateOfBirth = dob; break;
                case nameof(Member.MembershipNumber): member.MembershipNumber = value; break;
                case nameof(Member.Designation): member.Designation = value; break;
                case nameof(Member.ProfessionalSector): member.ProfessionalSector = value; break;
                case nameof(Member.PresentAddress): member.PresentAddress = value; break;
                case nameof(Member.PermanentAddress): member.PermanentAddress = value; break;
                case nameof(Member.SubjectGroup): member.SubjectGroup = value; break;
                case nameof(Member.LastCertificateFromGHC): member.LastCertificateFromGHC = value; break;
                case nameof(Member.EmergencyContactName): member.EmergencyContactName = value; break;
                case nameof(Member.EmergencyContactRelation): member.EmergencyContactRelation = value; break;
                case nameof(Member.EmergencyContactPhone): member.EmergencyContactPhone = value; break;
                case nameof(Member.Gender):
                    if (Enum.TryParse<Enums.Gender>(value, true, out var gender)) member.Gender = gender; break;
                case nameof(Member.BloodGroup):
                    if (Enum.TryParse<Enums.BloodGroup>(value, true, out var bg)) member.BloodGroup = bg; break;
                case nameof(Member.MembershipType):
                    if (Enum.TryParse<Enums.MembershipType>(value, true, out var mt)) member.MembershipType = mt; break;
            }
        }
    }
}
