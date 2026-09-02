# UC-11: Ens JWT-udløbstid i token og auth-response

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** BUG_FIX
- **Størrelse:** Lille backend-konfigurationsrettelse

## Mål

JWT'ets faktiske udløbstid og feltet `ExpiresAt` i login-/registreringsresponsen skal beregnes fra samme konfigurationsværdi. PayNSync må ikke udstede et token på 30 dage og samtidig fortælle klienten, at det udløber efter 8 timer.

## Beslutning

Den eksisterende konfigurationsværdi `Jwt:ExpiresInMinutes` er eneste source of truth. Use casen ændrer ikke den valgte levetid; den fjerner kun den hardcodede afvigelse.

## Scope

- Genbrug den samme token-expiration fra `JwtTokenService` eller en lille fælles auth-konfiguration.
- Fjern hardcodet `AddMinutes(480)` fra alle auth-svar.
- Login, personregistrering, merchantregistrering og Google-login skal rapportere samme udløbstid som det udstedte JWT.
- Manglende/ugyldig konfiguration skal håndteres ensartet og må ikke give et token med en anden levetid end response.
- Ingen token refresh i denne use case.

## Acceptkriterier

### AC1 – Samme udløb

For alle auth-flows matcher response-feltet `ExpiresAt` tokenets `exp` med højst en lille tidsmæssig afrundingsforskel.

### AC2 – Konfiguration

Når `Jwt:ExpiresInMinutes` ændres i testkonfigurationen, ændres både token og response tilsvarende uden kodeændring.

### AC3 – Kompatibilitet

JWT-format, issuer, audience, signeringsalgoritme og eksisterende responsefelter ændres ikke.

## Test

- Deterministisk test med kendt levetid.
- Dæk login og relevante registrerings-/Google-flows.
- Decode token lokalt i test; ingen ekstern Google- eller anden netværksadgang.

## Ikke en del af use casen

- Token refresh.
- Ændring af standardlevetiden 43200 minutter.
- Roller/claims eller endpoint-autorisation.
- Frontend session-redesign.
