using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.API.Auth;
using Movies.API.Health;
using Movies.API.Middlewares;
using Movies.Application.Database;
using Movies.Application.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true
    };
});

builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy(AuthConstants.Policies.AdminUser, policy => policy.RequireClaim(AuthConstants.Claims.AdminUser, "true")); // Only auths an admin user through JWT
    options.AddPolicy(AuthConstants.Policies.AdminUser, policy => policy.AddRequirements(new AdminAuthRequirement(builder.Configuration["ApiKey"]!))); // Auths an admin user through JWT or API Key

    options.AddPolicy(AuthConstants.Policies.TrustedMember, policy => policy.RequireAssertion(context =>
        context.User.HasClaim(match => match is { Type: AuthConstants.Claims.AdminUser, Value: "true" }) ||
        context.User.HasClaim(match => match is { Type: AuthConstants.Claims.TrustedMember, Value: "true" })
    ));
});

// Add services to the container.
builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(pb => pb.Cache());
    options.AddPolicy("MoviesCache", x =>
    {
        x.Cache()
        .Expire(TimeSpan.FromMinutes(1))
        .SetVaryByQuery(new[] { "title", "year", "sortBy", "page", "pageSize" })
        .Tag("movies");
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddApplication()
    .AddDatabase(builder.Configuration["Database:ConnectionString"]!);

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("_health");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

if (app.Environment.IsDevelopment())
{
    await dbInitializer.SeedAsync();
}

app.Run();
