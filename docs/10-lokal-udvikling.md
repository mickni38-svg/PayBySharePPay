# 10 – Lokal Udvikling

## Krav til udviklermaskine

| Krav | Version | Bemærkning |
|---|---|---|
| .NET SDK | 9.0+ | [download](https://dotnet.microsoft.com/download) |
| Visual Studio | 2022/2026 | Community eller højere |
| Node.js | 18+ | [download](https://nodejs.org/) |
| npm | 9+ | Følger med Node.js |
| Angular CLI | 17+ | `npm install -g @angular/cli` |
| SQL Server | Express eller højere | Lokal database |
| Azure CLI | Nyeste | Til deployment |
| SWA CLI | Nyeste | `npm install -g @azure/static-web-apps-cli` |

---

## Trin-for-trin: Start løsningen lokalt

### 1. Klon repository

```powershell
git clone https://github.com/mickni38-svg/PayBySharePPay.git
cd PayBySharePPay
```

---

### 2. Opret lokal database

Sæt din SQL Server connection string i [`appsettings.json`](../src/Api.PayBySharePay/appsettings.json):

```json
"ConnectionStrings": {
  "PayBySharePayDb": "Server=.\\SQLEXPRESS;Database=PayBySharePay;Trusted_Connection=True;TrustServerCertificate=True"
}
```

---

### 3. Kør database migrations

```powershell
cd src\Api.PayBySharePay
dotnet ef database update --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj
```

---

### 4. (Valgfri) Seed testdata

```powershell
cd src\Tools.PayBySharePay
dotnet run -- seed
```

---

### 5. Start backend API

```powershell
cd src\Api.PayBySharePay
dotnet run
# API kører på: https://localhost:7071 (eller http://localhost:5071)
# Swagger: https://localhost:7071/swagger
```

Eller åbn `PayBySharePPay.sln` i Visual Studio og sæt `Api.PayBySharePay` som startup-projekt.

---

### 6. Start Angular frontend

```powershell
cd src\Frontend.PayBySharePay
npm install
npx ng serve
# Tilgængeligt på: http://localhost:4200
```

---

### 7. Start MerchantDemo (valgfri)

MerchantDemoHostedService starter automatisk i Development-tilstand, når API'et kører.

Eller manuelt:

```powershell
cd src\Frontend.MerchantDemo
npm install
npm start
# Tilgængeligt på: http://localhost:8081
```

---

## Kør tests

```powershell
cd tests\Tests.PayBySharePay
dotnet test
```

Eller via Visual Studio Test Explorer.

---

## Flush og reseed

```powershell
cd src\Tools.PayBySharePay
dotnet run -- flush   # Slet alt data
dotnet run -- seed    # Seed testdata igen
```

---

## Kendte lokale problemer

### Problem: EF migrations fejler pga. Visual Studio låser DLL'er
**Løsning:** Luk Visual Studio eller kør med `--configuration Release`:
```powershell
dotnet ef database update --configuration Release ...
```

### Problem: MerchantDemo vises ikke korrekt lokalt
**Løsning:** Tjek at API kører på port 5071 og `appsettings.json` har `ApiBaseUrl: http://localhost:5071`.

### Problem: CORS-fejl ved API-kald
**Løsning:** Tjek at frontend-port matcher CORS-whitelist i [`Program.cs`](../src/Api.PayBySharePay/Program.cs).

### Problem: Angular build fejler
**Løsning:** Kør `npm install` og tjek Angular CLI version:
```powershell
npx ng version
```

---

## Se også

- [Konfiguration](09-konfiguration.md)
- [Database](07-database.md)
- [Fejlfinding](13-fejlfinding.md)
