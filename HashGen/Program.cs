using System;
using BCrypt.Net;
string hash = BCrypt.Net.BCrypt.HashPassword("SuperAdminPassword123!", 11);
Console.WriteLine(hash);
