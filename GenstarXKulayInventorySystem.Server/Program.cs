using GenstarXKulayInventorySystem.Server;
using GenstarXKulayInventorySystem.Server.Mapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrEmpty(port))
{
    // Running on Railway or another PaaS
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container  
builder.Services.AddControllers();


// Enable CORS to allow calls from your Blazor WebAssembly client  
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
       policy.WithOrigins(
            "https://localhost:7035", // Dev client
            "https://genstarxkulayinventorysystem-production.up.railway.app", 
            "https://helpful-gentleness-production.up.railway.app" 
        )
              .AllowAnyMethod()
              .AllowAnyHeader());
});
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<InventoryDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt");
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

        RoleClaimType = ClaimTypes.Role, // ✅ Ensure this matches your JWT
        NameClaimType = ClaimTypes.Name
    };
});


builder.Services.AddAuthorization();

// Swagger/OpenAPI configuration  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Your API Title",
        Version = "v1"
    });
});


builder.Services.AddHttpContextAccessor();


// AutoMapper profile
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});


// Register DbContextFactory (with retry on failure) MSSQL
//builder.Services.AddDbContextFactory<InventoryDbContext>(options =>
//{
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        sqlOptions =>
//        {
//            sqlOptions.EnableRetryOnFailure();
//        });
//}, ServiceLifetime.Scoped);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convert DATABASE_URL to Npgsql format
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};Pooling=true;Trust Server Certificate=true;";
}
else
{
    // fallback to local
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContextFactory<InventoryDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    });
}, ServiceLifetime.Scoped);



builder.Services.AddHostedService<SalesHostedService>();
//Register Services
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
builder.Services.AddScoped<JwtService>();
    


var app = builder.Build(); // Build must happen BEFORE using app.Services

// Apply migrations to create/update database automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider; // 👈 this was missing

    var dbContext = services.GetRequiredService<InventoryDbContext>();
    dbContext.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await InventoryDbContext.SeedUserAsync(userManager, roleManager);

}



// Middleware pipeline  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Title v1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();

app.UseCors("AllowClient");

// 🔑 Add this line
app.UseAuthentication();

app.UseAuthorization();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.MapControllers();

app.Run();