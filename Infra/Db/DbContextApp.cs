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