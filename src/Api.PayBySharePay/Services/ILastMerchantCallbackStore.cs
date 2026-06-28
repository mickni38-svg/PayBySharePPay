using Service.PayBySharePay.DTOs;
using System.Collections.Concurrent;

namespace Api.PayBySharePay.Services;

/// <summary>
/// Opbevarer seneste sendte merchant callback payload per ordre.
/// Bruges kun i dev/test til inspektion via GET /api/dev/merchant-callbacks/latest.
/// </summary>
public interface ILastMerchantCallbackStore
{
    void Set(int orderId, PayNSyncFinalGroupOrderDto payload);
    PayNSyncFinalGroupOrderDto? Get(int orderId);
}

/// <summary>In-memory singleton implementation.</summary>
public sealed class InMemoryLastMerchantCallbackStore : ILastMerchantCallbackStore
{
    private readonly ConcurrentDictionary<int, PayNSyncFinalGroupOrderDto> _store = new();

    public void Set(int orderId, PayNSyncFinalGroupOrderDto payload)
        => _store[orderId] = payload;

    public PayNSyncFinalGroupOrderDto? Get(int orderId)
        => _store.TryGetValue(orderId, out var p) ? p : null;
}
