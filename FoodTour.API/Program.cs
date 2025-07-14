using FoodTour.API.Data;
using FoodTour.API.Models;
using FoodTour.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// 💾 Database
builder.Services.AddDbContext<FoodTourDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyCnn")));

// 🔐 Momo Settings
builder.Services.Configure<MomoSettings>(
    builder.Configuration.GetSection("MomoSettings"));

// 📦 Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🧠 Swagger (add only ONCE)
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// 🧠 Http Clients & Services
builder.Services.AddHttpClient<CohereAIService>()
    .AddTypedClient<CohereAIService>((http, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new CohereAIService(http, config);
    });
builder.Services.AddHttpClient<GoogleGeocodingService>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddTransient<MomoService>();
builder.Services.AddHttpClient<PlaceController>();
builder.Services.AddHttpClient();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddHttpClient<GeminiService>();

// 🔐 JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// 🌐 CORS (Frontend Vercel domain)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://eataround.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ✅ Order matters: UseCors must be BEFORE Auth
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// 📑 Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// 🚫 Optional: disable HTTPS redirection if not needed
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
