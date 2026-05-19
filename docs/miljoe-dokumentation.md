# PayBySharePay – Miljødokumentation

Sidst opdateret: maj 2026

---

## Oversigt

PayBySharePay har to Azure-miljøer: **Test** og **Produktion**.

Begge miljøer kører på den samme Azure resource group `paybysharepay-rg` i regionen `Denmark East` (SQL/API) og `West Europe` (Static Web Apps).

---

## Miljøer

### Test

| Komponent | Ressource | URL |
|---|---|---|
| Frontend (Angular) | Azure Static Web App: `paybysharepay-frontend` (test slot) | https://purple-coast-0d01c1003.7.azurestaticapps.net |
| Merchant Demo | Azure Static Web App: `paybysharepay-merchant-demo` (test slot) | https://brave-flower-0026a7503.7.azurestaticapps.net |
| API (.NET 9) | Azure App Service: `paybysharepay-api` | https://paybysharepay-api.azurewebsites.net |
| Database | Azure SQL: `PayBySharePay-Test` på server `paybysharepay-server` | paybysharepay-server.database.windows.net |

### Produktion

| Komponent | Ressource | URL |
|---|---|---|
| Frontend (Angular) | Azure Static Web App: `paybysharepay-frontend` (prod slot) | https://icy-water-0750d2703.7.azurestaticapps.net |
| Merchant Demo | Azure Static Web App: `paybysharepay-merchant-demo` (prod slot) | https://ashy-bay-0e753db03.7.azurestaticapps.net |
| API (.NET 9) | Azure App Service: `paybysharepay-api-win` | https://paybysharepay-api-win.azurewebsites.net |
| Database | Azure SQL: `PayBySharePay` på server `paybysharepay-server` | paybysharepay-server.database.windows.net |
| Landing page | Azure Static Web App: `paybysharepay-landing` | https://victorious-smoke-06a8d2c03.7.azurestaticapps.net |
| Domæne (landing) | Custom domain via Simply.com | https://paybysharepay.dk |

---

## Azure-ressourcer

| Ressource | Type | Navn |
|---|---|---|
| Resource Group | Resource Group | `paybysharepay-rg` |
| SQL Server | Azure SQL Server | `paybysharepay-server` |
| Database (prod) | Azure SQL Database | `PayBySharePay` |
| Database (test) | Azure SQL Database | `PayBySharePay-Test` |
| API (prod) | App Service (Windows) | `paybysharepay-api-win` |
| API (test) | App Service (Linux) | `paybysharepay-api` |
| Frontend | Static Web App | `paybysharepay-frontend` |
| Merchant Demo | Static Web App | `paybysharepay-merchant-demo` |
| Landing Page | Static Web App | `paybysharepay-landing` |

---

## Databaser

### SQL Server

```
Server:   paybysharepay-server.database.windows.net
Auth:     Active Directory Default (Managed Identity / Azure CLI login)
Region:   Denmark East
SKU:      General Purpose, Serverless, Gen5 1 vCore
```

### Databaser

| Navn | Miljø | Indhold |
|---|---|---|
| `PayBySharePay` | Prod | Tom – klar til produktion. Skema oprettet via EF Core migrations maj 2026. |
| `PayBySharePay-Test` | Test | Eksisterende testdata. Omdøbt fra `PayBySharePay` maj 2026. |

### Connection string format

```
Server=paybysharepay-server.database.windows.net;Database=<DBNAVN>;Authentication=Active Directory Default;TrustServerCertificate=False;Encrypt=True;
```

---

## App Service konfiguration

Følgende app settings er sat direkte i Azure App Service (overskriver appsettings.json):

### paybysharepay-api-win (prod)

| Nøgle | Værdi |
|---|---|
| `ConnectionStrings__PayBySharePayDb` | Connection string til `PayBySharePay` (prod DB) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### paybysharepay-api (test)

| Nøgle | Værdi |
|---|---|
| `ConnectionStrings__PayBySharePayDb` | Connection string til `PayBySharePay-Test` (test DB) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

> **Bemærk:** JWT-nøgle og andre secrets sættes som App Settings i Azure – de må aldrig ligge i `appsettings.json` i kodebasen.

---

## Lokalt udviklingsmiljø

### API – appsettings.json

