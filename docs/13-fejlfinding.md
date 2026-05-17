# 13 – Fejlfinding

## Problem: EF migrations fejler fordi Visual Studio låser DLL'er

**Symptom:**
```
Could not load file or assembly 'DataStorage.PayBySharePay.dll'
```

**Årsag:** Visual Studio har DLL-filen åben i debug-sessionen.

**Løsning:**
```powershell
# Kør migrations med Release-konfiguration
dotnet ef database update --configuration Release `
  --project src\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj `
  --startup-project src\Api.PayBySharePay\Api.PayBySharePay.csproj
```

---

## Problem: CORS-fejl i browser

**Symptom:**
```
Access to XMLHttpRequest at 'https://...' from origin 'http://localhost:4200' has been blocked by CORS policy
```

**Årsag:** Frontend-origin er ikke med i CORS-whitelist i `Program.cs`.

**Løsning:** Tilføj origin til CORS-policy i [`Program.cs`](../src/Api.PayBySharePay/Program.cs):
```csharp
policy.WithOrigins("http://localhost:4200", ...)
```

---

## Problem: JWT-token er udløbet

**Symptom:** API returnerer `401 Unauthorized` selvom brugeren er logget ind.

**Årsag:** Token er udløbet (standard: 30 dage).

**Løsning:** Log ud og log ind igen. Tjek `Jwt:ExpiresInMinutes` i `appsettings.json`.

---

## Problem: MerchantDemo viser ingen data

**Symptom:** Siden loader, men ordredata mangler eller fejler.

**Årsag:**
- Forkert `?token=` parameter i URL
- API-URL peger på forkert miljø
- CORS tillader ikke MerchantDemo-origin

**Løsning:**
1. Tjek at token i URL er korrekt (hentes fra seed/order)
2. Tjek at `?api=` parameter eller standard-URL er rigtig
3. Tjek CORS-whitelist i `Program.cs` inkluderer MerchantDemo URL

---

## Problem: Angular build fejler

**Symptom:**
```
An unhandled exception occurred: Cannot find module '@angular/core'
```

**Løsning:**
```powershell
cd src\Frontend.PayBySharePay
npm install
```

---

## Problem: Azure deployment fejler med exit code 1 (SWA CLI)

**Symptom:**
```
Deployment failed with exit code 1
```

**Årsag 1:** Forkert SWA deployment token.  
**Løsning:** Hent ny token fra Azure Portal → Static Web App → Manage deployment token.

**Årsag 2:** Forkert sti i deployment script (SWA CLI duplikerer sti).  
**Løsning:** Brug absolut sti til deploy-mappen:
```powershell
swa deploy "C:\...\Frontend.MerchantDemo" --deployment-token $token
```

---

## Problem: API starter ikke i Azure (App Service)

**Symptom:** App Service returnerer HTTP 500 eller "Application Error".

**Årsag:** Manglende Application Settings (f.eks. `ConnectionStrings__PayBySharePayDb` eller `Jwt__Key`).

**Løsning:** Gå til Azure Portal → App Service → Configuration → Application Settings og tilføj alle påkrævede nøgler.

---

## Problem: Seed fejler i produktion pga. manglende kolonne

**Symptom:**
```
Invalid column name 'IsRead'
```

**Årsag:** Migration `AddMessageIsRead` er ikke kørt i produktion.

**Løsning:** Kør migrations som beskrevet i [Azure deployment](11-azure-deployment-prod.md#2-kør-migrations-på-azure-sql).

---

## Problem: Angular SPA viser blank side i Azure Static Web Apps

**Symptom:** App vises ikke – kun blank hvid side.

**Årsag:** Routing mangler i `staticwebapp.config.json` – alle routes skal pege på `index.html`.

**Løsning:** Tilføj fallback route i `staticwebapp.config.json`:
```json
{
  "navigationFallback": {
	"rewrite": "/index.html"
  }
}
```

---

## Se også

- [Lokal udvikling](10-lokal-udvikling.md)
- [Azure deployment](11-azure-deployment-prod.md)
- [Konfiguration](09-konfiguration.md)
