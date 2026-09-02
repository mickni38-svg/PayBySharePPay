# UC-08: JWT-identitet og autorisation af host-handlinger

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** SECURITY_FIX
- **Størrelse:** Én afgrænset backend-slice uden databaseændring

## Status

✅ Implementeret på `main` den 2. september 2026.

Implementeringen bruger JWT `NameIdentifier`/`sub` på `/approve`, `/cancel`, `/complete` og legacy `/pay`. Body-feltet `requestingParticipantId` er bevaret for bagudkompatibilitet, men ignoreres ved autorisation. Ugyldigt identitets-claim giver 401, og fejlet host-ejerskab mappes til et generisk 403-svar før stateændring eller eksternt betalingskald.

## Mål

Beskyttede ordrehandlinger skal identificere den aktuelle bruger fra det validerede JWT-token. Klienten må ikke kunne opnå host-rettigheder ved at sende en anden brugers ID i request-body.

## Forudsætninger

Læs `.github/copilot-instructions.md`, `.ai/workflows/security-fix.md`, `docs/architecture.md`, `docs/business-rules.md` og de berørte controller-, service- og testfiler. Opret analyse, `implementation-plan.md` og `test-plan.md`, og vent på godkendelse før kodeændringer.

## Scope

- Kortlæg de eksisterende state-changing host-endpoints i `OrdersController`.
- Udled bruger-ID fra JWT-claimet `sub`/NameIdentifier gennem én eksisterende eller lille genbrugelig mekanisme.
- Brug JWT-identiteten ved host-ejerskabskontrol for godkendelse/capture, annullering og andre eksisterende host-only ordrehandlinger.
- Fjern eller ignorér `requestingParticipantId` fra body, når handlingen er beskyttet af JWT. En kontraktændring skal beskrives og godkendes før implementering.
- Map manglende/ugyldig authentication til 401 og manglende ejerskab til 403. `UnauthorizedAccessException` må ikke ende som 500.
- Bevar eksisterende servicevalidering og idempotens.

## Acceptkriterier

### AC1 – Aktuel bruger fra JWT

**Givet** et gyldigt JWT for bruger A  
**Når** A udfører en beskyttet host-handling  
**Så** anvender backend bruger A's ID fra tokenet og ikke et ID leveret af klienten.

### AC2 – Manipuleret request

**Givet** bruger B's JWT og en request-body med host A's ID  
**Når** B forsøger at godkende eller annullere A's ordre  
**Så** returneres 403, og ordre- og betalingsstatus ændres ikke.

### AC3 – Manglende token

**Givet** et beskyttet endpoint uden gyldigt JWT  
**Når** endpointet kaldes  
**Så** returneres 401.

### AC4 – Ingen regression

Eksisterende host kan fortsat godkende/capture og annullere egne ordrer. Betalingsflow, provider-kald og statusmaskine ændres ikke.

## Test

- Controller-/integrationstest for 401, 403 og succes.
- Manipuleret body-ID må ikke give adgang.
- `UnauthorizedAccessException` mappes til 403.
- Ingen live Vipps/MobilePay-kald; mock/fake eksisterende services/provider.

## Ikke en del af use casen

- Roller/claims ud over eksisterende bruger-ID.
- Ændring af login eller tokenlevetid.
- Beskyttelse af `DevController` (UC-09).
- Webhook-autentifikation (UC-10).
- Refaktorering af hele auth-arkitekturen.
