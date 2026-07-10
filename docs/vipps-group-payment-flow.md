# PayNSync – Group Payment Flow (for Vipps Support)

**System:** PayNSync – gruppebetalingsplatform  
**Formaal:** Forklare flow og credential-arkitektur til Vipps support  
**Dato:** 2026-07  

---

## Hvad er PayNSync?

PayNSync er en platform hvor en Host (gruppeleder) opretter en gruppeordre
hos en Merchant (f.eks. en restaurant). Hver deltager i gruppen bestiller
individuelt og betaler selv via MobilePay/Vipps.
Host godkender til sidst, og alle betalinger captures samlet.

---

## Credential-arkitektur

PayNSync er bygget som en **partner/PSP-model**:

```
PayNSync ejer (gemt i appsettings / GitHub Secrets):
  - client_id          (PayNSync platformkonto)
  - client_secret      (PayNSync platformkonto)
  - Ocp-Apim-Subscription-Key  (PayNSync abonnement)

Merchant leverer kun:
  - Merchant Serial Number (MSN)
```

Merchants behover IKKE at have en API-integration.
De leverer udelukkende deres MSN til PayNSync, som herefter
haandterer alle API-kald paa vegne af merchanten.

---

## Pseudo-kode: Fuldt gruppebetalingsflow

### TRIN 1 – Host opretter gruppeordre

```
HOST opretter ordre i PayNSync:
  order = {
    title: "Fredagspizza",
    merchantMSN: "123456",   <-- eneste merchant-information
    deltagere: [Alice, Bob, Charlie]
  }
  
  // PayNSync genererer unikt ParticipantToken per deltager
  foreach deltager:
    orderParticipant.participantToken = ny GUID
    send invitations-link til deltager
```

---

### TRIN 2 – Deltager bestiller hos merchant

```
DELTAGER aabner merchant-link med sit participantToken

MERCHANT DEMO sender til PayNSync API:
  POST /api/merchant-orders
  {
    participantToken: "abc-123-...",   <-- validerer hvem deltager er
    orderLines: [
      { name: "Margherita", quantity: 1, unitPrice: 89.00 }
    ]
  }

PAYNSYNC gemmer bestillingen og starter reservation:
  draft = gem MerchantOrderDraft + MerchantOrderLines
  orderParticipant.status = "OrderSubmitted"
```

---

### TRIN 3 – PayNSync henter access token (PayNSync egne credentials)

```
PAYNYNC kalder Vipps token endpoint:

  POST https://apitest.vipps.no/accesstoken/get
  Headers:
    client_id:                  <PayNSync client_id>
    client_secret:              <PayNSync client_secret>
    Ocp-Apim-Subscription-Key:  <PayNSync subscription_key>
    Merchant-Serial-Number:     "123456"  <-- kun MSN fra merchant

  --> returnerer Bearer access_token (caches per session)
```

> Ingen merchant credentials involveret – kun MSN.

---

### TRIN 4 – PayNSync opretter betaling hos Vipps (reservation)

```
PAYNYNC kalder Vipps ePayment API paa vegne af merchant:

  POST https://apitest.vipps.no/epayment/v1/payments
  Headers:
    Authorization:              Bearer <access_token>
    Ocp-Apim-Subscription-Key:  <PayNSync subscription_key>
    Merchant-Serial-Number:     "123456"   <-- kun MSN fra merchant
    Idempotency-Key:            "reserve-{paymentId}-{orderId}-{participantId}"

  Body:
  {
    "amount": { "value": 8900, "currency": "DKK" },
    "paymentMethod": { "type": "WALLET" },
    "reference": "{participantPaymentId}",
    "userFlow": "WEB_REDIRECT",
    "returnUrl": "https://api.paynsync.dk/payment-return",
    "webhookUrl": "https://api.paynsync.dk/api/payments/vipps/callbacks/{participantPaymentId}",
    "paymentDescription": "PayNSync gruppebestilling"
  }

  --> Vipps returnerer redirectUrl (link til MobilePay-popup)
  participantPayment.status = "ReservationStarted"
```

