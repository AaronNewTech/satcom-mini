using Microsoft.EntityFrameworkCore;
using Satcom.Api.Domain;


namespace Satcom.Api;


public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
public DbSet<Satellite> Satellites => Set<Satellite>();
public DbSet<GroundStation> GroundStations => Set<GroundStation>();
public DbSet<Telemetry> Telemetries => Set<Telemetry>();


protected override void OnModelCreating(ModelBuilder modelBuilder)
{
modelBuilder.Entity<Satellite>(b =>
{
b.HasKey(x => x.Id);
b.Property(x => x.NoradId).HasMaxLength(16);
});


modelBuilder.Entity<GroundStation>(b =>
{
b.HasKey(x => x.Id);
b.HasIndex(x => new { x.Lat, x.Lon });
});


modelBuilder.Entity<Telemetry>(b =>
{
b.HasKey(x => x.Id);
b.HasIndex(x => new { x.SatelliteId, x.ReceivedAtUtc });
b.HasIndex(x => new { x.StationId, x.ReceivedAtUtc });
b.HasOne(x => x.Satellite).WithMany().HasForeignKey(x => x.SatelliteId);
b.HasOne(x => x.Station).WithMany().HasForeignKey(x => x.StationId);
});
}
}