```json
{
  "AppSettings": {
	"ApiBaseUrl": "http://localhost:5071",
	"MerchantDemoUrl": "http://localhost:8081"
  },
  "ConnectionStrings": {
	"PayBySharePayDb": "Server=DESKTOP-HNI6DDI\\SQLEXPRESS;Database=PayBySharePay;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
	"Key": "SBYS-DEV-SECRET-REPLACE-IN-PRODUCTION-MIN-32-CHARS",
	"Issuer": "sbys-api",
	"Audience": "sbys-frontend",
	"ExpiresInMinutes": 43200
  }
}
```

Lokalt bruges `SQLEXPRESS` på udviklermaskinen med Windows-autentifikation.

---

## Deploy scripts

Alle deploy scripts ligger i roden af repositoriet og køres fra `C:\Users\Michael\source\repos\PayBySharePPay\`.

| Script | Formål |
|---|---|
| `deploy-prod.ps1` | Deployer Angular, MerchantDemo og .NET API til **produktion** |
| `deploy-test.ps1` | Deployer Angular, MerchantDemo og .NET API til **test** |
| `deploy-landing.ps1` | Deployer landing page til Azure Static Web Apps |
| `deploy-azure.ps1` | Generel Azure deploy (se scriptets indhold) |

### Eksempel: deploy til test

```powershell
cd C:\Users\Michael\source\repos\PayBySharePPay
.\deploy-test.ps1
```

### Eksempel: deploy til prod

```powershell
cd C:\Users\Michael\source\repos\PayBySharePPay
.\deploy-prod.ps1
```

### Trin i deploy-scripts (test og prod)

1. Bygger Angular (`ng build --configuration production`)
2. Deployer Angular til Azure Static Web Apps
3. Deployer MerchantDemo til Azure Static Web Apps
4. Bygger og publisher .NET 9 API (`dotnet publish`)
5. Pakker output til zip
6. Deployer API til Azure App Service via `az webapp deploy`

---

## EF Core migrations

Migrations ligger i projektet `src/DataStorage.PayBySharePay/Migrations/`.

### Køre migrations mod prod

```powershell
cd src\Api.PayBySharePay
$env:ConnectionStrings__PayBySharePayDb = "Server=paybysharepay-server.database.windows.net;Database=PayBySharePay;Authentication=Active Directory Default;TrustServerCertificate=False;Encrypt=True;"
dotnet ef database update --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj --startup-project .\Api.PayBySharePay.csproj
```

### Køre migrations mod test

```powershell
cd src\Api.PayBySharePay
$env:ConnectionStrings__PayBySharePayDb = "Server=paybysharepay-server.database.windows.net;Database=PayBySharePay-Test;Authentication=Active Directory Default;TrustServerCertificate=False;Encrypt=True;"
dotnet ef database update --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj --startup-project .\Api.PayBySharePay.csproj
```

### Oprette ny migration

```powershell
cd src\Api.PayBySharePay
dotnet ef migrations add <MigrationNavn> --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj --startup-project .\Api.PayBySharePay.csproj
```

---

## Projekter i solution

| Projekt | Type | Formål |
|---|---|---|
| `Api.PayBySharePay` | ASP.NET Core Web API (.NET 9) | REST API – backend for frontend og merchants |
| `DataStorage.PayBySharePay` | Class Library (.NET 9) | EF Core DbContext, entiteter, repositories, migrations |
| `Service.PayBySharePay` | Class Library (.NET 9) | Forretningslogik og services |
| `Tools.PayBySharePay` | Class Library (.NET 9) | Hjælpeværktøjer og utilities |
| `Tests.PayBySharePay` | Test projekt (.NET 9) | Unit tests |

Frontend (Angular) ligger i `src/Frontend.PayBySharePay/` og er ikke en del af .NET solution.

---

## Git og branch-strategi

| Branch | Formål |
|---|---|
| `main` | Primær branch – deployes til test og prod |

Repository: https://github.com/mickni38-svg/PayBySharePPay

---

## Domæner og DNS

| Domæne | Peger på | Konfigureret via |
|---|---|---|
| `paybysharepay.dk` | Azure Static Web App (landing page) | Simply.com (CNAME) |

---

## Historik over vigtige infrastrukturændringer

| Dato | Ændring |
|---|---|
| Maj 2026 | Eksisterende database `PayBySharePay` omdøbt til `PayBySharePay-Test` |
| Maj 2026 | Ny tom prod-database `PayBySharePay` oprettet og migreret med EF Core |
| Maj 2026 | `paybysharepay-api` (test) opdateret til at pege på `PayBySharePay-Test` |
| Maj 2026 | Landing page deployet til `paybysharepay-landing` Static Web App med custom domain `paybysharepay.dk` |