---

### TRIN 5 – Deltager godkender i MobilePay-appen

```
DELTAGER aabner redirectUrl i browser
  --> MobilePay-popup vises paa telefonens skarm
  --> Deltager godkender betalingen

VIPPS sender webhook-callback til PayNSync:
  POST https://api.paynsync.dk/api/payments/vipps/callbacks/{participantPaymentId}
  Body: { "name": "AUTHORIZED", "reference": "{participantPaymentId}", ... }

PAYNYNC opdaterer status:
  participantPayment.status = "Reserved"
  // Penge er BLOKERET paa deltagerens konto -- IKKE trukket endnu
  
  // Tjek om ALLE deltagere i ordren er Reserved
  if alle deltagere har status "Reserved":
    order.status = "ReadyToPay"
    send besked til host: "Alle har reserveret – du kan nu godkende"
```

---

### TRIN 6 – Host godkender og capture looper (pengene trækkes)

```
HOST trykker "Godkend" i PayNSync-appen

PAYNYNC looper igennem ALLE reserverede betalinger for ordren:

  order.status = "Capturing"

  foreach deltager in reserveredeBetlinger:

    // Hent nyt token (eller brug cached) med PayNSync credentials + merchant MSN
    token = GetAccessToken(
      client_id:        <PayNSync client_id>,
      client_secret:    <PayNSync client_secret>,
      subscription_key: <PayNSync subscription_key>,
      MSN:              "123456"
    )

    // Capture denne deltagers betaling
    POST https://apitest.vipps.no/epayment/v1/payments/{providerPaymentId}/capture
    Headers:
      Authorization:              Bearer <token>
      Ocp-Apim-Subscription-Key:  <PayNSync subscription_key>
      Merchant-Serial-Number:     "123456"
      Idempotency-Key:            "capture-{paymentId}-{orderId}"
    Body:
      { "modificationAmount": { "value": 8900, "currency": "DKK" } }

    if capture succeded:
      participantPayment.status = "Captured"
      // Penge er nu TRUKKET fra deltagerens konto
    else:
      participantPayment.status = "CaptureFailed"
      order.status = "PartiallyFailed"
      STOP loop

  if alle captured:
    order.status = "Paid"
    // Send merchant-callback med ordreoversigt
```

---

## Samlet status-progression per betaling

```
Deltager bestiller:
  Created --> ReservationStarted

Deltager godkender i MobilePay:
  ReservationStarted --> Reserved        (penge BLOKERET)

Host godkender, capture kaldes:
  Reserved --> CapturePending --> Captured  (penge TRUKKET)

Naar alle deltagere er Captured:
  Order.Status = "Paid"
```

---

## Opsummering af hvad PayNSync sender til Vipps

| API-kald | Vores credentials | Merchant-info |
|----------|------------------|---------------|
| `POST /accesstoken/get` | client_id + client_secret + subscription_key | MSN |
| `POST /epayment/v1/payments` (reserve) | Bearer token + subscription_key | MSN |
| `POST /epayment/v1/payments/{id}/capture` | Bearer token + subscription_key | MSN |
| `POST /epayment/v1/payments/{id}/cancel` | Bearer token + subscription_key | MSN |

> **Konklusion:** Merchant leverer udelukkende MSN til PayNSync.  
> Alle API-credentials (client_id, client_secret, subscription_key) ejes og opbevares  
> af PayNSync. Ingen kritiske nogler gemmes hos eller sendes til merchants.

---

## Antal API-kald til Vipps per gruppeordre

```
1 token-kald      per reservation (caches – kan genbruges)
N reserve-kald    (ét per deltager)
N capture-kald    (ét per deltager naar host godkender)

Eksempel med 5 deltagere:
  5 x POST /epayment/v1/payments          (reservation)
  5 x POST /epayment/v1/payments/{id}/capture  (capture)
  = 10 betalings-API-kald + token-kald
```
