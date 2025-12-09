using System;
using System.Collections.Generic;
using cs2price_prediction.Data;
using cs2price_prediction.Security;
using cs2price_prediction.Services.Admin.Stickers;
using cs2price_prediction.Services.Admin.WeaponTypes;
using cs2price_prediction.Services.Admin.Weapons;
using cs2price_prediction.Services.Admin.Skins;
using cs2price_prediction.Services.Admin.WearTiers;
using cs2price_prediction.Services.Admin.SkinWearTiers;
using cs2price_prediction.Services.Meta;
using cs2price_prediction.Services.Prediction;
using cs2price_prediction.Services.Stickers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// === AI / OpenAI usings ===
using cs2price_prediction.Config;
using cs2price_prediction.Services.AI.AiStickerService;
using cs2price_prediction.Services.AI.AiPromptService;
using cs2price_prediction.Services.AI.AiExplanation;
using cs2price_prediction.Services.AI.Llm;

var builder = WebApplication.CreateBuilder(args);

// ============ 1. DbContext FOR APPLICATION (READ ONLY, cs2_user) ============

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connStr =
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        builder.Configuration["ConnectionStrings:DefaultConnection"] ??
        builder.Configuration["ConnectionStrings__DefaultConnection"];

    if (string.IsNullOrWhiteSpace(connStr))
        throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    options.UseNpgsql(connStr);
});

// ============ 2. Services ============

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + schema for admin API key
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CS2 Price Prediction API",
        Version = "v1"
    });

    // Description of the API key authorization scheme
    c.AddSecurityDefinition("AdminApiKey", new OpenApiSecurityScheme
    {
        Description = "Admin API key. Enter the value for the X-Admin-Token header.",
        Name = "X-Admin-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    // Requirement: Swagger will send this key with all requests
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "AdminApiKey"
                },
                In = ParameterLocation.Header,
                Name = "X-Admin-Token"
            },
            new List<string>()
        }
    });
});

// HttpClient for ML service
builder.Services.AddHttpClient("MlService", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var mlUrl =
        config["ML_SERVICE_URL"] ??
        config["MlService:BaseUrl"] ??
        "http://localhost:8000";

    client.BaseAddress = new Uri(mlUrl);
});

// ============ OpenAI / AI layer ============

// 1) OpenAI settings from appsettings: section "OpenAI"
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var options = config.GetSection("OpenAI").Get<OpenAiOptions>() ?? new OpenAiOptions();
    return options;
});

// 2) LLM client (OpenAI) via HttpClient (typed client)
builder.Services.AddHttpClient<ILLMClient, OpenAiClient>();

// 3) Failover service (gpt-4o-mini <-> gpt-4.1-mini)
builder.Services.AddScoped<ILlmFailoverService, LlmFailoverService>();

// 4) AI stickers + prompts + explanations
builder.Services.AddScoped<IAiStickerService, AiStickerService>();
builder.Services.AddScoped<IAiPromptFactory, AiPromptFactory>();
builder.Services.AddScoped<IAiExplanationService, AiExplanationService>();

// Main (public) services
builder.Services.AddScoped<IStickerService, StickerService>();
builder.Services.AddScoped<IMetaService, MetaService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();

// Admin DbContext factory
builder.Services.AddScoped<IAdminDbContextFactory, AdminDbContextFactory>();

// Admin CRUD services
builder.Services.AddScoped<IAdminStickerService, AdminStickerService>();
builder.Services.AddScoped<IAdminWeaponTypeService, AdminWeaponTypeService>();
builder.Services.AddScoped<IAdminWeaponService, AdminWeaponService>();
builder.Services.AddScoped<IAdminSkinService, AdminSkinService>();
builder.Services.AddScoped<IAdminWearTierService, AdminWearTierService>();
builder.Services.AddScoped<IAdminSkinWearTierService, AdminSkinWearTierService>();

// DbSeeder
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

// ============ 3. MIGRATIONS + SEED UNDER ADMIN (cs2_admin) ON STARTUP ============

using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<DbSeeder>();

    var adminConnStr =
        config.GetConnectionString("AdminConnection") ??
        config["ConnectionStrings:AdminConnection"] ??
        config["ConnectionStrings__AdminConnection"];

    if (string.IsNullOrWhiteSpace(adminConnStr))
        throw new InvalidOperationException("Connection string 'AdminConnection' is not configured.");

    var adminOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    adminOptionsBuilder.UseNpgsql(adminConnStr);

    using (var adminDb = new AppDbContext(adminOptionsBuilder.Options))
    {
        // Run migrations using admin connection
        await adminDb.Database.MigrateAsync();

        // Seed initial data using admin connection
        var seeder = new DbSeeder(adminDb, logger, env);
        await seeder.SeedAsync();
    }
}

// ============ 4. Normal API startup (READ ONLY, cs2_user) ============

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// first reject unauthorized requests to /api/admin/*
app.UseMiddleware<AdminApiKeyMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
