using GenstarXKulayInventorySystem.Server;
using GenstarXKulayInventorySystem.Server.Mapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Detect environment
Console.WriteLine($"Starting in environment: {builder.Environment.EnvironmentName}");

// Handle PaaS environments like Railway or MonsterASP
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7035",
                "http://genstar-kulay-inventory.runasp.net",
                "http://twodragon88.premiumasp.net",
                "http://twodragon88-corp.premiumasp.net",
                "https://twodragon88.shop")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<InventoryDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
});

builder.Services.AddAuthorization();

// Swagger (Development Only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Genstar XKulay Inventory API",
            Version = "v1"
        });
    });
}

builder.Services.AddHttpContextAccessor();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

// Password Policy
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

// SQL Server
builder.Services.AddDbContextFactory<InventoryDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure();
        });
}, ServiceLifetime.Scoped);

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.EnableDebugging = false;

// Hosted Services
var isSwaggerBuild = builder.Environment.IsEnvironment("SwaggerBuild");

if (!isSwaggerBuild)
{
    builder.Services.AddHostedService<SalesHostedService>();
}

// Scoped Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPurchaseOrderItemService, PurchaseOrderItemService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<ISaleItemService, SaleItemService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IDailySaleReportService, DailySaleReportService>();
builder.Services.AddScoped<IOperationsProviderService, OperationsProviderService>();
builder.Services.AddScoped<IPullOutRequestService, PullOutRequestService>();
builder.Services.AddScoped<IRequestItemsService, RequestItemsService>();
builder.Services.AddScoped<IStatementReportService, StatementReportService>();
builder.Services.AddScoped<IWayBillService, WayBillService>();
builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!)
    });

var app = builder.Build();

// Database Migration & Seeding
if (!isSwaggerBuild)
{
    using var scope = app.Services.CreateScope();

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        db.Database.Migrate();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await InventoryDbContext.SeedUserAsync(userManager, roleManager);

        Console.WriteLine("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Swagger Middleware (Development Only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Genstar XKulay Inventory API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseBlazorFrameworkFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();