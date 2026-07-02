using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Microsoft.Extensions.Logging;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

/// <summary>
/// Orkestrerer reserve- og capture-flowet for en gruppebetaling.
/// Alle payment-provider-kald sker via IPaymentProvider — aldrig direkte.
/// </summary>
public sealed class GroupPaymentOrchestrationService(
    IPaymentProvider paymentProvider,
    IParticipantPaymentStateService stateService,
    IParticipantPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IParticipantRepository participantRepository,
    IMerchantCallbackService merchantCallbackService,
    ILogger<GroupPaymentOrchestrationService> logger) : IGroupPaymentOrchestrationService
{
    public async Task<ReserveParticipantPaymentResult> ReserveParticipantPaymentAsync(
        int orderId,
        int participantId,
        string? merchantId,
        long amountMinorUnits,
        string currency,
        string returnUrl,
        string callbackUrl,
        string? testPhoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        // Idempotens: find eksisterende ikke-cancelled/failed/expired betaling for denne deltager/ordre
        var existing = (await paymentRepository.GetByOrderIdAsync(orderId))
            .FirstOrDefault(p => p.ParticipantId == participantId
                                 && p.Status != ParticipantPaymentStatus.Cancelled
                                 && p.Status != ParticipantPaymentStatus.ReservationFailed
                                 && p.Status != ParticipantPaymentStatus.Expired);

        if (existing != null)
        {
            // Afvis hvis betaling allerede er Captured
            if (existing.Status == ParticipantPaymentStatus.Captured)
            {
                logger.LogWarning(
                    "[Orchestration] Re-submit afvist: ParticipantPayment {Id} er allerede Captured for Participant {ParticipantId}",
                    existing.Id, participantId);
                return new ReserveParticipantPaymentResult(
                    Success: false,
                    ParticipantPaymentId: existing.Id,
                    ProviderPaymentId: existing.ProviderPaymentId,
                    RedirectUrl: null,
                    ErrorCode: "ALREADY_CAPTURED",
                    ErrorMessage: "Betalingen er allerede gennemført og kan ikke ændres.");
            }

            // Returnér eksisterende info hvis allerede Reserved
            if (existing.Status == ParticipantPaymentStatus.Reserved)
            {
                logger.LogInformation(
                    "[Orchestration] Idempotent: ParticipantPayment {Id} er allerede Reserved for Participant {ParticipantId}",
                    existing.Id, participantId);
                return new ReserveParticipantPaymentResult(
                    Success: true,
                    ParticipantPaymentId: existing.Id,
                    ProviderPaymentId: existing.ProviderPaymentId,
                    RedirectUrl: null,
                    ErrorCode: null,
                    ErrorMessage: null);
            }

            // For andre ikke-afsluttede statuser (ReservationStarted, Created): returnér idempotent
            logger.LogInformation(
                "[Orchestration] Idempotent reserve: ParticipantPayment {Id} already exists for Participant {ParticipantId} on Order {OrderId}, Status={Status}",
                existing.Id, participantId, orderId, existing.Status);
            return new ReserveParticipantPaymentResult(
                Success: true,
                ParticipantPaymentId: existing.Id,
                ProviderPaymentId: existing.ProviderPaymentId,
                RedirectUrl: null,
                ErrorCode: null,
                ErrorMessage: null);
        }

        // Opret ny ParticipantPayment i state Created
        var payment = await stateService.CreateAsync(
            orderId, participantId, merchantId, amountMinorUnits, currency,
            providerName: "Fake",
            cancellationToken: cancellationToken);

        var idempotencyKey = $"reserve-{payment.Id}-{orderId}-{participantId}";

        // Callback URL bruger den faktiske ParticipantPaymentId — ignorerer hvad caller angiver
        var resolvedCallbackUrl = $"{callbackUrl.TrimEnd('/')}/{payment.Id}";

        var order = await orderRepository.GetByIdWithDetailsAsync(orderId);
        var merchant = order?.MerchantParticipant;

        if (merchant is null)
            return new ReserveParticipantPaymentResult(false, payment.Id, null, null, "NO_MERCHANT", $"Ordre {orderId} har ingen tilknyttet merchant.");

        if (string.IsNullOrWhiteSpace(merchant.VippsMerchantSerialNumber) ||
            string.IsNullOrWhiteSpace(merchant.VippsClientId) ||
            string.IsNullOrWhiteSpace(merchant.VippsClientSecret) ||
            string.IsNullOrWhiteSpace(merchant.VippsSubscriptionKey))
        {
            logger.LogError("[Orchestration] Merchant {MerchantId} mangler Vipps-konfiguration (MSN/ClientId/Secret/SubscriptionKey).", merchant.Id);
            return new ReserveParticipantPaymentResult(false, payment.Id, null, null, "MERCHANT_MISSING_VIPPS_CONFIG", $"Merchant '{merchant.Name}' mangler Vipps-konfiguration.");
        }

        var merchantMsn = merchant.VippsMerchantSerialNumber;
        var merchantClientId = merchant.VippsClientId;
        var merchantClientSecret = merchant.VippsClientSecret;
        var merchantSubscriptionKey = merchant.VippsSubscriptionKey;

        var request = new ReservePaymentRequest(
            GroupPaymentId: orderId.ToString(),
            ParticipantPaymentId: $"pp-{payment.Id:D8}",
            MerchantId: merchantId ?? "unknown",
            AmountMinorUnits: amountMinorUnits,
            Currency: currency,
            Description: $"Gruppebetaling ordre #{orderId}",
            ReturnUrl: returnUrl,
            CallbackUrl: resolvedCallbackUrl,
            IdempotencyKey: idempotencyKey,
            TestPhoneNumber: testPhoneNumber,
            MerchantSerialNumber: merchantMsn,
            MerchantClientId: merchantClientId,
            MerchantClientSecret: merchantClientSecret,
            MerchantSubscriptionKey: merchantSubscriptionKey);

        // Sæt ReservationStarted med et temp-id indtil vi får svar fra provider
        var tempProviderId = $"pending-{payment.Id}";
        await stateService.SetReservationStartedAsync(payment.Id, tempProviderId, idempotencyKey, cancellationToken);

        ReservePaymentResult result;
        try
        {
            result = await paymentProvider.ReserveAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Orchestration] ReserveAsync threw for ParticipantPayment {Id}", payment.Id);
            await stateService.SetReservationFailedAsync(payment.Id, "EXCEPTION", ex.Message, idempotencyKey, cancellationToken);
            return new ReserveParticipantPaymentResult(false, payment.Id, null, null, "EXCEPTION", ex.Message);
        }

        if (!result.Success)
        {
            await stateService.SetReservationFailedAsync(payment.Id, result.ErrorCode, result.ErrorMessage, idempotencyKey, cancellationToken);
            return new ReserveParticipantPaymentResult(false, payment.Id, null, null, result.ErrorCode, result.ErrorMessage);
        }

        // Opdater providerPaymentId med det rigtige id fra provider
        var savedPayment = await paymentRepository.GetByIdAsync(payment.Id)
            ?? throw new InvalidOperationException($"ParticipantPayment {payment.Id} not found after reserve.");
        savedPayment.ProviderPaymentId = result.ProviderPaymentId;
        await paymentRepository.SaveChangesAsync();

        // Fake provider returnerer med det samme Reserved — sæt status
        if (result.Status is "Reserved" or "Authorized")
        {
            await stateService.SetReservedAsync(payment.Id, idempotencyKey, cancellationToken);
        }

        logger.LogInformation(
            "[Orchestration] Reservation OK: ParticipantPayment {Id}, ProviderPaymentId={ProviderPaymentId}, RedirectUrl={RedirectUrl}",
            payment.Id, result.ProviderPaymentId, result.RedirectUrl);

        return new ReserveParticipantPaymentResult(
            Success: true,
            ParticipantPaymentId: payment.Id,
            ProviderPaymentId: result.ProviderPaymentId,
            RedirectUrl: result.RedirectUrl,
            ErrorCode: null,
            ErrorMessage: null);
    }

    public async Task<ApproveAndCaptureResult> ApproveAndCaptureAllAsync(
        int orderId,
        int requestingParticipantId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre {orderId} ikke fundet.");

        if (order.CreatedByParticipantId != requestingParticipantId)
            throw new UnauthorizedAccessException("Kun ordrevært (host) kan godkende og capture.");

        if (order.Status == "Paid")
        {
            logger.LogInformation("[Orchestration] Idempotent: Order {OrderId} already Paid", orderId);
            return new ApproveAndCaptureResult
            {
                AllCaptured = true,
                OrderStatus = "Paid",
                Results = []
            };
        }

        // Tillad retry hvis processen er afbrudt (HostApproved/Capturing/PartiallyFailed)
        if (order.Status is not ("ReadyToPay" or "HostApproved" or "Capturing" or "PartiallyFailed"))
            throw new InvalidOperationException($"Ordren kan ikke godkendes i status '{order.Status}'. Alle deltagere skal have indsendt bestilling.");

        var payments = (await paymentRepository.GetByOrderIdAsync(orderId)).ToList();

        // Doc 05: inkludér CaptureFailed i retry-puljen
        var reservedPayments = payments
            .Where(p => p.Status == ParticipantPaymentStatus.Reserved
                        || p.Status == ParticipantPaymentStatus.CaptureFailed)
            .ToList();

        if (reservedPayments.Count == 0)
            throw new InvalidOperationException("Ingen reserverede betalinger fundet. Alle deltagere skal have reserveret betaling.");

        // Doc 05: Sæt HostApproved på ordren inden capture-loop
        order.Status = "HostApproved";
        await orderRepository.SaveChangesAsync();

        // Doc 05: Sæt alle Reserved → CapturePending inden capture starter
        foreach (var payment in reservedPayments)
        {
            await stateService.SetCapturePendingAsync(payment.Id, correlationId: $"capture-{payment.Id}", cancellationToken: cancellationToken);
        }

        // Doc 05: Sæt Capturing på ordren
        order.Status = "Capturing";
        await orderRepository.SaveChangesAsync();

        var captureResults = new List<ParticipantCaptureResult>();
        var allCaptured = true;

        foreach (var payment in reservedPayments)
        {
            // Reload payment for opdateret status efter SetCapturePendingAsync
            var freshPayment = await paymentRepository.GetByIdAsync(payment.Id) ?? payment;

            // Idempotens: spring Captured over
            if (freshPayment.Status == ParticipantPaymentStatus.Captured)
            {
                var participant = await participantRepository.GetByIdAsync(payment.ParticipantId);
                captureResults.Add(new ParticipantCaptureResult
                {
                    ParticipantId = payment.ParticipantId,
                    ParticipantName = participant?.Name ?? "Ukendt",
                    ParticipantPaymentId = payment.Id,
                    Success = true,
                    ProviderCaptureId = null
                });
                continue;
            }

            var idempotencyKey = $"capture-{payment.Id}-{orderId}";
            var captureRequest = new CapturePaymentRequest(
                ProviderPaymentId: freshPayment.ProviderPaymentId ?? throw new InvalidOperationException($"Payment {payment.Id} mangler ProviderPaymentId"),
                AmountMinorUnits: freshPayment.AmountMinorUnits,
                Currency: freshPayment.Currency,
                IdempotencyKey: idempotencyKey,
                MerchantSerialNumber: order.MerchantParticipant?.VippsMerchantSerialNumber,
                MerchantClientId: order.MerchantParticipant?.VippsClientId,
                MerchantClientSecret: order.MerchantParticipant?.VippsClientSecret,
                MerchantSubscriptionKey: order.MerchantParticipant?.VippsSubscriptionKey);

            CapturePaymentResult captureResult;
            try
            {
                captureResult = await paymentProvider.CaptureAsync(captureRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Orchestration] CaptureAsync threw for ParticipantPayment {Id}", payment.Id);
                await stateService.SetCaptureFailedAsync(payment.Id, "EXCEPTION", ex.Message, idempotencyKey, cancellationToken);

                // Doc 05: Sæt PartiallyFailed på ordren og stop loopet
                order.Status = "PartiallyFailed";
                await orderRepository.SaveChangesAsync();

                var p2 = await participantRepository.GetByIdAsync(payment.ParticipantId);
                captureResults.Add(new ParticipantCaptureResult
                {
                    ParticipantId = payment.ParticipantId,
                    ParticipantName = p2?.Name ?? "Ukendt",
                    ParticipantPaymentId = payment.Id,
                    Success = false,
                    ErrorCode = "EXCEPTION",
                    ErrorMessage = ex.Message
                });
                allCaptured = false;
                break; // Doc 05: Stop capture-loopet ved fejl
            }

            var participant2 = await participantRepository.GetByIdAsync(payment.ParticipantId);

            if (captureResult.Success)
            {
                await stateService.SetCapturedAsync(payment.Id, idempotencyKey, cancellationToken);

                logger.LogInformation(
                    "[Orchestration] Captured ParticipantPayment {Id} for Participant {ParticipantId}",
                    payment.Id, payment.ParticipantId);

                captureResults.Add(new ParticipantCaptureResult
                {
                    ParticipantId = payment.ParticipantId,
                    ParticipantName = participant2?.Name ?? "Ukendt",
                    ParticipantPaymentId = payment.Id,
                    Success = true,
                    ProviderCaptureId = captureResult.ProviderCaptureId
                });
            }
            else
            {
                await stateService.SetCaptureFailedAsync(payment.Id, captureResult.ErrorCode, captureResult.ErrorMessage, idempotencyKey, cancellationToken);

                // Doc 05: Sæt PartiallyFailed på ordren og stop loopet
                order.Status = "PartiallyFailed";
                await orderRepository.SaveChangesAsync();

                allCaptured = false;

                captureResults.Add(new ParticipantCaptureResult
                {
                    ParticipantId = payment.ParticipantId,
                    ParticipantName = participant2?.Name ?? "Ukendt",
                    ParticipantPaymentId = payment.Id,
                    Success = false,
                    ErrorCode = captureResult.ErrorCode,
                    ErrorMessage = captureResult.ErrorMessage
                });
                break; // Doc 05: Stop capture-loopet ved fejl
            }
        }

        // Doc 05: Sæt Paid når alle er captured
        if (allCaptured)
        {
            order.Status = "Paid";
            await orderRepository.SaveChangesAsync();

            logger.LogInformation("[Orchestration] All payments captured. Order {OrderId} → Paid", orderId);

            try
            {
                await SendMerchantCallbackAsync(order, captureResults, payments, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Orchestration] Merchant callback fejlede for Order {OrderId}. Ordre forbliver Paid.", orderId);
                // Callback-fejl ruller ikke betalingen tilbage
            }
        }

        return new ApproveAndCaptureResult
        {
            AllCaptured = allCaptured,
            OrderStatus = order.Status,
            Results = captureResults
        };
    }

    public async Task<CancelOrderResult> CancelOrderAsync(
        int orderId,
        int requestingParticipantId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre {orderId} ikke fundet.");

        if (order.CreatedByParticipantId != requestingParticipantId)
            throw new UnauthorizedAccessException("Kun ordrevært (host) kan annullere ordren.");

        if (order.Status == "Cancelled")
        {
            logger.LogInformation("[Orchestration] Idempotent cancel: Order {OrderId} already Cancelled", orderId);
            return new CancelOrderResult { Success = true, OrderStatus = "Cancelled" };
        }

        if (order.Status == "Paid")
            throw new InvalidOperationException("En betalt ordre kan ikke annulleres.");

        var payments = (await paymentRepository.GetByOrderIdAsync(orderId)).ToList();

        var cancelledCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        foreach (var payment in payments)
        {
            // Spring over betalinger der allerede er afsluttet
            if (payment.Status is ParticipantPaymentStatus.Captured
                or ParticipantPaymentStatus.Cancelled
                or ParticipantPaymentStatus.Expired)
            {
                skippedCount++;
                continue;
            }

            if (string.IsNullOrEmpty(payment.ProviderPaymentId))
            {
                await stateService.SetCancelledAsync(payment.Id,
                    correlationId: $"cancel-order-{orderId}-{payment.Id}",
                    cancellationToken: cancellationToken);
                cancelledCount++;
                continue;
            }

            var cancelRequest = new CancelPaymentRequest(
                ProviderPaymentId: payment.ProviderPaymentId,
                Reason: "OrderCancelled",
                IdempotencyKey: $"cancel-{payment.Id}-{orderId}");

            try
            {
                var cancelResult = await paymentProvider.CancelAsync(cancelRequest, cancellationToken);
                if (cancelResult.Success)
                {
                    await stateService.SetCancelledAsync(payment.Id,
                        correlationId: $"cancel-order-{orderId}-{payment.Id}",
                        cancellationToken: cancellationToken);
                    cancelledCount++;
                }
                else
                {
                    errors.Add($"Payment {payment.Id}: {cancelResult.ErrorCode} — {cancelResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Orchestration] CancelAsync threw for ParticipantPayment {Id}", payment.Id);
                errors.Add($"Payment {payment.Id}: EXCEPTION — {ex.Message}");
            }
        }

        order.Status = "Cancelled";
        await orderRepository.SaveChangesAsync();

        logger.LogInformation(
            "[Orchestration] Order {OrderId} cancelled. Cancelled={C}, Skipped={S}, Errors={E}",
            orderId, cancelledCount, skippedCount, errors.Count);

        return new CancelOrderResult
        {
            Success = errors.Count == 0,
            OrderStatus = "Cancelled",
            CancelledCount = cancelledCount,
            SkippedCount = skippedCount,
            Errors = errors
        };
    }

    public async Task<CaptureStatusDto> GetCaptureStatusAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre {orderId} ikke fundet.");

        var payments = (await paymentRepository.GetByOrderIdAsync(orderId)).ToList();

        var paymentStatuses = payments.Select(pp =>
        {
            var participantName = order.OrderParticipants
                .FirstOrDefault(op => op.ParticipantId == pp.ParticipantId)
                ?.Participant.Name ?? "Ukendt";

            return new ParticipantPaymentStatusDto
            {
                ParticipantId = pp.ParticipantId,
                ParticipantName = participantName,
                Amount = pp.AmountMinorUnits / 100m,
                Currency = pp.Currency,
                Status = pp.Status.ToString(),
                ProviderTransactionId = pp.ProviderPaymentId
            };
        }).ToList();

        return new CaptureStatusDto
        {
            OrderId = orderId,
            OrderStatus = order.Status,
            Payments = paymentStatuses
        };
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private async Task SendMerchantCallbackAsync(
        DataStorage.PayBySharePay.Entities.Order order,
        List<ParticipantCaptureResult> captureResults,
        List<DataStorage.PayBySharePay.Entities.ParticipantPayment> allPayments,
        CancellationToken cancellationToken)
    {
        var callbackUrl = order.MerchantParticipant?.GroupOrderUrl;

        // Byg deltager-sektion: ét element pr. captured deltager
        var participants = captureResults
            .Where(r => r.Success)
            .Select(r =>
            {
                var payment = allPayments.FirstOrDefault(p => p.ParticipantId == r.ParticipantId);

                // Find drafts og lines for denne deltager
                var draft = order.MerchantOrderDrafts
                    .FirstOrDefault(d => d.ParticipantId == r.ParticipantId);

                var lines = (draft?.Lines ?? Enumerable.Empty<DataStorage.PayBySharePay.Entities.MerchantOrderLine>())
                    .Select(l => new PayNSyncFinalOrderLineDto
                    {
                        Sku = l.LineId,
                        Name = l.Name,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        LineTotal = l.LineTotal
                    }).ToList();

                var participantName = order.OrderParticipants
                    .FirstOrDefault(op => op.ParticipantId == r.ParticipantId)
                    ?.Participant.Name ?? r.ParticipantName;

                return new PayNSyncFinalParticipantOrderDto
                {
                    ParticipantId = r.ParticipantId,
                    DisplayName = participantName,
                    Amount = payment != null ? payment.AmountMinorUnits / 100m : 0m,
                    PaymentStatus = "Captured",
                    ProviderPaymentId = payment?.ProviderPaymentId,
                    MerchantDraftId = draft?.MerchantDraftReference,
                    Lines = lines
                };
            }).ToList();

        var totalAmount = participants.Sum(p => p.Amount);

        var payload = new PayNSyncFinalGroupOrderDto
        {
            EventType = "GroupOrderPaid",
            PaynsyncOrderId = order.Id,
            MerchantId = order.MerchantParticipantId,
            Status = "Paid",
            Currency = allPayments.FirstOrDefault()?.Currency ?? "DKK",
            TotalAmount = totalAmount,
            PaidAtUtc = DateTime.UtcNow,
            Participants = participants
        };

        await merchantCallbackService.SendGroupOrderPaidAsync(
            payload: payload,
            callbackUrl: callbackUrl,
            cancellationToken: cancellationToken);
    }
}
