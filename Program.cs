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
using PortfolioPro.Core.DTOs;
using PortfolioPro.Validators;
using System.Security.Claims;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var builder = WebApplication.CreateBuilder(args);
/** This program will have some great juice wrld level
information in it at some point. But you gotta treat
it like biscuits and gravy and just savor the code
for now <3**/

// --- Dependency Injection Container ---

// Register the DB factory as a singleton (one instance for the app's lifetime)
builder.Services.AddSingleton<DbConnectionFactory>();
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseServiceKey = builder.Configuration["Supabase:ApiKey"]; // Use the secret key here

builder.Services.AddSingleton(provider =>
    new Supabase.Client(supabaseUrl, supabaseServiceKey, new Supabase.SupabaseOptions
    {
        AutoConnectRealtime = true
    }));

// Register repositories and services with scoped lifetime (new instance per web request)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IStorageService, sbstorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Register HttpClient for external API calls (like supabase storage)
builder.Services.AddHttpClient();

// --- Security & Authentication Configuration ---

// Configure JWT Bearer authentication to protect our API routes
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Verify that the token was signed by our secret key.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            // Ensure the token came from our server (Issuer) and is meant for our app (Audience). 
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // Reject tokens that have expired.
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            // Map standard JWT claims to .NET User Identities.
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
    });

// Enable the Authorization middleware.
builder.Services.AddAuthorization();

// Automatically find and register all FluentValidation classes in the project.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Standardize error responses for the API
builder.Services.AddProblemDetails();

builder.Services.Configure<SupabaseOptions>(
    builder.Configuration.GetSection(SupabaseOptions.SectionName));

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // This creates the interactive webpage
}

// Enable the app to recognize WHO the user is based on the JWT
app.UseAuthentication();

// Enable the app to decide WHAT the user can do
app.UseAuthorization();

// --- Route Mapping ---

// Register the minimal API endpoint groups we defined in our Endpoint files
app.MapUserEndpoints();
app.MapProjectEndpoints();

// Root health-check endpoint
app.MapGet("/", () => "PortfolioPro API is running smoothly.");

// Start the web server.
app.Run();