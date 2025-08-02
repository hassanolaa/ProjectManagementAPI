
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models.Configuration;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Services.Implementations;
using TaskManagementAPI.Services.Interfaces;
using System.Threading.RateLimiting;
using TaskManagementAPI.Middleware;
using Microsoft.AspNetCore.RateLimiting;


var builder = WebApplication.CreateBuilder(args);





// Add services to the container.

// Configure strongly typed settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<RateLimitingSettings>(builder.Configuration.GetSection("RateLimiting"));

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders().AddRoles<IdentityRole>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings!.Key);

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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Rate Limiting Configuration
var rateLimitSettings = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingSettings>();

builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter - applies to all endpoints by default
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetPartitionKey(httpContext),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = rateLimitSettings?.GlobalLimitPerMinute ?? 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Authentication endpoints - strict rate limiting
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = rateLimitSettings?.AuthLimitPer15Minutes ?? 10;
        opt.Window = TimeSpan.FromMinutes(15);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    // Password reset - very strict rate limiting
    options.AddFixedWindowLimiter("PasswordResetPolicy", opt =>
    {
        opt.PermitLimit = rateLimitSettings?.PasswordResetLimitPerHour ?? 3;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueLimit = 0;
    });

    // Task creation - moderate rate limiting
    options.AddFixedWindowLimiter("TaskCreationPolicy", opt =>
    {
        opt.PermitLimit = rateLimitSettings?.TaskCreationLimitPerMinute ?? 50;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 2;
    });

    // File upload - strict rate limiting
    options.AddFixedWindowLimiter("FileUploadPolicy", opt =>
    {
        opt.PermitLimit = rateLimitSettings?.FileUploadLimitPerMinute ?? 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Read operations - lenient rate limiting
    options.AddFixedWindowLimiter("ReadPolicy", opt =>
    {
        opt.PermitLimit = 200;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 5;
    });

    // Configure rejection response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] = retryAfter.TotalSeconds.ToString();
        }

        context.HttpContext.Response.Headers["X-RateLimit-Policy"] = context.HttpContext.GetEndpoint()?.Metadata
            .GetMetadata<EnableRateLimitingAttribute>()?.PolicyName ?? "Global";

        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", 
            cancellationToken);
    };
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // React app
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// Register application services
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Create roles if they don't exist
        string[] roles = { "User", "Admin", "Manager", "TeamLead" };

        foreach (string roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    logger.LogInformation("Role {RoleName} created successfully", roleName);
                }
                else
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Role {RoleName} already exists", roleName);
            }
        }

        // Create default admin user (optional)
        const string adminEmail = "admin@taskmanagement.com";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Default admin user created successfully");
            }
        }

        logger.LogInformation("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Global exception handling in production
    app.UseMiddleware<GlobalExceptionMiddleware>();
}
app.UseHttpsRedirection();


app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Security Headers Middleware
// app.Use(async (context, next) =>
// {
//     context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
//     context.Response.Headers.Add("X-Frame-Options", "DENY");
//     context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
//     context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
//     if (!app.Environment.IsDevelopment())
//     {
//         context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
//     }
    
//     await next();
// });

app.UseCors("AllowSpecificOrigins");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



static string GetPartitionKey(HttpContext context)
{
    // Use user ID if authenticated, otherwise use IP address
    var userId = context.User?.Identity?.Name;
    if (!string.IsNullOrEmpty(userId))
    {
        return $"user_{userId}";
    }
    
    var ipAddress = context.Connection.RemoteIpAddress?.ToString();
    return $"ip_{ipAddress ?? "unknown"}";
}