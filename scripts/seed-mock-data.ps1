# GHCAA Real-like Data Seeding Script
# This script populates the database with realistic alumni data for testing purposes

$baseUrl = "https://localhost:7214/api"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

Write-Host "--- GHCAA Mock Data Seeding ---" -ForegroundColor Cyan

$names = @("Arif Hasan", "Sultana Ahmed", "Tanvir Rahman", "Mousumi Akter", "Kamrul Islam", "Ziaul Haque", "Nasrin Sultana", "Golam Sarwar", "Farzana Yeasmin", "Mahbub Alam")
$subject = @("Science", "Commerce", "Arts")
$sectors = @("IT", "Finance", "Government", "Business", "Teaching")

foreach ($name in $names) {
    $email = $name.Replace(" ", "").ToLower() + "@example.com"
    $body = @{
        fullName = $name
        email = $email
        mobileNo = "017" + (Get-Random -Minimum 10000000 -Maximum 99999999)
        nid = (Get-Random -Minimum 1000000000 -Maximum 9999999999).ToString()
        fatherName = "Late " + $name.Split(" ")[1]
        motherName = "Mrs. Ahmed"
        dateOfBirth = "1985-05-15"
        gender = 0 # Male
        bloodGroup = 2 # B+
        presentAddress = "Dhaka, Bangladesh"
        permanentAddress = "Comilla, Bangladesh"
        emergencyContactName = "Brother"
        emergencyContactRelation = "Brother"
        emergencyContactPhone = "01800000000"
        ghcLastCertificatePassingYear = (Get-Random -Minimum 2000 -Maximum 2015)
        subjectGroup = $subject[(Get-Random -Maximum 3)]
        professionalSector = $sectors[(Get-Random -Maximum 5)]
        designation = "Senior Professional"
        membershipType = 2 # General
    }

    $json = $body | ConvertTo-Json
    Write-Host "Registering $name..." -NoNewline
    try {
        Invoke-RestMethod -Uri "$baseUrl/account/register" -Method Post -Body $json -ContentType "application/json"
        Write-Host " [OK]" -ForegroundColor Green
    } catch {
        Write-Host " [FAILED]" -ForegroundColor Red
    }
}

Write-Host "`nSeeding complete. Use Admin panel to approve these members." -ForegroundColor Cyan
