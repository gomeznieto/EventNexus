using System.Text;
using EventNexus.API.Middleware;
using EventNexus.Application.Interfaces;
using EventNexus.Infrastructure.Data;
using EventNexus.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// ------------------------------------------------------------ //

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();

// DBCONTEXT
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

// IDENTITY
builder.Services.AddIdentityCore<IdentityUser>(opt =>
{
    opt.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// AUTENTICACION Y JWT
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        // Validar que la firma (Key) sea correcta
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

        // Validar el Emisor
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        // Validar la Audiencia
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        // Validar la fecha de expiración (imprescindible)
        ValidateLifetime = true,

        // Opcional pero recomendado: quitar los 5 minutos de tolerancia que da .NET por defecto
        ClockSkew = TimeSpan.Zero
    };
});

// SERVICIOS
builder.Services.AddScoped<IAuthService, AuthService>();

// ------------------------------------------------------------ //

var app = builder.Build();

// MIDDLEWARES
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapHealthChecks("health");
app.MapControllers();

app.Run();

