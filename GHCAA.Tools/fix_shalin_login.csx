#!/usr/bin/env dotnet-script
// Quick diagnostic + fix for shalin login failure
// Run with: dotnet run --project GHCAA.API (checks logs)

// This is a standalone script - run via dotnet ef or direct DB query
// The actual fix is applied below via EF migration seed update

using System;
using BCrypt.Net;

// Generate a fresh known-good hash for "shalin" password
var testPassword = "Shalin@2024!"; 
var hash = BCrypt.Net.BCrypt.HashPassword(testPassword, 11);
Console.WriteLine($"Fresh hash for '{testPassword}':");
Console.WriteLine(hash);

// Verify the seed hash from users.json works
var seedHash = "$2a$11$CQ4KnTDZ7qUQMNre86iruOpgOx8fEoMe2G3RF/1U4cCLa5ltYtE1O";
Console.WriteLine($"\nSeed hash verifies 'Shalin@2024!': {BCrypt.Net.BCrypt.Verify(testPassword, seedHash)}");
Console.WriteLine($"Seed hash verifies 'SuperAdminPassword123!': {BCrypt.Net.BCrypt.Verify("SuperAdminPassword123!", seedHash)}");
Console.WriteLine($"Seed hash verifies 'shalin': {BCrypt.Net.BCrypt.Verify("shalin", seedHash)}");
Console.WriteLine($"Seed hash verifies 'Shalin123!': {BCrypt.Net.BCrypt.Verify("Shalin123!", seedHash)}");
Console.WriteLine($"Seed hash verifies 'Admin@123': {BCrypt.Net.BCrypt.Verify("Admin@123", seedHash)}");
Console.WriteLine($"Seed hash verifies 'password': {BCrypt.Net.BCrypt.Verify("password", seedHash)}");
Console.WriteLine($"Seed hash verifies 'Password@123': {BCrypt.Net.BCrypt.Verify("Password@123", seedHash)}");
