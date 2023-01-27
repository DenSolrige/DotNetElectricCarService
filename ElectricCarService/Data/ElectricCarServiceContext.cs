using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ElectricCarService.Data;

public class ElectricCarServiceContext: DbContext
{
    public DbSet<ChargingStationEntity> ChargingStations { get; set; } = null!;

    public ElectricCarServiceContext(DbContextOptions<ElectricCarServiceContext> options): base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChargingStationEntity>().HasIndex(_ => _.Identifier).IsUnique();
        modelBuilder.Entity<ChargingStationEntity>().Property(_ => _.Status).HasConversion<int>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=dotnet;Username=myusername;Password=mypassword");
    }
}

[Table("ChargingStation")]
public class ChargingStationEntity
{
    public int Id { get; set; }
    public string Identifier { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int Number { get; set; }
    public ChargingStationStatus Status { get; set; }
}

public enum ChargingStationStatus
{
    AVAILABLE = 1,
    CHARGING = 2,
    SUSPENDED = 3
}