using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductManagement.API.Middleware;
using ProductManagement.Core.Interfaces;
using ProductManagement.Infrastructure.Data;
using ProductManagement.Infrastructure.Repositories;
using ProductManagement.Infrastructure.Services;
using Serilog;
using Serilog.Events;

// 1. Initialize Serilog Logger (Console + Rolling File Sink)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Product Management API web host...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog as the logging provider
    builder.Host.UseSerilog();

    // 2. Add Database Context (SQLite)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=products.db";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    // 3. Register Repositories and Services in DI Container
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();

    // 4. Configure JWT Authentication
    var jwtSecret = builder.Configuration["JwtSettings:Secret"] 
        ?? "SuperSecretKeyForProductManagementApiAssessment2026!#*$";
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ProductManagementAPI";
    var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "ProductManagementClients";

    builder.Services.AddAuthentication(options =>
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
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // 5. Add Controllers
    builder.Services.AddControllers();

    // 6. Configure Swagger with JWT Bearer Support
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Product Management API",
            Version = "v1",
            Description = "ASP.NET Core Web API with JWT Authentication, Product Management, Serilog, and Global Exception Handling."
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer' [space] and then your valid JWT token.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI...'"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                Array.Empty<string>()
            }
        });
    });

    // 7. Add CORS for Frontend integration
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

    // 8. Custom Global Exception Middleware (Must be before any other middleware)
    app.UseMiddleware<ExceptionMiddleware>();

    // 9. Serilog HTTP Request Logging
    app.UseSerilogRequestLogging();

    // 10. Initialize & Migrate Database on Startup
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<AppDbContext>();
        await DbInitializer.InitializeAsync(context, logger);
    }

    // 11. Configure HTTP Request Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Product Management API Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
