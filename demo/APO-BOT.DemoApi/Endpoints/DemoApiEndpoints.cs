using System.Globalization;
using APO_BOT.DemoApi.Data;
using APO_BOT.Models;
using Microsoft.EntityFrameworkCore;

namespace APO_BOT.DemoApi.Endpoints;

public static class DemoApiEndpoints
{
    public static IEndpointRouteBuilder MapDemoApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", async (DemoDbContext db, CancellationToken cancellationToken) =>
        {
            await db.Database.OpenConnectionAsync(cancellationToken);
            var sqliteVersion = db.Database.GetDbConnection().ServerVersion;
            await db.Database.CloseConnectionAsync();
            return Results.Ok(new { status = "ok", database = "sqlite", sqliteVersion });
        });
        endpoints.MapPost("/api/v1/demo/reset", ResetDemoAsync);

        var api = endpoints.MapGroup("/api/v1");
        MapSystem(api);
        MapDashboard(api);
        MapSettings(api);
        MapStock(api);
        MapOrders(api);
        MapLoad(api);
        MapStatistics(api);
        return endpoints;
    }

    private static void MapSystem(RouteGroupBuilder api)
    {
        api.MapGet("/system/context", async (DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var alertCount = await db.Alerts.CountAsync(alert => !alert.Resolved, cancellationToken);
            return Results.Ok(new SystemContextDto
            {
                PharmacyName = await GetSettingAsync(db, "pharmacyName", cancellationToken),
                UserDisplayName = await GetSettingAsync(db, "userDisplayName", cancellationToken),
                ServerTime = DateTimeOffset.UtcNow,
                LightOn = bool.TryParse(await GetSettingAsync(db, "lightOn", cancellationToken), out var lightOn) && lightOn,
                AlertCount = alertCount
            });
        });

        api.MapPut("/system/light", async (SetLightRequest request, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            await SetSettingAsync(db, "lightOn", request.Enabled.ToString().ToLowerInvariant(), cancellationToken);
            return Results.NoContent();
        });

        api.MapPost("/system/actions", async (MachineActionRequest request, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            db.MachineCommands.Add(new MachineCommandEntity
            {
                Action = request.Action.ToString(),
                CreatedAtUtc = DateTime.UtcNow
            });
            await SetSettingAsync(db, "machineStatus", request.Action == MachineAction.Pause ? "paused" : "poweredOff", cancellationToken);
            return Results.NoContent();
        });
    }

    private static void MapDashboard(RouteGroupBuilder api)
    {
        api.MapGet("/dashboard", async (DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var capacityTotalText = await GetSettingAsync(db, "capacityTotal", cancellationToken);
            var capacityTotal = int.TryParse(capacityTotalText, out var parsedCapacity) ? Math.Max(parsedCapacity, 1) : 1;
            var storedUnits = await db.StockUnits.CountAsync(cancellationToken);
            var percentage = Math.Clamp(storedUnits * 100m / capacityTotal, 0, 100);

            var movementRows = await (
                from movement in db.Movements.AsNoTracking()
                join product in db.Products.AsNoTracking() on movement.ProductId equals product.Id
                orderby movement.OccurredAtUtc descending
                select new { movement, product.Description })
                .Take(4)
                .ToListAsync(cancellationToken);

            var dispensationRows = await (
                from dispensation in db.Dispensations.AsNoTracking()
                join product in db.Products.AsNoTracking() on dispensation.ProductId equals product.Id
                join output in db.Outputs.AsNoTracking() on dispensation.OutputId equals output.Id
                orderby dispensation.CreatedAtUtc descending
                select new { dispensation, product.Description, OutputName = output.Name })
                .Take(9)
                .ToListAsync(cancellationToken);

            var alerts = await db.Alerts.AsNoTracking()
                .Where(alert => !alert.Resolved)
                .OrderByDescending(alert => alert.CreatedAtUtc)
                .Take(10)
                .ToListAsync(cancellationToken);
            var cameras = await db.Cameras.AsNoTracking().OrderBy(camera => camera.Name).ToListAsync(cancellationToken);
            var priorityText = await GetSettingAsync(db, "priorityMode", cancellationToken);

            return Results.Ok(new DashboardDto
            {
                Capacity = new StorageCapacityDto
                {
                    Percentage = percentage,
                    Status = percentage >= 85 ? "Alto" : percentage >= 60 ? "Medio" : "Bajo",
                    StatusDetail = $"{storedUnits} de {capacityTotal} huecos"
                },
                RecentEntries = movementRows.Select(row => new MovementDto
                {
                    ProductId = row.movement.ProductId,
                    ProductDescription = row.Description,
                    Quantity = row.movement.Quantity,
                    OccurredAt = AsOffset(row.movement.OccurredAtUtc)
                }).ToArray(),
                RecentDispensations = dispensationRows.Select(row => new DispensationSummaryDto
                {
                    Id = row.dispensation.Id,
                    ProductDescription = row.Description,
                    Quantity = row.dispensation.Quantity,
                    DispensedAt = AsOffset(row.dispensation.CreatedAtUtc),
                    OutputName = row.OutputName
                }).ToArray(),
                Alerts = alerts.Select(MapAlert).ToArray(),
                PriorityMode = ParsePriorityMode(priorityText),
                Cameras = cameras.Select(camera => new CameraFeedDto
                {
                    Id = camera.Id,
                    Name = camera.Name,
                    StreamUrl = camera.StreamUrl,
                    Available = camera.Available
                }).ToArray()
            });
        });

        api.MapPut("/dashboard/priority", async (SetPriorityModeRequest request, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            await SetSettingAsync(db, "priorityMode", ToCamelCase(request.Mode.ToString()), cancellationToken);
            return Results.NoContent();
        });

        api.MapGet("/alerts", async (bool includeResolved, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var query = db.Alerts.AsNoTracking().AsQueryable();
            if (!includeResolved)
            {
                query = query.Where(alert => !alert.Resolved);
            }

            var alerts = await query
                .OrderBy(alert => alert.Resolved)
                .ThenByDescending(alert => alert.CreatedAtUtc)
                .ToListAsync(cancellationToken);
            return Results.Ok(alerts.Select(MapAlert).ToArray());
        });

        api.MapPatch("/alerts/{alertId}/resolve", async (string alertId, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var alert = await db.Alerts.FindAsync([alertId], cancellationToken);
            if (alert is null)
            {
                return Results.NotFound();
            }

            alert.Resolved = true;
            await db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });
    }

    private static void MapSettings(RouteGroupBuilder api)
    {
        api.MapGet("/settings", async (DemoDbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await BuildSystemPreferencesAsync(db, cancellationToken)));

        api.MapPut("/settings", async (UpdateSystemPreferencesRequest request, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var errors = ValidatePreferences(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            await SetSettingAsync(db, "pharmacyName", request.PharmacyName.Trim(), cancellationToken);
            await SetSettingAsync(db, "userDisplayName", request.UserDisplayName.Trim(), cancellationToken);
            await SetSettingAsync(db, "alertSoundEnabled", request.AlertSoundEnabled.ToString().ToLowerInvariant(), cancellationToken);
            await SetSettingAsync(db, "screenBrightness", request.ScreenBrightness.ToString(CultureInfo.InvariantCulture), cancellationToken);
            await SetSettingAsync(db, "refreshIntervalSeconds", request.RefreshIntervalSeconds.ToString(CultureInfo.InvariantCulture), cancellationToken);
            return Results.Ok(await BuildSystemPreferencesAsync(db, cancellationToken));
        });
    }

    private static void MapStock(RouteGroupBuilder api)
    {
        api.MapGet("/stock", async (
            DemoDbContext db,
            string? code,
            string? description,
            string? category,
            bool? hasStock,
            int? minimumStock,
            string? observation,
            string? sortBy,
            bool descending,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var products = await db.Products.AsNoTracking().ToListAsync(cancellationToken);
            var stockByProduct = await db.StockUnits.AsNoTracking()
                .GroupBy(unit => unit.ProductId)
                .Select(group => new { ProductId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.ProductId, item => item.Count, cancellationToken);

            IEnumerable<ProductSummaryDto> query = products.Select(product => new ProductSummaryDto
            {
                Id = product.Id,
                Code = product.Code,
                Description = product.Description,
                Category = product.Category,
                Stock = stockByProduct.GetValueOrDefault(product.Id),
                Observation = product.Observation,
                CanOrder = product.CanOrder
            });

            query = ApplyContains(query, item => item.Code, code);
            query = ApplyContains(query, item => item.Description, description);
            query = ApplyContains(query, item => item.Category, category);
            query = ApplyContains(query, item => item.Observation ?? string.Empty, observation);
            if (hasStock.HasValue)
            {
                query = query.Where(item => hasStock.Value ? item.Stock > 0 : item.Stock == 0);
            }
            if (minimumStock.HasValue)
            {
                query = query.Where(item => item.Stock >= minimumStock.Value);
            }

            query = (sortBy?.ToLowerInvariant(), descending) switch
            {
                ("code", false) => query.OrderBy(item => item.Code),
                ("code", true) => query.OrderByDescending(item => item.Code),
                ("category", false) => query.OrderBy(item => item.Category).ThenBy(item => item.Description),
                ("category", true) => query.OrderByDescending(item => item.Category).ThenBy(item => item.Description),
                ("stock", false) => query.OrderBy(item => item.Stock).ThenBy(item => item.Description),
                ("stock", true) => query.OrderByDescending(item => item.Stock).ThenBy(item => item.Description),
                (_, true) => query.OrderByDescending(item => item.Description),
                _ => query.OrderBy(item => item.Description)
            };

            var filtered = query.ToArray();
            return Results.Ok(new PagedResult<ProductSummaryDto>
            {
                Items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToArray(),
                Page = page,
                PageSize = pageSize,
                TotalCount = filtered.Length
            });
        });

        api.MapGet("/stock/{productId}", async (string productId, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
            if (product is null)
            {
                return Results.NotFound();
            }

            var stock = await db.StockUnits.CountAsync(unit => unit.ProductId == productId, cancellationToken);
            return Results.Ok(new ProductDetailDto
            {
                Id = product.Id,
                Code = product.Code,
                Description = product.Description,
                Supplier = product.Supplier,
                Category = product.Category,
                IsSnsBillable = product.IsSnsBillable,
                Situation = product.Situation,
                NationalCode = product.NationalCode,
                Ean13 = product.Ean13,
                Stock = stock
            });
        });

        api.MapGet("/stock/{productId}/units", async (string productId, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            if (!await db.Products.AnyAsync(product => product.Id == productId, cancellationToken))
            {
                return Results.NotFound();
            }

            var units = await db.StockUnits.AsNoTracking()
                .Where(unit => unit.ProductId == productId)
                .OrderBy(unit => unit.ExpirationDate)
                .ThenBy(unit => unit.Id)
                .ToListAsync(cancellationToken);
            return Results.Ok(units.Select(MapStockUnit).ToArray());
        });

        api.MapGet("/outputs", async (DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var outputs = await db.Outputs.AsNoTracking().OrderBy(output => output.Name).ToListAsync(cancellationToken);
            return Results.Ok(outputs.Select(output => new OutputDto { Id = output.Id, Name = output.Name, Available = output.Available }).ToArray());
        });

        api.MapPost("/dispensations", CreateDispensationAsync);
    }

    private static void MapOrders(RouteGroupBuilder api)
    {
        api.MapGet("/orders", async (DemoDbContext db, string? status, int page, int pageSize, CancellationToken cancellationToken) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = db.Orders.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(order => order.Status == status);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var orders = await query.OrderByDescending(order => order.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            var outputNames = await db.Outputs.AsNoTracking().ToDictionaryAsync(output => output.Id, output => output.Name, cancellationToken);
            return Results.Ok(new PagedResult<OrderSummaryDto>
            {
                Items = orders.Select(order => MapOrderSummary(order, outputNames.GetValueOrDefault(order.OutputId, string.Empty))).ToArray(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        });

        api.MapGet("/orders/active", async (DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var order = await db.Orders.AsNoTracking()
                .Where(item => item.Status != "Completado" && item.Status != "Cancelado")
                .OrderByDescending(item => item.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
            return order is null ? Results.NoContent() : Results.Ok(await BuildOrderDetailAsync(db, order, cancellationToken));
        });

        api.MapGet("/orders/{orderId}", async (string orderId, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(item => item.Id == orderId, cancellationToken);
            return order is null ? Results.NotFound() : Results.Ok(await BuildOrderDetailAsync(db, order, cancellationToken));
        });
    }

    private static void MapLoad(RouteGroupBuilder api)
    {
        api.MapGet("/load/configuration", async (DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var windows = await db.LoadWindows.AsNoTracking().OrderBy(item => item.Name).ToListAsync(cancellationToken);
            var shelves = await db.LoadShelves.AsNoTracking().OrderBy(item => item.Name.Length).ThenBy(item => item.Name).ToListAsync(cancellationToken);
            return Results.Ok(new LoadConfigurationDto
            {
                Windows = windows.Select(item => new LoadWindowDto { Id = item.Id, Name = item.Name, Available = item.Available }).ToArray(),
                Shelves = shelves.Select(item => new LoadShelfDto { Id = item.Id, Name = item.Name, Available = item.Available }).ToArray()
            });
        });

        api.MapGet("/load/sessions/active", async (DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var session = await db.LoadSessions.AsNoTracking()
                .Where(item => item.Status != "Detenida" && item.Status != "Completada")
                .OrderByDescending(item => item.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
            return session is null ? Results.NoContent() : Results.Ok(await BuildLoadSessionAsync(db, session, cancellationToken));
        });

        api.MapPost("/load/sessions", async (StartLoadRequest request, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var errors = ValidateLoadRequest(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }
            if (!await db.LoadWindows.AnyAsync(item => item.Id == request.WindowId && item.Available, cancellationToken)
                || !await db.LoadShelves.AnyAsync(item => item.Id == request.ShelfId && item.Available, cancellationToken))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["configuration"] = ["La ventana o la balda no esta disponible."] });
            }

            var activeSessions = await db.LoadSessions.Where(item => item.Status != "Detenida" && item.Status != "Completada").ToListAsync(cancellationToken);
            foreach (var active in activeSessions)
            {
                active.Status = "Completada";
                active.UpdatedAtUtc = DateTime.UtcNow;
            }

            var session = new LoadSessionEntity
            {
                Id = $"LOAD-{Guid.NewGuid():N}"[..13].ToUpperInvariant(),
                DeliveryNote = request.DeliveryNote,
                Supplier = request.Supplier,
                DeliveryNoteCode = request.DeliveryNoteCode,
                WindowId = request.WindowId,
                ShelfId = request.ShelfId,
                Status = "Iniciada",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            db.LoadSessions.Add(session);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/load/sessions/{session.Id}", await BuildLoadSessionAsync(db, session, cancellationToken));
        });

        api.MapPost("/load/sessions/{sessionId}/commands", async (string sessionId, LoadCommandRequest request, DemoDbContext db, CancellationToken cancellationToken) =>
        {
            var session = await db.LoadSessions.SingleOrDefaultAsync(item => item.Id == sessionId, cancellationToken);
            if (session is null)
            {
                return Results.NotFound();
            }

            session.Status = request.Command switch
            {
                LoadCommand.Start => "En carga",
                LoadCommand.Pause => "Pausada",
                LoadCommand.Stop => "Detenida",
                LoadCommand.StartShelf => "Cargando balda",
                LoadCommand.PauseShelf => "Balda pausada",
                LoadCommand.StopShelf => "Balda detenida",
                _ => session.Status
            };
            if (!string.IsNullOrWhiteSpace(request.ShelfId))
            {
                session.ShelfId = request.ShelfId;
            }
            session.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(await BuildLoadSessionAsync(db, session, cancellationToken));
        });
    }

    private static void MapStatistics(RouteGroupBuilder api)
    {
        api.MapGet("/statistics/history", async (
            DemoDbContext db,
            string type,
            string period,
            string? productId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var normalizedType = type.ToLowerInvariant();
            var query = db.HistoryRecords.AsNoTracking().AsQueryable();
            if (normalizedType != "article")
            {
                query = query.Where(record => record.Type == normalizedType);
            }
            if (!string.IsNullOrWhiteSpace(productId))
            {
                query = query.Where(record => record.ProductId == productId);
            }

            var periodStart = DateTime.UtcNow - ParsePeriod(period);
            query = query.Where(record => record.OccurredAtUtc >= periodStart);
            if (from.HasValue)
            {
                var fromUtc = from.Value.UtcDateTime;
                query = query.Where(record => record.OccurredAtUtc >= fromUtc);
            }
            if (to.HasValue)
            {
                var toUtc = to.Value.UtcDateTime;
                query = query.Where(record => record.OccurredAtUtc <= toUtc);
            }

            var successful = await BuildHistoryPageAsync(query.Where(record => !record.Rejected), page, pageSize, cancellationToken);
            var rejected = await BuildHistoryPageAsync(query.Where(record => record.Rejected), page, pageSize, cancellationToken);
            return Results.Ok(new HistoryResultDto { Successful = successful, Rejected = rejected });
        });

        api.MapGet("/statistics/capacity", async (DemoDbContext db, string period, CancellationToken cancellationToken) =>
        {
            var normalizedPeriod = period.ToLowerInvariant();
            var points = await db.ChartPoints.AsNoTracking()
                .Where(point => point.Period == normalizedPeriod)
                .OrderBy(point => point.SortOrder)
                .ToListAsync(cancellationToken);
            return Results.Ok(new CapacityStatisticsDto
            {
                Throughput = points.Where(point => point.Series == "throughput").Select(MapChartPoint).ToArray(),
                Capacity = points.Where(point => point.Series == "capacity").Select(MapChartPoint).ToArray()
            });
        });
    }

    private static async Task<IResult> CreateDispensationAsync(CreateDispensationRequest request, DemoDbContext db, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.ProductId)) errors["productId"] = ["Seleccione un producto."];
        if (string.IsNullOrWhiteSpace(request.OutputId)) errors["outputId"] = ["Seleccione una salida."];
        if (request.Quantity <= 0) errors["quantity"] = ["La cantidad debe ser mayor que cero."];
        if (errors.Count > 0) return Results.ValidationProblem(errors);

        var product = await db.Products.SingleOrDefaultAsync(item => item.Id == request.ProductId, cancellationToken);
        var output = await db.Outputs.SingleOrDefaultAsync(item => item.Id == request.OutputId && item.Available, cancellationToken);
        if (product is null || output is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["resource"] = ["El producto o la salida no existe o no esta disponible."] });
        }

        var unitQuery = db.StockUnits.Where(unit => unit.ProductId == request.ProductId);
        if (request.UnitIds.Count > 0)
        {
            unitQuery = unitQuery.Where(unit => request.UnitIds.Contains(unit.Id));
        }
        var units = await unitQuery.OrderBy(unit => unit.ExpirationDate).Take(request.Quantity).ToListAsync(cancellationToken);
        if (units.Count < request.Quantity)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["quantity"] = ["No hay suficientes unidades disponibles."] });
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var createdAt = DateTime.UtcNow;
        var dispensation = new DispensationEntity
        {
            Id = $"DSP-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            ProductId = request.ProductId,
            OutputId = request.OutputId,
            Quantity = request.Quantity,
            Priority = ToCamelCase(request.Priority.ToString()),
            Status = "Completada",
            CreatedAtUtc = createdAt
        };
        db.Dispensations.Add(dispensation);
        db.HistoryRecords.AddRange(units.Select(unit => new HistoryRecordEntity
        {
            Type = "dispensation",
            ProductId = product.Id,
            UnitId = unit.Id,
            Code = product.Code,
            Description = product.Description,
            Module = unit.Module,
            Shelf = unit.Shelf,
            PositionX = unit.PositionX,
            PositionY = unit.PositionY,
            ExpirationDate = unit.ExpirationDate,
            Batch = unit.Batch,
            SerialNumber = unit.SerialNumber,
            OccurredAtUtc = createdAt,
            OutputName = output.Name
        }));
        db.StockUnits.RemoveRange(units);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Results.Created($"/api/v1/dispensations/{dispensation.Id}", new DispensationResultDto
        {
            Id = dispensation.Id,
            Status = dispensation.Status,
            CreatedAt = AsOffset(createdAt)
        });
    }

    private static async Task<OrderDetailDto> BuildOrderDetailAsync(DemoDbContext db, OrderEntity order, CancellationToken cancellationToken)
    {
        var outputName = await db.Outputs.AsNoTracking().Where(output => output.Id == order.OutputId).Select(output => output.Name).SingleOrDefaultAsync(cancellationToken) ?? string.Empty;
        var rows = await (
            from line in db.OrderLines.AsNoTracking()
            join product in db.Products.AsNoTracking() on line.ProductId equals product.Id
            where line.OrderId == order.Id
            orderby product.Description
            select new { line, product.Description })
            .ToListAsync(cancellationToken);
        return new OrderDetailDto
        {
            Id = order.Id,
            RequestedBy = order.RequestedBy,
            Organization = order.Organization,
            OutputName = outputName,
            Status = order.Status,
            CreatedAt = AsOffset(order.CreatedAtUtc),
            Lines = rows.Select(row => new OrderLineDto
            {
                Id = row.line.Id,
                ProductId = row.line.ProductId,
                ProductDescription = row.Description,
                RequestedQuantity = row.line.RequestedQuantity,
                CompletedQuantity = row.line.CompletedQuantity,
                Observation = row.line.Observation
            }).ToArray()
        };
    }

    private static async Task<LoadSessionDto> BuildLoadSessionAsync(DemoDbContext db, LoadSessionEntity session, CancellationToken cancellationToken)
    {
        var items = await db.LoadItems.AsNoTracking().Where(item => item.SessionId == session.Id).OrderBy(item => item.Id).ToListAsync(cancellationToken);
        return new LoadSessionDto
        {
            Id = session.Id,
            Status = session.Status,
            LoadedItems = items.Where(item => !item.Rejected).Select(MapLoadItem).ToArray(),
            RejectedItems = items.Where(item => item.Rejected).Select(MapLoadItem).ToArray()
        };
    }

    private static async Task<PagedResult<HistoryEntryDto>> BuildHistoryPageAsync(IQueryable<HistoryRecordEntity> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var records = await query.OrderByDescending(record => record.OccurredAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<HistoryEntryDto>
        {
            Items = records.Select(MapHistoryEntry).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static async Task<IResult> ResetDemoAsync(DemoDbContext db, CancellationToken cancellationToken)
    {
        await DemoDataSeeder.ResetAsync(db, cancellationToken);
        return Results.Ok(new { status = "reset", database = "sqlite" });
    }

    private static async Task<string> GetSettingAsync(DemoDbContext db, string key, CancellationToken cancellationToken) =>
        await db.SystemSettings.AsNoTracking().Where(setting => setting.Key == key).Select(setting => setting.Value).SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

    private static async Task SetSettingAsync(DemoDbContext db, string key, string value, CancellationToken cancellationToken)
    {
        var setting = await db.SystemSettings.FindAsync([key], cancellationToken);
        if (setting is null)
        {
            db.SystemSettings.Add(new SystemSettingEntity { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<SystemPreferencesDto> BuildSystemPreferencesAsync(DemoDbContext db, CancellationToken cancellationToken)
    {
        var brightnessText = await GetSettingAsync(db, "screenBrightness", cancellationToken);
        var refreshText = await GetSettingAsync(db, "refreshIntervalSeconds", cancellationToken);
        return new SystemPreferencesDto
        {
            PharmacyName = await GetSettingAsync(db, "pharmacyName", cancellationToken),
            UserDisplayName = await GetSettingAsync(db, "userDisplayName", cancellationToken),
            AlertSoundEnabled = bool.TryParse(await GetSettingAsync(db, "alertSoundEnabled", cancellationToken), out var alertSoundEnabled) && alertSoundEnabled,
            ScreenBrightness = int.TryParse(brightnessText, out var brightness) ? Math.Clamp(brightness, 20, 100) : 85,
            RefreshIntervalSeconds = int.TryParse(refreshText, out var refreshInterval) ? Math.Clamp(refreshInterval, 10, 300) : 30
        };
    }

    private static Dictionary<string, string[]> ValidatePreferences(UpdateSystemPreferencesRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.PharmacyName)) errors["pharmacyName"] = ["El nombre de la farmacia es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.UserDisplayName)) errors["userDisplayName"] = ["El nombre de usuario es obligatorio."];
        if (request.ScreenBrightness is < 20 or > 100) errors["screenBrightness"] = ["El brillo debe estar entre 20 y 100."];
        if (request.RefreshIntervalSeconds is < 10 or > 300) errors["refreshIntervalSeconds"] = ["El intervalo debe estar entre 10 y 300 segundos."];
        return errors;
    }

    private static IEnumerable<ProductSummaryDto> ApplyContains(IEnumerable<ProductSummaryDto> query, Func<ProductSummaryDto, string> selector, string? value) =>
        string.IsNullOrWhiteSpace(value) ? query : query.Where(item => selector(item).Contains(value, StringComparison.OrdinalIgnoreCase));

    private static OrderSummaryDto MapOrderSummary(OrderEntity order, string outputName) => new()
    {
        Id = order.Id,
        RequestedBy = order.RequestedBy,
        Organization = order.Organization,
        OutputName = outputName,
        Status = order.Status,
        CreatedAt = AsOffset(order.CreatedAtUtc)
    };

    private static StockUnitDto MapStockUnit(StockUnitEntity unit) => new()
    {
        Id = unit.Id,
        ProductId = unit.ProductId,
        Module = unit.Module,
        Shelf = unit.Shelf,
        PositionX = unit.PositionX,
        PositionY = unit.PositionY,
        ExpirationDate = unit.ExpirationDate,
        LoadedAt = unit.LoadedAtUtc.HasValue ? AsOffset(unit.LoadedAtUtc.Value) : null,
        Batch = unit.Batch,
        SerialNumber = unit.SerialNumber
    };

    private static AlertDto MapAlert(AlertEntity alert) => new()
    {
        Id = alert.Id,
        Message = alert.Message,
        Location = alert.Location,
        Severity = alert.Severity,
        CreatedAt = AsOffset(alert.CreatedAtUtc),
        Resolved = alert.Resolved
    };

    private static LoadItemDto MapLoadItem(LoadItemEntity item) => new()
    {
        Id = item.Id,
        Code = item.Code,
        Description = item.Description,
        Module = item.Module,
        Shelf = item.Shelf,
        PositionX = item.PositionX,
        PositionY = item.PositionY,
        ExpirationDate = item.ExpirationDate,
        Batch = item.Batch,
        SerialNumber = item.SerialNumber,
        Status = item.Status,
        RejectionCause = item.RejectionCause
    };

    private static HistoryEntryDto MapHistoryEntry(HistoryRecordEntity record) => new()
    {
        Id = record.UnitId,
        Code = record.Code,
        Description = record.Description,
        Module = record.Module,
        Shelf = record.Shelf,
        PositionX = record.PositionX,
        PositionY = record.PositionY,
        ExpirationDate = record.ExpirationDate,
        Batch = record.Batch,
        SerialNumber = record.SerialNumber,
        OccurredAt = AsOffset(record.OccurredAtUtc),
        OutputName = record.OutputName,
        RejectionCause = record.RejectionCause
    };

    private static ChartPointDto MapChartPoint(ChartPointEntity point) => new()
    {
        Label = point.Label,
        PrimaryValue = (decimal)point.PrimaryValue,
        SecondaryValue = (decimal)point.SecondaryValue,
        Timestamp = AsOffset(point.TimestampUtc)
    };

    private static Dictionary<string, string[]> ValidateLoadRequest(StartLoadRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.DeliveryNote)) errors["deliveryNote"] = ["El albaran es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.Supplier)) errors["supplier"] = ["El proveedor es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.DeliveryNoteCode)) errors["deliveryNoteCode"] = ["El codigo de albaran es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.WindowId)) errors["windowId"] = ["Seleccione una ventana."];
        if (string.IsNullOrWhiteSpace(request.ShelfId)) errors["shelfId"] = ["Seleccione una balda."];
        return errors;
    }

    private static TimeSpan ParsePeriod(string value) => value.ToLowerInvariant() switch
    {
        "week" => TimeSpan.FromDays(7),
        "month" => TimeSpan.FromDays(31),
        _ => TimeSpan.FromDays(2)
    };

    private static PriorityMode ParsePriorityMode(string value) => Enum.TryParse<PriorityMode>(value, true, out var mode) ? mode : PriorityMode.Dispense;
    private static string ToCamelCase(string value) => string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
    private static DateTimeOffset AsOffset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
