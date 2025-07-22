var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
        policy.WithOrigins("https://localhost:44332") // <-- Update to match your Blazor WASM client URL
        //policy.WithOrigins("https://app.mysite.com") // Production client domain example
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Swagger config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
