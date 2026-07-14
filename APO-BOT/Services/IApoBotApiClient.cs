using APO_BOT.Models;

namespace APO_BOT.Services;

public interface IApoBotApiClient
{
    Task<SystemContextDto> GetSystemContextAsync(CancellationToken cancellationToken = default);
    Task SetLightAsync(bool enabled, CancellationToken cancellationToken = default);
    Task ExecuteMachineActionAsync(MachineAction action, CancellationToken cancellationToken = default);

    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertDto>> GetAlertsAsync(bool includeResolved = false, CancellationToken cancellationToken = default);
    Task SetPriorityModeAsync(PriorityMode mode, CancellationToken cancellationToken = default);
    Task ResolveAlertAsync(string alertId, CancellationToken cancellationToken = default);

    Task<SystemPreferencesDto> GetSystemPreferencesAsync(CancellationToken cancellationToken = default);
    Task<SystemPreferencesDto> UpdateSystemPreferencesAsync(UpdateSystemPreferencesRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<ProductSummaryDto>> GetStockAsync(StockQuery query, CancellationToken cancellationToken = default);
    Task<ProductDetailDto> GetProductAsync(string productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockUnitDto>> GetProductUnitsAsync(string productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutputDto>> GetOutputsAsync(CancellationToken cancellationToken = default);
    Task<DispensationResultDto> CreateDispensationAsync(CreateDispensationRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<OrderSummaryDto>> GetOrdersAsync(OrderQuery query, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetActiveOrderAsync(CancellationToken cancellationToken = default);
    Task<OrderDetailDto> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);

    Task<LoadConfigurationDto> GetLoadConfigurationAsync(CancellationToken cancellationToken = default);
    Task<LoadSessionDto?> GetActiveLoadSessionAsync(CancellationToken cancellationToken = default);
    Task<LoadSessionDto> StartLoadAsync(StartLoadRequest request, CancellationToken cancellationToken = default);
    Task<LoadSessionDto> ExecuteLoadCommandAsync(string sessionId, LoadCommandRequest request, CancellationToken cancellationToken = default);

    Task<HistoryResultDto> GetHistoryAsync(HistoryQuery query, CancellationToken cancellationToken = default);
    Task<CapacityStatisticsDto> GetCapacityStatisticsAsync(StatisticsPeriod period, CancellationToken cancellationToken = default);
}
