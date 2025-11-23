
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Services;
using System.Security.Claims;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// ----- Logging -----
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console());

// ----- Configuration -----
var connectionString = builder.Configuration.GetConnectionString("AgroFlowConnection");
// ⚡ НАДЁЖНОЕ ОПРЕДЕЛЕНИЕ EF TOOLS
bool isEfTool = builder.Configuration["DOTNET_RUNNING_IN_PROJECT_TOOL"] == "true";
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:5173",  // если фронт на 5173
            "http://localhost:5174"   // если на 5174
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            NameClaimType = ClaimTypes.NameIdentifier // ← обязательно
        };
    });
builder.Services.AddAuthorization();
// ----- Services -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// EF Core (наша БД)
builder.Services.AddDbContext<DocflowDbContext>(opt =>
    opt.UseSqlServer(connectionString));

// Регистрируем TelegramBotService как HostedService (singleton)



//Services
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

builder.Services.AddHostedService<TelegramBotService>();

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddSingleton<TelegramNotificationWorker>();      // Worker как singleton
builder.Services.AddHostedService(sp => sp.GetRequiredService<TelegramNotificationWorker>());
builder.Services.AddTransient<ITelegramNotifier, TelegramNotifier>();


// ----- App -----
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var uploadRoot = builder.Configuration.GetValue<string>("FileStorage:BasePath")
                 ?? Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");

// Если путь относительный → делаем абсолютным
if (!Path.IsPathRooted(uploadRoot))
{
    uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), uploadRoot);
}

Directory.CreateDirectory(uploadRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads"
});

var avatarPath = builder.Configuration.GetValue<string>("FileStorage:AvatarPath")
                 ?? Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");

// Если путь относительный → делаем абсолютным
if (!Path.IsPathRooted(avatarPath))
{
    avatarPath = Path.Combine(Directory.GetCurrentDirectory(), avatarPath);
}
if (!Directory.Exists(avatarPath))
    Directory.CreateDirectory(avatarPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(avatarPath),
    RequestPath = "/avatars"
});



app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();




// сидирование
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DocflowDbContext>();
    db.Database.Migrate();
    await DbInitializer.SeedAsync(db);
}

app.MapGet("/", () => Results.Ok("Docflow backend is running"));

app.Run();