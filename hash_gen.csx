using BCrypt.Net;

// Generate a fresh BCrypt hash for SuperAdminPassword123!
string hash = BCrypt.Net.BCrypt.HashPassword("SuperAdminPassword123!", 11);
Console.WriteLine($"New BCrypt Hash: {hash}");
