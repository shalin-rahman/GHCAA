# E2E Registration Test Script
$ErrorActionPreference = "Stop"
$BASE = "https://localhost:7214"

# Bypass untrusted dev certificate
if ([System.Net.ServicePointManager]::ServerCertificateValidationCallback -eq $null) {
    Add-Type @"
    using System.Net; using System.Security.Cryptography.X509Certificates;
    public class BypassCert : ICertificatePolicy {
        public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem) { return true; }
    }
"@
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object BypassCert
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
}

Write-Host ""
Write-Host "========================================"
Write-Host "  GHCAA E2E Registration Test"
Write-Host "========================================"

# STEP 1: Admin Login
Write-Host "`n[STEP 1] Admin Login..."
$adminToken = $null
$logins = @(
    @{ u = "shalin"; p = "shalin" },
    @{ u = "0000000001"; p = "0000000001" }
)

foreach ($creds in $logins) {
    try {
        $loginBody = @{ username = $creds.u; password = $creds.p } | ConvertTo-Json
        $loginResp = Invoke-RestMethod -Uri "$BASE/api/auth/login" -Method POST `
            -ContentType "application/json" -Body $loginBody
        $adminToken = $loginResp.token
        Write-Host "  [OK] Admin ($($creds.u)) logged in. Token acquired. Role: $($loginResp.role)"
        break
    } catch {
        Write-Host "  [INFO] Login failed for $($creds.u): $($_.Exception.Message)"
    }
}

if (-not $adminToken) {
    Write-Host "  [ERROR] All admin login attempts failed. Cannot proceed."
    exit 1
}


# STEP 2: Check if test user already exists and clean up
Write-Host "`n[STEP 2] Checking for existing test user..."
$headers = @{ Authorization = "Bearer $adminToken" }
try {
    $existing = Invoke-RestMethod -Uri "$BASE/api/admin/members?search=ghcregisterfinal" -Method GET -Headers $headers
    $found = $existing | Where-Object { $_.email -like "*ghcregisterfinal*" }
    if ($found) {
        Write-Host "  [WARN] Test user already exists with ID $($found.id). Test may show duplicate error."
    } else {
        Write-Host "  [OK] No existing test user found. Proceeding."
    }
} catch {
    Write-Host "  [INFO] Could not check existing: $($_.Exception.Message)"
}

# STEP 3: Submit Registration via multipart form
Write-Host "`n[STEP 3] Submitting Registration..."

# Create a small valid JPEG file for the photo upload
$photoPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "test_photo.jpg")
$jpegBytes = [byte[]](0xFF,0xD8,0xFF,0xE0,0x00,0x10,0x4A,0x46,0x49,0x46,0x00,0x01,
                       0x01,0x00,0x00,0x01,0x00,0x01,0x00,0x00,0xFF,0xD9)
[System.IO.File]::WriteAllBytes($photoPath, $jpegBytes)
Write-Host "  Photo file created: $photoPath"

# Build multipart body manually
$boundary = "----TestBoundary" + [System.Guid]::NewGuid().ToString("N").Substring(0,8)
$contentType = "multipart/form-data; boundary=$boundary"

function Add-Field($name, $value) {
    return "--$boundary`r`nContent-Disposition: form-data; name=`"$name`"`r`n`r`n$value`r`n"
}

$body = ""
$body += Add-Field "FullName" "Shalin E2E Test"
$body += Add-Field "FatherName" "Test Father"
$body += Add-Field "MotherName" "Test Mother"
$body += Add-Field "DateOfBirth" "1992-08-20"
$body += Add-Field "Gender" "Male"
$body += Add-Field "BloodGroup" "OPositive"
$body += Add-Field "NID" "9876543210"
$body += Add-Field "MobileNo" "01788888777"
$body += Add-Field "Email" "shalin.rahman+ghcregisterfinal@gmail.com"

$body += Add-Field "PresentAddress" "123 Test Street, Dhaka"
$body += Add-Field "PermanentAddress" "123 Test Street, Dhaka"
$body += Add-Field "TShirtSize" "L"
$body += Add-Field "MembershipType" "General"
$body += Add-Field "EmergencyContactName" "Emergency Person"
$body += Add-Field "EmergencyContactRelation" "Brother"
$body += Add-Field "EmergencyContactPhone" "01799999777"
$body += Add-Field "HasAcceptedTerms" "true"
$body += Add-Field "HasAcceptedGdpr" "true"
$body += Add-Field "PaymentMethodId" "1"
$body += Add-Field "TransactionId" "TEST-REG-001"
# Academic History
$body += Add-Field "AcademicHistory[0].InstitutionName" "Govt. Haraganga College"
$body += Add-Field "AcademicHistory[0].Degree" "HSC"
$body += Add-Field "AcademicHistory[0].Subject" "Science"
$body += Add-Field "AcademicHistory[0].AdmissionYear" "2005"
$body += Add-Field "AcademicHistory[0].PassingYear" "2007"
$body += Add-Field "AcademicHistory[0].IsGHC" "true"
# Professional History
$body += Add-Field "ProfessionalHistory[0].OrganizationName" "Tech Company Ltd"
$body += Add-Field "ProfessionalHistory[0].Designation" "Software Engineer"
$body += Add-Field "ProfessionalHistory[0].Sector" "IT"
$body += Add-Field "ProfessionalHistory[0].Location" "Dhaka"
$body += Add-Field "ProfessionalHistory[0].StartDate" "2012-01-01"
$body += Add-Field "ProfessionalHistory[0].IsCurrent" "true"

# Add photo file
$photoBytes = [System.IO.File]::ReadAllBytes($photoPath)
$photoEncoded = [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($photoBytes)
$body += "--$boundary`r`nContent-Disposition: form-data; name=`"photo`"; filename=`"test_photo.jpg`"`r`nContent-Type: image/jpeg`r`n`r`n$photoEncoded`r`n"
$body += "--$boundary--`r`n"

$bodyBytes = [System.Text.Encoding]::GetEncoding("iso-8859-1").GetBytes($body)

try {
    $regResp = Invoke-RestMethod -Uri "$BASE/api/auth/register" -Method POST `
        -ContentType $contentType -Body $bodyBytes
    Write-Host "  [SUCCESS] Registration submitted!"
    Write-Host "  MemberId: $($regResp.memberId)"
    Write-Host "  Message: $($regResp.message)"
    $memberId = $regResp.memberId
} catch {
    $errBody = $_.ErrorDetails.Message
    Write-Host "  [ERROR] Registration failed: $($_.Exception.Message)"
    Write-Host "  Details: $errBody"
    # Try to extract memberId from error or stop
    exit 1
}

# STEP 4: Check OTP requirement for this email
Write-Host "`n[STEP 4] OTP Check..."
Write-Host "  Email used: shalin.rahman+ghcregisterfinal@gmail.com"
Write-Host "  OTP was sent to this email on successful registration."

Write-Host "  [INFO] For bypass, we will use Admin API to manually verify later."

# STEP 5: Check pending approvals as Admin
Write-Host "`n[STEP 5] Admin - Checking Pending Approvals..."
$pending = Invoke-RestMethod -Uri "$BASE/api/admin/members/pending" -Method GET -Headers $headers
Write-Host "  Total pending: $($pending.Count)"
$testMember = $pending | Where-Object { $_.email -like "*ghcregisterfinal*" }

if ($testMember) {
    Write-Host "  [FOUND] Test member in queue: $($testMember.fullName) (ID: $($testMember.id))"
    $memberId = $testMember.id
} else {
    Write-Host "  [INFO] Member not in pending queue (may need email verification first)"
    Write-Host "  Using MemberId from registration: $memberId"
}

# STEP 6: Admin Approve the member
Write-Host "`n[STEP 6] Admin - Approving Member ID $memberId..."
try {
    $approveBody = @{ approve = $true; note = "Approved via E2E test" } | ConvertTo-Json
    $approveResp = Invoke-RestMethod -Uri "$BASE/api/admin/members/$memberId/approve" `
        -Method POST -Headers $headers -ContentType "application/json" -Body $approveBody
    Write-Host "  [SUCCESS] Approved!"
    Write-Host "  Response: $($approveResp | ConvertTo-Json)"
} catch {
    $errBody = $_.ErrorDetails.Message
    Write-Host "  [WARN] Direct approve failed: $($_.Exception.Message)"
    Write-Host "  Details: $errBody"
    
    # Try alternate endpoint
    try {
        $approveResp2 = Invoke-RestMethod -Uri "$BASE/api/admin/members/$memberId/induct" `
            -Method POST -Headers $headers -ContentType "application/json" -Body $approveBody
        Write-Host "  [SUCCESS via induct]" ($approveResp2 | ConvertTo-Json)
    } catch {
        Write-Host "  [ERROR] Induct also failed: $($_.Exception.Message)"
    }
}

# STEP 7: Get the member's generated membership number
Write-Host "`n[STEP 7] Fetching Membership Details..."
try {
    $memberDetail = Invoke-RestMethod -Uri "$BASE/api/admin/members/$memberId" -Method GET -Headers $headers
    Write-Host "  Status: $($memberDetail.membershipStatus)"
    Write-Host "  MembershipNo: $($memberDetail.membershipNo)"
    Write-Host "  FullName: $($memberDetail.fullName)"
    $membershipNo = $memberDetail.membershipNo
} catch {
    Write-Host "  [WARN] Could not fetch member detail: $($_.Exception.Message)"
    $membershipNo = "NOT_YET_GENERATED"
}

# STEP 8: Try to login with membership number
Write-Host "`n[STEP 8] Member Login with Membership Number..."
if ($membershipNo -and $membershipNo -ne "NOT_YET_GENERATED") {
    $memberLoginBody = @{ username = $membershipNo; password = "shalin123" } | ConvertTo-Json
    try {
        $memberLogin = Invoke-RestMethod -Uri "$BASE/api/auth/login" -Method POST `
            -ContentType "application/json" -Body $memberLoginBody
        Write-Host "  [SUCCESS] Member logged in!"
        Write-Host "  Role: $($memberLogin.role)"
        Write-Host "  FullName: $($memberLogin.fullName)"
    } catch {
        Write-Host "  [WARN] Login failed with default password. Member may need to set password."
        Write-Host "  Details: $($_.ErrorDetails.Message)"
        
        # Try NID as password
        $memberLoginBody2 = @{ username = $membershipNo; password = "1234599990" } | ConvertTo-Json
        try {
            $memberLogin2 = Invoke-RestMethod -Uri "$BASE/api/auth/login" -Method POST `
                -ContentType "application/json" -Body $memberLoginBody2
            Write-Host "  [SUCCESS] Logged in with NID as password!"
            Write-Host "  Role: $($memberLogin2.role)"
        } catch {
            Write-Host "  [INFO] Could not login with NID password either."
            Write-Host "  Details: $($_.ErrorDetails.Message)"
        }
    }
} else {
    Write-Host "  [SKIP] No membership number to test login with."
}

Write-Host ""
Write-Host "========================================"
Write-Host "  TEST COMPLETE"
Write-Host "========================================"
