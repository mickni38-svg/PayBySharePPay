# PayBySharePPay

**PayBySharePPay** er en dansk webapplikation der gør det nemmere at dele betalinger og håndtere gruppeordrer – fx pizza-aftener, fester eller fællesindkøb, hvor én betaler og de andre skal refundere.

[![API](https://img.shields.io/badge/API-Azure_App_Service-blue)](https://paybysharepay-api-win.azurewebsites.net)
[![Frontend](https://img.shields.io/badge/Frontend-Azure_Static_Web_Apps-blue)](https://icy-water-0750d2703.7.azurestaticapps.net)
[![MerchantDemo](https://img.shields.io/badge/MerchantDemo-Azure_Static_Web_Apps-blue)](https://ashy-bay-0e753db03.7.azurestaticapps.net)

---

## Hvad gør løsningen?

- Opret betalingsordrer med deltagere
- Send notifikationslinks til deltagere (via MerchantDemo-siden)
- Deltagere kan bekræfte betaling uden at logge ind
- Overblik over hvem der har betalt og hvem der mangler
- Beskedindbakke og aktivitetsoversigt

---

## Teknologistack

| Lag | Teknologi |
|---|---|
| Backend API | ASP.NET Core 9, C#, Entity Framework Core |
| Frontend | Angular (TypeScript) |
| Deltager-betalingsside | Vanilla HTML/JS (MerchantDemo) |
| Database | SQL Server / Azure SQL |
| Authentication | JWT Bearer Tokens |
| Hosting | Azure App Service + Azure Static Web Apps |
| Deployment | PowerShell + Azure CLI + SWA CLI |

---

## Hurtig start (lokal udvikling)

```powershell
# 1. Klon repository
git clone https://github.com/mickni38-svg/PayBySharePPay.git
cd PayBySharePPay

# 2. Kør database migrations
cd src\Api.PayBySharePay
dotnet ef database update --project ..\DataStorage.PayBySharePay\DataStorage.PayBySharePay.csproj

# 3. Start API
dotnet run

# 4. Start Angular frontend (nyt terminal-vindue)
cd src\Frontend.PayBySharePay
npm install && npx ng serve
```

Se [Lokal udvikling](docs/10-lokal-udvikling.md) for fuld guide.

---

## Dokumentation

| Dokument | Beskrivelse |
|---|---|
| [01 – Overblik](docs/01-overblik.md) | Hvad er systemet, brugerflows og teknologistack |
| [02 – Arkitektur](docs/02-arkitektur.md) | Lagdeling, dataflow og arkitekturbeslutninger |
| [03 – Projektstruktur](docs/03-projektstruktur.md) | Alle projekter og mapper i solution |
| [04 – Backend](docs/04-backend.md) | Controllers, services, repositories og middleware |
| [05 – Frontend](docs/05-frontend.md) | Angular features, MerchantDemo og API-kommunikation |
| [06 – API Endpoints](docs/06-api-endpoints.md) | Alle API-endpoints med request/response |
| [07 – Database](docs/07-database.md) | Entiteter, migrations og seed |
| [08 – Authentication og Security](docs/08-authentication-security.md) | JWT, CORS, sikkerhedsvurdering |
| [09 – Konfiguration](docs/09-konfiguration.md) | appsettings, miljøvariabler, Azure App Settings |
| [10 – Lokal udvikling](docs/10-lokal-udvikling.md) | Trin-for-trin guide til lokal opsætning |
| [11 – Azure Deployment](docs/11-azure-deployment-prod.md) | Deployment til Azure, mangler og anbefalinger |
| [12 – Test og Kvalitet](docs/12-test-og-kvalitet.md) | Tests, testdækning og anbefalinger |
| [13 – Fejlfinding](docs/13-fejlfinding.md) | Kendte problemer og løsninger |
| [14 – Vigtige kode-links](docs/14-vigtige-kode-links.md) | Direkte links til centrale filer og klasser |
| [15 – Screenshots](docs/15-screenshots.md) | Screenshots og TODO-liste for billeder |

---

## Mangler (kendte)

| Mangel | Prioritet | Beskrivelse |
|---|---|---|
| ❌ CI/CD pipeline | Høj | Ingen automatisk deployment ved push – alt er manuelt via `deploy-azure.ps1` |
| ❌ Testmiljø (staging) | Høj | Ingen separat staging/QA miljø i Azure |
| ❌ Azure Key Vault | Høj | Secrets håndteres via App Service Settings, ikke Key Vault |
| ❌ Custom domain | Medium | Bruger Azure-genererede URLs i stedet for `paybysharepay.dk` |
| ❌ Application Insights | Medium | Ingen telemetri eller performance-monitoring i produktion |
| ❌ Refresh tokens | Medium | JWT-tokens er gyldige i 30 dage – ingen refresh-mekanisme |
| ❌ Rollebaseret adgang | Medium | Alle brugere har samme adgangsniveau |
| ❌ Testdækning | Medium | Sparsom unit test dækning, ingen integration tests |
| ❌ Rate limiting | Lav | Ingen beskyttelse mod brute force på login |
| ❌ Backup-strategi | Lav | Ingen dokumenteret backup-plan for Azure SQL |
| ❌ Screenshots i docs | Lav | `docs/15-screenshots.md` indeholder kun TODOs |

---

## Mulige UI/UX forbedringer

| Forbedring | Beskrivelse |
|---|---|
| 📱 Mobiloptimering | Responsivt design bør forbedres – appen bruges primært på mobil |
| 🔔 Push notifikationer | Real-time notifikationer i stedet for manuel reload |
| 💬 SMS/email integration | Send faktiske SMS/email-notifikationer til deltagere (ikke kun links) |
| 🎨 Dark mode | Mørkt tema som brugervalg |
| ⚡ Loading states | Bedre feedback ved API-kald (skeleton loaders, spinners) |
| 🔍 Søgning og filtrering | Søg og filtrer ordrer efter dato, status, deltager |
| 📊 Statistik/dashboard | Oversigt over samlede betalinger, historik og tendenser |
| 🔁 Gentag ordre | Mulighed for at kopiere en tidligere ordre med samme deltagere |
| ✅ Ordrestatus-flow | Tydeligere visuel statusindikator (oprettet → aktiv → betalt → afsluttet) |
| 👥 Grupper | Gem faste deltagergrupper til genbrug (fx "Pizza-vennerne") |
| 🌐 Internationalisering | Dansk + engelsk sprogunderstøttelse |
| 🔐 Social login | Login med Google/Microsoft i stedet for email/password |
| 📋 Ordrehistorik eksport | Eksport til PDF eller CSV |

---

## Fordele ved nuværende løsning

| Fordel | Beskrivelse |
|---|---|
| ✅ Simpel arkitektur | Overskuelig N-tier struktur med klar separation of concerns |
| ✅ Lav driftsomkostning | Azure Static Web Apps (gratis tier) + App Service Basic |
| ✅ Ingen auth til deltager | MerchantDemo kræver ikke login – lavt friktion for betalere |
| ✅ EF Core migrations | Versionsstyret databaseskema i kode |
| ✅ Swagger i dev | Nem API-udforskning og test lokalt |
| ✅ Konfigurérbare URLs | API-URL og MerchantDemo-URL er konfigurérbare pr. miljø |
| ✅ Tools-konsolapp | Fleksibelt seed/flush-tool til dev og prod |
| ✅ .NET 9 | Moderne, høj-performance platform med langsigtet support |

---

## Ulemper ved nuværende løsning

| Ulempe | Beskrivelse |
|---|---|
| ❌ Manuel deployment | Ingen CI/CD – alt kræver manuelt script-kørsel |
| ❌ Ingen staging miljø | Ændringer deployes direkte til produktion |
| ❌ Lav testdækning | Risiko for regressioner ved ændringer |
| ❌ Ingen monitorering | Ingen alarmer eller automatisk fejldetektering |
| ❌ Vanilla JS MerchantDemo | Sværere at vedligeholde og udvide end et moderne framework |
| ❌ Lang JWT-varighed | 30-dages tokens er en sikkerhedsrisiko ved kompromitterede enheder |
| ❌ Ingen rollekontrol | Alle brugere har potentielt adgang til alle data |
| ❌ Ingen real-time | Ingen WebSocket/SignalR – deltagere skal manuelt genindlæse |

---

## Produktion URLs

| Service | URL |
|---|---|
| Angular Frontend | https://icy-water-0750d2703.7.azurestaticapps.net |
| MerchantDemo | https://ashy-bay-0e753db03.7.azurestaticapps.net |
| API | https://paybysharepay-api-win.azurewebsites.net |

