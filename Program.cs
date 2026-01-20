using FluentValidation;
using PortfolioPro.Data;
using PortfolioPro.Endpoints;
using PortfolioPro.Repositories;
using PortfolioPro.Services;
using PortfolioPro.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using PortfolioPro.interfaces;
using PortfolioPro.Core.Services;

var builder = WebApplication.CreateBuilder(args);
/** This program will have some great juice wrld level
information in it at some point. But you gotta treat
it like biscuits and gravy and just savor the code
for now <3**/

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IStorageService, sbstorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            NameClaimType = "nameid",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();

var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();


app.MapUserEndpoints();
app.MapProjectEndpoints();

app.MapGet("/", () => "PortfolioPro API is running smoothly.");

app.Run();