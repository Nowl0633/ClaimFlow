using System.Text;
using ClaimFlow.Data;
using Login.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Policies.Controllers;
using Policies.Services;
using Quotes.Services;
using Registration.Controllers;
using Registration.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("ClaimFlow")));

builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

// frontend runs on 5200, has to match otherwise the browser blocks the requests
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// each service is its own project so we have to tell asp.net where to find the controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CustomersController).Assembly)
    .AddApplicationPart(typeof(Login.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(Quotes.Controllers.QuotesController).Assembly)
    .AddApplicationPart(typeof(PoliciesController).Assembly);

// without this the default validation error format is a nightmare to parse in javascript
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = new List<string>();

        foreach (var entry in ctx.ModelState)
        {
            if (entry.Value?.Errors.Count > 0)
            {
                foreach (var err in entry.Value.Errors)
                    errors.Add(err.ErrorMessage);
            }
        }

        return new BadRequestObjectResult(new { errors });
    };
});

builder.Services.AddEndpointsApiExplorer();

// this whole block is boilerplate so the lock icon in swagger actually works
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
