# PayBySharePay – Dokumentation

## Hvad er PayBySharePay?

PayBySharePay er en webapplikation til **fælles bestillinger og delt betaling**. Brugere kan oprette en ordre (fx en pizzaaften eller biograftur), invitere deltagere, lade en restaurant/merchant styre bestillingen og håndtere betalinger.

## Målgruppe

Private brugere der ønsker at dele udgifter og koordinere bestillinger i grupper.

## Overblik over dokumentation

| Fil | Indhold |
|-----|---------|
| [README.md](README.md) | Denne fil – projektintro og kom-i-gang |
| [architecture.md](architecture.md) | Arkitektur, projekter, DI, dataflow |
| [features.md](features.md) | Funktionalitetsliste med status |
| [user-flows.md](user-flows.md) | Brugerflows og navigation |
| [data-and-state.md](data-and-state.md) | Modeller, state, datakilder |
| [api-and-integration.md](api-and-integration.md) | API-endpoints og integrationer |
| [testing.md](testing.md) | Teststatus og anbefalinger |
| [technical-debt.md](technical-debt.md) | Teknisk gæld og mangler |
| [copilot-next-steps.md](copilot-next-steps.md) | Prioriterede næste udviklingsopgaver |

## Teknisk overblik

| Lag | Teknologi |
|-----|-----------|
| Frontend | Angular 18, standalone components, signals |
| API | ASP.NET Core 9, REST, JWT |
| Service | .NET 9 class library |
| Datadlag | EF Core, SQL Server / Azure SQL |
| Hosting | Azure Static Web App (frontend), Azure App Service (API), Azure SQL |

## Kom i gang som udvikler

### Krav
- .NET 9 SDK
- Node.js + Angular CLI
- SQL Server lokal (SQLEXPRESS) eller adgang til Azure SQL

### Start lokalt

**API:**
```bash
cd src/Api.PayBySharePay
dotnet run
```

**Frontend:**
```bash
cd src/Frontend.PayBySharePay
npm install
ng serve
```

**Migrer database:**
```bash
cd src/Api.PayBySharePay
dotnet ef database update
```

**Seed testdata:**
```bash
cd src/Tools.PayBySharePay
dotnet run seed
```

**Seed mod prod:**
```bash
dotnet run -- --conn "Server=paybysharepay-server.database.windows.net;..." seed
```

### Dev-loginkonti (lokalt og prod)
Efter seed kan du logge ind med fx `test1@dev.dk` – ingen password kræves i MVP.
