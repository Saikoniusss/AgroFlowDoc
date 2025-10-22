using Elsa.EntityFrameworkCore;
using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
var builder = WebApplication.CreateBuilder(args);

// ----- Logging -----
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console());

// ----- Configuration -----
var connectionString = builder.Configuration.GetConnectionString("AgroFlowConnection");

// ----- Services -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core (наша БД)
builder.Services.AddDbContext<DocflowDbContext>(opt =>
    opt.UseSqlServer(connectionString));

// Elsa (workflow engine)
builder.Services.AddElsa(elsa =>
{
    elsa.UseWorkflowManagement(management =>
        management.UseEntityFrameworkCore(ef => ef.UseSqlServer(connectionString)));

    elsa.UseWorkflowRuntime(runtime =>
        runtime.UseEntityFrameworkCore(ef => ef.UseSqlServer(connectionString)));
});

// HealthChecks
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

// ----- App -----
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok("Docflow backend is running"));

app.Run();

// ----- Placeholder для контекста -----
public class DocflowDbContext : DbContext
{
    public DocflowDbContext(DbContextOptions<DocflowDbContext> options) : base(options) { }
}