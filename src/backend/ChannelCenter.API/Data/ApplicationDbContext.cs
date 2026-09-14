using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    // This property tells EF Core to create a 'Patients' table
    public DbSet<Patient> Patients { get; set; } 
}