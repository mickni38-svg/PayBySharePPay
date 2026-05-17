# 11 – Azure Deployment (Produktion)

## Overblik over Azure-ressourcer

| Ressource | Navn | Formål |
|---|---|---|
| Azure App Service | `paybysharepay-api-win` | ASP.NET Core API hosting |
| Azure Static Web Apps | `icy-water-0750d2703.7.azurestaticapps.net` | Angular SPA frontend |
| Azure Static Web Apps | `ashy-bay-0e753db03.7.azurestaticapps.net` | MerchantDemo frontend |
| Azure SQL Database | (Azure SQL) | Produktionsdatabase |

---

## Deployment-script

Deployment køres via [`deploy-azure.ps1`](../deploy-azure.ps1):

```powershell
.\deploy-azure.ps1
```

Scriptet udfører følgende trin:

1. **Angular build** – `ng build --configuration production`
2. **Deploy Angular SPA** – `swa deploy` til `icy-water-...`
3. **Deploy MerchantDemo** – `swa deploy` til `ashy-bay-...`
4. **Publish API** – `dotnet publish -c Release`
5. **Zip API output** – `Compress-Archive`
6. **Deploy API** – `az webapp deploy` til App Service

---

## Trin-for-trin: Fuld produktion-deployment

### 1. Log ind i Azure

```powershell
az login
```

---

### 2. Kør migrations på Azure SQL

```powershell
dotnet ef database update --configuration Release `
  --project src\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj `
  --startup-project src\Api.PayBySharePay\Api.PayBySharePay.csproj
```

> ⚠️ Kræver at `appsettings.Production.json` har den rigtige Azure SQL connection string, eller sæt miljøvariabel `ASPNETCORE_ENVIRONMENT=Production`.

---

### 3. Kør deployment script

```powershell
.\deploy-azure.ps1
```

---

### 4. Verificér deployment

| URL | Forvented respons |
|---|---|
| `https://paybysharepay-api-win.azurewebsites.net/api/auth/login` | HTTP 405 (GET ikke tilladt) |
| `https://icy-water-0750d2703.7.azurestaticapps.net` | Angular app vises |
| `https://ashy-bay-0e753db03.7.azurestaticapps.net` | MerchantDemo vises |

---

## Azure App Service – Application Settings (SKAL sættes)

Gå til: Azure Portal → App Service `paybysharepay-api-win` → Configuration → Application Settings

| Nøgle | Beskrivelse |
|---|---|
| `ConnectionStrings__PayBySharePayDb` | Azure SQL connection string |
| `Jwt__Key` | Produktions JWT-signeringsnøgle (stærk, min. 32 tegn) |
| `AppSettings__ApiBaseUrl` | `https://paybysharepay-api-win.azurewebsites.net` |
| `AppSettings__MerchantDemoUrl` | `https://ashy-bay-0e753db03.7.azurestaticapps.net` |

---

## Seed data i produktion

```powershell
cd src\Tools.PayBySharePay
dotnet run -- flush --conn "<azure-sql-connection-string>"
dotnet run -- seed `
  --conn "<azure-sql-connection-string>" `
  --merchant-url "https://ashy-bay-0e753db03.7.azurestaticapps.net" `
  --api-url "https://paybysharepay-api-win.azurewebsites.net"
```

---

## Mangler i deployment-opsætning

### Testmiljø mangler

❌ **Der findes pt. ikke et dedikeret testmiljø (staging/QA) i Azure.**

Anbefalede tiltag:
- Opret separate Azure Static Web Apps til staging-frontend og staging-MerchantDemo
- Opret en App Service deployment slot (`staging`) til API
- Opret en separat Azure SQL database til testmiljøet
- Brug GitHub Actions til automatisk CI/CD til staging ved push til `develop`-branch

### Produktionsmiljø mangler

| Mangel | Konsekvens | Anbefaling |
|---|---|---|
| ❌ Ingen CI/CD pipeline | Manuel deployment via script | Opret GitHub Actions workflow |
| ❌ Ingen Azure Key Vault | Secrets i App Service Settings | Brug Azure Key Vault + Managed Identity |
| ❌ Ingen custom domain | Bruger Azure-standard URLs | Tilknyt `paybysharepay.dk` |
| ❌ Ingen SSL-certifikat til custom domain | Kræves ved custom domain | Konfigurer i App Service |
| ❌ Ingen Application Insights | Ingen telemetri/monitoring | Aktivér i App Service |
| ❌ Ingen automatisk skalering | Single instance | Konfigurer auto-scale regler |
| ❌ Ingen backup-strategi | Risiko for datatab | Aktivér Azure SQL backup |
| ❌ Ingen deployment slot (blue/green) | Downtime ved deployment | Tilføj staging slot + swap |

---

## Rollback

Der er pt. **ingen formel rollback-strategi.**

Manuel rollback:
1. Find seneste fungerende zip-package
2. Geninstaller via `az webapp deploy`
3. Kør evt. database migration tilbage: `dotnet ef database update <migration-name>`

---

## Logs og monitoring

```powershell
# Se live-logs fra App Service
az webapp log tail --name paybysharepay-api-win --resource-group <resource-group>
```

Eller via Azure Portal → App Service → Log stream.

---

## Se også

- [Konfiguration](09-konfiguration.md)
- [Database](07-database.md)
- [Fejlfinding](13-fejlfinding.md)
