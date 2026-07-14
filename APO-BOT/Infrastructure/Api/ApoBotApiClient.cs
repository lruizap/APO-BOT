using System.Net;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using APO_BOT.Models;
using APO_BOT.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace APO_BOT.Infrastructure.Api;

public sealed class ApoBotApiClient(HttpClient httpClient, IOptions<ApiOptions> options) : IApoBotApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly ApiOptions apiOptions = options.Value;

    public Task<SystemContextDto> GetSystemContextAsync(CancellationToken cancellationToken = default) =>
        SendAsync<SystemContextDto>(HttpMethod.Get, "api/v1/system/context", cancellationToken: cancellationToken);

    public Task SetLightAsync(bool enabled, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Put, "api/v1/system/light", new SetLightRequest { Enabled = enabled }, cancellationToken);

    public Task ExecuteMachineActionAsync(MachineAction action, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, "api/v1/system/actions", new MachineActionRequest { Action = action }, cancellationToken);

    public Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        SendAsync<DashboardDto>(HttpMethod.Get, "api/v1/dashboard", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<AlertDto>> GetAlertsAsync(bool includeResolved = false, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<AlertDto>>(
            HttpMethod.Get,
            QueryHelpers.AddQueryString("api/v1/alerts", "includeResolved", includeResolved.ToString().ToLowerInvariant()),
            cancellationToken: cancellationToken);

    public Task SetPriorityModeAsync(PriorityMode mode, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Put, "api/v1/dashboard/priority", new SetPriorityModeRequest { Mode = mode }, cancellationToken);

    public Task ResolveAlertAsync(string alertId, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Patch, $"api/v1/alerts/{Uri.EscapeDataString(alertId)}/resolve", null, cancellationToken);

    public Task<SystemPreferencesDto> GetSystemPreferencesAsync(CancellationToken cancellationToken = default) =>
        SendAsync<SystemPreferencesDto>(HttpMethod.Get, "api/v1/settings", cancellationToken: cancellationToken);

    public Task<SystemPreferencesDto> UpdateSystemPreferencesAsync(UpdateSystemPreferencesRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<SystemPreferencesDto>(HttpMethod.Put, "api/v1/settings", request, cancellationToken);

    public Task<PagedResult<ProductSummaryDto>> GetStockAsync(StockQuery query, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["code"] = query.Code,
            ["description"] = query.Description,
            ["category"] = query.Category,
            ["hasStock"] = query.HasStock?.ToString().ToLowerInvariant(),
            ["minimumStock"] = query.MinimumStock?.ToString(CultureInfo.InvariantCulture),
            ["observation"] = query.Observation,
            ["sortBy"] = query.SortBy,
            ["descending"] = query.Descending.ToString().ToLowerInvariant(),
            ["page"] = query.Page.ToString(),
            ["pageSize"] = query.PageSize.ToString()
        };

        return SendAsync<PagedResult<ProductSummaryDto>>(HttpMethod.Get, BuildQuery("api/v1/stock", parameters), cancellationToken: cancellationToken);
    }

    public Task<ProductDetailDto> GetProductAsync(string productId, CancellationToken cancellationToken = default) =>
        SendAsync<ProductDetailDto>(HttpMethod.Get, $"api/v1/stock/{Uri.EscapeDataString(productId)}", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<StockUnitDto>> GetProductUnitsAsync(string productId, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<StockUnitDto>>(HttpMethod.Get, $"api/v1/stock/{Uri.EscapeDataString(productId)}/units", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<OutputDto>> GetOutputsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<OutputDto>>(HttpMethod.Get, "api/v1/outputs", cancellationToken: cancellationToken);

    public Task<DispensationResultDto> CreateDispensationAsync(CreateDispensationRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<DispensationResultDto>(HttpMethod.Post, "api/v1/dispensations", request, cancellationToken);

    public Task<PagedResult<OrderSummaryDto>> GetOrdersAsync(OrderQuery query, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["status"] = query.Status,
            ["page"] = query.Page.ToString(),
            ["pageSize"] = query.PageSize.ToString()
        };

        return SendAsync<PagedResult<OrderSummaryDto>>(HttpMethod.Get, BuildQuery("api/v1/orders", parameters), cancellationToken: cancellationToken);
    }

    public Task<OrderDetailDto?> GetActiveOrderAsync(CancellationToken cancellationToken = default) =>
        SendOptionalAsync<OrderDetailDto>(HttpMethod.Get, "api/v1/orders/active", cancellationToken);

    public Task<OrderDetailDto> GetOrderAsync(string orderId, CancellationToken cancellationToken = default) =>
        SendAsync<OrderDetailDto>(HttpMethod.Get, $"api/v1/orders/{Uri.EscapeDataString(orderId)}", cancellationToken: cancellationToken);

    public Task<LoadConfigurationDto> GetLoadConfigurationAsync(CancellationToken cancellationToken = default) =>
        SendAsync<LoadConfigurationDto>(HttpMethod.Get, "api/v1/load/configuration", cancellationToken: cancellationToken);

    public Task<LoadSessionDto?> GetActiveLoadSessionAsync(CancellationToken cancellationToken = default) =>
        SendOptionalAsync<LoadSessionDto>(HttpMethod.Get, "api/v1/load/sessions/active", cancellationToken);

    public Task<LoadSessionDto> StartLoadAsync(StartLoadRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<LoadSessionDto>(HttpMethod.Post, "api/v1/load/sessions", request, cancellationToken);

    public Task<LoadSessionDto> ExecuteLoadCommandAsync(string sessionId, LoadCommandRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<LoadSessionDto>(HttpMethod.Post, $"api/v1/load/sessions/{Uri.EscapeDataString(sessionId)}/commands", request, cancellationToken);

    public Task<HistoryResultDto> GetHistoryAsync(HistoryQuery query, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["type"] = JsonNamingPolicy.CamelCase.ConvertName(query.Type.ToString()),
            ["period"] = JsonNamingPolicy.CamelCase.ConvertName(query.Period.ToString()),
            ["productId"] = query.ProductId,
            ["from"] = query.From?.ToString("O"),
            ["to"] = query.To?.ToString("O"),
            ["page"] = query.Page.ToString(),
            ["pageSize"] = query.PageSize.ToString()
        };

        return SendAsync<HistoryResultDto>(HttpMethod.Get, BuildQuery("api/v1/statistics/history", parameters), cancellationToken: cancellationToken);
    }

    public Task<CapacityStatisticsDto> GetCapacityStatisticsAsync(StatisticsPeriod period, CancellationToken cancellationToken = default) =>
        SendAsync<CapacityStatisticsDto>(HttpMethod.Get, QueryHelpers.AddQueryString("api/v1/statistics/capacity", "period", JsonNamingPolicy.CamelCase.ConvertName(period.ToString())), cancellationToken: cancellationToken);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await SendHttpAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result ?? throw new ApiException("El backend devolvio una respuesta vacia inesperada.", response.StatusCode);
    }

    private async Task<T?> SendOptionalAsync<T>(HttpMethod method, string path, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(method, path);
        using var response = await SendHttpAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task SendWithoutResponseAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await SendHttpAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private void EnsureConfigured()
    {
        if (!apiOptions.Enabled || httpClient.BaseAddress is null)
        {
            throw new ApiNotConfiguredException();
        }
    }

    private async Task<HttpResponseMessage> SendHttpAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.SendAsync(request, completionOption, cancellationToken);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException("La solicitud al backend ha superado el tiempo de espera.", innerException: exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ApiException("No se ha podido conectar con el backend.", innerException: exception);
        }
    }

    private static string BuildQuery(string path, IReadOnlyDictionary<string, string?> parameters)
    {
        return QueryHelpers.AddQueryString(path, parameters.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))!);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ApiProblem? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ApiProblem>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            // The status code remains available even when the backend does not return Problem Details JSON.
        }

        var message = problem?.Detail ?? problem?.Title ?? $"Error del backend ({(int)response.StatusCode}).";
        throw new ApiException(message, response.StatusCode, problem);
    }
}
