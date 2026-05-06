# PayBySharePay

PayBySharePay er en løsning til fælles bestillinger, delt betaling og statusoverblik mellem privatpersoner og virksomheder/merchants.

## Indhold

- [Teknologistak](#teknologistak)
- [Arkitektur](#arkitektur)
- [Kom i gang](#kom-i-gang)
- [API endpoints](#api-endpoints)
- [Migrationer](#migrationer)
- [Projektstruktur](#projektstruktur)

---

## Teknologistak

| Lag | Teknologi |
|-----|-----------|
| Frontend | Angular + TypeScript |
| Backend | ASP.NET Core Web API, .NET 9 |
| Data access | EF Core 9 |
| Database | SQL Server |
| UI | Mobil-først, tilpasset iPhone 14 |

---

## Arkitektur

Løsningen er opdelt i fire lag med én retning af afhængigheder:

```
Frontend (Angular) → Api.PayBySharePay → Service.PayBySharePay → DataStorage.PayBySharePay
```

| Projekt | Ansvar |
|---------|--------|
| `Frontend.PayBySharePay` | Angular UI, routing, HTTP-kald, responsive layouts |
| `Api.PayBySharePay` | HTTP endpoints, DTO'er, inputvalidering, DI-opsætning |
| `Service.PayBySharePay` | Forretningsregler, use cases, mapping |
| `DataStorage.PayBySharePay` | EF Core entiteter, DbContext, repositories |

### Domænemodel

Systemet bygger på disse centrale entiteter:

- **`Participant`** – repræsenterer enten en `Person` eller en `Merchant` (styret af `ParticipantType` enum)
- **`FriendRelation`** – relation mellem to deltagere
- **`Order`** – en fælles bestilling med titel, kategori, besked og status
- **`OrderParticipant`** – kobler deltagere til en ordre
- **`Payment`** – registreret betaling på en ordre fra en deltager
- **`Message`** – besked knyttet til en ordre

En merchant har ekstra felter: firmanavn, CVR, momsnummer, kontaktoplysninger, betalingsreference m.v.

---

## Kom i gang

### Krav

- .NET 9 SDK
- SQL Server (eller SQL Server Express)
- Node.js + Angular CLI (til frontend)

### Backend

1. Opdatér connection string i `src/Api.PayBySharePay/appsettings.json`:

```json
"ConnectionStrings": {
  "PayBySharePayDb": "Server=DIN_SERVER;Database=PayBySharePay;Trusted_Connection=True;TrustServerCertificate=True"
}
```

2. Kør EF Core migrationer (se [Migrationer](#migrationer))

3. Start backend:

```powershell
cd src\Api.PayBySharePay
dotnet run
```

API'et starter som standard på `https://localhost:7xxx` og Swagger UI åbner på rod-URL'en (`/`).

### Frontend

```powershell
cd src\Frontend.PayBySharePay
npm install
ng serve
```

Frontendet starter på `http://localhost:4200`.

---

## API endpoints

Se fuld Swagger-dokumentation ved at starte backend og åbne rod-URL'en.

### Deltagere – `POST /api/participants/person`

Opretter en ny person-deltager.

```json
{
  "name": "Anders Andersen",
  "email": "anders@example.com",
  "phone": "12345678"
}
```

### Deltagere – `POST /api/participants/merchant`

Opretter en ny merchant-deltager.

```json
{
  "name": "Acme ApS",
  "companyName": "Acme ApS",
  "cvrNumber": "12345678",
  "contactEmail": "kontakt@acme.dk"
}
```

### Deltagere – `GET /api/participants/search?query=anders`

Søger efter deltagere (både personer og merchants) på navn.

### Venner – `POST /api/friends`

Opretter en venrelation mellem to deltagere.

```json
{
  "initiatorId": 1,
  "receiverId": 2
}
```

### Ordrer – `POST /api/orders`

Opretter en ny ordre og tilknytter deltagere.

```json
{
  "title": "Fælles middag",
  "category": "Restaurantbesøg",
  "message": "Vi spiser kl. 19",
  "participantIds": [1, 2, 3]
}
```

### Ordrer – `GET /api/orders/{id}/overview`

Henter overblik over en ordre inkl. deltagere, status og betalinger.

### Betalinger – `POST /api/payments`

Registrerer en betaling på en ordre.

```json
{
  "orderId": 1,
  "participantId": 2,
  "amount": 150.00
}
```

### Beskeder – `GET /api/messages/order/{orderId}`

Henter alle beskeder for en ordre.

### Beskeder – `POST /api/messages`

Opretter en besked på en ordre.

```json
{
  "orderId": 1,
  "participantId": 1,
  "content": "Jeg har betalt min del"
}
```

---

## Migrationer

### Opret database ud fra eksisterende migration

```powershell
cd src\Api.PayBySharePay
dotnet ef database update --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj
```

### Opret ny migration

```powershell
dotnet ef migrations add Navn --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj
```

---

## Projektstruktur

```
PayBySharePay/
├── docs/                          # Dokumentation og krav
├── src/
│   ├── Api.PayBySharePay/
│   │   ├── Controllers/           # HTTP endpoints
│   │   ├── DTOs/                  # Request-objekter
│   │   ├── Middleware/            # ExceptionHandlingMiddleware
│   │   └── Program.cs
│   ├── Service.PayBySharePay/
│   │   ├── DTOs/                  # Service-niveau DTO'er
│   │   ├── Interfaces/            # IParticipantService, IOrderService m.fl.
│   │   ├── Services/              # Implementeringer
│   │   └── Extensions/
│   ├── DataStorage.PayBySharePay/
│   │   ├── Context/               # PayBySharePayDbContext
│   │   ├── Entities/              # EF Core entiteter
│   │   ├── Repositories/          # Interfaces og implementeringer
│   │   ├── Migrations/
│   │   └── Extensions/
│   └── Tests.PayBySharePay/       # Enhedstests
└── PayBySharePay.sln
```

---

## Fejlhåndtering

API'et håndterer fejl centralt via `ExceptionHandlingMiddleware`:

| Exception | HTTP-statuskode |
|-----------|----------------|
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `InvalidOperationException` | 409 Conflict |
| Øvrige | 500 Internal Server Error |

---

## Copilot step guide

Denne sektion er til videreudvikling trin for trin i Copilot.

### Løsning og projektnavne

- Solution: `PayBySharePay`
- Frontend: `Frontend.PayBySharePay`
- Service-lag: `Service.PayBySharePay`
- API-lag: `Api.PayBySharePay`
- Data/repository-lag: `DataStorage.PayBySharePay`

### Vigtig domæneregel

En deltager kan være enten:

- `Person`
- `Merchant`

En merchant er også en deltager, men har ekstra firma- og betalingsoplysninger.

---

## Sådan bruger du filerne i Copilot

Ved **hvert step** skal du:

1. Vedhæfte de filer der står under step’et
2. Kopiere prompten fra README
3. Køre step’et
4. Indsætte/oprette koden i projektet
5. Bygge løsningen
6. Rette fejl
7. Gå videre til næste step

## Faste regel-filer

Disse filer bruges igen og igen:

- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `03-ui-designregler.md`
- `04-koderegler.md`

---

# STEP 1 – Opret solution og projekter

## Upload disse filer
- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `04-koderegler.md`
- `05-step-01-solution-og-projekter.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 01 nu.

Opret solutionen PayBySharePay og opret projekterne:
- Api.PayBySharePay
- Service.PayBySharePay
- DataStorage.PayBySharePay
- Frontend.PayBySharePay

Backend skal være .NET 9.
Frontend skal være Angular + TypeScript.

Lav korrekt reference-struktur mellem projekterne.
Lav ikke domænemodel, repositories, services, controllers eller UI-sider endnu, ud over det som er nødvendigt for at solutionen kan bygges.

Vis:
- alle kommandoer
- hele indholdet af alle filer du opretter eller ændrer
- hvilke project references der skal oprettes

Stop når step 01 er færdigt.
```

---

# STEP 2 – Domænemodel og DbContext

## Upload disse filer
- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `04-koderegler.md`
- `06-step-02-domænemodel-og-dbcontext.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 02 nu.

Implementér domænemodellen og DbContext i DataStorage.PayBySharePay.
Løsningen skal understøtte både Person og Merchant som deltagertyper.

Opret entiteter, enums, relationer, Fluent API-konfiguration og DbContext.
Klargør løsningen til EF Core migrationer og SQL Server.

Lav ikke repositories, services, controllers eller Angular endnu.

Vis:
- hele indholdet af alle filer du opretter eller ændrer
- hvilke NuGet-pakker der skal installeres
- eksempel på connection string
- kommandoer til migration

Stop når step 02 er færdigt.
```

---

# STEP 3 – Repositories

## Upload disse filer
- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `04-koderegler.md`
- `07-step-03-repositories.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 03 nu.

Implementér repository-laget i DataStorage.PayBySharePay.
Repository-laget må kun håndtere dataadgang.

Opret interfaces og konkrete repository-klasser til de vigtigste entiteter og queries, herunder søgning efter deltagere og håndtering af merchants.

Brug async/await og EF Core korrekt.
Vis hele indholdet af alle filer du opretter eller ændrer.
Vis også hvilke DI-registreringer og project references der skal til.

Stop når step 03 er færdigt.
```

---

# STEP 4 – Services

## Upload disse filer
- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `04-koderegler.md`
- `08-step-04-services.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 04 nu.

Implementér service-laget i Service.PayBySharePay.
Service-laget skal indeholde forretningslogik og bruge repository-laget.

Implementér services til use cases som:
- søg deltagere
- opret person
- opret merchant
- tilføj ven
- opret ordre
- hent ordreoverblik
- registrer betaling
- hent og opret beskeder

Brug DTO'er hvor det giver mening.
Lav ikke controllers endnu.

Vis hele indholdet af alle filer du opretter eller ændrer.
Stop når step 04 er færdigt.
```

---

# STEP 5 – API

## Upload disse filer
- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `04-koderegler.md`
- `09-step-05-api.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 05 nu.

Implementér API-laget i Api.PayBySharePay.
Opret controllers, request/response DTO'er og dependency injection.

API'et skal som minimum understøtte:
- deltagersøgning
- opret person
- opret merchant
- venner
- ordrer
- ordreoverblik
- betalinger
- beskeder

Controllers må ikke indeholde forretningslogik.
Brug services fra Service.PayBySharePay.

Vis hele indholdet af alle filer du opretter eller ændrer.
Vis også den komplette Program.cs.

Stop når step 05 er færdigt.
```

---

# STEP 6 – Angular setup

## Upload disse filer
- `01-kravspecifikation.md`
- `03-ui-designregler.md`
- `04-koderegler.md`
- `10-step-06-angular-setup.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 06 nu.

Opsæt Frontend.PayBySharePay som Angular + TypeScript projekt.
Lav routing, app-struktur, core/shared-struktur og services til backend-kald.

Projektet skal være mobil-først og passe til iPhone 14.
Lav ikke de fulde sider endnu.

Vis:
- Angular CLI-kommandoer
- hele indholdet af alle filer du opretter eller ændrer

Stop når step 06 er færdigt.
```

---

# STEP 7 – Layout og navigation

## Upload disse filer
- `01-kravspecifikation.md`
- `03-ui-designregler.md`
- `04-koderegler.md`
- `11-step-07-layout-navigation.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 07 nu.

Implementér globalt layout, shell og bundnavigation i Angular.
Designet skal være mobilvenligt og følge designreglerne.

Lav kun layout og navigation nu.
Lav ikke hele indholdssider endnu.

Vis hele indholdet af alle filer du opretter eller ændrer.
Stop når step 07 er færdigt.
```

---

# STEP 8 – Forside

## Upload disse filer
- `01-kravspecifikation.md`
- `03-ui-designregler.md`
- `12-step-08-forside.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 08 nu.

Implementér forsiden i Angular.
Forsiden skal følge designreglerne og vise tydelige handlingskort.

Lav kun forsiden nu.
Brug mock-data hvis backend ikke er nødvendig for layoutet.

Vis hele indholdet af alle filer du opretter eller ændrer.
Stop når step 08 er færdigt.
```

---

# STEP 9 – Find deltagere

## Upload disse filer
- `01-kravspecifikation.md`
- `03-ui-designregler.md`
- `13-step-09-find-deltagere.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 09 nu.

Implementér siden Find deltagere i Angular.
Siden skal kunne vise både personer og merchants.
Vis søgning, lister, badges/markering af deltager-type og handling til at tilføje ven eller vælge deltager.

Brug backend hvis API'et er klar, ellers mock-data som let kan udskiftes.

Vis hele indholdet af alle filer du opretter eller ændrer.
Stop når step 09 er færdigt.
```

---

# STEP 10 – Ordreoverblik

## Upload disse filer
- `01-kravspecifikation.md`
- `03-ui-designregler.md`
- `14-step-10-ordreoverblik.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 10 nu.

Implementér skærmen Ordreoverblik i Angular.
Skærmen skal vise ordretype, besked, deltagere, status og tydelig handling til betaling.
Den skal kunne vise både personer og merchants.

Layoutet skal matche designreglerne og være mobil-først.

Vis hele indholdet af alle filer du opretter eller ændrer.
Stop når step 10 er færdigt.
```

---

# STEP 11 – Integration

## Upload disse filer
- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `03-ui-designregler.md`
- `04-koderegler.md`
- `15-step-11-integration.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 11 nu.

Integrér Angular-frontend med backend API.
Implementér eller færdiggør frontend-services og models til:
- deltagersøgning
- opret merchant/person
- venner
- ordrer
- betalinger
- beskeder

Ret kun det der er nødvendigt for integration.
Vis hele indholdet af alle filer du opretter eller ændrer.

Stop når step 11 er færdigt.
```

---

# STEP 12 – Tests

## Upload disse filer
- `01-kravspecifikation.md`
- `04-koderegler.md`
- `16-step-12-tests.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 12 nu.

Opret backend-tests for centrale use cases og acceptance criteria.
Fokusér på services og den vigtigste forretningslogik.

Vis:
- hvordan testprojektet skal oprettes
- hvilke pakker der skal installeres
- hele indholdet af testfilerne

Stop når step 12 er færdigt.
```

---

# STEP 13 – Dokumentation og oprydning

## Upload disse filer
- `01-kravspecifikation.md`
- `02-arkitektur-og-lag.md`
- `03-ui-designregler.md`
- `04-koderegler.md`
- `17-step-13-dokumentation-og-oprydning.md`
- `18-use-cases-og-acceptance.md`

## Prompt
```text
Brug de vedhæftede markdown-filer som bindende instruktioner.

Udfør kun step 13 nu.

Gennemgå løsningen og færdiggør dokumentation og oprydning.
Lav eller opdatér README, arkitekturdokumentation, API-beskrivelse og udviklerdokumentation.

Ret små inkonsistenser i navngivning og struktur, men lav ikke nye features.

Vis hele indholdet af alle dokumentationsfiler du opretter eller ændrer.
Hvis du ændrer kode, så vis hele filerne.

Stop når step 13 er færdigt.
```

---

## Fejlretningsprompt du kan genbruge

```text
Ret kun de konkrete build- eller runtime-fejl i det aktuelle step.
Følg stadig de vedhæftede markdown-filer som bindende krav.
Lav ingen nye features.
Vis hele de filer du ændrer.
```
