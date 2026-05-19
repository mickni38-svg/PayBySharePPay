# Miljøer – Opsætning og Deploy Guide

Dette dokument beskriver step-by-step hvad der er sat op, og hvad du skal gøre for at fuldføre opsætningen af de tre miljøer for PayBySharePay.

---

## Overblik over miljøer

| Miljø | Formål | URL |
|---|---|---|
| **Landing Page** | Markedsføringsside – paybysharepay.dk peger hertil | Azure Static Web App (ny) |
| **Test** | Udviklingsmiljø – bruges til at teste inden prod-deploy | Azure Static Web Apps + App Service (ny) |
| **Prod** | Produktionsmiljø – det nuværende Azure-miljø | `icy-water-0750d2703.7.azurestaticapps.net` / `paybysharepay-api-win` |

---

## Hvad der allerede er gjort (i kodebasen)

1. **`src/Landing.PayBySharePay/index.html`** – simpel landing page med screenshots og feature-beskrivelser
2. **`src/Landing.PayBySharePay/staticwebapp.config.json`** – SWA-konfiguration
3. **`deploy-prod.ps1`** – deploy-script til produktionsmiljøet (eksisterende Azure-ressourcer)
4. **`deploy-test.ps1`** – deploy-script til testmiljøet (placeholders – se trin herunder)
5. **`deploy-landing.ps1`** – deploy-script til landing page (placeholder – se trin herunder)

---

## Trin 1: Tilføj screenshots til landing page

1. Gem dine screenshots som:
   - `src/Landing.PayBySharePay/images/screenshot-wizard.png`
   - `src/Landing.PayBySharePay/images/screenshot-order.png`
   - `src/Landing.PayBySharePay/images/screenshot-home.png`
2. (Valgfrit) Tilføj dit logo som `src/Landing.PayBySharePay/images/logo.png`

---

## Trin 2: Opret Landing Page Static Web App i Azure

1. Gå til [portal.azure.com](https://portal.azure.com)
2. Klik **Opret ressource** → søg **Static Web App** → **Opret**
3. Udfyld:
   - **Ressourcegruppe:** `paybysharepay-rg` (eller ny)
   - **Navn:** `paybysharepay-landing`
   - **Hosting plan:** Free
   - **Region:** West Europe
   - **Deployment source:** Other (vi deployer manuelt med script)
4. Klik **Gennemse + Opret** → **Opret**
5. Når ressourcen er oprettet: gå til **Manage deployment token**
6. Kopiér token og indsæt det i `deploy-landing.ps1`:
   ```powershell
   $landingToken = "<dit token her>"
   ```
7. Kør `.\deploy-landing.ps1` fra rod-mappen

---

## Trin 3: Peg paybysharepay.dk på Landing Page (Simply.com)

1. Log ind på [Simply.com](https://www.simply.com)
2. Gå til **Domæner** → **paybysharepay.dk** → **DNS**
3. Find den Azure Static Web App URL for landing page (fx `brave-rock-0123abc.azurestaticapps.net`)
4. I Azure landing page SWA: gå til **Custom domains** → **+ Add**  
   - Tilføj `paybysharepay.dk` og `www.paybysharepay.dk`
   - Azure giver dig et **CNAME** og/eller **TXT** record til validering
5. I Simply.com DNS – tilføj/opdatér:
   - **CNAME** for `www` → `<din-landing-swa>.azurestaticapps.net`
   - **A/ALIAS** for `@` (rod-domæne) → følg Azures vejledning (TXT for validering)
6. Vent op til 48 timer på DNS-propagering (typisk < 1 time)

---

## Trin 4: Opret Test Static Web Apps i Azure

Du skal oprette **to** nye Static Web Apps til test-frontend og test-merchant:

### Frontend (test)
1. **Opret ressource** → Static Web App
   - Navn: `paybysharepay-frontend-test`
   - Ressourcegruppe: `paybysharepay-test-rg` (opret ny)
2. Hent deployment token → indsæt i `deploy-test.ps1`:
   ```powershell
   $frontendToken = "<dit frontend test token>"
   ```

### MerchantDemo (test)
1. **Opret ressource** → Static Web App
   - Navn: `paybysharepay-merchant-test`
   - Ressourcegruppe: `paybysharepay-test-rg`
2. Hent deployment token → indsæt i `deploy-test.ps1`:
   ```powershell
   $merchantToken = "<dit merchant test token>"
   ```

---

## Trin 5: Opret Test App Service (API) i Azure

1. **Opret ressource** → App Service
   - Navn: `paybysharepay-api-test`
   - Ressourcegruppe: `paybysharepay-test-rg`
   - Runtime stack: **.NET 9**
   - OS: **Windows**
   - Plan: Free (F1) eller Basic (B1)
2. Opdatér `deploy-test.ps1` med de rigtige navne:
   ```powershell
   $apiResourceGroup = "paybysharepay-test-rg"
   $apiAppName       = "paybysharepay-api-test"
   ```

---

## Trin 6: Kør Test Deploy

Når alle tokens og navne er udfyldt i `deploy-test.ps1`:

```powershell
.\deploy-test.ps1
```

Test-miljøet vil være tilgængeligt på de Azure-genererede URLs (fx `https://paybysharepay-frontend-test.azurestaticapps.net`).

---

## Trin 7: Deploy til Produktion

Når test er godkendt, deploy til prod:

```powershell
.\deploy-prod.ps1
```

Dette deployer til det eksisterende produktionsmiljø som paybysharepay.dk **ikke** længere peger på (nu peger den på landing page).

---

## Overblik: Deploy scripts

| Script | Miljø | Kør hvornår |
|---|---|---|
| `deploy-landing.ps1` | Landing page | Når landing page ændres |
| `deploy-test.ps1` | Test | Ved hver ny feature/ændring til test |
| `deploy-prod.ps1` | Produktion | Når test er godkendt |
| `deploy-azure.ps1` | (legacy – samme som prod) | Erstattes af `deploy-prod.ps1` |

---

## Ressource-oversigt

### Produktion (eksisterende)
- **Frontend SWA:** `icy-water-0750d2703.7.azurestaticapps.net`
- **MerchantDemo SWA:** `ashy-bay-0e753db03.7.azurestaticapps.net`
- **API App Service:** `paybysharepay-api-win` (resource group: `paybysharepay-rg`)

### Test (oprettet)
- **Frontend SWA:** `purple-coast-0d01c1003.7.azurestaticapps.net` (resource group: `paybysharepay-test-rg`)
- **MerchantDemo SWA:** `brave-flower-0026a7503.7.azurestaticapps.net` (resource group: `paybysharepay-test-rg`)
- **API App Service:** `paybysharepay-api-test.azurewebsites.net` (resource group: `paybysharepay-test-rg`)

### Landing Page (oprettet)
- **Landing SWA:** `victorious-smoke-06a8d2c03.7.azurestaticapps.net` → skal pege på `paybysharepay.dk`
