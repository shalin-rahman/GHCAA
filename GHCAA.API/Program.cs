using System.Text;
using GHCAA.Application;
using GHCAA.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using GHCAA.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Register layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddAppAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDirectoryBrowser();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // serve wwwroot/uploads
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
