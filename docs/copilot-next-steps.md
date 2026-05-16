# Næste udviklingsopgaver (Copilot-guide)

Opgaverne er prioriteret efter forretningsværdi og sikkerhed. Brug `Estimeret kompleksitet` til sprintplanlægning.

---

## 1. Reel authentication (password)

**Beskrivelse:**
Login kræver i dag kun email. Der er ingen password-validering. Udskift med reel auth (hashed password eller ekstern provider).

**Filer der skal ændres:**
- `src/Api.PayBySharePay/Controllers/AuthController.cs`
- `src/DataStorage.PayBySharePay/Entities/Participant.cs` (tilføj PasswordHash)
- `src/Service.PayBySharePay/Services/ParticipantService.cs`
- `src/Frontend.PayBySharePay/src/app/features/login/login.component.ts`

**Acceptkriterier:**
- Bruger skal angive password ved login
- Password gemmes hashed (BCrypt eller ASP.NET Identity)
- Forkert password returnerer 401

**Copilot-prompt:**
```
Implementér password-baseret login i PayBySharePay.
Tilføj PasswordHash på Participant-entity.
Brug BCrypt til hashing.
Opdater AuthController.Login til at validere password.
Opdater login-siden i Angular til at vise password-felt.
Bevar eksisterende dev-seed-konti ved at sætte et default dev-password.
```

**Kompleksitet:** Medium

---

## 2. JWT-nøgle til Azure Key Vault / miljøvariabel

**Beskrivelse:**
JWT-nøglen er en placeholder i `appsettings.json`. Skal flyttes til sikker konfiguration.

**Filer der skal ændres:**
- `src/Api.PayBySharePay/appsettings.json`
- `src/Api.PayBySharePay/appsettings.Production.json`
- Azure App Service konfiguration (miljøvariabel `Jwt__Key`)

**Acceptkriterier:**
- JWT-nøgle i prod hentes fra miljøvariabel eller Azure Key Vault
- `appsettings.json` indeholder kun placeholder til dev

**Copilot-prompt:**
```
Opdater Api.PayBySharePay til at hente Jwt:Key fra miljøvariablen JWT__KEY i produktion.
Bevar fallback til appsettings.json i development.
```

**Kompleksitet:** Lav

---

## 3. Server-side validering på API request-DTOs

**Beskrivelse:**
API request-DTOs mangler `[Required]`, `[StringLength]` og lignende data annotations. Ugyldige requests kan nå service-laget.

**Filer der skal ændres:**
- `src/Api.PayBySharePay/DTOs/*.cs` (alle request-klasser)

**Acceptkriterier:**
- Manglende påkrævede felter returnerer 400 Bad Request med fejlbesked
- `ModelState` valideres i controllers (eller via `[ApiController]`-attribut)

**Copilot-prompt:**
```
Tilføj [Required], [StringLength] og [Range] data annotations til alle request-DTO-klasser i Api.PayBySharePay/DTOs/.
Sørg for at [ApiController]-attributten på alle controllers automatisk returnerer 400 ved ModelState-fejl.
```

**Kompleksitet:** Lav

---

## 4. Unit tests for service-laget

**Beskrivelse:**
Testprojektet er tomt. Skriv unit tests for de centrale services.

**Filer der skal ændres:**
- `src/Tests.PayBySharePay/UnitTest1.cs` (omdøb og udvid)
- Tilføj `Moq` eller `NSubstitute` som NuGet-pakke

**Acceptkriterier:**
- `PaymentService.RegisterPaymentAsync` er dækket (happy path + fejlcases)
- `OrderService.CreateOrderAsync` er dækket
- Tests kører grønt med `dotnet test`

**Copilot-prompt:**
```
Skriv unit tests for PaymentService og OrderService i Tests.PayBySharePay.
Brug Moq til at mocke repositories.
Test happy path og fejlcases (KeyNotFoundException, ArgumentException).
```

**Kompleksitet:** Medium

---

## 5. JoinToken invitations-flow

**Beskrivelse:**
`Order.JoinToken` genereres allerede, men der er intet API-endpoint eller UI til at acceptere en invitation via link.

**Filer der skal ændres:**
- `src/Api.PayBySharePay/Controllers/OrdersController.cs` (nyt endpoint: `POST /api/orders/join/{token}`)
- `src/Service.PayBySharePay/Services/OrderService.cs`
- `src/Frontend.PayBySharePay/src/app/app.routes.ts` (ny route: `/join/:token`)
- Ny Angular-komponent: `JoinOrderComponent`

**Acceptkriterier:**
- Bruger kan åbne et invitationslink (`/join/<token>`)
- Brugeren tilføjes som deltager på ordren
- Ugyldigt/udløbet token returnerer 404

**Copilot-prompt:**
```
Implementér JoinToken-baseret invitations-flow i PayBySharePay.
Tilføj POST /api/orders/join/{token} endpoint der finder ordren og tilføjer den autentificerede bruger som deltager.
Opret Angular-komponent til /join/:token route der kalder endpointet og redirecter til ordredetaljer.
```

**Kompleksitet:** Medium

---

## 6. Backend aktivitetslog

**Beskrivelse:**
Aktivitetsfeed bygges i dag client-side fra ordredata. En dedikeret aktivitetslog i backend ville give bedre historik og læst/ulæst-tracking.

**Filer der skal ændres:**
- `src/DataStorage.PayBySharePay/Entities/` (ny: `ActivityLog.cs`)
- `src/DataStorage.PayBySharePay/Context/PayBySharePayDbContext.cs`
- `src/Service.PayBySharePay/` (ny service og interface)
- `src/Api.PayBySharePay/Controllers/` (nyt endpoint)
- `src/Frontend.PayBySharePay/src/app/core/services/activity.service.ts` (brug API i stedet)

**Acceptkriterier:**
- Aktiviteter gemmes i databasen når de sker (betaling, statusskift, invitation)
- API returnerer aktiviteter for en bruger
- Frontend henter fra API frem for at beregne client-side
- Læst/ulæst kan markeres

**Copilot-prompt:**
```
Implementér en backend aktivitetslog i PayBySharePay.
Tilføj ActivityLog entity og migration.
Lav ActivityLogService der skriver til loggen når betalinger, ordrestatusændringer og invitationer sker.
Tilføj GET /api/activity?participantId=X endpoint.
Opdater Angular ActivityService til at hente fra API.
```

**Kompleksitet:** Høj

---

## 7. Merchant-UI i frontend

**Beskrivelse:**
Merchants kan kun administreres via API/CLI-tools. Der mangler en UI-komponent til at oprette og administrere merchant-drafts.

**Filer der skal ændres:**
- `src/Frontend.PayBySharePay/src/app/features/` (ny: `merchant-order/`)
- `src/Frontend.PayBySharePay/src/app/app.routes.ts`

**Acceptkriterier:**
- Merchant-bruger kan se sine ordrer
- Merchant kan oprette draft med ordrelinjer
- Merchant kan markere draft som klar

**Copilot-prompt:**
```
Opret en merchant-order komponent i Angular for PayBySharePay.
Komponenten skal vise merchant-drafts for den indloggede merchant.
Brug eksisterende MerchantOrdersController API-endpoints.
Tilføj route /merchant-orders.
```

**Kompleksitet:** Medium
