using System.Security.Claims;
using System.Text;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizApi.DAL;
using QuizApi.MapsterConfig.cs;
using QuizApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ItemDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ItemDbContextConnection' not found.");

// ===== Configuration & Logging =====


// Configures Swagger (OpenAPI) for API and enable JWT authentication within the Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Quiz API",
        Version = "v1"
    });

    // Add JWT Auth
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



// ===== Database Services =====
builder.Services.AddDbContext<QuizDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:QuizDbContextConnection"]);
});



builder.Services.AddIdentity<AuthUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<QuizDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Allow requests from this origin (React frontend)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// ===== Authentication Services =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // Set to true in production
        options.TokenValidationParameters = new TokenValidationParameters() // The validation rules for the token.
        {
            ValidateIssuer = true, // Validate that the token was issued by a trusted issuer
            ValidateAudience = true, // Validate the token recipient
            ValidateLifetime = true, // Validate that the token is not expired
            ValidateIssuerSigningKey = true, // Validate the JWT signature
            ValidAudience = builder.Configuration["Jwt:Audience"], // Reading the recipient of the token
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Reading the issuer of the token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration."))), // Reading the secret key used to sign the token
            RoleClaimType = ClaimTypes.Role // User role information
        };
        options.Events = new JwtBearerEvents // Debugging tools for JWT authentication process 
        {
            OnMessageReceived = context => // Triggered when a message is received, logs the token,useful for checking if the token is being sent correctly by the client
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
            Console.WriteLine($"OnMessageReceived – Token: {token}");
            return Task.CompletedTask;
        },
            OnTokenValidated = context => // Triggered when a token is successfully validated
            {
                Console.WriteLine("OnTokenValidated: SUCCESS");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context => // Triggered when authentication fails for any reason 
            {
                Console.WriteLine($"OnAuthenticationFailed: {context.Exception.Message}");
                Console.WriteLine($"Exception Type: {context.Exception.GetType().Name}");
                if (context.Exception.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {context.Exception.InnerException.Message}");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context => // Triggered when an unauthenticated request attempts to access an authorized resource. Send appropriate error message back to the client (401)
            {
                Console.WriteLine($"OnChallenge: {context.Error} – {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();



// ===== Application Services =====
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddDistributedMemoryCache(); // required backing store for session middleware
builder.Services.AddSession();// used for authentication and authorisation

// ===== API Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== Third-Party Services =====
MapsterConfig.Configure();

var app = builder.Build();

// ===== Development Configuration =====
if (app.Environment.IsDevelopment())
{
    await DBInit.Seed(app);
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===== Create Default Roles and Admin User =====
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();

    string[] roleNames = { "Admin", "User" };

    foreach (var name in roleNames)
        if (!await roleManager.RoleExistsAsync(name))
            await roleManager.CreateAsync(new IdentityRole(name));

    var admin = await userManager.FindByNameAsync("admin");
    if (admin == null)
    {
        admin = new AuthUser { UserName = "admin", Email = "dansetorget@gmail.com" };
        await userManager.CreateAsync(admin, "Dansetorget123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// ===== Middlewares (order is important!) =====
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");

// Custom middleware that manually inspect the JWT token for debugging purposes
// If you encounter bugs, you can comment this in

/*app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        var headerValue = authHeader.FirstOrDefault();
        if (headerValue?.StartsWith("Bearer ") == true)
        {
            var token = headerValue.Substring("Bearer ".Length).Trim();

            // Decode the token to see its contents (without verification)
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            Console.WriteLine($"--> Token Issuer: {jsonToken.Issuer}");
            Console.WriteLine($"--> Token Audience: {jsonToken.Audiences.FirstOrDefault()}");
            Console.WriteLine($"--> Token Expiry: {jsonToken.ValidTo}");
            Console.WriteLine($"--> Current Time: {DateTime.UtcNow}");
            Console.WriteLine($"--> Config Issuer: {builder.Configuration["Jwt:Issuer"]}");
            Console.WriteLine($"--> Config Audience: {builder.Configuration["Jwt:Audience"]}");
        }
    }

    await next.Invoke();
});*/

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
