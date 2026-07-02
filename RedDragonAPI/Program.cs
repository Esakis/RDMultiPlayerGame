using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja MSSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Serwisy
builder.Services.AddScoped<IKingdomService, KingdomService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IMilitaryService, MilitaryService>();
builder.Services.AddScoped<IBattleService, BattleService>();
builder.Services.AddScoped<ITurnService, TurnService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IGeneralService, GeneralService>();
builder.Services.AddScoped<IPactService, PactService>();
builder.Services.AddScoped<IMarketService, MarketService>();
builder.Services.AddScoped<ILabyrinthService, LabyrinthService>();
builder.Services.AddScoped<IDragonService, DragonService>();
builder.Services.AddSingleton<JwtHelper>();

// Background service dla przeliczenia
builder.Services.AddHostedService<DailyResetService>();

// JWT Authentication
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// CORS dla Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("http://localhost:4200", "http://localhost:4201")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate w development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Seed: konto super admina i domyślna opłata za księstwo (30 zł)
    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        db.Users.Add(new RedDragonAPI.Models.Entities.User
        {
            Email = "admin@reddragon.pl",
            Username = "SuperAdmin",
            PasswordHash = PasswordHasher.Hash("Admin123!"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });
    }
    if (!db.GameSettings.Any(s => s.Key == RedDragonAPI.Models.Entities.GameSetting.KingdomPriceKey))
    {
        db.GameSettings.Add(new RedDragonAPI.Models.Entities.GameSetting
        {
            Key = RedDragonAPI.Models.Entities.GameSetting.KingdomPriceKey,
            Value = RedDragonAPI.Models.Entities.GameSetting.DefaultKingdomPrice
                .ToString(System.Globalization.CultureInfo.InvariantCulture)
        });
    }
    db.SaveChanges();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
