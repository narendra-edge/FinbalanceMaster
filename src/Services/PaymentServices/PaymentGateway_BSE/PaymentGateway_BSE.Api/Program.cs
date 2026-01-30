using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1. Configure Services (DI container)
// --------------------------------------------------

// Controllers (API endpoints)
builder.Services.AddControllers();

// Swagger / OpenAPI config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mutual Fund API",
        Version = "v1",
        Description = "API for Mutual Fund Catalog (Schemes, NAVs, AMCs, etc.)"
    });
});

// Enable CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.AllowAnyOrigin()   // In prod: replace with React app domain
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Logging (console)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// --------------------------------------------------
// 2. Configure Middleware Pipeline
// --------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mutual Fund API v1");
        c.RoutePrefix = "swagger"; // Swagger at /swagger
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp"); // Enable CORS before auth

app.UseAuthorization();

app.MapControllers();

app.Run();
