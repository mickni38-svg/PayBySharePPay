# UC-09: Beskyt udvikler-endpoints i produktion

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** SECURITY_FIX
- **Størrelse:** Lille backend- og konfigurationsændring

## Mål

Endpoints der nulstiller eller ændrer testdata må ikke være tilgængelige i Simply/produktion. De skal fortsat kunne bruges i det dokumenterede lokale udviklingsmiljø.

## Forudsætninger

Læs repo-instruktionerne, `.ai/workflows/security-fix.md`, `DevController`, `Program.cs`, environment-konfigurationen og nærmeste tests. Lav analyse og planer, og vent på godkendelse før implementering.

## Scope

- Identificér alle actions i `DevController`, herunder reset og seed af merchant-URL'er.
- Gør controlleren utilgængelig uden for Development med det mindste eksisterende ASP.NET Core-mønster.
- Production/Simply skal returnere 404 for dev-ruter, så de ikke annonceres som administrative funktioner.
- Development-adfærd bevares.
- Swagger/OpenAPI i ikke-Development må ikke eksponere dev-endpoints.
- Der må ikke tilføjes et nyt hemmeligt admin-password eller credentials.

## Acceptkriterier

### AC1 – Produktion

**Givet** environment `Simply` eller `Production`  
**Når** en dev-rute kaldes  
**Så** findes endpointet ikke, og ingen data ændres.

### AC2 – Lokal udvikling

**Givet** environment `Development`  
**Når** en eksisterende dev-rute kaldes  
**Så** fungerer den som før.

### AC3 – Dokumentation

Swagger i ikke-Development viser ikke dev-ruterne, og `docs/current-state.md`/arkitekturdokumentation opdateres efter verificeret implementering.

## Test

- Verificér route-registrering i Development og fravær i Simply/Production.
- Verificér at repository/service ikke kaldes uden for Development.
- Ingen databaseændring og ingen nye dependencies.

## Ikke en del af use casen

- Et generelt adminpanel.
- Nye seed-funktioner.
- JWT-roller.
- Ændring af almindelige produktionsendpoints.
