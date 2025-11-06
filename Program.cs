using Mapster;
using Microsoft.EntityFrameworkCore;
using QuizApi.DAL;
using QuizApi.MapsterConfig.cs;
using Serilog;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

// ===== Configuration & Logging =====
// (Add any configuration or logging setup here if needed)


// ===== Database Services =====
builder.Services.AddDbContext<QuizDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:QuizDbContextConnection"]);
});

// ===== Application Services =====
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

// ===== API Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== Third-Party Services =====
MapsterConfig.Configure();
builder.Services.AddMapster();

var app = builder.Build();

// ===== Development Configuration =====
if (app.Environment.IsDevelopment())
{
    DBInit.Seed(app);
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ===== Middleware Pipeline =====
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseAuthentication();
app.MapControllers();

app.Run();