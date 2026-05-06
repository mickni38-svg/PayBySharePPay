# SBYS AI Context — Security

## Overordnet sikkerhedsmodel

SBYS bør bruge flere sikkerhedslag:

1. JWT til almindelige brugere
2. joinToken til invitationer og merchant links
3. HMAC-signering til merchant server-to-server calls
4. HMAC-signering til SBYS webhooks
5. Rate limiting og expiry senere

## JWT
JWT bruges til:
- SBYS web frontend/API
- user login
- opret gruppebetaling
- se egne gruppebetalinger
- acceptere invitationer

JWT bør ikke bruges som eneste sikkerhed for merchant links.

## groupPaymentId
groupPaymentId må aldrig alene give adgang til en gruppebetaling.

Det skal altid kombineres med:
- authenticated user
- membership check
- joinToken
- eller anden authorization

## joinToken
Invitation/merchant link bør bruge joinToken.

Token bør:
- være random og svært at gætte
- gemmes i database
- knyttes til groupPaymentId og participantId
- have expiry
- kunne markeres som used/revoked

## HMAC
Merchant API calls bør signeres.

Forslag til headers:
- X-SBYS-MerchantId
- X-SBYS-Timestamp
- X-SBYS-Signature

Signature:
```text
HMACSHA256(secret, timestamp + "." + rawBody)
```

## Secrets
Gem ikke secrets i klar tekst i produktion.

I dev kan man midlertidigt gemme test values, men produktion bør bruge:
- hashing
- encryption
- key vault
- secret rotation

## Status
Brug præcise statusnavne:
- Authorized/Reserved betyder reserveret men ikke nødvendigvis captured
- Captured/Paid betyder endeligt trukket
- Ready betyder alle nødvendige betalinger er klar
