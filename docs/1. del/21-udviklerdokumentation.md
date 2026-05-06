# Udviklerdokumentation – PayBySharePay

## Forudsætninger

| Værktøj | Version |
|---------|---------|
| .NET SDK | 9.x |
| SQL Server / SQL Server Express | 2019 eller nyere |
| Node.js | 18+ |
| Angular CLI | 17+ |
| Visual Studio | 2022+ eller VS Code |

---

## Klargøring af løsning

### 1. Klon repository

```powershell
git clone https://github.com/mickni38-svg/PayBySharePPay.git
cd PayBySharePPay
```

### 2. Konfigurér connection string

Åbn `src\Api.PayBySharePay\appsettings.json` og opdatér:

```json
"ConnectionStrings": {
  "PayBySharePayDb": "Server=DIN_SERVER\\SQLEXPRESS;Database=PayBySharePay;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Erstat `DIN_SERVER\\SQLEXPRESS` med din egen SQL Server-instans.

### 3. Kør database-migration

```powershell
cd src\Api.PayBySharePay
dotnet ef database update --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj
```

Dette opretter databasen `PayBySharePay` og alle tabeller ud fra den eksisterende migration `InitialCreate`.

---

## Start backend

```powershell
cd src\Api.PayBySharePay
dotnet run
```

Backend starter på `https://localhost:{port}`.  
Swagger UI åbner automatisk på rod-URL'en (`/`) i Development-mode.

---

## Start frontend

```powershell
cd src\Frontend.PayBySharePay
npm install
ng serve
```

Frontend er tilgængeligt på `http://localhost:4200`.

---

## Byg løsningen

```powershell
cd src
dotnet build
```

---

## Kør tests

```powershell
cd src
dotnet test
```

---

## Tilføj ny migration

Brug dette mønster, når entiteter ændres:

```powershell
cd src\Api.PayBySharePay
dotnet ef migrations add NavnPåMigration --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj
dotnet ef database update --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj
```

---

## Tilføj ny feature (eksempel)

### 1. Entitet i DataStorage

Tilføj klasse i `src\DataStorage.PayBySharePay\Entities\`.  
Registrér `DbSet` i `PayBySharePayDbContext` og konfigurér relation i `OnModelCreating` om nødvendigt.

### 2. Repository

Opret interface i `src\DataStorage.PayBySharePay\Repositories\` og implementering.  
Registrér begge i `DataStorageServiceExtensions.AddDataStorage`.

### 3. Service

Opret interface i `src\Service.PayBySharePay\Interfaces\` og implementering i `Services\`.  
Tilføj relevante DTO'er i `src\Service.PayBySharePay\DTOs\`.  
Registrér i `ServiceLayerExtensions.AddServiceLayer`.

### 4. Controller

Tilføj controller i `src\Api.PayBySharePay\Controllers\`.  
Tilføj request-DTO'er i `src\Api.PayBySharePay\DTOs\`.

---

## Projektreferences

```
Api.PayBySharePay
  ├── → Service.PayBySharePay
  └── → DataStorage.PayBySharePay

Service.PayBySharePay
  └── → DataStorage.PayBySharePay
```

Frontend kommunikerer kun via HTTP og har ingen compile-time reference til backend-projekterne.

---

## Kendte konfigurationsdetaljer

- Connection string er gemt i `appsettings.json` – overskrivning pr. miljø sker i `appsettings.Development.json` eller via environment variables.
- `ExceptionHandlingMiddleware` er registreret tidligst i pipeline for at fange alle exceptions.
- Swagger er kun aktivt i Development-mode.
