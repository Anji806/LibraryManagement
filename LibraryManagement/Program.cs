using LibraryManagement.Data;
using LibraryManagement.Exceptions;
using LibraryManagement.Helpers;
using LibraryManagement.Repositories.Implementations;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Implementations;
using LibraryManagement.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// ==================================================
// CONTROLLERS
// ==================================================

// Avoid JSON serialization errors caused by reference cycles
// (e.g., Author -> Books -> Author). Configure the System.Text.Json
// serializer to ignore cycles when writing responses.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


// ==================================================
// DATABASE
// ==================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));


// ==================================================
// REPOSITORIES
// ==================================================

builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<
    IBorrowTransactionRepository,
    BorrowTransactionRepository>();


// ==================================================
// SERVICES
// ==================================================

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IBorrowService, BorrowService>();


// ==================================================
// JWT TOKEN GENERATOR
// ==================================================

builder.Services.AddScoped<JwtTokenGenerator>();


// ==================================================
// JWT AUTHENTICATION
// ==================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT Key is missing from appsettings.json.");
        }

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(key))
            };
    });


// ==================================================
// AUTHORIZATION
// ==================================================

builder.Services.AddAuthorization();


// ==================================================
// SWAGGER
// ==================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter your JWT token. Example: Bearer {your-token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


// ==================================================
// BUILD APPLICATION
// ==================================================

var app = builder.Build();


// ==================================================
// SWAGGER
// ==================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ==================================================
// HTTPS
// ==================================================

app.UseHttpsRedirection();


// ==================================================
// GLOBAL EXCEPTION HANDLER
// ==================================================

app.UseMiddleware<GlobalExceptionHandler>();


// ==================================================
// AUTHENTICATION
// ==================================================

app.UseAuthentication();


// ==================================================
// AUTHORIZATION
// ==================================================

app.UseAuthorization();


// ==================================================
// MAP CONTROLLERS
// ==================================================

app.MapControllers();


// ==================================================
// RUN APPLICATION
// ==================================================

app.Run();