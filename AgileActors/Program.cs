using AgileActors.Models;
using AgileActors.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the API aggregation services
builder.Services.AddScoped<IAggregatorService, WeatherService>();
builder.Services.AddScoped<IAggregatorService, SpotifyService>();
builder.Services.AddScoped<IAggregatorService, NewsService>();

// Add CORS policy for frontend and dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(
                "https://www.pc-soft.gr",
                "http://www.pc-soft.gr",
                "https://pc-soft.gr",
                "http://pc-soft.gr",
                "https://localhost:5001",
                "http://localhost:5000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Enable Swagger and Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.ConfigObject.AdditionalItems["withCredentials"] = false;
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

// Use CORS before authorization
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
