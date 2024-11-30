using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;

namespace MinimalApi.Infra.Db;

public class DbContextApp : DbContext 
{
    private readonly IConfiguration _configurationAppSettings;

    public DbContextApp(IConfiguration configurationAppSettings) {
        _configurationAppSettings = configurationAppSettings;
    }
    public DbSet<Admin> Admins {get; set;} = default!;
    public DbSet<Vehicle> Vehicles  {get; set;} = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>().HasData(
            new Admin
            {
                Id = 1,
                Email = "admin@test.com",
                Password = "pass1234",
                Role = "admin"
            }
            );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var connectionString = _configurationAppSettings.GetConnectionString("mysql")?.ToString();
            if(!string.IsNullOrEmpty(connectionString)) {
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        }
    }
}