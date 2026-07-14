using Microsoft.EntityFrameworkCore;

namespace APO_BOT.DemoApi.Data;

public sealed class DemoDbContext(DbContextOptions<DemoDbContext> options) : DbContext(options)
{
    public DbSet<SystemSettingEntity> SystemSettings => Set<SystemSettingEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<StockUnitEntity> StockUnits => Set<StockUnitEntity>();
    public DbSet<OutputEntity> Outputs => Set<OutputEntity>();
    public DbSet<MovementEntity> Movements => Set<MovementEntity>();
    public DbSet<DispensationEntity> Dispensations => Set<DispensationEntity>();
    public DbSet<AlertEntity> Alerts => Set<AlertEntity>();
    public DbSet<CameraEntity> Cameras => Set<CameraEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderLineEntity> OrderLines => Set<OrderLineEntity>();
    public DbSet<LoadWindowEntity> LoadWindows => Set<LoadWindowEntity>();
    public DbSet<LoadShelfEntity> LoadShelves => Set<LoadShelfEntity>();
    public DbSet<LoadSessionEntity> LoadSessions => Set<LoadSessionEntity>();
    public DbSet<LoadItemEntity> LoadItems => Set<LoadItemEntity>();
    public DbSet<HistoryRecordEntity> HistoryRecords => Set<HistoryRecordEntity>();
    public DbSet<ChartPointEntity> ChartPoints => Set<ChartPointEntity>();
    public DbSet<MachineCommandEntity> MachineCommands => Set<MachineCommandEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSettingEntity>().HasKey(entity => entity.Key);
        modelBuilder.Entity<ProductEntity>().HasIndex(entity => entity.Code).IsUnique();
        modelBuilder.Entity<StockUnitEntity>().HasIndex(entity => entity.SerialNumber).IsUnique();
        modelBuilder.Entity<StockUnitEntity>().HasIndex(entity => entity.ProductId);
        modelBuilder.Entity<MovementEntity>().HasIndex(entity => entity.OccurredAtUtc);
        modelBuilder.Entity<DispensationEntity>().HasIndex(entity => entity.CreatedAtUtc);
        modelBuilder.Entity<AlertEntity>().HasIndex(entity => new { entity.Resolved, entity.CreatedAtUtc });
        modelBuilder.Entity<OrderLineEntity>().HasIndex(entity => entity.OrderId);
        modelBuilder.Entity<LoadItemEntity>().HasIndex(entity => entity.SessionId);
        modelBuilder.Entity<HistoryRecordEntity>().HasIndex(entity => new { entity.Type, entity.OccurredAtUtc });
        modelBuilder.Entity<ChartPointEntity>().HasIndex(entity => new { entity.Series, entity.Period, entity.SortOrder });
    }
}
