using cs2price_prediction.Data;
using cs2price_prediction.Services.Stickers;
using cs2price_prediction.Services.Meta;
using cs2price_prediction.Services.Prediction;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// BD
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connStr =
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        builder.Configuration["ConnectionStrings:DefaultConnection"] ??
        builder.Configuration["ConnectionStrings__DefaultConnection"];

    if (string.IsNullOrWhiteSpace(connStr))
    {
        throw new InvalidOperationException("Connection string for database is not configured.");
    }

    options.UseNpgsql(connStr);
});

// Services

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("MlService", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var mlUrl =
        config["ML_SERVICE_URL"] ??
        config["MlService:BaseUrl"] ??
        "http://localhost:8000";

    client.BaseAddress = new Uri(mlUrl);
});

// StickerService from DB
builder.Services.AddScoped<IStickerService, StickerService>();
// MetaService
builder.Services.AddScoped<IMetaService, MetaService>();
// PredictionService
builder.Services.AddScoped<IPredictionService, PredictionService>();
// DbSeeder
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

// SEED BD

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
