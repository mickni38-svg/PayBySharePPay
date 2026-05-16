using System.Text;
using Api.PayBySharePay.Auth;
using Api.PayBySharePay.Middleware;
using Api.PayBySharePay.Services;
using DataStorage.PayBySharePay.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Service.PayBySharePay.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<MerchantDemoHostedService>();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:4200", "https://localhost:4200",
                  "http://localhost:4201", "https://localhost:4201",
                  "http://localhost:8081", "https://localhost:8081",
                  "https://icy-water-0750d2703.7.azurestaticapps.net",
                  "https://paybysharepay.dk",
                  "https://www.paybysharepay.dk"
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── JWT ──────────────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key er ikke konfigureret.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<JwtTokenService>();

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PayBySharePay API",
        Version = "v1",
        Description = "API til fælles bestillinger og delt betaling"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Indsæt JWT token: Bearer {token}"
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Data + Services ───────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("PayBySharePayDb")
    ?? throw new InvalidOperationException("Connection string 'PayBySharePayDb' er ikke konfigureret.");

builder.Services.AddDataStorage(connectionString);
builder.Services.AddServiceLayer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PayBySharePay API v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("Frontend");

// UseHttpsRedirection er deaktiveret i dev – merchant-demo kører på http://localhost:8081
// og kan ikke følge redirect til self-signed HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
