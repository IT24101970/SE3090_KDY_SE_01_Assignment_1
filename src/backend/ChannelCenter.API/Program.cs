using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ChannelCenter.API.Data;
using ChannelCenter.API.Services.Admin;
using ChannelCenter.API.Services.Appointment;
using ChannelCenter.API.Services.Auth;
using ChannelCenter.API.Services.IntakeAgent;
using ChannelCenter.API.Services.Patient;
using ChannelCenter.API.Services.SafetyAuditor;
using ChannelCenter.API.Services.Triage;
using ChannelCenter.API.Services.DoctorScheduling;

var builder = WebApplication.CreateBuilder(args);

// ── Connection String ────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=channel_center_db;Username=postgres;Password=postgres";

// ── Core MVC & Routing ───────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
                  string.IsNullOrEmpty(origin) ||
                  new Uri(origin).Host == "localhost" ||
                  new Uri(origin).Host == "127.0.0.1")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey  = jwtSection["SecretKey"] ?? "ChannelCenterDevSecret_MustBe32CharsOrMore!";
var issuer     = jwtSection["Issuer"]    ?? "ChannelCenterAPI";
var audience   = jwtSection["Audience"] ?? "ChannelCenterClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "ChannelCenter API",
        Version     = "v1",
        Description = "AI-Integrated Hospital Channeling System – Backend API"
    });

    // Add Bearer token support button in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });

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

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── Patient Management & Appointment Lifecycle (Component 1) ─────────────────
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IIntakeAgentService, IntakeAgentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── Medical Triage & Specialist Matching Service Layer (Component 3) ─────────
builder.Services.AddScoped<ITriageService, TriageService>();

// ── Student 2: Doctor Scheduling & Consultation Management Service ─────────────
builder.Services.AddScoped<IDoctorSchedulingService, DoctorSchedulingService>();

// ── Admin Service Layer (Component 4) ─────────────────────────────────────────
builder.Services.AddScoped<IAgentWorkflowService, AgentWorkflowService>();
builder.Services.AddScoped<IAdminOverrideService, AdminOverrideService>();
builder.Services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISafetyAuditorReadService, SafetyAuditorReadService>();

// Internal Safety Auditor calls use a separate shared secret, never a browser JWT.
builder.Services.Configure<SafetyAuditorOptions>(
    builder.Configuration.GetSection(SafetyAuditorOptions.SectionName));
builder.Services.AddHttpClient<ISafetyAuditorService, SafetyAuditorService>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<SafetyAuditorOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 1, 60));
    if (!string.IsNullOrWhiteSpace(options.InternalServiceKey))
    {
        client.DefaultRequestHeaders.Add("X-Internal-Service-Key", options.InternalServiceKey);
    }
});

// ── Build App ─────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed database with initial data (development only, idempotent)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedAsync(db);
}

app.Run();