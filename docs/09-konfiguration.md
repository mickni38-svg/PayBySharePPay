# 09 – Konfiguration

## Konfigurationsfiler

| Fil | Anvendelse |
|---|---|
| [`appsettings.json`](../src/Api.PayBySharePay/appsettings.json) | Base-konfiguration (lokal udvikling) |
| [`appsettings.Production.json`](../src/Api.PayBySharePay/appsettings.Production.json) | Produktion (overskrives af Azure App Service Settings) |
| [`environment.ts`](../src/Frontend.PayBySharePay/src/environments/environment.ts) | Angular – lokal API URL |
| [`environment.prod.ts`](../src/Frontend.PayBySharePay/src/environments/environment.prod.ts) | Angular – prod API URL |

---

## Konfigurationsnøgler

| Nøgle | Bruges til | Lokal | Produktion | Secret? |
|---|---|---|---|---|
| `ConnectionStrings:PayBySharePayDb` | Database-forbindelse | SQL Express lokal | Azure SQL | ✅ Ja |
| `Jwt:Key` | JWT-signeringsnøgle | Dev-placeholder | Azure App Settings | ✅ Ja |
| `Jwt:Issuer` | JWT issuer | `sbys-api` | `sbys-api` | Nej |
| `Jwt:Audience` | JWT audience | `sbys-frontend` | `sbys-frontend` | Nej |
| `Jwt:ExpiresInMinutes` | Token-varighed (min) | 43200 (30 dage) | 43200 | Nej |
| `AppSettings:ApiBaseUrl` | API base URL i notifikationslinks | `http://localhost:5071` | `https://paybysharepay-api-win.azurewebsites.net` | Nej |
| `AppSettings:MerchantDemoUrl` | MerchantDemo URL i notifikationslinks | `http://localhost:8081` | `https://ashy-bay-0e753db03.7.azurestaticapps.net` | Nej |

> ⚠️ **Skriv aldrig rigtige secrets i kode eller dokumentation.**

---

## appsettings.json (lokal)

```json
{
  "AppSettings": {
	"ApiBaseUrl": "http://localhost:5071",
	"MerchantDemoUrl": "http://localhost:8081"
  },
  "ConnectionStrings": {
	"PayBySharePayDb": "Server=...\\SQLEXPRESS;Database=PayBySharePay;..."
  },
  "Jwt": {
	"Key": "SBYS-DEV-SECRET-REPLACE-IN-PRODUCTION-MIN-32-CHARS",
	"Issuer": "sbys-api",
	"Audience": "sbys-frontend",
	"ExpiresInMinutes": 43200
  }
}
```

---

## Azure App Service Settings (produktion)

Følgende nøgler **skal** sættes i Azure App Service → Configuration → Application Settings:

| Nøgle | Kommentar |
|---|---|
| `ConnectionStrings__PayBySharePayDb` | Azure SQL connection string |
| `Jwt__Key` | Stærk hemmelig nøgle (min. 32 tegn) |
| `AppSettings__ApiBaseUrl` | `https://paybysharepay-api-win.azurewebsites.net` |
| `AppSettings__MerchantDemoUrl` | `https://ashy-bay-0e753db03.7.azurestaticapps.net` |

> Azure bruger `__` (dobbelt underscore) som separator for nøgler med hierarki.

---

## Angular environment-filer

**Lokal** (`environment.ts`):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5071'
};
```

**Produktion** (`environment.prod.ts`):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://paybysharepay-api-win.azurewebsites.net'
};
```

---

## Hvad må IKKE commit'es

- Rigtige Azure SQL connection strings
- Produktions JWT-nøgle
- Azure deployment tokens (SWA tokens)
- Brugernavn/passwords

---

## Se også

- [Authentication og security](08-authentication-security.md)
- [Azure deployment](11-azure-deployment-prod.md)
- [Lokal udvikling](10-lokal-udvikling.md)
