using ConstructionJournal.Data;
using ConstructionJournal.Interfaces;
using ConstructionJournal.Services;
using ConstructionJournal.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using WebApp.ConstructionJournal.Data;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog для структурированного логирования
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Construction Journal API",
        Version = "v1",
        Description = "API для системы электронного журнала строительного контроля"
    });

    // Добавляем поддержку JWT в Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database Configuration
builder.Services.AddDbContext<ConstructionJournalDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString); // Для PostgreSQL
    // options.UseSqlServer(connectionString); // Для SQL Server
});

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ContractorOnly", policy =>
        policy.RequireRole("Contractor"));
    options.AddPolicy("ClientOnly", policy =>
        policy.RequireRole("Client"));
    options.AddPolicy("RegulatorOnly", policy =>
        policy.RequireRole("Regulator"));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Application Services
builder.Services.AddScoped<IWorkJournalService, WorkJournalService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IGeoLocationService, GeoLocationService>();

// LLM Services
builder.Services.AddScoped<ILLMAnalysisService, OpenAILLMService>();
builder.Services.AddScoped<IContractComplianceService, ContractComplianceService>();
builder.Services.AddScoped<IMaterialInspectionService, MaterialInspectionService>();

// File Storage Service
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();

// HttpClient for external APIs
builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>();
builder.Services.AddHttpClient<ILLMAnalysisService, OpenAILLMService>();

// Background Services for LLM processing
builder.Services.AddHostedService<LLMProcessingService>();

// Caching
builder.Services.AddMemoryCache();
// builder.Services.AddDistributedRedisCache(options => 
// {
//     options.Configuration = builder.Configuration["Redis:ConnectionString"];
// });

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ConstructionJournalDbContext>()
    .AddCheck<LLMHealthCheck>("llm_service");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Construction Journal API v1");
    });

    // Initialize development data
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ConstructionJournalDbContext>();
        context.Database.EnsureCreated();
        // await DbInitializer.Initialize(context);
    }
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Custom Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
