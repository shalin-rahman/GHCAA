@echo off
curl.exe -s -X POST http://localhost:5087/api/auth/login -H "Content-Type: application/json" -d "{\"username\":\"superadmin\",\"password\":\"SuperAdminPassword123!\"}"
echo.
echo Exit code: %errorlevel%
pause
