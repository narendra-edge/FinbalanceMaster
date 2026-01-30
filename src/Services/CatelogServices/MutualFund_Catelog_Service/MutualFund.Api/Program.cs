using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MutualFund.Application.Interfaces.Services;
using MutualFund.Application.Mapper;
using MutualFund.Application.Services;
using MutualFund.Core.Interfaces;
using MutualFund.Infrastructure.Persistence;
using MutualFund.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// add configuration & db
builder.Services.AddDbContext<MutualFundDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("MutualFundDb"))
);

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// CORS
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mutual Fund API", Version = "v1" });
});

// Controllers
builder.Services.AddControllers();

// Register repositories & services
builder.Services.AddScoped<IAmfiSchemeRepository, AmfiSchemeRepository>();
builder.Services.AddScoped<IRtaSchemeDataRepository, RtaSchemeDataRepository>();
builder.Services.AddScoped<ISchemeMappingRepository, SchemeMappingRepository>();
builder.Services.AddScoped<ISchemeMasterFinalRepository, SchemeMasterFinalRepository>();

builder.Services.AddScoped<IAmfiService, AmfiService>();
builder.Services.AddScoped<IRtaSchemeDataService, RtaSchemeDataService>();
builder.Services.AddScoped<ISchemeMappingService, SchemeMappingService>();
builder.Services.AddScoped<ISchemeMasterFinalService, SchemeMasterFinalService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
