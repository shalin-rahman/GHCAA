using System.Text;
using GHCAA.Application;
using GHCAA.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using GHCAA.API.Extensions;
using GHCAA.API.Middleware;
using GHCAA.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Register layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddAppAuthorization();

// Configure Request Limits from Settings
var maxBodySize = configuration.GetValue<long>("AppSettings:MaxRequestBodySize", 104857600);
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options => 
{
    options.Limits.MaxRequestBodySize = maxBodySize;
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = maxBodySize;
    x.MemoryBufferThreshold = (int)maxBodySize;
});

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDirectoryBrowser();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddScoped<GHCAA.Application.Interfaces.IRealTimeService, GHCAA.API.Services.RealTimeService>();

builder.Services.AddCors(options =>
{
    var allowedOrigins = configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularApp");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();

app.UseWebSockets();
app.UseStaticFiles(); // serve wwwroot/uploads
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GHCAA.API.Hubs.ChatHub>("/hubs/chat");
app.Run();
