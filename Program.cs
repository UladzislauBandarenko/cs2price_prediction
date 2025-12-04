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

var builder = WebApplication.CreateBuilder(args);

// ============ 1. DbContext ДЛЯ ПРИЛОЖЕНИЯ (READ ONLY, cs2_user) ============

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

// ============ 2. Сервисы ============

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + схема для admin API key
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CS2 Price Prediction API",
        Version = "v1"
    });

    // Описание схемы авторизации по API-ключу
    c.AddSecurityDefinition("AdminApiKey", new OpenApiSecurityScheme
    {
        Description = "Admin API key. Введите значение для заголовка X-Admin-Token.",
        Name = "X-Admin-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    // Требование: Swagger будет подставлять этот ключ во все запросы
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

builder.Services.AddHttpClient("MlService", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var mlUrl =
        config["ML_SERVICE_URL"] ??
        config["MlService:BaseUrl"] ??
        "http://localhost:8000";

    client.BaseAddress = new Uri(mlUrl);
});

// Основные (публичные) сервисы
builder.Services.AddScoped<IStickerService, StickerService>();
builder.Services.AddScoped<IMetaService, MetaService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();

// Фабрика админского контекста
builder.Services.AddScoped<IAdminDbContextFactory, AdminDbContextFactory>();

// Admin CRUD сервисы
builder.Services.AddScoped<IAdminStickerService, AdminStickerService>();
builder.Services.AddScoped<IAdminWeaponTypeService, AdminWeaponTypeService>();
builder.Services.AddScoped<IAdminWeaponService, AdminWeaponService>();
builder.Services.AddScoped<IAdminSkinService, AdminSkinService>();
builder.Services.AddScoped<IAdminWearTierService, AdminWearTierService>();
builder.Services.AddScoped<IAdminSkinWearTierService, AdminSkinWearTierService>();

// DbSeeder
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

// ============ 3. MIGRATIONS + SEED ПОД АДМИНОМ (cs2_admin) ПРИ ЗАПУСКЕ ============

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
        // миграции под админом
        await adminDb.Database.MigrateAsync();

        // сидирование под админом
        var seeder = new DbSeeder(adminDb, logger, env);
        await seeder.SeedAsync();
    }
}

// ============ 4. Обычный запуск API (READ ONLY, cs2_user) ============

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// сначала режем неавторизованные запросы к /api/admin/*
app.UseMiddleware<AdminApiKeyMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.Run();
