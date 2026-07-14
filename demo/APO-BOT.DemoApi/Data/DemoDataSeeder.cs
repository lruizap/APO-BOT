using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace APO_BOT.DemoApi.Data;

public static class DemoDataSeeder
{
    public static async Task InitializeAsync(DemoDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        if (!await db.Products.AnyAsync(cancellationToken))
        {
            await SeedAsync(db, cancellationToken);
        }

        await EnsureStartupAlertAsync(db, cancellationToken);
    }

    public static async Task ResetAsync(DemoDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureDeletedAsync(cancellationToken);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await SeedAsync(db, cancellationToken);
    }

    private static async Task SeedAsync(DemoDbContext db, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var products = CreateProducts();
        var stockCounts = new Dictionary<string, int>
        {
            ["product-durogesic"] = 7,
            ["product-serc"] = 15,
            ["product-ibuhexal"] = 0,
            ["product-synofen"] = 21,
            ["product-zatril"] = 5,
            ["product-amoxicilina"] = 3
        };

        db.SystemSettings.AddRange(
            new SystemSettingEntity { Key = "pharmacyName", Value = "Farmacia Zaragoza" },
            new SystemSettingEntity { Key = "userDisplayName", Value = "Farmaceutica de demostracion" },
            new SystemSettingEntity { Key = "lightOn", Value = "true" },
            new SystemSettingEntity { Key = "priorityMode", Value = "dispense" },
            new SystemSettingEntity { Key = "machineStatus", Value = "running" },
            new SystemSettingEntity { Key = "capacityTotal", Value = "80" },
            new SystemSettingEntity { Key = "alertSoundEnabled", Value = "true" },
            new SystemSettingEntity { Key = "screenBrightness", Value = "85" },
            new SystemSettingEntity { Key = "refreshIntervalSeconds", Value = "30" });

        db.Products.AddRange(products);
        db.StockUnits.AddRange(CreateStockUnits(products, stockCounts, now));

        db.Outputs.AddRange(
            new OutputEntity { Id = "output-direct", Name = "Directa", Available = true },
            new OutputEntity { Id = "output-counter-1", Name = "Mostrador 1", Available = true },
            new OutputEntity { Id = "output-counter-2", Name = "Mostrador 2", Available = true },
            new OutputEntity { Id = "output-counter-3", Name = "Mostrador 3", Available = true },
            new OutputEntity { Id = "output-counter-4", Name = "Mostrador 4", Available = true },
            new OutputEntity { Id = "output-counter-5", Name = "Mostrador 5", Available = true },
            new OutputEntity { Id = "output-counter-6", Name = "Mostrador 6", Available = false });

        db.Movements.AddRange(
            new MovementEntity { ProductId = "product-serc", Quantity = 1, OccurredAtUtc = now.AddMinutes(-18) },
            new MovementEntity { ProductId = "product-durogesic", Quantity = 4, OccurredAtUtc = now.AddMinutes(-31) },
            new MovementEntity { ProductId = "product-ibuhexal", Quantity = 2, OccurredAtUtc = now.AddMinutes(-47) },
            new MovementEntity { ProductId = "product-synofen", Quantity = 6, OccurredAtUtc = now.AddHours(-1) });

        db.Dispensations.AddRange(
            CreateDispensation("DSP-1009", "product-durogesic", "output-direct", 2, now.AddMinutes(-5)),
            CreateDispensation("DSP-1008", "product-ibuhexal", "output-counter-1", 1, now.AddMinutes(-13)),
            CreateDispensation("DSP-1007", "product-serc", "output-direct", 1, now.AddMinutes(-28)),
            CreateDispensation("DSP-1006", "product-synofen", "output-counter-2", 1, now.AddMinutes(-42)),
            CreateDispensation("DSP-1005", "product-zatril", "output-counter-1", 2, now.AddMinutes(-53)),
            CreateDispensation("DSP-1004", "product-durogesic", "output-direct", 1, now.AddHours(-1).AddMinutes(-4)));

        db.Alerts.AddRange(
            new AlertEntity { Id = "alert-1", Message = "Dispensacion no realizada", Location = "E2 / F25", Severity = "high", CreatedAtUtc = now.AddMinutes(-12) },
            new AlertEntity { Id = "alert-2", Message = "Dispensacion no realizada", Location = "E4 / F16", Severity = "medium", CreatedAtUtc = now.AddMinutes(-24) },
            new AlertEntity { Id = "alert-3", Message = "Falta stock Ibuhexal Akut 600 MG", Severity = "medium", CreatedAtUtc = now.AddMinutes(-38) },
            new AlertEntity { Id = "alert-resolved", Message = "Puerta de mantenimiento abierta", Severity = "low", CreatedAtUtc = now.AddHours(-2), Resolved = true });

        db.Cameras.AddRange(
            new CameraEntity { Id = "camera-1", Name = "Camara 1", Available = false },
            new CameraEntity { Id = "camera-2", Name = "Camara 2", Available = false });

        db.Orders.Add(new OrderEntity
        {
            Id = "198345",
            RequestedBy = "Maria Jesus Perez Garcia",
            Organization = "Hospital Universitario Miguel Servet",
            OutputId = "output-direct",
            Status = "En preparacion",
            CreatedAtUtc = now.AddMinutes(-35)
        });
        db.OrderLines.AddRange(
            CreateOrderLine("line-1", "product-durogesic", 12, 10),
            CreateOrderLine("line-2", "product-ibuhexal", 20, 20),
            CreateOrderLine("line-3", "product-serc", 10, 2),
            CreateOrderLine("line-4", "product-synofen", 7, 2),
            CreateOrderLine("line-5", "product-zatril", 5, 5));

        db.LoadWindows.AddRange(
            new LoadWindowEntity { Id = "window-1", Name = "1", Available = true },
            new LoadWindowEntity { Id = "window-2", Name = "2", Available = true },
            new LoadWindowEntity { Id = "window-3", Name = "3", Available = false });
        db.LoadShelves.AddRange(Enumerable.Range(1, 12).Select(index => new LoadShelfEntity
        {
            Id = $"shelf-{index}",
            Name = index.ToString(),
            Available = index != 10
        }));

        db.LoadSessions.Add(new LoadSessionEntity
        {
            Id = "LOAD-1001",
            DeliveryNote = "ALB-2026-0142",
            Supplier = "Distribuciones Farmaceuticas Demo",
            DeliveryNoteCode = "DEMO-0142",
            WindowId = "window-1",
            ShelfId = "shelf-1",
            Status = "En carga",
            CreatedAtUtc = now.AddMinutes(-22),
            UpdatedAtUtc = now.AddMinutes(-3)
        });
        db.LoadItems.AddRange(CreateLoadItems(now));

        db.HistoryRecords.AddRange(CreateHistory(now));
        db.ChartPoints.AddRange(CreateChartPoints(now));

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureStartupAlertAsync(DemoDbContext db, CancellationToken cancellationToken)
    {
        const string startupAlertId = "alert-startup-demo";
        var alert = await db.Alerts.SingleOrDefaultAsync(item => item.Id == startupAlertId, cancellationToken);
        if (alert is null)
        {
            db.Alerts.Add(new AlertEntity
            {
                Id = startupAlertId,
                Message = "Aviso de comprobacion",
                Location = "Sistema APObot",
                Severity = "medium",
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            alert.Message = "Aviso de comprobacion";
            alert.Location = "Sistema APObot";
            alert.Severity = "medium";
            alert.Resolved = false;
            alert.CreatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static ProductEntity[] CreateProducts() =>
    [
        new() { Id = "product-durogesic", Code = "8124", Description = "Durogesic Matrix 100 5 Parches", Supplier = "Laboratorios Matrix SL", Category = "Analgesicos", IsSnsBillable = true, Situation = "Alta", NationalCode = "662577", Ean13 = "8470006625774", Observation = "8 uds. caducan en 4 semanas" },
        new() { Id = "product-serc", Code = "8417", Description = "Serc 16 MG 30 Comp", Supplier = "Mylan Pharmaceuticals", Category = "Analgesicos", IsSnsBillable = true, Situation = "Alta", NationalCode = "650214", Ean13 = "8470006502143", CanOrder = true },
        new() { Id = "product-ibuhexal", Code = "7322", Description = "Ibuhexal Akut 600 MG Blandas", Supplier = "Hexal Farmaceutica", Category = "Antiinflamatorio", IsSnsBillable = true, Situation = "Alta", NationalCode = "701328", Ean13 = "8470007013280", Observation = "Sin stock", CanOrder = true },
        new() { Id = "product-synofen", Code = "9421", Description = "Synofen Ibuprofeno Capsulas 40 MG", Supplier = "Synofen Pharma", Category = "Antiinflamatorio", IsSnsBillable = true, Situation = "Alta", NationalCode = "688421", Ean13 = "8470006884218" },
        new() { Id = "product-zatril", Code = "4123", Description = "Zatril 300 10 Parches", Supplier = "Zatril Iberia", Category = "Analgesicos", IsSnsBillable = false, Situation = "Alta", NationalCode = "612305", Ean13 = "8470006123058" },
        new() { Id = "product-amoxicilina", Code = "5519", Description = "Amoxicilina 500 MG 24 Capsulas", Supplier = "Cinfa", Category = "Antibioticos", IsSnsBillable = true, Situation = "Alta", NationalCode = "651904", Ean13 = "8470006519042", Observation = "Stock bajo", CanOrder = true }
    ];

    private static IEnumerable<StockUnitEntity> CreateStockUnits(IEnumerable<ProductEntity> products, IReadOnlyDictionary<string, int> stockCounts, DateTime now)
    {
        var unitIndex = 0;
        foreach (var product in products)
        {
            for (var index = 0; index < stockCounts[product.Id]; index++)
            {
                unitIndex++;
                yield return new StockUnitEntity
                {
                    Id = $"UNIT-{unitIndex:0000}",
                    ProductId = product.Id,
                    Module = ((char)('A' + unitIndex % 4)).ToString(),
                    Shelf = unitIndex % 24 + 1,
                    PositionX = unitIndex % 5 + 1,
                    PositionY = unitIndex % 3 + 1,
                    ExpirationDate = DateOnly.FromDateTime(now.AddMonths(4 + unitIndex % 14)),
                    LoadedAtUtc = now.AddDays(-(unitIndex % 60 + 1)),
                    Batch = $"L{product.Code}{unitIndex % 7:00}",
                    SerialNumber = $"SN{product.Code}{unitIndex:0000}"
                };
            }
        }
    }

    private static DispensationEntity CreateDispensation(string id, string productId, string outputId, int quantity, DateTime createdAt) => new()
    {
        Id = id,
        ProductId = productId,
        OutputId = outputId,
        Quantity = quantity,
        Priority = "high",
        Status = "Completada",
        CreatedAtUtc = createdAt
    };

    private static OrderLineEntity CreateOrderLine(string id, string productId, int requested, int completed) => new()
    {
        Id = id,
        OrderId = "198345",
        ProductId = productId,
        RequestedQuantity = requested,
        CompletedQuantity = completed
    };

    private static LoadItemEntity[] CreateLoadItems(DateTime now) =>
    [
        new() { Id = "LOAD-ITEM-1", SessionId = "LOAD-1001", Code = "8124", Description = "Durogesic Matrix 100 5 Parches", Module = "A", Shelf = 1, PositionX = 1, PositionY = 1, ExpirationDate = DateOnly.FromDateTime(now.AddMonths(9)), Batch = "L812401", SerialNumber = "LOAD8124001", Status = "Cargado" },
        new() { Id = "LOAD-ITEM-2", SessionId = "LOAD-1001", Code = "8417", Description = "Serc 16 MG 30 Comp", Module = "A", Shelf = 1, PositionX = 2, PositionY = 1, ExpirationDate = DateOnly.FromDateTime(now.AddMonths(12)), Batch = "L841702", SerialNumber = "LOAD8417002", Status = "Cargado" },
        new() { Id = "LOAD-ITEM-3", SessionId = "LOAD-1001", Code = "9421", Description = "Synofen Ibuprofeno Capsulas 40 MG", Module = "A", Shelf = 1, PositionX = 3, PositionY = 1, ExpirationDate = DateOnly.FromDateTime(now.AddMonths(7)), Batch = "L942103", SerialNumber = "LOAD9421003", Status = "Cargado" },
        new() { Id = "LOAD-ITEM-4", SessionId = "LOAD-1001", Code = "7322", Description = "Ibuhexal Akut 600 MG Blandas", Batch = "L732204", SerialNumber = "LOAD7322004", Status = "Rechazado", Rejected = true, RejectionCause = "Codigo no reconocido por el escaner" }
    ];

    private static IEnumerable<HistoryRecordEntity> CreateHistory(DateTime now)
    {
        var descriptions = new[]
        {
            ("product-durogesic", "8124", "Durogesic Matrix 100 5 Parches"),
            ("product-serc", "8417", "Serc 16 MG 30 Comp"),
            ("product-synofen", "9421", "Synofen Ibuprofeno Capsulas 40 MG"),
            ("product-zatril", "4123", "Zatril 300 10 Parches")
        };

        var id = 0;
        for (var index = 0; index < 14; index++)
        {
            var product = descriptions[index % descriptions.Length];
            id++;
            yield return new HistoryRecordEntity
            {
                Type = index % 2 == 0 ? "dispensation" : "load",
                ProductId = product.Item1,
                UnitId = $"HIST-{id:000}",
                Code = product.Item2,
                Description = product.Item3,
                Module = ((char)('A' + index % 4)).ToString(),
                Shelf = index % 12 + 1,
                PositionX = index % 5 + 1,
                PositionY = index % 3 + 1,
                ExpirationDate = DateOnly.FromDateTime(now.AddMonths(5 + index)),
                Batch = $"H{index:000}",
                SerialNumber = $"HISTSERIAL{index:000}",
                OccurredAtUtc = now.AddHours(-(index + 1) * 3),
                OutputName = index % 2 == 0 ? "Directa" : null,
                Rejected = index is 5 or 11,
                RejectionCause = index is 5 or 11 ? "Lectura de codigo incompleta" : null
            };
        }
    }

    private static IEnumerable<ChartPointEntity> CreateChartPoints(DateTime now)
    {
        var periods = new[]
        {
            (Name: "days", Count: 16, Step: TimeSpan.FromHours(3)),
            (Name: "week", Count: 7, Step: TimeSpan.FromDays(1)),
            (Name: "month", Count: 12, Step: TimeSpan.FromDays(2.5))
        };

        var id = 0;
        foreach (var period in periods)
        {
            for (var index = 0; index < period.Count; index++)
            {
                var timestamp = now.Subtract(TimeSpan.FromTicks(period.Step.Ticks * (period.Count - index - 1)));
                id++;
                yield return new ChartPointEntity { Id = id, Series = "throughput", Period = period.Name, Label = FormatChartLabel(period.Name, timestamp), PrimaryValue = 20 + index % 5 * 7 + index, SecondaryValue = 14 + index % 4 * 6 + index * .7, TimestampUtc = timestamp, SortOrder = index };
                id++;
                yield return new ChartPointEntity { Id = id, Series = "capacity", Period = period.Name, Label = FormatChartLabel(period.Name, timestamp), PrimaryValue = 45 + index % 4 * 4 + index * .8, SecondaryValue = 35 - index % 3 * 2 - index * .3, TimestampUtc = timestamp, SortOrder = index };
            }
        }
    }

    private static string FormatChartLabel(string period, DateTime timestamp) => period switch
    {
        "days" => timestamp.ToString("HH:mm", CultureInfo.InvariantCulture),
        "week" => timestamp.ToString("ddd", CultureInfo.GetCultureInfo("es-ES")),
        _ => timestamp.ToString("dd/MM", CultureInfo.InvariantCulture)
    };
}
