using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.Services.DoctorScheduling;

var builder = WebApplication.CreateBuilder(args);

// Grab the connection string from
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the DbContext to use PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Student 2: Register Doctor Scheduling & Consultation Management Service
builder.Services.AddScoped<IDoctorSchedulingService, DoctorSchedulingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
