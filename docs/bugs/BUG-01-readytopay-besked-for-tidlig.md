# BUG-01 — "Alle har bestilt"-besked sendes for tidligt

**Status:** Åben  
**Prioritet:** 🔴 Høj  
**Opdaget:** 2026-07-04  
**Komponent:** `MerchantOrderService` / `OrderService.CheckAndSetReadyToPayAsync`

---

## Symptom

Host modtager beskeden:

> ✅ *"Alle deltagere har bestilt til 'Pizza'. Du kan nu gennemføre betalingen: [Gå til Overblik]"*

...i det øjeblik den **sidste deltager trykker "Bestil"** på merchant-siden — men **inden** den pågældendes MobilePay-reservation er gennemført.

Hvis Host er hurtig og trykker "Godkend" med det samme, kan han forsøge at capture en betaling der endnu ikke er `Reserved` hos Vipps. Det resulterer i fejl.

---

## Rodårsag

I `MerchantOrderService.InitOrderAsync` kaldes metoderne i forkert rækkefølge:

```csharp
// 1. Sæt deltager til OrderSubmitted
orderParticipant.Status = "OrderSubmitted";
await _db.SaveChangesAsync();

// 2. ⚠️ FEJL: Besked sendes her — INDEN reservationen er startet
await _orderService.CheckAndSetReadyToPayAsync(dto.OrderId);

// 3. Start reservation hos Vipps (asynkron — kræver webhook-callback)
var reserveResult = await _orchestration.ReserveParticipantPaymentAsync(...);
```

`CheckAndSetReadyToPayAsync` sender beskeden til Host så snart **alle** deltagere har `OrderSubmitted` — men Vipps-reservationen er på dette tidspunkt kun *startet*, ikke *bekræftet*.

For MobilePay/Vipps-flowet er den korrekte trigger **`CheckAndSetReadyToPayByReservedAsync`**, som allerede eksisterer og kaldes korrekt fra `VippsCallbackController` (webhook). Denne metode sender **sin egen** besked til Host og sætter ordre til `ReadyToPay` — men kun når **alle** betalinger er `Reserved`.

---

## Konsekvens

| Scenarie | Hvad sker |
|---|---|
| Host er langsom (venter) | Ingen problem — webhook opdaterer korrekt til `ReadyToPay` |
| Host er hurtig (trykker straks) | Host kan forsøge approve/capture mens sidst deltagers betaling stadig er `ReservationStarted` → capture fejler |
| MobilePay-flow (Vipps) | `CheckAndSetReadyToPayAsync` sætter `ReadyToPay` for tidligt — `CheckAndSetReadyToPayByReservedAsync` (via webhook) sætter den igen korrekt, men beskeden er allerede sendt |

---

## To beskeder med samme formål

Der eksisterer i dag to metoder der begge sender "klar til betaling"-besked til Host:

| Metode | Trigger | Besked |
|---|---|---|
| `CheckAndSetReadyToPayAsync` | Alle `OrderSubmitted` (merchant-bestilling gemt) | `"Alle deltagere har bestilt..."` |
| `CheckAndSetReadyToPayByReservedAsync` | Alle betalinger `Reserved` (Vipps webhook) | `"Alle har bestilt og reserveret betaling..."` |

Kun den **sidstnævnte** er korrekt for Vipps/MobilePay-flowet.

---

## Foreslået løsning

I `MerchantOrderService.InitOrderAsync`: fjern kaldet til `CheckAndSetReadyToPayAsync` når `Provider = MobilePay/Vipps`.

Den korrekte rækkefølge for Vipps-flowet er:

```
1. Deltager bestiller (OrderSubmitted)
2. ReserveParticipantPaymentAsync → Vipps sender redirect-URL
3. Deltager godkender i MobilePay-appen
4. Vipps sender webhook → AUTHORIZED
5. VippsCallbackController → SetReservedAsync
6. CheckAndSetReadyToPayByReservedAsync
   → Alle Reserved? → Order.Status = "ReadyToPay" + besked til Host ✅
```

`CheckAndSetReadyToPayAsync` (linje 107 i `MerchantOrderService`) bør kun bruges til **FakePaymentProvider** (synkront flow uden webhook).

---

## Berørte filer

| Fil | Linje | Ændring |
|---|---|---|
| `src/Service.PayBySharePay/Services/MerchantOrderService.cs` | ~107 | Fjern eller konditionér `CheckAndSetReadyToPayAsync`-kald |
| `src/Service.PayBySharePay/Services/OrderService.cs` | ~346 | `CheckAndSetReadyToPayAsync` — kun relevant for Fake-provider |

---

## Test-scenarie til verifikation

1. Opret ordre med 2 deltagere og MobilePay som provider
2. Begge deltagere bestiller via merchant-demo
3. Verificér at Host **ikke** modtager beskeden efter trin 2
4. Godkend begge reservationer i MobilePay-testappen
5. Verificér at Host **nu** modtager beskeden (efter alle er `Reserved`)
