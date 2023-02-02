using System.ComponentModel.DataAnnotations.Schema;
using ElectricCarService.Controllers;
using Microsoft.EntityFrameworkCore;

namespace ElectricCarService.Data;

public class ElectricCarServiceContext: DbContext
{
    public DbSet<ChargingStationEntity> ChargingStations { get; set; } = null!;
    public DbSet<TransactionEntity> Transactions { get; set; } = null!;
    public DbSet<ChargeEntity> Charges { get; set; } = null!;
    public DbSet<ElectricRateEntity> ElectricRate { get; set; } = null!;
    public DbSet<CompanyPriceEntity> CompanyPrice { get; set; } = null!;

    public ElectricCarServiceContext(DbContextOptions<ElectricCarServiceContext> options): base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChargingStationEntity>().HasIndex(_ => _.Identifier).IsUnique();
        modelBuilder.Entity<ChargingStationEntity>().Property(_ => _.Status).HasConversion<int>();
        modelBuilder.Entity<TransactionEntity>().Property(_ => _.Status).HasConversion<int>();
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
    public List<TransactionEntity> Transactions  { get; set; } = null!;
}

public enum ChargingStationStatus
{
    Available = 1,
    Charging = 2,
    Suspended = 3
}

[Table("Transaction")]
public class TransactionEntity
{
    public int Id { get; set; }
    public ChargingStationEntity ChargingStation { get; set; } = null!;
    public double CompanyFlatCharge { get; set; }
    public double CompanyRate { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset StopTime { get; set; }
    public List<ChargeEntity> Charges  { get; set; } = null!;
}

public enum TransactionStatus
{
    Charging = 1,
    Suspended = 2,
    Finished = 3
}

[Table("Charge")]
public class ChargeEntity
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public double ChargeAmount { get; set; }
    public TransactionEntity Transaction { get; set; } = null!;
}

[Table("ElectricRate")]
public class ElectricRateEntity
{
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public double Value { get; set; }
}

[Table("CompanyPrice")]
public class CompanyPriceEntity
{
    public int Id { get; set; }
    public double FlatCharge { get; set; }
    public double Rate { get; set; }
}





























